using System.Text;
using Game;
using HarmonyLib;
using UnityEngine;
using static MonsterConAccessibilityMod.HookLog;

namespace MonsterConAccessibilityMod
{
    // Online lobby match code.
    //
    // OnlineLobbyOnMatchView / OnlineLobbyOnMatchResumedView .SetMatchInfo() write the
    // room code into matchNameLabel. It runs on Show and again whenever the player
    // list refreshes, so we only announce when the code changes and reset on Hide.
    // The announcement is delayed and queued so the hovered-button focus speech
    // doesn't cut it off. C key (HotkeyAccessibility) re-reads the code.
    internal static class OnlineCodeAccessibility
    {
        private const float AnnounceDelay = 0.4f;
        private const float SuppressFocusSeconds = 2.5f;

        private static UILabel _label;
        private static string _lastCode;
        private static string _pendingCode;
        private static float _pendingAt;

        internal static void OnSetMatchInfo(UILabel label)
        {
            _label = label;
            string code = ReadCode();
            if (string.IsNullOrEmpty(code) || code == _lastCode)
            {
                return;
            }

            _lastCode = code;
            _pendingCode = code;
            _pendingAt = Time.realtimeSinceStartup + AnnounceDelay;
        }

        internal static void OnHide(UILabel label)
        {
            if (_label != label)
            {
                return;
            }

            _label = null;
            _lastCode = null;
            _pendingCode = null;
        }

        internal static void Tick()
        {
            if (_pendingCode == null || Time.realtimeSinceStartup < _pendingAt)
            {
                return;
            }

            string code = _pendingCode;
            _pendingCode = null;
            Plugin.Speak("Online code: " + SpellOut(code) + ". Press C to hear it again.", false);
            Plugin.SuppressFocusSpeech(SuppressFocusSeconds, "online:code");
        }

        internal static void SpeakCurrentCode()
        {
            string code = ReadCode();
            if (string.IsNullOrEmpty(code))
            {
                Plugin.Speak("No online code right now.", true);
                return;
            }

            _pendingCode = null;
            Plugin.Speak("Online code: " + SpellOut(code), true);
        }

        private static string ReadCode()
        {
            if (_label == null || !_label.gameObject.activeInHierarchy)
            {
                return null;
            }

            return UiTextReader.Clean(_label.text);
        }

        // "48271" -> "4, 8, 2, 7, 1" so screen readers read digit by digit.
        private static string SpellOut(string code)
        {
            var sb = new StringBuilder();
            foreach (char c in code)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
    }

    [HarmonyPatch(typeof(OnlineLobbyOnMatchView), "SetMatchInfo")]
    internal static class OnlineLobbyOnMatchViewSetMatchInfoPatch
    {
        private static void Postfix(OnlineLobbyOnMatchView __instance)
        {
            Run("OnlineLobbyOnMatchView.SetMatchInfo postfix", () => OnlineCodeAccessibility.OnSetMatchInfo(__instance.matchNameLabel));
        }
    }

    [HarmonyPatch(typeof(OnlineLobbyOnMatchView), "Hide")]
    internal static class OnlineLobbyOnMatchViewHidePatch
    {
        private static void Postfix(OnlineLobbyOnMatchView __instance)
        {
            Run("OnlineLobbyOnMatchView.Hide postfix", () => OnlineCodeAccessibility.OnHide(__instance.matchNameLabel));
        }
    }

    [HarmonyPatch(typeof(OnlineLobbyOnMatchResumedView), "SetMatchInfo")]
    internal static class OnlineLobbyOnMatchResumedViewSetMatchInfoPatch
    {
        private static void Postfix(OnlineLobbyOnMatchResumedView __instance)
        {
            Run("OnlineLobbyOnMatchResumedView.SetMatchInfo postfix", () => OnlineCodeAccessibility.OnSetMatchInfo(__instance.matchNameLabel));
        }
    }

    [HarmonyPatch(typeof(OnlineLobbyOnMatchResumedView), "Hide")]
    internal static class OnlineLobbyOnMatchResumedViewHidePatch
    {
        private static void Postfix(OnlineLobbyOnMatchResumedView __instance)
        {
            Run("OnlineLobbyOnMatchResumedView.Hide postfix", () => OnlineCodeAccessibility.OnHide(__instance.matchNameLabel));
        }
    }
}
