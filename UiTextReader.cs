using System.Collections.Generic;
using System.Text.RegularExpressions;
using BeautifulGlitch;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class UiTextReader
    {
        private static readonly Regex NguiColorTag = new Regex(@"\[[0-9a-fA-F]{6}(?:[0-9a-fA-F]{2})?\]|\[-\]", RegexOptions.Compiled);
        private static readonly Regex InlineMarkupTag = new Regex(@"\[(?:/?[a-zA-Z]+|[0-9a-fA-F]{2})\]", RegexOptions.Compiled);
        private static readonly Regex Whitespace = new Regex(@"\s+", RegexOptions.Compiled);

        internal static string DescribeObject(GameObject go)
        {
            try
            {
                if (go == null)
                {
                    return string.Empty;
                }

                string knownText;
                if (AccessibilityDescriber.TryDescribeObject(go, out knownText))
                {
                    return knownText;
                }

                List<string> parts = new List<string>();
                ButtonHandler handler = go.GetComponent<ButtonHandler>() ?? go.GetComponentInParent<ButtonHandler>();
                if (handler != null && handler.label != null)
                {
                    Add(parts, handler.label.text);
                }

                UILabel label = go.GetComponent<UILabel>();
                if (label != null)
                {
                    Add(parts, label.text);
                }

                UILabel[] childLabels = go.GetComponentsInChildren<UILabel>(true);
                int maxChildLabels = handler != null ? 2 : 1;
                if (handler == null && label == null && childLabels.Length > 2)
                {
                }
                else
                {
                    for (int i = 0; i < childLabels.Length && parts.Count < maxChildLabels; i++)
                    {
                        Add(parts, childLabels[i].text);
                    }
                }

                if (parts.Count == 0)
                {
                    Add(parts, Humanize(go.name));
                }

                return string.Join(". ", parts.ToArray());
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("UiTextReader.DescribeObject failed: " + ex);
                return string.Empty;
            }
        }

        internal static string Clean(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string text = value.Replace("\\n", " ");
            text = text.Replace("\r", " ").Replace("\n", " ");
            text = text.Replace("[n]", " ");
            text = text.Replace("[PC]", "player");
            text = text.Replace("[NPC]", "NPC");
            text = NguiColorTag.Replace(text, string.Empty);
            text = InlineMarkupTag.Replace(text, string.Empty);
            text = Whitespace.Replace(text, " ").Trim();
            if (Regex.IsMatch(text, @"^[\.\,\;\:\!\?\-]+$"))
            {
                return string.Empty;
            }
            text = Regex.Replace(text, @"\s+([\.\,\;\:\!\?])", "$1");
            return text;
        }

        private static void Add(List<string> parts, string value)
        {
            value = Clean(value);
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] == value)
                {
                    return;
                }
            }
            parts.Add(value);
        }

        private static string Humanize(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string text = name.Replace("_", " ").Replace("-", " ");
            text = Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
            text = Regex.Replace(text, @"\b(btn|button)\b", "button", RegexOptions.IgnoreCase);
            return text;
        }
    }
}
