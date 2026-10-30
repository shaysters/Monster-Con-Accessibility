using System.Reflection;
using BeautifulGlitch;
using BeautifulGlitchConfig;
using Game;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class SettingsAccessibility
    {
        internal static bool TrySpeakFocus(GameObject focusedObject)
        {
            try
            {
                SettingsWidget widget;
                string direction;
                if (!TryGetSettingsWidget(focusedObject, out widget, out direction))
                {
                    return TrySpeakTitleSettingsButton(focusedObject) || TrySpeakCustomizeFocus(focusedObject);
                }

                string text = Describe(widget, direction);
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }

                Plugin.SpeakOnce("settings:" + widget.type + ":" + direction, text, true, 0.8f);
                return true;
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("SettingsAccessibility.TrySpeakFocus failed: " + ex);
                return false;
            }
        }

        internal static void SpeakValue(SettingsWidget widget)
        {
            SpeakValue(widget, null);
        }

        internal static void SpeakValue(SettingsWidget widget, int? barValue)
        {
            try
            {
                if (!IsFocusedWidget(widget))
                {
                    return;
                }

                string text = DescribeValue(widget, barValue);
                if (!string.IsNullOrEmpty(text))
                {
                    Plugin.SpeakProtected("settings-value:" + widget.type, text, true, 0.5f, 0.75f);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("SettingsAccessibility.SpeakValue failed: " + ex);
            }
        }

        internal static void SpeakCustomizeValue(TriggerWarningWidget widget)
        {
            SpeakCustomizeValue("trigger-warning:" + (widget == null ? "null" : widget.type.ToString()), DescribeTriggerWarning(widget, string.Empty, true), 0.25f);
        }

        internal static void SpeakCustomizeValue(AllowSelfLoveWidget widget)
        {
            SpeakCustomizeValue("allow-self-love", DescribeBooleanCustomize("Allow self-love endings", ValueText(widget == null ? null : widget.valueLabel), "Allows the solo final choice to produce successful alone endings.", string.Empty, true), 0.25f);
        }

        internal static void SpeakCustomizeValue(TriggerSeenPlotlinesRandomlyWidget widget)
        {
            SpeakCustomizeValue("seen-plotlines-randomly", DescribeBooleanCustomize("Trigger seen plotlines randomly", ValueText(widget == null ? null : widget.valueLabel), "When enabled, already unlocked plotlines may appear randomly again.", string.Empty, true), 0.25f);
        }

        internal static void SpeakCustomizeValue(ReplaceAprilCreepypastaWidget widget)
        {
            SpeakCustomizeValue("replace-april-creepypasta", DescribeBooleanCustomize("Replace April creepy visuals", ValueText(widget == null ? null : widget.valueLabel), "Replaces April's creepy visual content. Text is not changed.", string.Empty, true), 0.25f);
        }

        internal static void SpeakCustomizeValue(OutfitPackWidget widget)
        {
            if (widget == null)
            {
                return;
            }

            string name = CleanOrFallback(widget.titleLabel == null ? string.Empty : widget.titleLabel.text, Humanize(widget.type.ToString()));
            string state = GetPrivateBool(widget, "mIsOutfitPackEnabled") ? "enabled" : "disabled";
            SpeakCustomizeValue("outfit-pack:" + widget.type, "Outfit pack, " + name + ", " + state + ".", 0.25f);
        }

        private static bool IsFocusedWidget(SettingsWidget widget)
        {
            if (widget == null)
            {
                return false;
            }

            GameObject focused = UICamera.hoveredObject;
            if (focused == null)
            {
                focused = UICamera.controllerNavigationObject;
            }
            if (focused == null)
            {
                focused = UICamera.selectedObject;
            }

            if (focused == null)
            {
                return false;
            }

            return (widget.leftArrow != null && widget.leftArrow.gameObject == focused) ||
                   (widget.rightArrow != null && widget.rightArrow.gameObject == focused);
        }

        private static bool TryGetSettingsWidget(GameObject focusedObject, out SettingsWidget widget, out string direction)
        {
            widget = null;
            direction = string.Empty;
            if (focusedObject == null)
            {
                return false;
            }

            SettingsWidget[] widgets = Object.FindObjectsOfType<SettingsWidget>();
            for (int i = 0; i < widgets.Length; i++)
            {
                SettingsWidget candidate = widgets[i];
                if (candidate == null || !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (candidate.leftArrow != null && candidate.leftArrow.gameObject == focusedObject)
                {
                    widget = candidate;
                    direction = "left";
                    return true;
                }

                if (candidate.rightArrow != null && candidate.rightArrow.gameObject == focusedObject)
                {
                    widget = candidate;
                    direction = "right";
                    return true;
                }
            }

            return false;
        }

        private static bool TrySpeakTitleSettingsButton(GameObject focusedObject)
        {
            try
            {
                if (focusedObject == null)
                {
                    return false;
                }

                TitleSettings settings = Object.FindObjectOfType<TitleSettings>();
                if (settings == null || !settings.gameObject.activeInHierarchy)
                {
                    return false;
                }

                if (IsWidgetObject(settings.backButton, focusedObject))
                {
                    Plugin.SpeakOnce("settings-command:back", "Back. Return to the main menu. Some setting changes may reload the title screen.", true, 0.8f);
                    return true;
                }

                if (IsWidgetObject(settings.resetTutorialsButton, focusedObject))
                {
                    Plugin.SpeakOnce("settings-command:reset-tutorials", "Reset tutorials. Opens a confirmation popup to show tutorials again.", true, 0.8f);
                    return true;
                }

                if (IsWidgetObject(settings.deleteProgressButton, focusedObject))
                {
                    Plugin.SpeakOnce("settings-command:delete-progress", "Delete progress. Opens a confirmation popup before deleting saved progress.", true, 0.8f);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("SettingsAccessibility.TrySpeakTitleSettingsButton failed: " + ex);
            }

            return false;
        }

        private static bool TrySpeakCustomizeFocus(GameObject focusedObject)
        {
            try
            {
                if (focusedObject == null)
                {
                    return false;
                }

                TitleCustomize customize = Object.FindObjectOfType<TitleCustomize>();
                if (customize == null || !customize.gameObject.activeInHierarchy || !customize.customizeContainer.activeInHierarchy)
                {
                    return false;
                }

                TriggerWarningWidget triggerWarning;
                string direction;
                if (TryGetTriggerWarningWidget(focusedObject, out triggerWarning, out direction))
                {
                    return SpeakCustomizeFocus("trigger-warning:" + triggerWarning.type + ":" + direction, DescribeTriggerWarning(triggerWarning, direction, false));
                }

                if (MatchesCustomizeColliders(customize.allowSelfLoveWidget, focusedObject, out direction))
                {
                    string text = DescribeBooleanCustomize("Allow self-love endings", ValueText(customize.allowSelfLoveWidget.valueLabel), "Allows the solo final choice to produce successful alone endings.", direction, false);
                    return SpeakCustomizeFocus("allow-self-love:" + direction, text);
                }

                if (MatchesCustomizeColliders(customize.triggerSeenPlotlinesRandomlyWidget, focusedObject, out direction))
                {
                    string text = DescribeBooleanCustomize("Trigger seen plotlines randomly", ValueText(customize.triggerSeenPlotlinesRandomlyWidget.valueLabel), "When enabled, already unlocked plotlines may appear randomly again.", direction, false);
                    return SpeakCustomizeFocus("seen-plotlines-randomly:" + direction, text);
                }

                if (MatchesCustomizeColliders(customize.replaceAprilCreepypastaWidget, focusedObject, out direction))
                {
                    string text = DescribeBooleanCustomize("Replace April creepy visuals", ValueText(customize.replaceAprilCreepypastaWidget.valueLabel), "Replaces April's creepy visual content. Text is not changed.", direction, false);
                    return SpeakCustomizeFocus("replace-april-creepypasta:" + direction, text);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("SettingsAccessibility.TrySpeakCustomizeFocus failed: " + ex);
            }

            return false;
        }

        private static bool TryGetTriggerWarningWidget(GameObject focusedObject, out TriggerWarningWidget widget, out string direction)
        {
            widget = null;
            direction = string.Empty;
            TriggerWarningWidget[] widgets = Object.FindObjectsOfType<TriggerWarningWidget>();
            for (int i = 0; i < widgets.Length; i++)
            {
                TriggerWarningWidget candidate = widgets[i];
                if (candidate == null || !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (IsColliderObject(candidate.leftArrowCollider, focusedObject))
                {
                    widget = candidate;
                    direction = "left";
                    return true;
                }
                if (IsColliderObject(candidate.rightArrowCollider, focusedObject))
                {
                    widget = candidate;
                    direction = "right";
                    return true;
                }
                if (IsColliderObject(candidate.infoCollider, focusedObject))
                {
                    widget = candidate;
                    direction = "info";
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesCustomizeColliders(AllowSelfLoveWidget widget, GameObject focusedObject, out string direction)
        {
            direction = string.Empty;
            return widget != null && MatchesColliderTriplet(widget.leftArrowCollider, widget.rightArrowCollider, widget.infoCollider, focusedObject, out direction);
        }

        private static bool MatchesCustomizeColliders(TriggerSeenPlotlinesRandomlyWidget widget, GameObject focusedObject, out string direction)
        {
            direction = string.Empty;
            return widget != null && MatchesColliderTriplet(widget.leftArrowCollider, widget.rightArrowCollider, widget.infoCollider, focusedObject, out direction);
        }

        private static bool MatchesCustomizeColliders(ReplaceAprilCreepypastaWidget widget, GameObject focusedObject, out string direction)
        {
            direction = string.Empty;
            return widget != null && MatchesColliderTriplet(widget.leftArrowCollider, widget.rightArrowCollider, widget.infoCollider, focusedObject, out direction);
        }

        private static bool MatchesColliderTriplet(Collider left, Collider right, Collider info, GameObject focusedObject, out string direction)
        {
            if (IsColliderObject(left, focusedObject))
            {
                direction = "left";
                return true;
            }
            if (IsColliderObject(right, focusedObject))
            {
                direction = "right";
                return true;
            }
            if (IsColliderObject(info, focusedObject))
            {
                direction = "info";
                return true;
            }

            direction = string.Empty;
            return false;
        }

        private static bool IsColliderObject(Collider collider, GameObject focusedObject)
        {
            return collider != null && collider.gameObject == focusedObject;
        }

        private static bool SpeakCustomizeFocus(string key, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            Plugin.SpeakOnce("customize-focus:" + key, text, true, 0.8f);
            return true;
        }

        private static void SpeakCustomizeValue(string key, string text, float duplicateSeconds)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Plugin.SpeakProtected("customize-value:" + key, text, true, duplicateSeconds, 0.4f);
        }

        private static string DescribeTriggerWarning(TriggerWarningWidget widget, string direction, bool valueOnly)
        {
            if (widget == null)
            {
                return string.Empty;
            }

            string title = CleanOrFallback(widget.titleLabel == null ? string.Empty : widget.titleLabel.text, Humanize(widget.type.ToString()));
            string value = ValueText(widget.valueLabel);
            string text = "Trigger warning filter, " + title;
            if (!string.IsNullOrEmpty(value))
            {
                text += ", " + value;
            }
            text += ". " + TriggerWarningMeaning(value);
            if (!valueOnly)
            {
                text += " " + TriggerWarningHint(direction);
            }
            return text;
        }

        private static string DescribeBooleanCustomize(string title, string value, string meaning, string direction, bool valueOnly)
        {
            string text = title;
            if (!string.IsNullOrEmpty(value))
            {
                text += ", " + value;
            }
            text += ". " + meaning;
            if (!valueOnly)
            {
                text += " " + ToggleHint(direction);
            }
            return text;
        }

        private static string TriggerWarningMeaning(string value)
        {
            string lowered = (value ?? string.Empty).ToLowerInvariant();
            if (lowered.Contains("block"))
            {
                return "Matching content is blocked.";
            }
            if (lowered.Contains("notif") || lowered.Contains("notice"))
            {
                return "Matching content is allowed, but the game shows a warning icon.";
            }
            return "Matching content is allowed without extra warning.";
        }

        private static string TriggerWarningHint(string direction)
        {
            if (direction == "left")
            {
                return "Left arrow moves to the previous filter level.";
            }
            if (direction == "right")
            {
                return "Right arrow moves to the next filter level.";
            }
            return "Use left or right to choose Allow, Notify, or Block.";
        }

        private static string ToggleHint(string direction)
        {
            if (direction == "left")
            {
                return "Left arrow toggles this setting.";
            }
            if (direction == "right")
            {
                return "Right arrow toggles this setting.";
            }
            return "Use left or right to toggle this setting.";
        }

        private static bool IsWidgetObject(TweenableWidget widget, GameObject focusedObject)
        {
            return widget != null && widget.gameObject == focusedObject;
        }

        private static string DescribeValue(SettingsWidget widget, int? barValue)
        {
            if (widget == null)
            {
                return string.Empty;
            }

            string title = widget.titleLabel == null ? Humanize(widget.type.ToString()) : UiTextReader.Clean(widget.titleLabel.text);
            if (string.IsNullOrEmpty(title))
            {
                title = Humanize(widget.type.ToString());
            }

            string value = widget.valueLabel == null ? string.Empty : UiTextReader.Clean(widget.valueLabel.text);
            if (barValue.HasValue)
            {
                value = barValue.Value + " out of 10";
            }
            else if (string.IsNullOrEmpty(value) && widget.barFill != null)
            {
                value = Mathf.RoundToInt(widget.barFill.fillAmount * 10f) + " out of 10";
            }

            return string.IsNullOrEmpty(value) ? title : title + ", " + value;
        }

        private static string Describe(SettingsWidget widget, string direction)
        {
            if (widget == null)
            {
                return string.Empty;
            }

            string title = widget.titleLabel == null ? Humanize(widget.type.ToString()) : UiTextReader.Clean(widget.titleLabel.text);
            if (string.IsNullOrEmpty(title))
            {
                title = Humanize(widget.type.ToString());
            }

            string value = widget.valueLabel == null ? string.Empty : UiTextReader.Clean(widget.valueLabel.text);
            if (string.IsNullOrEmpty(value) && widget.barFill != null)
            {
                value = "slider";
            }

            string hint = GetHint(widget.type, direction);
            string text = title;
            if (!string.IsNullOrEmpty(value))
            {
                text += ", " + value;
            }
            if (!string.IsNullOrEmpty(hint))
            {
                text += ". " + hint;
            }
            return text;
        }

        private static string GetHint(SettingType type, string direction)
        {
            switch (type)
            {
                case SettingType.Music:
                    return DirectionHint(direction, "decreases music volume", "increases music volume", "changes music volume");
                case SettingType.Sfx:
                    return DirectionHint(direction, "decreases sound effects volume", "increases sound effects volume", "changes sound effects volume");
                case SettingType.Voice:
                    return DirectionHint(direction, "decreases voice volume", "increases voice volume", "changes voice volume");
                case SettingType.MusicVariation:
                    return DirectionHint(direction, "previous radio style", "next radio style", "changes the radio music style");
                case SettingType.WindowMode:
                    return "Toggles fullscreen and windowed mode.";
                case SettingType.WindowedResolution:
                    return DirectionHint(direction, "previous window size", "next window size", "changes the window size");
                case SettingType.Display:
                    return DirectionHint(direction, "previous display", "next display", "changes which monitor the game uses after restart");
                case SettingType.Language:
                    return DirectionHint(direction, "previous language", "next language", "changes language after leaving settings");
                case SettingType.StreamerMode:
                    return "Toggles streamer mode. It avoids copyrighted music.";
                case SettingType.InstinctMode:
                    return "Toggles instinct mode. It gives extra choice guidance.";
                case SettingType.UseAccessibleFont:
                    return "Toggles the accessible font. The title screen reloads after leaving settings.";
                case SettingType.UseDyslexicFont:
                    return "Toggles the dyslexic font. The title screen reloads after leaving settings.";
                case SettingType.EpilepsiaFilter:
                    return "Toggles the flashing lights filter.";
                default:
                    return string.Empty;
            }
        }

        private static string DirectionHint(string direction, string left, string right, string value)
        {
            if (direction == "left")
            {
                return "Left arrow " + left + ".";
            }
            if (direction == "right")
            {
                return "Right arrow " + right + ".";
            }
            return value + ".";
        }

        private static string Humanize(string name)
        {
            string text = name.Replace("_", " ").Replace("-", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
            return UiTextReader.Clean(text);
        }

        private static string ValueText(UILabel label)
        {
            return label == null ? string.Empty : UiTextReader.Clean(label.text);
        }

        private static string CleanOrFallback(string value, string fallback)
        {
            value = UiTextReader.Clean(value);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        private static bool GetPrivateBool(object target, string fieldName)
        {
            if (target == null)
            {
                return false;
            }

            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                return false;
            }

            object value = field.GetValue(target);
            return value is bool && (bool)value;
        }
    }
}
