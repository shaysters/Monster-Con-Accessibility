using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BepInEx.Logging;

namespace MonsterConAccessibilityMod
{
    // Prism (https://github.com/ethindp/prism) picks the best backend: a running screen
    // reader (NVDA, JAWS, ...) first, otherwise Windows TTS (SAPI / OneCore).
    // Minimal P/Invoke wrapper over Prism's C API; the prismatoid .NET bindings need a
    // newer runtime than the game's Mono. prism.dll (32-bit) is loaded by full path first
    // so the [DllImport] calls resolve to that already-loaded module.
    internal sealed class SpeechBridge : IDisposable
    {
        private const string PrismLib = "prism.dll";
        private const int PrismOk = 0;
        private const ulong SupportsOutput = 1UL << 5;
        private const ulong SupportsStop = 1UL << 7;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string lpFileName);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prism_init(IntPtr cfg);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prism_registry_acquire_best(IntPtr ctx);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prism_backend_name(IntPtr backend);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong prism_backend_get_features(IntPtr backend);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int prism_backend_speak(IntPtr backend, byte[] text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int prism_backend_output(IntPtr backend, byte[] text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int prism_backend_stop(IntPtr backend);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prism_error_string(int error);

        private readonly ManualLogSource _log;
        private IntPtr _ctx;
        private IntPtr _backend;
        private ulong _features;
        private string _activePrismPath;
        private string _lastText;
        private float _lastSpeechTime;

        internal SpeechBridge(ManualLogSource log)
        {
            _log = log;
        }

        internal void Initialize()
        {
            try
            {
                foreach (string dir in GetNativeSearchDirectories())
                {
                    if (InitializePrism(dir))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning("SpeechBridge.Initialize failed: " + ex);
            }

            string backend = _backend != IntPtr.Zero ? (PtrToUtf8(prism_backend_name(_backend)) ?? "unknown") : "none";
            _log.LogInfo("Speech backend: Prism/" + backend + " prismPath=\"" + (_activePrismPath ?? "") + "\"");
        }

        internal void Speak(string text, bool interrupt)
        {
            text = UiTextReader.Clean(text);
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            float now = UnityEngine.Time.realtimeSinceStartup;
            if (text == _lastText && now - _lastSpeechTime < 0.25f)
            {
                return;
            }
            _lastText = text;
            _lastSpeechTime = now;

            if (_backend == IntPtr.Zero)
            {
                return;
            }

            try
            {
                byte[] utf8 = ToUtf8(text);
                // Output = speech + braille on screen readers that support it.
                int result = (_features & SupportsOutput) != 0
                    ? prism_backend_output(_backend, utf8, interrupt)
                    : prism_backend_speak(_backend, utf8, interrupt);
                if (result != PrismOk)
                {
                    _log.LogWarning("Prism speak failed: " + PtrToUtf8(prism_error_string(result)));
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning("Speech failed: " + ex.Message);
            }
        }

        internal void Silence()
        {
            try
            {
                if (_backend != IntPtr.Zero && (_features & SupportsStop) != 0)
                {
                    prism_backend_stop(_backend);
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning("SpeechBridge.Silence failed: " + ex);
            }
        }

        public void Dispose()
        {
            try
            {
                // Do not shut down or unload the native speech library while Unity/Mono is shutting down.
                // The game process is exiting, and unloading native speech DLLs during domain teardown
                // can crash inside mono_domain_free on this Unity version.
            }
            catch (Exception ex)
            {
                _log.LogWarning("SpeechBridge.Dispose failed: " + ex);
            }
        }

        private bool InitializePrism(string baseDir)
        {
            try
            {
                string prismPath = Path.Combine(baseDir, PrismLib);
                if (!File.Exists(prismPath))
                {
                    return false;
                }

                if (LoadLibraryW(prismPath) == IntPtr.Zero)
                {
                    _log.LogWarning("Could not load prism.dll. Win32 error " + Marshal.GetLastWin32Error());
                    return false;
                }

                _activePrismPath = prismPath;
                _ctx = prism_init(IntPtr.Zero);
                if (_ctx == IntPtr.Zero)
                {
                    _log.LogWarning("prism_init failed.");
                    return false;
                }

                _backend = prism_registry_acquire_best(_ctx);
                if (_backend == IntPtr.Zero)
                {
                    _log.LogWarning("Prism found no speech backend.");
                    return false;
                }

                _features = prism_backend_get_features(_backend);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogWarning("InitializePrism failed for baseDir=\"" + baseDir + "\": " + ex);
                return false;
            }
        }

        private static string[] GetNativeSearchDirectories()
        {
            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            string gameRoot = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(pluginDir);
                if (dir.Parent != null && dir.Parent.Parent != null)
                {
                    gameRoot = dir.Parent.Parent.FullName;
                }
            }
            catch (Exception)
            {
            }

            return new[] { pluginDir, gameRoot };
        }

        private static byte[] ToUtf8(string text)
        {
            byte[] bytes = new byte[Encoding.UTF8.GetByteCount(text) + 1];
            Encoding.UTF8.GetBytes(text, 0, text.Length, bytes, 0);
            return bytes;
        }

        private static string PtrToUtf8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            int length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
            {
                length++;
            }

            byte[] bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
