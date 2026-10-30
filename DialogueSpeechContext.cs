using BeautifulGlitch;
using Game;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class DialogueSpeechContext
    {
        private static string _speaker;
        private static string _line;
        private static float _time;

        internal static void SetNext(EventScene scene, string line)
        {
            _speaker = DescribeSpeaker(scene == null ? string.Empty : scene.whoSpeaks);
            _line = UiTextReader.Clean(line);
            _time = Time.realtimeSinceStartup;
        }

        internal static string Apply(string line)
        {
            string cleanLine = UiTextReader.Clean(line);
            if (string.IsNullOrEmpty(cleanLine) || string.IsNullOrEmpty(_speaker))
            {
                return cleanLine;
            }

            if (Time.realtimeSinceStartup - _time > 1f)
            {
                return cleanLine;
            }

            if (!string.IsNullOrEmpty(_line) && _line != cleanLine)
            {
                return cleanLine;
            }

            string speaker = _speaker;
            _speaker = string.Empty;
            _line = string.Empty;
            return speaker + ": " + cleanLine;
        }

        private static string DescribeSpeaker(string rawSpeaker)
        {
            rawSpeaker = UiTextReader.Clean(rawSpeaker);
            if (string.IsNullOrEmpty(rawSpeaker))
            {
                return string.Empty;
            }

            if (rawSpeaker == "player")
            {
                PlayerData player = GetCurrentPlayer();
                if (player != null)
                {
                    return AccessibilityDescriber.PrettyName(player);
                }
            }

            if (rawSpeaker == "narratorbody")
            {
                return "narrator";
            }

            return Humanize(rawSpeaker);
        }

        private static PlayerData GetCurrentPlayer()
        {
            try
            {
                MainGameSceneController main = SceneController.Instance as MainGameSceneController;
                object turnManager = GetField(main, "turnManager");
                if (turnManager != null)
                {
                    object playerTurnManager = InvokeNoArg(turnManager, "GetPlayerTurnManager");
                    object player = InvokeNoArg(playerTurnManager, "GetCurrentPlayer");
                    return player as PlayerData;
                }
            }
            catch (System.Exception)
            {
            }

            return null;
        }

        private static object GetField(object target, string fieldName)
        {
            if (target == null)
            {
                return null;
            }

            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(target);
        }

        private static object InvokeNoArg(object target, string methodName)
        {
            if (target == null)
            {
                return null;
            }

            System.Reflection.MethodInfo method = target.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            return method == null ? null : method.Invoke(target, null);
        }

        private static string Humanize(string value)
        {
            string text = value.Replace("_", " ").Replace("-", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
            return UiTextReader.Clean(text);
        }
    }
}
