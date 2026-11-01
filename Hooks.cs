using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BeautifulGlitch;
using Game;
using HarmonyLib;
using UnityEngine;
using static MonsterConAccessibilityMod.HookLog;

namespace MonsterConAccessibilityMod
{
    [HarmonyPatch(typeof(CoreNavigationManager), "SetHovered", new[] { typeof(GameObject) })]
    internal static class CoreNavigationManagerSetHoveredGameObjectPatch
    {
        private static void Postfix(GameObject aGameObject)
        {
            Run("CoreNavigationManager.SetHovered(GameObject) postfix", () =>
            {
                Plugin.SpeakFocus(aGameObject);
            });
        }
    }

    [HarmonyPatch(typeof(CoreNavigationManager), "SetHovered", new[] { typeof(ButtonHandler) })]
    internal static class CoreNavigationManagerSetHoveredButtonHandlerPatch
    {
        private static void Postfix(ButtonHandler aHandler)
        {
            Run("CoreNavigationManager.SetHovered(ButtonHandler) postfix", () =>
            {
                if (aHandler != null)
                {
                    Plugin.SpeakFocus(aHandler.gameObject);
                }
                else
                {
                }
            });
        }
    }

    [HarmonyPatch(typeof(CoreNavigationManager), "SetSystemHovered", new[] { typeof(GameObject) })]
    internal static class CoreNavigationManagerSetSystemHoveredGameObjectPatch
    {
        private static void Postfix(GameObject aGameObject)
        {
            Run("CoreNavigationManager.SetSystemHovered(GameObject) postfix", () =>
            {
                Plugin.SpeakFocus(aGameObject);
            });
        }
    }

    [HarmonyPatch(typeof(UICamera), "SetHovered2", new[] { typeof(GameObject) })]
    internal static class UICameraSetHovered2Patch
    {
        private static void Postfix(GameObject value)
        {
            Run("UICamera.SetHovered2 postfix", () =>
            {
                Plugin.SpeakFocus(value);
            });
        }
    }

    [HarmonyPatch(typeof(UICamera), "Notify", new[] { typeof(GameObject), typeof(string), typeof(object) })]
    internal static class UICameraNotifyPatch
    {
        private static void Postfix(GameObject go, string funcName, object obj)
        {
            Run("UICamera.Notify postfix", () =>
            {
                if (go != null && funcName == "OnHover" && obj is bool && (bool)obj)
                {
                    Plugin.SpeakFocus(go);
                }
            });
        }
    }

    [HarmonyPatch(typeof(ButtonHandler), "PlayHoverOver")]
    internal static class ButtonHandlerPlayHoverOverPatch
    {
        private static void Postfix(ButtonHandler __instance)
        {
            Run("ButtonHandler.PlayHoverOver postfix", () =>
            {
                if (__instance != null)
                {
                    Plugin.SpeakFocus(__instance.gameObject);
                }
            });
        }
    }

    [HarmonyPatch(typeof(ButtonGroup), "HoverOverButton", new[] { typeof(int) })]
    internal static class ButtonGroupHoverOverButtonPatch
    {
        private static void Postfix(ButtonGroup __instance, int aIdx)
        {
            Run("ButtonGroup.HoverOverButton postfix", () =>
            {
                if (__instance == null || __instance.buttonsHandlers == null || aIdx < 0 || aIdx >= __instance.buttonsHandlers.Count)
                {
                    return;
                }

                ButtonHandler handler = __instance.buttonsHandlers[aIdx];
                if (handler != null)
                {
                    Plugin.SpeakFocus(handler.gameObject);
                }
            });
        }
    }

    [HarmonyPatch(typeof(ActionButton), "OnHover", new[] { typeof(bool) })]
    internal static class ActionButtonOnHoverPatch
    {
        private static void Postfix(ActionButton __instance, bool aHovered)
        {
            Run("ActionButton.OnHover postfix", () =>
            {
                if (__instance != null && aHovered)
                {
                    Plugin.SpeakFocus(__instance.gameObject);
                }
            });
        }
    }

    [HarmonyPatch(typeof(ActionButton), "SetData", new[] { typeof(string) })]
    internal static class ActionButtonSetDataPatch
    {
        private static void Postfix(ActionButton __instance, string aActionText)
        {
            Run("ActionButton.SetData postfix", () =>
            {
                string objectName = __instance == null ? "null" : __instance.name;
            });
        }
    }

    [HarmonyPatch(typeof(LocationActionButton), "SetData", new[] { typeof(string) })]
    internal static class LocationActionButtonSetDataStringPatch
    {
        private static void Postfix(LocationActionButton __instance, string aActionText)
        {
            Run("LocationActionButton.SetData(string) postfix", () =>
            {
                string objectName = __instance == null ? "null" : __instance.name;
            });
        }
    }

    [HarmonyPatch(typeof(LocationActionButton), "SetData", new[] { typeof(EventFlow) })]
    internal static class LocationActionButtonSetDataEventPatch
    {
        private static void Postfix(LocationActionButton __instance, EventFlow aEvent)
        {
            Run("LocationActionButton.SetData(EventFlow) postfix", () =>
            {
                string objectName = __instance == null ? "null" : __instance.name;
                string action = aEvent == null ? string.Empty : aEvent.action;
                string eventName = aEvent == null ? string.Empty : aEvent.eventName;
            });
        }
    }

    [HarmonyPatch(typeof(EventChoiceButton), "Show")]
    internal static class EventChoiceButtonShowPatch
    {
        private static void Postfix(EventChoiceButton __instance, string aText)
        {
            Run("EventChoiceButton.Show postfix", () =>
            {
                string objectName = __instance == null ? "null" : __instance.name;
            });
        }
    }

