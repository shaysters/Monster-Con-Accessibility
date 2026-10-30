using System.Collections.Generic;
using BeautifulGlitch;
using BeautifulGlitchConfig;
using Game;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class ScreenAccessibility
    {
        internal static string DescribePlayersNumberView(PlayersNumberView view)
        {
            return "Choose number of players.";
        }

        internal static string DescribePlayersSelectView(PlayersSelectView view)
        {
            string prompt = view == null || view.choosePlayerLabel == null ? string.Empty : UiTextReader.Clean(view.choosePlayerLabel.text);
            if (string.IsNullOrEmpty(prompt))
            {
                prompt = "Choose a character";
            }

            prompt = System.Text.RegularExpressions.Regex.Replace(prompt, @"\s*Move between characters to hear each option\.?", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            return prompt;
        }

        internal static string DescribeCustomizeWidget(PlayerCustomizeWidget widget)
        {
            return "Customize player. Choose name, pronouns, then OK.";
        }

        internal static string DescribeComicTypeView(ComicTypeView view)
        {
            return "Choose a comic genre.";
        }

        internal static string DescribeLocationPrompt(string locationKind)
        {
            return "Choose a " + locationKind + ".";
        }

        internal static string DescribeStatsCheatSheet()
        {
            return "Stats screen. " + AccessibilityDescriber.DescribeStats();
        }

        internal static string DescribeSequenceCheatSheet()
        {
            try
            {
                MainGameSceneController main = SceneController.Instance as MainGameSceneController;
                object turnManager = GetField(main, "turnManager");
                object sessionManager = GetField(GeneralManager.Instance, "sessionManager");
                object runConditions = InvokeNoArg(sessionManager, "GetRunConditions");
                object sequenceObject = GetField(runConditions, "turnSequence");
                List<TurnData> sequence = sequenceObject as List<TurnData>;
                int currentIdx = GetIntByMethod(turnManager, "GetCurrentTurnIdx", -1);
                if (sequence == null || sequence.Count == 0)
                {
                    return "Schedule screen. No turn schedule found.";
                }

                List<TurnType> visibleTurns = new List<TurnType>();
                List<string> visible = new List<string>();
                int visibleCurrentIdx = -1;
                for (int i = 0; i < sequence.Count; i++)
                {
                    TurnData turn = sequence[i];
                    if (turn == null || !DoesTurnTypeShow(turn.turnType))
                    {
                        continue;
                    }

                    if (i <= currentIdx)
                    {
                        visibleCurrentIdx = visible.Count;
                    }

                    visibleTurns.Add(turn.turnType);
                    visible.Add(DescribeTurnName(turn.turnType));
                }

                if (visible.Count == 0)
                {
                    return "Schedule screen. No readable turns found.";
                }

                if (visibleCurrentIdx < 0 || visibleCurrentIdx >= visible.Count)
                {
                    visibleCurrentIdx = 0;
                }

                List<string> parts = new List<string>();
                parts.Add("Schedule screen. Limited accessibility");
                parts.Add("Current stop " + (visibleCurrentIdx + 1) + " of " + visible.Count + ": " + visible[visibleCurrentIdx]);
                if (visibleCurrentIdx + 1 < visible.Count)
                {
                    parts.Add("Next: " + visible[visibleCurrentIdx + 1]);
                }
                else
                {
                    parts.Add("This is the last visible scheduled stop");
                }

                parts.Add("The full visual timeline is not fully readable by TTS");
                return string.Join(". ", parts.ToArray()) + ".";
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("ScreenAccessibility.DescribeSequenceCheatSheet failed: " + ex);
                return "Schedule screen.";
            }
        }

        internal static string DescribeBacklog(BacklogPanel panel)
        {
            try
            {
                List<string> backlog = DescribeBacklogFromManager();
                if (backlog.Count > 0)
                {
                    return string.Join(". ", backlog.ToArray());
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("ScreenAccessibility.DescribeBacklog manager read failed: " + ex);
            }

            if (panel == null)
            {
                return "Backlog. This screen reviews recent dialogue and choices.";
            }

            List<string> parts = new List<string>();
            parts.Add("Backlog. This screen reviews recent dialogue and choices. Use Back to close it");
            UILabel[] labels = panel.GetComponentsInChildren<UILabel>(true);
            for (int i = 0; i < labels.Length && parts.Count < 10; i++)
            {
                if (labels[i] == null)
                {
                    continue;
                }

                AddUnique(parts, UiTextReader.Clean(labels[i].text));
            }

            if (parts.Count == 1)
            {
                parts.Add("No readable backlog text found.");
            }

            return string.Join(". ", parts.ToArray());
        }

        private static List<string> DescribeBacklogFromManager()
        {
            List<string> parts = new List<string>();
            MainGameSceneController main = SceneController.Instance as MainGameSceneController;
            object turnManager = GetField(main, "turnManager");
            object backlogManager = GetField(turnManager, "backlogManager");
            object dialoguesObject = InvokeNoArg(backlogManager, "GetLastDialogues", 20);
            List<BacklogDialogueData> dialogues = dialoguesObject as List<BacklogDialogueData>;
            if (dialogues == null || dialogues.Count == 0)
            {
                return parts;
            }

            parts.Add("Backlog. This screen reviews recent dialogue and choices. Last entries, oldest to newest");
            int start = System.Math.Max(0, dialogues.Count - 6);
            for (int i = start; i < dialogues.Count && parts.Count < 8; i++)
            {
                BacklogDialogueData dialogue = dialogues[i];
                if (dialogue == null)
                {
                    continue;
                }

                string text = UiTextReader.Clean(InvokeNoArg(dialogue, "GetText") as string);
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                string speaker = UiTextReader.Clean(InvokeNoArg(dialogue, "GetSpeaker") as string);
                if (!string.IsNullOrEmpty(speaker))
                {
                    speaker = AccessibilityDescriber.HumanizePublic(speaker);
                    text = speaker + ": " + text;
                }

                AddUnique(parts, text);
            }

            return parts.Count == 1 ? new List<string>() : parts;
        }

        private static void AddButtons(List<string> parts, ButtonGroup group)
        {
            if (parts == null || group == null || group.buttonsHandlers == null)
            {
                return;
            }

            for (int i = 0; i < group.buttonsHandlers.Count && parts.Count < 10; i++)
            {
                ButtonHandler handler = group.buttonsHandlers[i];
                if (handler == null || !handler.gameObject.activeInHierarchy)
                {
                    continue;
                }

                string text = UiTextReader.DescribeObject(handler.gameObject);
                if (!string.IsNullOrEmpty(text) && !parts.Contains(text))
                {
                    parts.Add(text);
                }
            }
        }

        private static bool DoesTurnTypeShow(TurnType turnType)
        {
            switch (turnType)
            {
                case TurnType.ChooseNpcEndings:
                case TurnType.Challenge:
                case TurnType.PitstopResults:
                case TurnType.PlayerOrder:
                case TurnType.ComicEndings:
                    return false;
                default:
                    return true;
            }
        }

        private static string DescribeTurnName(TurnType turnType)
        {
            string name = string.Empty;
            try
            {
                object textManager = GetField(GeneralManager.Instance, "textManager");
                object value = InvokeNoArg(textManager, "GetTurnName", turnType);
                name = value as string;
            }
            catch (System.Exception)
            {
            }

            if (string.IsNullOrEmpty(UiTextReader.Clean(name)))
            {
                name = AccessibilityDescriber.HumanizePublic(turnType.ToString());
            }

            return UiTextReader.Clean(name);
        }

        private static string DescribeTurnPurpose(TurnType turnType)
        {
            switch (turnType)
            {
                case TurnType.MainTurn:
                    return "Choose a convention location and play an event.";
                case TurnType.Cafeteria:
                    return "Choose a food court table.";
                case TurnType.Dates:
                    return "Choose a date location or phone contact.";
                case TurnType.Comic:
                    return "Answer comic questions and change stats.";
                case TurnType.ConShop:
                    return "Spend money on shop items.";
                case TurnType.ChooseNpc:
                    return "Choose who to ask out for the final date.";
                case TurnType.Pitstop:
                    return "Choose a pit stop location.";
                case TurnType.Road:
                    return "Continue the trip between convention stops.";
                case TurnType.Destination:
                    return "Reach the destination.";
                case TurnType.IndividualEndings:
                    return "See player endings.";
                case TurnType.DestinationProgress:
                    return "Review destination progress.";
                default:
                    return "This is a scheduled game step.";
            }
        }

        private static string DescribePlayerOrder(object turnManager)
        {
            object playerTurnManager = InvokeNoArg(turnManager, "GetPlayerTurnManager");
            int currentPlayerIdx = GetIntByMethod(playerTurnManager, "GetCurrentPlayerIdx", -1);
            object playersObject = InvokeNoArg(playerTurnManager, "GetPlayerTurns");
            List<PlayerData> players = playersObject as List<PlayerData>;
            if (players == null || players.Count == 0)
            {
                return string.Empty;
            }

            List<string> names = new List<string>();
            for (int i = 0; i < players.Count && names.Count < 6; i++)
            {
                string name = AccessibilityDescriber.PrettyName(players[i]);
                if (i == currentPlayerIdx)
                {
                    name = "current player " + name;
                }
                AddUnique(names, name);
            }

            if (names.Count == 0)
            {
                return string.Empty;
            }

            return "Player order: " + string.Join(", ", names.ToArray());
        }

        private static void AddUnique(List<string> parts, string text)
        {
            text = UiTextReader.Clean(text);
            if (string.IsNullOrEmpty(text) || parts.Contains(text))
            {
                return;
            }
            parts.Add(text);
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

        private static object InvokeNoArg(object target, string methodName, params object[] args)
        {
            if (target == null)
            {
                return null;
            }

            System.Reflection.MethodInfo method = target.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            return method == null ? null : method.Invoke(target, args);
        }

        private static int GetIntByMethod(object target, string methodName, int fallback)
        {
            object value = InvokeNoArg(target, methodName);
            return value is int ? (int)value : fallback;
        }
    }
}
