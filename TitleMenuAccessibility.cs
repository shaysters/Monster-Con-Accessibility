using BeautifulGlitch;
using Game;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class TitleMenuAccessibility
    {
        private static TitleSectionButton _pendingSection;
        private static string _pendingLabel;
        private static float _pendingTime;
        private static bool _consumed;
        private static TitleSectionButton _currentSection;
        private static string _currentLabel;

        internal static void RecordOpen(TitleSectionButton section, string sectionLabel)
        {
            if (section == null)
            {
                return;
            }

            _pendingSection = section;
            _pendingLabel = UiTextReader.Clean(sectionLabel);
            _pendingTime = Time.realtimeSinceStartup;
            _consumed = false;
            _currentSection = section;
            _currentLabel = _pendingLabel;
        }

        internal static string DecorateFocus(GameObject focusedObject, string focusText)
        {
            if (string.IsNullOrEmpty(focusText))
            {
                return focusText;
            }

            if (_currentSection != null &&
                !string.IsNullOrEmpty(_currentLabel) &&
                _currentSection.gameObject.activeInHierarchy &&
                IsSubsectionButton(_currentSection, focusedObject))
            {
                return _currentLabel + " menu. " + StripMenuPrefix(focusText, _currentLabel);
            }

            if (_pendingSection == null || _consumed || string.IsNullOrEmpty(_pendingLabel))
            {
                return focusText;
            }

            if (Time.realtimeSinceStartup - _pendingTime <= 4f &&
                _pendingSection.gameObject.activeInHierarchy &&
                IsSubsectionButton(_pendingSection, focusedObject))
            {
                _consumed = true;
                return _pendingLabel + " menu. " + StripMenuPrefix(focusText, _pendingLabel);
            }

            return focusText;
        }

        internal static bool HasOpenSection()
        {
            return _currentSection != null && _currentSection.gameObject.activeInHierarchy && !string.IsNullOrEmpty(_currentLabel);
        }

        private static string StripMenuPrefix(string text, string label)
        {
            string prefix = label + " menu. ";
            if (!string.IsNullOrEmpty(text) && text.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return text.Substring(prefix.Length);
            }

            return text;
        }

        private static bool IsSubsectionButton(TitleSectionButton section, GameObject focusedObject)
        {
            try
            {
                System.Reflection.FieldInfo field = typeof(TitleSectionButton).GetField("subsectionButtons");
                ButtonHandler[] buttons = field == null ? null : field.GetValue(section) as ButtonHandler[];
                if (buttons == null)
                {
                    return false;
                }

                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] != null && buttons[i].gameObject == focusedObject)
                    {
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
            }

            return false;
        }
    }
}