    [HarmonyPatch(typeof(AdjustableTextBox), "Show", new[] { typeof(string), typeof(ColorProfile), typeof(bool) })]
    internal static class AdjustableTextBoxShowPatch
    {
        private static void Postfix(AdjustableTextBox __instance, string aText)
        {
            Run("AdjustableTextBox.Show postfix", () =>
            {
                string text = UiTextReader.Clean(aText);
                if (!string.IsNullOrEmpty(text))
                {
                    string objectName = __instance == null ? "null" : __instance.name;
                    Plugin.SpeakProtected("dialogue:adjustable:" + objectName, DialogueSpeechContext.Apply(text), true, 0.6f, 2.5f);
                }
            });
        }
    }

    [HarmonyPatch(typeof(SimpleTextBox), "Show", new[] { typeof(string) })]
    internal static class SimpleTextBoxShowPatch
    {
        private static void Postfix(SimpleTextBox __instance, string aText)
        {
            Run("SimpleTextBox.Show postfix", () =>
            {
                if (__instance == null || __instance is AdjustableTextBox || __instance is SimpleTextBox_ComicPanels)
                {
                    return;
                }

                string text = UiTextReader.Clean(aText);
                if (!string.IsNullOrEmpty(text))
                {
                    string objectName = __instance.name;
                    Plugin.SpeakProtected("dialogue:simple:" + objectName, DialogueSpeechContext.Apply(text), true, 0.6f, 2.5f);
                }
            });
        }
    }

    [HarmonyPatch(typeof(SimpleTextBox_ComicPanels), "NotifyShowText")]
    internal static class SimpleTextBoxComicPanelsNotifyShowTextPatch
    {
        private static void Postfix(SimpleTextBox_ComicPanels __instance)
        {
            Run("SimpleTextBox_ComicPanels.NotifyShowText postfix", () =>
            {
                if (__instance == null || __instance.label == null)
                {
                    return;
                }

                string text = UiTextReader.Clean(__instance.label.text);
                if (!string.IsNullOrEmpty(text))
                {
                    Plugin.SpeakProtected("dialogue:comic:" + __instance.name, DialogueSpeechContext.Apply(text), true, 0.6f, 2.5f);
                }
            });
        }
    }

    [HarmonyPatch(typeof(Popup), "ShowPopupWithSuitableBodyType")]
    internal static class PopupShowPatch
    {
        private static void Postfix(string aMessageText, string aTitleText)
        {
            Run("Popup.ShowPopupWithSuitableBodyType postfix", () =>
            {
                string text = string.IsNullOrEmpty(aTitleText) ? aMessageText : aTitleText + ". " + aMessageText;
            });
        }
    }

    [HarmonyPatch(typeof(InfoPopup), "Show")]
    internal static class InfoPopupShowPatch
    {
        private static void Postfix(string aText, string aTitleText)
        {
            Run("InfoPopup.Show postfix", () =>
            {
                SpeakPopup("InfoPopup", aText, aTitleText, null, null, null);
            });
        }
    }

    [HarmonyPatch(typeof(OkPopup), "Show")]
    internal static class OkPopupShowPatch
    {
        private static void Postfix(string aText, string aTitleText, string aOkButtonText)
        {
            Run("OkPopup.Show postfix", () =>
            {
                SpeakPopup("OkPopup", aText, aTitleText, aOkButtonText, null, null);
            });
        }
    }

    [HarmonyPatch(typeof(YesNoPopup), "Show")]
    internal static class YesNoPopupShowPatch
    {
        private static void Postfix(string aText, string aTitleText, string aYesButtonText, string aNoButtonText)
        {
            Run("YesNoPopup.Show postfix", () =>
            {
                SpeakPopup("YesNoPopup", aText, aTitleText, aYesButtonText, aNoButtonText, null);
            });
        }
    }

    [HarmonyPatch(typeof(ThreeButtonPopup), "Show")]
    internal static class ThreeButtonPopupShowPatch
    {
        private static void Postfix(string aText, string aTitleText, string aFirstButtonText, string aSecondButtonText, string aThirdButtonText)
        {
            Run("ThreeButtonPopup.Show postfix", () =>
            {
                SpeakPopup("ThreeButtonPopup", aText, aTitleText, aFirstButtonText, aSecondButtonText, aThirdButtonText);
            });
        }
    }

    [HarmonyPatch(typeof(YesNoPopupView), "SetData")]
    internal static class YesNoPopupViewSetDataPatch
    {
        private static void Postfix(string aText, string aYesButtonText, string aNoButtonText)
        {
            Run("YesNoPopupView.SetData postfix", () =>
            {
                string text = aText;
                if (!string.IsNullOrEmpty(aYesButtonText))
                {
                    text += ". " + aYesButtonText;
                }
                if (!string.IsNullOrEmpty(aNoButtonText))
                {
                    text += ". " + aNoButtonText;
                }
                Plugin.SpeakProtected("Game.YesNoPopupView", text, true, 2f, EstimatePopupFocusSuppressSeconds(text));
            });
        }
    }

    [HarmonyPatch(typeof(OkPopupView), "SetData")]
    internal static class OkPopupViewSetDataPatch
    {
        private static void Postfix(string aText, string aButtonText)
        {
            Run("OkPopupView.SetData postfix", () =>
            {
                string text = string.IsNullOrEmpty(aButtonText) ? aText : aText + ". " + aButtonText;
                Plugin.SpeakProtected("Game.OkPopupView", text, true, 2f, EstimatePopupFocusSuppressSeconds(text));
            });
        }
    }

