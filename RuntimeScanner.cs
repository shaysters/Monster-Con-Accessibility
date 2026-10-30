using BeautifulGlitch;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    public sealed class RuntimeScanner : MonoBehaviour
    {
        private GameObject _lastFocusObject;
        private bool _wasWindowFocused = true;

        public void Start()
        {
            try
            {
            }
            catch (System.Exception ex)
            {
                LogFailure("RuntimeScanner.Start", ex);
            }
        }

        public void Update()
        {
            try
            {
                Tick("RuntimeScanner.Update");
            }
            catch (System.Exception ex)
            {
                LogFailure("RuntimeScanner.Update", ex);
            }
        }

        internal void Tick(string source)
        {
            try
            {
                bool windowFocused = Plugin.IsGameWindowFocused();
                if (!windowFocused)
                {
                    if (_wasWindowFocused)
                    {
                    }

                    _wasWindowFocused = false;
                    _lastFocusObject = null;
                    return;
                }

                if (!_wasWindowFocused)
                {
                    _wasWindowFocused = true;
                }

                HotkeyAccessibility.Tick(source);

                GameObject focused = UICamera.hoveredObject;
                if (focused == null)
                {
                    focused = UICamera.controllerNavigationObject;
                }
                if (focused == null)
                {
                    focused = UICamera.selectedObject;
                }

                if (focused != null && focused != _lastFocusObject)
                {
                    _lastFocusObject = focused;
                    Plugin.SpeakFocus(focused);
                }
            }
            catch (System.Exception ex)
            {
                LogFailure(source + " Tick", ex);
            }
        }

        private static void LogFailure(string label, System.Exception ex)
        {
            Plugin.Log?.LogWarning(label + " failed: " + ex);
        }
    }
}
