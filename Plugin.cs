using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using BeautifulGlitch;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonsterConAccessibilityMod
{
    [BepInPlugin("shay.monstercon.accessibility.tts", "Monster Con TTS Accessibility", "1.1.3")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string HarmonyId = "shay.monstercon.accessibility.tts";

        private Harmony _harmony;
        private bool _patchesInstalled;
        internal static ManualLogSource Log { get; private set; }
        internal static SpeechBridge Speech { get; private set; }
        private static GameObject _lastSpokenFocusObject;
        private static string _lastSpokenFocusText;
        private static float _lastSpokenFocusTime;
        private static string _lastSpokenPanelKey;
        private static string _lastSpokenPanelText;
        private static float _lastSpokenPanelTime;
        private static float _suppressFocusSpeechUntil;
        private static float _suppressFocusSpeechStarted;
        private static string _suppressFocusSpeechReason;
        private static string _lastSpokenText;
        private static float _lastSpokenTextTime;
        private static string _lastRepeatableSpokenText;
        private static float _lastRepeatableSpokenTextTime;
        private static readonly int CurrentProcessId = Process.GetCurrentProcess().Id;

        private void Awake()
        {
            try
            {
                Log = Logger;
                Logger.LogInfo("Process pointer size: " + IntPtr.Size + " bytes.");
                Speech = new SpeechBridge(Logger);
                Speech.Initialize();
                gameObject.AddComponent<RuntimeScanner>();

                SceneManager.sceneLoaded += OnSceneLoaded;
                InstallPatches();

                Logger.LogInfo("Monster Con TTS Accessibility loaded.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Plugin.Awake failed: " + ex);
            }
        }

        private void InstallPatches()
        {
            if (_patchesInstalled)
            {
                return;
            }

            try
            {
                _harmony = new Harmony(HarmonyId);
                _harmony.PatchAll(typeof(Plugin).Assembly);
                _patchesInstalled = true;
                Logger.LogInfo("Harmony patches installed delayed.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Harmony delayed patch install failed: " + ex);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {
                Logger.LogInfo("Scene loaded: " + scene.name);
                InstallPatches();
            }
            catch (Exception ex)
            {
                Logger.LogWarning("OnSceneLoaded failed: " + ex);
            }
        }

        private void OnDestroy()
        {
            try
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Plugin.OnDestroy failed: " + ex);
            }
        }

        private void DisposeRuntimeResources()
        {
            try
            {
                if (_harmony != null)
                {
                    _harmony.UnpatchSelf();
                    _harmony = null;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (Speech != null)
                {
                    Speech.Dispose();
                    Speech = null;
                }
            }
            catch (Exception)
            {
            }
        }

        internal static void Speak(string text, bool interrupt)
        {
            Speak(text, interrupt, true);
        }

        private static void Speak(string text, bool interrupt, bool updateRepeatText)
        {
            try
            {
                text = UiTextReader.Clean(text);
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                float now = Time.realtimeSinceStartup;
                if (text == _lastSpokenText && now - _lastSpokenTextTime < 0.35f)
                {
                    return;
                }

                if (updateRepeatText)
                {
                    _lastRepeatableSpokenText = text;
                    _lastRepeatableSpokenTextTime = now;
                }

                _lastSpokenText = text;
                _lastSpokenTextTime = now;
                Speech?.Speak(text, interrupt);
            }
            catch (Exception ex)
            {
                Log?.LogWarning("Plugin.Speak failed: " + ex);
            }
        }

        internal static void SpeakFocus(GameObject focusedObject)
        {
            try
            {
                string objectName = focusedObject == null ? "null" : focusedObject.name;

                if (focusedObject == null)
                {
                    return;
                }

                if (!IsGameWindowFocused())
                {
                    return;
                }

                if (!focusedObject.activeInHierarchy)
                {
                    return;
                }

                if (ShouldIgnoreFocusObject(objectName))
                {
                    return;
                }

                ButtonHandler handler = focusedObject.GetComponent<ButtonHandler>() ?? focusedObject.GetComponentInParent<ButtonHandler>();
                if (handler != null && handler.button != null && !handler.button.isEnabled)
                {
                    return;
                }

                if (ManualAccessibility.TryHandleFocus(focusedObject))
                {
                    return;
                }

                bool interruptFocusSpeech = true;
                if (Time.realtimeSinceStartup < _suppressFocusSpeechUntil)
                {
                    bool isPopupSuppression = !string.IsNullOrEmpty(_suppressFocusSpeechReason) && _suppressFocusSpeechReason.StartsWith("popup:", StringComparison.OrdinalIgnoreCase);
                    float suppressElapsed = Time.realtimeSinceStartup - _suppressFocusSpeechStarted;
                    if (isPopupSuppression && suppressElapsed >= 0.75f && IsPopupButtonFocus(focusedObject))
                    {
                    }
                    else
                    {
                        return;
                    }
                }

                if (SettingsAccessibility.TrySpeakFocus(focusedObject))
                {
                    return;
                }

                string text = UiTextReader.DescribeObject(focusedObject);
                text = TitleMenuAccessibility.DecorateFocus(focusedObject, text);
                if (!string.IsNullOrEmpty(text))
                {
                    float now = Time.realtimeSinceStartup;
                    if (focusedObject == _lastSpokenFocusObject && now - _lastSpokenFocusTime < 1f)
                    {
                        return;
                    }

                    if (text == _lastSpokenFocusText && now - _lastSpokenFocusTime < 2f)
                    {
                        return;
                    }

                    _lastSpokenFocusObject = focusedObject;
                    _lastSpokenFocusText = text;
                    _lastSpokenFocusTime = now;
                    Speak(text, interruptFocusSpeech, false);
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Log?.LogWarning("SpeakFocus failed: " + ex);
            }
        }

        internal static bool IsGameWindowFocused()
        {
            try
            {
                if (!Application.isFocused)
                {
                    return false;
                }

                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    return true;
                }

                int foregroundProcessId;
                GetWindowThreadProcessId(foregroundWindow, out foregroundProcessId);
                return foregroundProcessId == 0 || foregroundProcessId == CurrentProcessId;
            }
            catch (Exception ex)
            {
                Log?.LogWarning("IsGameWindowFocused failed: " + ex);
                return true;
            }
        }

        internal static bool SpeakOnce(string key, string text, bool interrupt, float duplicateSeconds)
        {
            try
            {
                text = UiTextReader.Clean(text);
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }

                float now = Time.realtimeSinceStartup;
                if (key == _lastSpokenPanelKey && text == _lastSpokenPanelText && now - _lastSpokenPanelTime < duplicateSeconds)
                {
                    return false;
                }

                _lastSpokenPanelKey = key;
                _lastSpokenPanelText = text;
                _lastSpokenPanelTime = now;
                Speak(text, interrupt, IsRepeatEligiblePanelKey(key));
                return true;
            }
            catch (Exception ex)
            {
                Log?.LogWarning("SpeakOnce failed: " + ex);
                return false;
            }
        }

        internal static void SpeakProtected(string key, string text, bool interrupt, float duplicateSeconds, float suppressFocusSeconds)
        {
            if (SpeakOnce(key, text, interrupt, duplicateSeconds))
            {
                SuppressFocusSpeech(suppressFocusSeconds, key);
            }
        }

        internal static void SuppressFocusSpeech(float seconds, string reason)
        {
            try
            {
                _suppressFocusSpeechUntil = Time.realtimeSinceStartup + seconds;
                _suppressFocusSpeechStarted = Time.realtimeSinceStartup;
                _suppressFocusSpeechReason = reason ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log?.LogWarning("SuppressFocusSpeech failed: " + ex);
            }
        }

        internal static void RepeatLastSpoken()
        {
            try
            {
                if (string.IsNullOrEmpty(_lastRepeatableSpokenText))
                {
                    Speak("No repeatable spoken text yet.", true, false);
                    return;
                }

                Speech?.Speak(_lastRepeatableSpokenText, true);
            }
            catch (Exception ex)
            {
                Log?.LogWarning("RepeatLastSpoken failed: " + ex);
            }
        }

        internal static void SpeakCurrentStats()
        {
            SpeakProtected("hotkey:stats", AccessibilityDescriber.DescribeStats(), true, 0.5f, 1f);
        }

        private static bool IsRepeatEligiblePanelKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return key.StartsWith("dialogue:", StringComparison.OrdinalIgnoreCase) ||
                   key.StartsWith("manual:", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldIgnoreFocusObject(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return true;
            }

            return objectName.IndexOf("UI Root", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   objectName.IndexOf("DummyButton", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   objectName.IndexOf("FakeButton", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   objectName.IndexOf("ColliderBlocker", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   objectName.IndexOf("Collider Blocker", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   objectName.IndexOf("Blocker", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   objectName.IndexOf("SectionOpener", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsPopupButtonFocus(GameObject focusedObject)
        {
            if (focusedObject == null || string.IsNullOrEmpty(focusedObject.name))
            {
                return false;
            }

            string name = focusedObject.name;
            return name.Equals("FirstButton", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("SecondButton", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("ThirdButton", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("YesButton", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("NoButton", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("OkButton", StringComparison.OrdinalIgnoreCase);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    }
}