    [HarmonyPatch(typeof(PhoneNpcInfoPopupView), "SetData")]
    internal static class PhoneNpcInfoPopupViewSetDataPatch
    {
        private static void Postfix(string aTitleText, string aText, string aButtonText)
        {
            Run("PhoneNpcInfoPopupView.SetData postfix", () =>
            {
                string text = string.IsNullOrEmpty(aTitleText) ? aText : aTitleText + ". " + aText;
                if (!string.IsNullOrEmpty(aButtonText))
                {
                    text += ". " + aButtonText;
                }
                Plugin.SpeakOnce("Game.PhoneNpcInfoPopupView", text, true, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(VirtualKeyboardRemotePopup), "Show", new[] { typeof(string) })]
    internal static class VirtualKeyboardRemotePopupShowPatch
    {
        private static void Postfix(string aText)
        {
            Run("VirtualKeyboardRemotePopup.Show postfix", () =>
            {
                Plugin.SpeakOnce("popup:virtual-keyboard", aText, true, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(AnnouncementWidget), "StartAnnouncement")]
    internal static class AnnouncementWidgetStartAnnouncementPatch
    {
        private static void Postfix(AnnouncementWidget __instance, string aTextToSet)
        {
            Run("AnnouncementWidget.StartAnnouncement postfix", () =>
            {
                string objectName = __instance == null ? "null" : __instance.name;
                Plugin.SpeakProtected("announcement:" + objectName, aTextToSet, true, 1f, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(ItemInfoPanel), "SetNameAndDescriptionAndPlay")]
    internal static class ItemInfoPanelSetNameAndDescriptionAndPlayPatch
    {
        private static void Postfix(string aName, string aDescription)
        {
            Run("ItemInfoPanel.SetNameAndDescriptionAndPlay postfix", () =>
            {
                string text = string.IsNullOrEmpty(aName) ? aDescription : aName + ". " + aDescription;
                Plugin.SpeakProtected("shop:item-info", text, true, 1f, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(ItemInfoPanel), "SetFullMessageAndPlay")]
    internal static class ItemInfoPanelSetFullMessageAndPlayPatch
    {
        private static void Postfix(string aFullMessage)
        {
            Run("ItemInfoPanel.SetFullMessageAndPlay postfix", () =>
            {
                Plugin.SpeakProtected("shop:message", aFullMessage, true, 1f, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(PlayerTurnView), "SetPlayer")]
    internal static class PlayerTurnViewSetPlayerPatch
    {
        private static void Postfix(CorePlayerData aPlayerData)
        {
            Run("PlayerTurnView.SetPlayer postfix", () =>
            {
                if (aPlayerData == null)
                {
                    return;
                }

                string text = AccessibilityDescriber.PrettyName(aPlayerData) + "'s turn. Make your choice.";
                Plugin.SpeakProtected("player-turn:" + AccessibilityDescriber.KeyForObject(aPlayerData), text, true, 1f, 2.5f);
            });
        }
    }

    [HarmonyPatch(typeof(MainGameEventView), "PlayNormalScene")]
    internal static class MainGameEventViewPlayNormalScenePatch
    {
        private static void Prefix(EventScene aScene, string aText)
        {
            Run("MainGameEventView.PlayNormalScene prefix", () =>
            {
                DialogueSpeechContext.SetNext(aScene, aText);
            });
        }
    }

    [HarmonyPatch(typeof(ConversationalEventView), "PlayConversationalScene")]
    internal static class ConversationalEventViewPlayConversationalScenePatch
    {
        private static void Prefix(EventScene aScene, string aText)
        {
            Run("ConversationalEventView.PlayConversationalScene prefix", () =>
            {
                DialogueSpeechContext.SetNext(aScene, aText);
            });
        }
    }

    [HarmonyPatch(typeof(MainGameEventView), "PlayOptionsScene")]
    internal static class MainGameEventViewPlayOptionsScenePatch
    {
        private static void Prefix(EventFlow aEvent, List<string> aTexts)
        {
            Run("MainGameEventView.PlayOptionsScene prefix", () =>
            {
                Plugin.SpeakProtected("choices:main:" + (aEvent == null ? "null" : aEvent.eventName), AccessibilityDescriber.DescribeChoicePrompt(aEvent), true, 0.8f, 0.9f);
            });
        }
    }

    [HarmonyPatch(typeof(ConversationalEventView), "PlayConversationalOptionsScene")]
    internal static class ConversationalEventViewPlayConversationalOptionsScenePatch
    {
        private static void Prefix(EventFlow aEvent, List<string> aTexts)
        {
            Run("ConversationalEventView.PlayConversationalOptionsScene prefix", () =>
            {
                Plugin.SpeakProtected("choices:conversational:" + (aEvent == null ? "null" : aEvent.eventName), AccessibilityDescriber.DescribeChoicePrompt(aEvent), true, 0.8f, 0.9f);
            });
        }
    }

    [HarmonyPatch(typeof(ConPopQuizView), "Show")]
    internal static class ConPopQuizViewShowPatch
    {
        private static void Postfix(ConPopQuizView __instance)
        {
            Run("ConPopQuizView.Show postfix", () =>
            {
                string question = __instance == null || __instance.quizQuestionLabel == null ? string.Empty : UiTextReader.Clean(__instance.quizQuestionLabel.text);
                string text = string.IsNullOrEmpty(question) ? "Top three question. Choose your first, second, and third favorite." : "Top three question, " + question + ". Choose your first, second, and third favorite.";
                Plugin.SpeakProtected("prologue:top-three", text, true, 1f, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(PlayersNumberView), "Show")]
    internal static class PlayersNumberViewShowPatch
    {
        private static void Postfix(PlayersNumberView __instance)
        {
            Run("PlayersNumberView.Show postfix", () =>
            {
                Plugin.SpeakProtected("prologue:player-number", ScreenAccessibility.DescribePlayersNumberView(__instance), true, 1f, 1.5f);
            });
        }
    }

    [HarmonyPatch(typeof(PlayersSelectView), "CoStartChoosePlayers")]
    internal static class PlayersSelectViewCoStartChoosePlayersPatch
    {
        private static void Postfix(PlayersSelectView __instance)
        {
            Run("PlayersSelectView.CoStartChoosePlayers postfix", () =>
            {
                Plugin.SpeakProtected("prologue:character-select", ScreenAccessibility.DescribePlayersSelectView(__instance), true, 1f, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(PlayersSelectView), "PreparePlayerSelection")]
    internal static class PlayersSelectViewPreparePlayerSelectionPatch
    {
        private static void Postfix(PlayersSelectView __instance)
        {
            Run("PlayersSelectView.PreparePlayerSelection postfix", () =>
            {
                Plugin.SpeakProtected("prologue:character-select-next", ScreenAccessibility.DescribePlayersSelectView(__instance), true, 1f, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(PlayerCustomizeWidget), "SetupCustomizePanelPositionAndShow")]
    internal static class PlayerCustomizeWidgetSetupPatch
    {
        private static void Postfix(PlayerCustomizeWidget __instance)
        {
            Run("PlayerCustomizeWidget.SetupCustomizePanelPositionAndShow postfix", () =>
            {
                Plugin.SpeakProtected("prologue:customize-player", ScreenAccessibility.DescribeCustomizeWidget(__instance), true, 1f, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(ProloguePcButtonsWidget), "SetDefaultAndHoverButton")]
    internal static class ProloguePcButtonsWidgetSetDefaultPatch
    {
        private static void Postfix()
        {
            Run("ProloguePcButtonsWidget.SetDefaultAndHoverButton postfix", () =>
            {
                PlayersSelectView view = Object.FindObjectOfType<PlayersSelectView>();
                if (view != null && view.gameObject.activeInHierarchy)
                {
                }
            });
        }
    }

    [HarmonyPatch(typeof(ComicTypeView), "Show")]
    internal static class ComicTypeViewShowPatch
    {
        private static void Postfix(ComicTypeView __instance)
        {
            Run("ComicTypeView.Show postfix", () =>
            {
                Plugin.SpeakProtected("prologue:comic-type", ScreenAccessibility.DescribeComicTypeView(__instance), true, 1f, 2f);
            });
        }
    }

    [HarmonyPatch(typeof(PrologueLikesView), "Show")]
    internal static class PrologueLikesViewShowPatch
    {
        private static void Postfix(PrologueLikesView __instance)
        {
            Run("PrologueLikesView.Show postfix", () =>
            {
            });
        }
    }

    [HarmonyPatch(typeof(PrologueLikesView), "SetDefaultAndHoverButton")]
    internal static class PrologueLikesViewSetDefaultAndHoverButtonPatch
    {
        private static void Prefix(PrologueLikesView __instance)
        {
            Run("PrologueLikesView.SetDefaultAndHoverButton prefix", () =>
            {
                Plugin.SpeakProtected("prologue:likes-ready", AccessibilityDescriber.DescribeLikesPrompt(__instance), true, 1f, 1.8f);
            });
        }
    }

    [HarmonyPatch(typeof(ComicView), "SetData")]
    internal static class ComicViewSetDataPatch
    {
        private static void Postfix(ComicView __instance, ComicEvent aComicEvent)
        {
            Run("ComicView.SetData postfix", () =>
            {
                string key = "comic:question:" + (aComicEvent == null ? "null" : aComicEvent.eventName);
                ComicSpeechTiming.SpeakQuestion(__instance, key, AccessibilityDescriber.DescribeComicQuestionPrompt(aComicEvent));
            });
        }
    }

    [HarmonyPatch(typeof(ComicView), "ShowFinishComic")]
    internal static class ComicViewShowFinishComicPatch
    {
        private static void Prefix(ComicEvent aComicEvent, int aChoiceIndex)
        {
            Run("ComicView.ShowFinishComic prefix", () =>
            {
                string text = AccessibilityDescriber.DescribeComicStatResult(aComicEvent, aChoiceIndex);
                float holdSeconds = ComicSpeechTiming.RecordStatResult(text);
                Plugin.SpeakProtected("comic:stat-result:" + (aComicEvent == null ? "null" : aComicEvent.eventName), text, true, 0.5f, holdSeconds + 0.25f);
            });
        }
    }

    [HarmonyPatch(typeof(DatesMenusView), "ShowNpcSelector")]
    internal static class DatesMenusViewShowNpcSelectorPatch
    {
        private static void Prefix(string aNpc1, string aNpc2)
        {
            Run("DatesMenusView.ShowNpcSelector prefix", () =>
            {
                Plugin.SpeakProtected("dates:npc-selector", "Choose who to ask out.", true, 1f, 0.9f);
            });
        }
    }

    [HarmonyPatch(typeof(MainTurnView), "DoResumeControlAfterPlayerIntroOrTutorial")]
    internal static class MainTurnViewResumeControlPatch
    {
        private static void Postfix()
        {
            Run("MainTurnView.DoResumeControlAfterPlayerIntroOrTutorial postfix", () =>
            {
                Plugin.SpeakOnce("location:main-turn", ScreenAccessibility.DescribeLocationPrompt("location"), false, 1f);
            });
        }
    }

    [HarmonyPatch(typeof(DatesView), "ResumeControlAfterPlayerIntroOrTutorial")]
    internal static class DatesViewResumeControlPatch
    {
        private static void Postfix()
        {
            Run("DatesView.ResumeControlAfterPlayerIntroOrTutorial postfix", () =>
            {
                Plugin.SpeakOnce("location:dates", "Choose a date location. Pepper indicators are visual only and are not readable by TTS.", false, 1f);
            });
        }
    }

    [HarmonyPatch(typeof(CafeteriaView), "ResumeControlAfterPlayerIntroOrTutorial")]
    internal static class CafeteriaViewResumeControlPatch
    {
        private static void Postfix()
        {
            Run("CafeteriaView.ResumeControlAfterPlayerIntroOrTutorial postfix", () =>
            {
                Plugin.SpeakOnce("location:cafeteria", "Choose a food court table.", false, 1f);
            });
        }
    }

    [HarmonyPatch(typeof(PitstopView), "SetHoveredAndDefault")]
    internal static class PitstopViewSetHoveredAndDefaultPatch
    {
        private static void Postfix()
        {
            Run("PitstopView.SetHoveredAndDefault postfix", () =>
            {
                Plugin.SpeakOnce("location:pitstop", ScreenAccessibility.DescribeLocationPrompt("pit stop location"), false, 1f);
            });
        }
    }

    [HarmonyPatch(typeof(StatsCheatSheetView), "Show")]
    internal static class StatsCheatSheetViewShowPatch
    {
        private static void Postfix()
        {
            Run("StatsCheatSheetView.Show postfix", () =>
            {
                Plugin.SpeakOnce("cheatsheet:stats", ScreenAccessibility.DescribeStatsCheatSheet(), true, 1f);
            });
        }
    }

    [HarmonyPatch(typeof(SequenceCheatSheetView), "Show")]
    internal static class SequenceCheatSheetViewShowPatch
    {
        private static void Postfix()
        {
            Run("SequenceCheatSheetView.Show postfix", () =>
            {
                Plugin.SpeakProtected("cheatsheet:sequence", ScreenAccessibility.DescribeSequenceCheatSheet(), true, 1f, 5f);
            });
        }
    }

    [HarmonyPatch(typeof(BacklogPanel), "OnEnable")]
    internal static class BacklogPanelOnEnablePatch
    {
        private static void Postfix(BacklogPanel __instance)
        {
            Run("BacklogPanel.OnEnable postfix", () =>
            {
                Plugin.SpeakProtected("backlog:open", ScreenAccessibility.DescribeBacklog(__instance), true, 1f, 7f);
            });
        }
    }

    [HarmonyPatch(typeof(PauseMenu), "ShowConfirmPanel")]
    internal static class PauseMenuShowConfirmPanelPatch
    {
        private static void Postfix(PauseMenu __instance)
        {
            Run("PauseMenu.ShowConfirmPanel postfix", () =>
            {
                string text = DescribePauseConfirmPanel(__instance);
                Plugin.SpeakProtected("pause:confirm-main-menu", text, true, 1f, 2f);
            });
        }

        private static string DescribePauseConfirmPanel(PauseMenu menu)
        {
            List<string> parts = new List<string>();
            parts.Add("Return to main menu?");
            if (menu != null && menu.confirmPanelContainer != null)
            {
                UILabel[] labels = menu.confirmPanelContainer.GetComponentsInChildren<UILabel>(true);
                for (int i = 0; i < labels.Length && parts.Count < 6; i++)
                {
                    string text = labels[i] == null ? string.Empty : UiTextReader.Clean(labels[i].text);
                    if (!string.IsNullOrEmpty(text) && !parts.Contains(text))
                    {
                        parts.Add(text);
                    }
                }
            }

            if (!parts.Contains("Yes"))
            {
                parts.Add("Yes");
            }
            if (!parts.Contains("No"))
            {
                parts.Add("No");
            }
            return string.Join(". ", parts.ToArray());
        }
    }

    [HarmonyPatch(typeof(DatesMenusView), "ShowPhoneNpcMenu")]
    internal static class DatesMenusViewShowPhoneNpcMenuPatch
    {
        private static void Prefix(List<DatesPhoneNpcProfile> aShownNpcs)
        {
            Run("DatesMenusView.ShowPhoneNpcMenu prefix", () =>
            {
                int count = aShownNpcs == null ? 0 : aShownNpcs.Count;
                Plugin.SpeakProtected("dates:phone-menu", "Choose a phone contact.", true, 1f, 0.9f);
            });
        }
    }

    [HarmonyPatch(typeof(ConfirmNpcWidget), "Show")]
    internal static class ConfirmNpcWidgetShowPatch
    {
        private static void Postfix(ConfirmNpcWidget __instance, string aNpcName)
        {
            Run("ConfirmNpcWidget.Show postfix", () =>
            {
                string title = string.Empty;
                if (__instance != null && __instance.titleLabel != null)
                {
                    title = __instance.titleLabel.text;
                }
                if (string.IsNullOrEmpty(UiTextReader.Clean(title)))
                {
                    title = aNpcName == "none" ? "Go alone?" : "Ask out " + aNpcName + "?";
                }
                Plugin.SpeakProtected("confirm-npc:" + aNpcName, title, true, 1f, 6f);
            });
        }
    }

    [HarmonyPatch(typeof(ConfirmNpcWidget), "HoverOverYesChosenNpc")]
    internal static class ConfirmNpcWidgetHoverOverYesPatch
    {
        private static void Postfix(ConfirmNpcWidget __instance)
        {
            Run("ConfirmNpcWidget.HoverOverYesChosenNpc postfix", () =>
            {
                ConfirmNpcWidgetSpeech.SpeakConfirmNpcButton(__instance == null ? null : __instance.yesButtonHandler, "Yes");
            });
        }
    }

    [HarmonyPatch(typeof(ConfirmNpcWidget), "HoverOverNoChosenNpc")]
    internal static class ConfirmNpcWidgetHoverOverNoPatch
    {
        private static void Postfix(ConfirmNpcWidget __instance)
        {
            Run("ConfirmNpcWidget.HoverOverNoChosenNpc postfix", () =>
            {
                ConfirmNpcWidgetSpeech.SpeakConfirmNpcButton(__instance == null ? null : __instance.noButtonHandler, "No");
            });
        }
    }

    internal static class ConfirmNpcWidgetSpeech
    {
        internal static void SpeakConfirmNpcButton(ButtonHandler handler, string fallback)
        {
            string text = handler == null ? string.Empty : UiTextReader.DescribeObject(handler.gameObject);
            if (string.IsNullOrEmpty(text))
            {
                text = fallback;
            }

            Plugin.SpeakOnce("confirm-npc:hover:" + fallback.ToLowerInvariant(), text, true, 0.25f);
        }
    }

    [HarmonyPatch(typeof(ChooseNpcView), "ShowChooseScene")]
    internal static class ChooseNpcViewShowChooseScenePatch
    {
        private static void Prefix()
        {
            Run("ChooseNpcView.ShowChooseScene prefix", () =>
            {
                Plugin.SpeakProtected("ending:choose-date", "Final date screen. Choose who to ask out.", true, 1f, 2.3f);
            });
        }
    }

    [HarmonyPatch(typeof(ChooseNpcView), "ShowAgainChooseScene")]
    internal static class ChooseNpcViewShowAgainChooseScenePatch
    {
        private static void Prefix()
        {
            Run("ChooseNpcView.ShowAgainChooseScene prefix", () =>
            {
                Plugin.SpeakProtected("ending:choose-date-again", "Final date screen. Choose who to ask out.", true, 1f, 2.3f);
            });
        }
    }

    [HarmonyPatch(typeof(ChooseNpcView), "ShowGlobalTurnResults")]
    internal static class ChooseNpcViewShowGlobalTurnResultsPatch
    {
        private static void Postfix()
        {
            Run("ChooseNpcView.ShowGlobalTurnResults postfix", () =>
            {
                Plugin.SpeakProtected("ending:stats-screen", "Ending stats screen. Press T to hear the current player's stats.", true, 2f, 1.5f);
            });
        }
    }

    [HarmonyPatch(typeof(ResultsView), "ShowView")]
    internal static class ResultsViewShowViewPatch
    {
        private static void Postfix(GlobalProgressData aTotalProgress, GlobalProgressData aSeenProgress, GlobalProgressData aNewProgress, int aTotalMemories, int aNewMemories, List<PlayerData> aPlayerRanking, List<int> aRankingPositions)
        {
            Run("ResultsView.ShowView postfix", () =>
            {
                string text = ResultsSpeech.DescribeInitialResults(aTotalProgress, aSeenProgress, aNewProgress, aTotalMemories, aNewMemories, aPlayerRanking, aRankingPositions);
                Plugin.SpeakProtected("results:show", text, true, 1f, 4f);
            });
        }
    }

    [HarmonyPatch(typeof(ResultsView), "AdvanceToProgressResultsPage")]
    internal static class ResultsViewAdvanceToProgressResultsPagePatch
    {
        private static void Postfix(ResultsView __instance)
        {
            Run("ResultsView.AdvanceToProgressResultsPage postfix", () =>
            {
                string text = ResultsSpeech.DescribeProgressPage(__instance);
                Plugin.SpeakProtected("results:progress", text, true, 1f, 4f);
            });
        }
    }

    internal static class ResultsSpeech
    {
        internal static string DescribeInitialResults(GlobalProgressData totalProgress, GlobalProgressData seenProgress, GlobalProgressData newProgress, int totalMemories, int newMemories, List<PlayerData> ranking, List<int> positions)
        {
            List<string> parts = new List<string>();
            parts.Add("Game results screen");
            if (ranking != null && ranking.Count > 1)
            {
                parts.Add(DescribeRanking(ranking, positions));
                parts.Add("Press advance to show progress results");
            }
            else
            {
                parts.Add(DescribeProgressData(totalProgress, seenProgress, newProgress, totalMemories, newMemories));
                parts.Add("Press advance to continue");
            }

            return string.Join(". ", parts.ToArray());
        }

        internal static string DescribeProgressPage(ResultsView view)
        {
            List<string> parts = new List<string>();
            parts.Add("Progress results screen");
            object totalProgress = GetField(view, "mTotalProgress");
            object seenProgress = GetField(view, "mSeenProgress");
            object newProgress = GetField(view, "mNewProgress");
            int totalMemories = GetIntField(view, "mTotalMemories");
            int newMemories = GetIntField(view, "mNewMemories");
            parts.Add(DescribeProgressData(totalProgress, seenProgress, newProgress, totalMemories, newMemories));
            parts.Add("Press advance to continue to credits");
            return string.Join(". ", parts.ToArray());
        }

        private static string DescribeRanking(List<PlayerData> ranking, List<int> positions)
        {
            List<string> parts = new List<string>();
            parts.Add("Final ranking");
            for (int i = 0; i < ranking.Count && i < 6; i++)
            {
                PlayerData player = ranking[i];
                if (player == null)
                {
                    continue;
                }

                int position = positions != null && i < positions.Count ? positions[i] : i + 1;
                parts.Add(Ordinal(position) + " place, " + AccessibilityDescriber.PrettyName(player));
            }

            return string.Join(". ", parts.ToArray());
        }

        private static string DescribeProgressData(object totalProgress, object seenProgress, object newProgress, int totalMemories, int newMemories)
        {
            List<string> parts = new List<string>();
            parts.Add("New this run: " + GetIntField(newProgress, "events") + " events, " + GetIntField(newProgress, "outcomes") + " outcomes, " + GetIntField(newProgress, "secretEndings") + " secret endings");
            parts.Add("Total seen: " + GetIntField(seenProgress, "events") + " of " + GetIntField(totalProgress, "events") + " events, " + GetIntField(seenProgress, "outcomes") + " of " + GetIntField(totalProgress, "outcomes") + " outcomes, " + GetIntField(seenProgress, "secretEndings") + " of " + GetIntField(totalProgress, "secretEndings") + " secret endings");
            if (newMemories != 0 || totalMemories != 0)
            {
                parts.Add("Memories: +" + newMemories + " this run, " + (totalMemories + newMemories) + " total");
            }
            return string.Join(". ", parts.ToArray());
        }

        private static string Ordinal(int value)
        {
            int mod100 = value % 100;
            if (mod100 >= 11 && mod100 <= 13)
            {
                return value + "th";
            }

            switch (value % 10)
            {
                case 1:
                    return value + "st";
                case 2:
                    return value + "nd";
                case 3:
                    return value + "rd";
                default:
                    return value + "th";
            }
        }

        private static object GetField(object target, string fieldName)
        {
            if (target == null)
            {
                return null;
            }

            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(target);
        }

        private static int GetIntField(object target, string fieldName)
        {
            object value = GetField(target, fieldName);
            return value is int ? (int)value : 0;
        }
    }

    [HarmonyPatch(typeof(CreditsSceneController), "Start")]
    internal static class CreditsSceneControllerStartPatch
    {
        private static void Postfix()
        {
            Run("CreditsSceneController.Start postfix", () =>
            {
            });
        }
    }

    [HarmonyPatch(typeof(FullCreditsView), "ShowNextPage")]
    internal static class FullCreditsViewShowNextPagePatch
    {
        private static void Postfix(FullCreditsView __instance)
        {
            Run("FullCreditsView.ShowNextPage postfix", () =>
            {
                SpeakFullCreditsPage(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(FullCreditsView), "StartMe")]
    internal static class FullCreditsViewStartMePatch
    {
        private static void Postfix(FullCreditsView __instance)
        {
            Run("FullCreditsView.StartMe postfix", () =>
            {
                SpeakFullCreditsPage(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(FullCreditsView), "ShowPrevPage")]
    internal static class FullCreditsViewShowPrevPagePatch
    {
        private static void Postfix(FullCreditsView __instance)
        {
            Run("FullCreditsView.ShowPrevPage postfix", () =>
            {
                SpeakFullCreditsPage(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(SettingsWidget), "SetLabelValue")]
    internal static class SettingsWidgetSetLabelValuePatch
    {
        private static void Postfix(SettingsWidget __instance)
        {
            Run("SettingsWidget.SetLabelValue postfix", () =>
            {
                SettingsAccessibility.SpeakValue(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(SettingsWidget), "SetBarValueInstant")]
    internal static class SettingsWidgetSetBarValueInstantPatch
    {
        private static void Postfix(SettingsWidget __instance, int aValue)
        {
            Run("SettingsWidget.SetBarValueInstant postfix", () =>
            {
                SettingsAccessibility.SpeakValue(__instance, aValue);
            });
        }
    }

    [HarmonyPatch(typeof(SettingsWidget), "AnimateBar")]
    internal static class SettingsWidgetAnimateBarPatch
    {
        private static void Postfix(SettingsWidget __instance, int aValue)
        {
            Run("SettingsWidget.AnimateBar postfix", () =>
            {
                SettingsAccessibility.SpeakValue(__instance, aValue);
            });
        }
    }

    [HarmonyPatch(typeof(TriggerWarningWidget), "UiNextTriggerWarning")]
    internal static class TriggerWarningWidgetUiNextPatch
    {
        private static void Postfix(TriggerWarningWidget __instance)
        {
            Run("TriggerWarningWidget.UiNextTriggerWarning postfix", () =>
            {
                SettingsAccessibility.SpeakCustomizeValue(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(TriggerWarningWidget), "UiPrevTriggerWarning")]
    internal static class TriggerWarningWidgetUiPrevPatch
    {
        private static void Postfix(TriggerWarningWidget __instance)
        {
            Run("TriggerWarningWidget.UiPrevTriggerWarning postfix", () =>
            {
                SettingsAccessibility.SpeakCustomizeValue(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(AllowSelfLoveWidget), "UiToggleAllowSelfLove")]
    internal static class AllowSelfLoveWidgetTogglePatch
    {
        private static void Postfix(AllowSelfLoveWidget __instance)
        {
            Run("AllowSelfLoveWidget.UiToggleAllowSelfLove postfix", () =>
            {
                SettingsAccessibility.SpeakCustomizeValue(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(TriggerSeenPlotlinesRandomlyWidget), "UiToggleTriggerSeenPlotlinesRandomly")]
    internal static class TriggerSeenPlotlinesRandomlyWidgetTogglePatch
    {
        private static void Postfix(TriggerSeenPlotlinesRandomlyWidget __instance)
        {
            Run("TriggerSeenPlotlinesRandomlyWidget.UiToggleTriggerSeenPlotlinesRandomly postfix", () =>
            {
                SettingsAccessibility.SpeakCustomizeValue(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(ReplaceAprilCreepypastaWidget), "UiToggleReplaceAprilCreepypasta")]
    internal static class ReplaceAprilCreepypastaWidgetTogglePatch
    {
        private static void Postfix(ReplaceAprilCreepypastaWidget __instance)
        {
            Run("ReplaceAprilCreepypastaWidget.UiToggleReplaceAprilCreepypasta postfix", () =>
            {
                SettingsAccessibility.SpeakCustomizeValue(__instance);
            });
        }
    }

    [HarmonyPatch(typeof(OutfitPackWidget), "UiToggleOutfitPack")]
    internal static class OutfitPackWidgetTogglePatch
    {
        private static void Postfix(OutfitPackWidget __instance)
        {
            Run("OutfitPackWidget.UiToggleOutfitPack postfix", () =>
            {
                SettingsAccessibility.SpeakCustomizeValue(__instance);
            });
        }
    }

    [HarmonyPatch]
    internal static class SimpleInfoManualInitialPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(SimpleInfoManualView), "ShowInitialPage");
        }

        private static void Postfix()
        {
            Run("SimpleInfoManualView.ShowInitialPage postfix", () =>
            {
                ManualAccessibility.SpeakPage("cover");
            });
        }
    }

    [HarmonyPatch]
    internal static class SimpleInfoManualSlidePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(SimpleInfoManualView), "ShowSlide");
        }

        private static void Postfix(int aSlideIndex)
        {
            Run("SimpleInfoManualView.ShowSlide postfix", () =>
            {
                ManualAccessibility.SpeakPage(aSlideIndex.ToString());
            });
        }
    }

    [HarmonyPatch(typeof(TitleUiManager), "UpdateInput")]
    internal static class TitleUiManagerUpdateInputPatch
    {
        private static ButtonHandler _lastSpokenTitleButton;

        private static void Postfix(TitleUiManager __instance)
        {
            Run("TitleUiManager.UpdateInput postfix", () =>
            {
                if (__instance == null || __instance.mainButtonGroup == null)
                {
                    return;
                }

                ButtonHandler handler = Traverse.Create(__instance.mainButtonGroup).Field("lastHovered").GetValue<ButtonHandler>();
                if (handler == null)
                {
                    handler = __instance.mainButtonGroup.defaultButtonHandler;
                }

                if (handler != null && handler != _lastSpokenTitleButton && handler.gameObject.activeInHierarchy && handler.button != null && handler.button.isEnabled)
                {
                    _lastSpokenTitleButton = handler;
                    Plugin.SpeakFocus(handler.gameObject);
                }
            });
        }
    }

    [HarmonyPatch(typeof(IntroCinematicSceneController), "Start")]
    internal static class IntroCinematicSceneControllerStartPatch
    {
        private static void Postfix()
        {
            Run("IntroCinematicSceneController.Start postfix", () =>
            {
                Plugin.Speak("Intro cinematic. Press any key to skip.", true);
            });
        }
    }

    internal static class ComicSpeechTiming
    {
        private static float _delayQuestionsUntil;

        internal static float RecordStatResult(string text)
        {
            int length = string.IsNullOrEmpty(text) ? 0 : text.Length;
            float holdSeconds = Mathf.Clamp(1.2f + (length / 50f), 1.75f, 3.25f);
            _delayQuestionsUntil = Mathf.Max(_delayQuestionsUntil, Time.realtimeSinceStartup + holdSeconds);
            return holdSeconds;
        }

        internal static void SpeakQuestion(MonoBehaviour host, string key, string text)
        {
            float delay = _delayQuestionsUntil - Time.realtimeSinceStartup;
            if (delay > 0.05f && host != null && host.gameObject != null && host.gameObject.activeInHierarchy)
            {
                host.StartCoroutine(CoSpeakQuestionAfterDelay(key, text, delay));
                return;
            }

            Plugin.SpeakProtected(key, text, true, 1f, 3f);
        }

        private static IEnumerator CoSpeakQuestionAfterDelay(string key, string text, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Plugin.SpeakProtected(key, text, true, 1f, 3f);
        }
    }

    [HarmonyPatch(typeof(TitleSectionButton), "OpenSection")]
    internal static class TitleSectionButtonOpenSectionPatch
    {
        private static void Postfix(TitleSectionButton __instance)
        {
            Run("TitleSectionButton.OpenSection postfix", () =>
            {
                if (__instance != null)
                {
                    string text = UiTextReader.DescribeObject(__instance.gameObject);
                    if (!string.IsNullOrEmpty(text))
                    {
                        TitleMenuAccessibility.RecordOpen(__instance, text);
                    }
                }
            });
        }
    }

    [HarmonyPatch(typeof(SceneController), "Update")]
    internal static class SceneControllerUpdatePatch
    {
        private static void Postfix()
        {
            Run("SceneController.Update postfix", () =>
            {
                HotkeyAccessibility.Tick("SceneController.Update");
                CreditsAccessibility.Tick();
                UpdateCheck.Tick();
            });
        }
    }

    internal static class HookLog
    {
        internal static string NameOf(GameObject value)
        {
            return value == null ? "null" : value.name;
        }

        internal static void Run(string label, System.Action action)
        {
            try
            {
                action();
            }
            catch (System.Exception ex)
            {
                LogFailure(label, ex);
            }
        }

        internal static void LogFailure(string label, System.Exception ex)
        {
            Plugin.Log?.LogWarning(label + " failed: " + ex);
        }

        internal static void SpeakPopup(string key, string message, string title, string firstButton, string secondButton, string thirdButton)
        {
            string text = string.IsNullOrEmpty(title) ? message : title + ". " + message;
            if (!string.IsNullOrEmpty(firstButton))
            {
                text += ". " + firstButton;
            }
            if (!string.IsNullOrEmpty(secondButton))
            {
                text += ". " + secondButton;
            }
            if (!string.IsNullOrEmpty(thirdButton))
            {
                text += ". " + thirdButton;
            }

            text = UiTextReader.Clean(text);
            Plugin.SpeakProtected("popup:" + key, text, true, 2f, EstimatePopupFocusSuppressSeconds(text));
        }

        internal static float EstimatePopupFocusSuppressSeconds(string text)
        {
            text = UiTextReader.Clean(text);
            if (string.IsNullOrEmpty(text))
            {
                return 2f;
            }

            float seconds = 1.5f + ((float)text.Length / 18f);
            if (seconds < 3f)
            {
                return 3f;
            }
            if (seconds > 10f)
            {
                return 10f;
            }
            return seconds;
        }

        internal static void SpeakFullCreditsPage(FullCreditsView view)
        {
            if (view == null)
            {
                return;
            }

            string page = view.pageNumberLabel == null ? string.Empty : UiTextReader.Clean(view.pageNumberLabel.text);
            string left = view.contentLabelLeft == null ? string.Empty : UiTextReader.Clean(view.contentLabelLeft.text);
            string right = view.contentLabelRight == null ? string.Empty : UiTextReader.Clean(view.contentLabelRight.text);
            string body = string.Empty;
            if (view.contentTextLeftContainer != null && view.contentTextLeftContainer.activeInHierarchy)
            {
                body = left;
            }
            else if (view.contentTextRightContainer != null && view.contentTextRightContainer.activeInHierarchy)
            {
                body = right;
            }
            else
            {
                body = string.IsNullOrEmpty(left) ? right : left;
            }

            string text = string.IsNullOrEmpty(page) ? body : "Credits page " + page + ". " + body;
            Plugin.SpeakProtected("full-credits:" + page, text, true, 1f, 3f);
        }
    }
}
