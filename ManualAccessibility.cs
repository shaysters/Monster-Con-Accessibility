using Game;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class ManualAccessibility
    {
        internal static void SpeakPage(string key)
        {
            try
            {
                string text = ManualPageDescriptions.Get(key);
                Plugin.SpeakProtected("manual:" + key, text, true, 2f, 8f);
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("ManualAccessibility.SpeakPage failed: " + ex);
            }
        }

        internal static bool TryHandleFocus(GameObject focusedObject)
        {
            try
            {
                if (focusedObject == null)
                {
                    return false;
                }

                SimpleInfoManualView manual = Object.FindObjectOfType<SimpleInfoManualView>();
                if (manual == null || !manual.gameObject.activeInHierarchy)
                {
                    return false;
                }

                if (manual.backButton != null && manual.backButton.gameObject == focusedObject)
                {
                    Plugin.SpeakOnce("manual-control:back", "Back. Return to the previous screen.", false, 0.8f);
                    return true;
                }
                if (manual.prevPage != null && manual.prevPage.gameObject == focusedObject)
                {
                    Plugin.SpeakOnce("manual-control:previous", "Previous page.", false, 0.8f);
                    return true;
                }
                if (manual.nextPage != null && manual.nextPage.gameObject == focusedObject)
                {
                    Plugin.SpeakOnce("manual-control:next", "Next page.", false, 0.8f);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("ManualAccessibility.TryHandleFocus failed: " + ex);
            }

            return false;
        }
    }
}
