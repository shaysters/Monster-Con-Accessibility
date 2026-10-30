using System.Collections.Generic;
using System.Reflection;
using BeautifulGlitch;
using BeautifulGlitchConfig;
using Game;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class AccessibilityDescriber
    {
        internal static bool TryDescribeObject(GameObject go, out string text)
        {
            text = string.Empty;
            try
            {
                if (go == null)
                {
                    return false;
                }

                if (go.name != null && go.name.IndexOf("NoneButton", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    text = "No location selected. Stay at the starting spot.";
                    return true;
                }

                ButtonHandler handler = go.GetComponent<ButtonHandler>() ?? go.GetComponentInParent<ButtonHandler>();
                if (handler != null && TryDescribeButton(handler, out text))
                {
                    return true;
                }

                MainTurnLocationWidget mainLocation = go.GetComponentInParent<MainTurnLocationWidget>();
                if (mainLocation != null && mainLocation.sign != null && mainLocation.sign.gameObject == go)
                {
                    text = "Location, " + GetLocationName(mainLocation.locationType);
                    return true;
                }

                CafeteriaLocationWidget cafeteriaLocation = go.GetComponentInParent<CafeteriaLocationWidget>();
                // Any focused object inside the table: controller navigation and mouse hover
                // don't always land on the button itself.
                if (cafeteriaLocation != null)
                {
                    text = DescribeCafeteriaLocation(cafeteriaLocation);
                    return true;
                }

                DatesLocationWidget datesLocation = go.GetComponentInParent<DatesLocationWidget>();
                if (datesLocation != null && datesLocation.button != null && datesLocation.button.gameObject == go)
                {
                    text = DescribeDatesLocation(datesLocation);
                    return true;
                }

                PitstopLocationWidget pitstopLocation = go.GetComponentInParent<PitstopLocationWidget>();
                if (pitstopLocation != null && pitstopLocation.pitstopButton != null && pitstopLocation.pitstopButton.gameObject == go)
                {
                    text = "Pit stop location " + Humanize(pitstopLocation.type.ToString());
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("AccessibilityDescriber.TryDescribeObject failed: " + ex);
            }

            return !string.IsNullOrEmpty(text);
        }

        internal static string DescribeStats()
        {
            try
            {
                List<string> parts = new List<string>();
                parts.Add("Visible stats");
                StatWidget[] statWidgets = Object.FindObjectsOfType<StatWidget>();
                for (int i = 0; i < statWidgets.Length; i++)
                {
                    StatWidget stat = statWidgets[i];
                    if (stat == null || !stat.gameObject.activeInHierarchy || stat.nameLabel == null || stat.valueLabel == null)
                    {
                        continue;
                    }

                    string name = UiTextReader.Clean(stat.nameLabel.text);
                    string value = UiTextReader.Clean(stat.valueLabel.text).Replace("[b]", "").Replace("[/b]", "");
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                    {
                        parts.Add(name + " " + value);
                    }
                }

                if (parts.Count == 1)
                {
                    return "No visible stats on this screen.";
                }

                return string.Join(". ", parts.ToArray());
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("AccessibilityDescriber.DescribeStats failed: " + ex);
                return "Stats are not available.";
            }
        }

        internal static string DescribeChoices(List<string> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                return "Make your choice.";
            }

            List<string> parts = new List<string>();
            parts.Add("Make your choice");
            for (int i = 0; i < choices.Count; i++)
            {
                string choice = UiTextReader.Clean(choices[i]);
                if (!string.IsNullOrEmpty(choice))
                {
                    parts.Add("Choice " + (i + 1) + ", " + choice);
                }
            }
            return string.Join(". ", parts.ToArray());
        }

        private static bool TryDescribeButton(ButtonHandler handler, out string text)
        {
            text = string.Empty;
            if (handler == null)
            {
                return false;
            }

            EndingIllustration ending = handler as EndingIllustration;
            if (ending != null)
            {
                text = ending.npcId == "none" ? "None of them" : "Ask out " + GetNpcName(ending.npcId);
                return true;
            }

            DatesNpcSelectorButton dateSelector = handler as DatesNpcSelectorButton;
            if (dateSelector != null)
            {
                text = "Ask out option, " + CleanOrFallback(dateSelector.npcNameLabel == null ? string.Empty : dateSelector.npcNameLabel.text, "unknown");
                return true;
            }

            DatesPhoneNpcButton phoneNpc = handler as DatesPhoneNpcButton;
            if (phoneNpc != null)
            {
                string name = CleanOrFallback(phoneNpc.npcName == null ? string.Empty : phoneNpc.npcName.text, "empty phone slot");
                text = "Phone contact, " + name;
                bool selectable = GetBoolByMethod(phoneNpc, "IsSelectable", true);
                if (!selectable)
                {
                    string tooltip = phoneNpc.tooltip == null || phoneNpc.tooltip.tooltipLabel == null ? string.Empty : UiTextReader.Clean(phoneNpc.tooltip.tooltipLabel.text);
                    text += string.IsNullOrEmpty(tooltip) ? ", unavailable" : ", unavailable, " + tooltip;
                }
                return true;
            }

            DatesActionButton dateAction = handler as DatesActionButton;
            if (dateAction != null)
            {
                text = "Date action, " + CleanOrFallback(dateAction.label == null ? string.Empty : dateAction.label.text, Humanize(dateAction.name));
                if (dateAction.lockedIcon != null && dateAction.lockedIcon.gameObject.activeInHierarchy)
                {
                    text += ", locked";
                }
                return true;
            }

            ConShopChoosePlayerButton shopPlayer = handler as ConShopChoosePlayerButton;
            if (shopPlayer != null)
            {
                text = "Mystery shop recipient, " + CleanOrFallback(shopPlayer.playerLabel == null ? string.Empty : shopPlayer.playerLabel.text, "empty player slot") + ". Choose who gets the figurine effect.";
                return true;
            }

            ConShopItemGroupButton shopGroup = handler as ConShopItemGroupButton;
            if (shopGroup != null)
            {
                ConShopItemGroupProfile group = InvokeNoArg<ConShopItemGroupProfile>(shopGroup, "GetItemGroupAssigned");
                text = group == null ? "Empty shop item slot" : DescribeConShopGroup(group);
                return true;
            }

            MetaShopItemButton metaShop = handler as MetaShopItemButton;
            if (metaShop != null)
            {
                object profile = GetField(metaShop, "itemProfile");
                if (profile == null)
                {
                    return false;
                }

                string id = GetFieldString(profile, "id");
                string price = GetFieldString(profile, "price");
                text = "Shop item, " + Humanize(id);
                if (!string.IsNullOrEmpty(price))
                {
                    text += ", price " + price;
                }
                if (metaShop.lockedSprite != null && metaShop.lockedSprite.enabled)
                {
                    text += ", locked";
                }
                if (metaShop.newTagSprite != null && metaShop.newTagSprite.enabled)
                {
                    text += ", new";
                }
                return true;
            }

            OutfitPackWidget outfitPack = handler as OutfitPackWidget;
            if (outfitPack != null)
            {
                string name = CleanOrFallback(outfitPack.titleLabel == null ? string.Empty : outfitPack.titleLabel.text, Humanize(outfitPack.type.ToString()));
                bool enabled = GetBoolField(outfitPack, "mIsOutfitPackEnabled");
                text = "Outfit pack, " + name + ", " + (enabled ? "enabled" : "disabled") + ". Press confirm to toggle.";
                return true;
            }

            LocationActionButton locationAction = handler as LocationActionButton;
            if (locationAction != null)
            {
                text = "Location action, " + CleanOrFallback(locationAction.label == null ? string.Empty : locationAction.label.text, Humanize(locationAction.name));
                return true;
            }

            ConPopQuizButton popQuiz = handler as ConPopQuizButton;
            if (popQuiz != null)
            {
                text = DescribeConPopQuizButton(popQuiz);
                return true;
            }

            PrologueLikeButton like = handler as PrologueLikeButton;
            if (like != null)
            {
                text = DescribePrologueLikeButton(like);
                return true;
            }

            RunModeButton runMode = handler as RunModeButton;
            if (runMode != null)
            {
                string label = CleanOrFallback(runMode.label == null ? string.Empty : runMode.label.text, Humanize(runMode.runMode.ToString()) + " game");
                string duration = CleanOrFallback(runMode.durationLabel == null ? string.Empty : runMode.durationLabel.text, string.Empty);
                text = string.IsNullOrEmpty(duration) ? label : label + ", " + duration;
                return true;
            }

            ProloguePcButton pc = handler as ProloguePcButton;
            if (pc != null)
            {
                object profile = GetField(pc, "pcProfile");
                text = "Character, " + Humanize(GetFieldString(profile, "id"));
                if (pc.dlcTag != null && pc.dlcTag.enabled)
                {
                    text += ", locked DLC character";
                }
                return true;
            }

            ProloguePlayerNameButton nameButton = handler as ProloguePlayerNameButton;
            if (nameButton != null)
            {
                text = "Player name option, " + CleanOrFallback(nameButton.label == null ? string.Empty : nameButton.label.text, Humanize(nameButton.name));
                return true;
            }

            ProloguePlayerPronounButton pronounButton = handler as ProloguePlayerPronounButton;
            if (pronounButton != null)
            {
                text = "Pronoun option, " + CleanOrFallback(pronounButton.label == null ? string.Empty : pronounButton.label.text, Humanize(pronounButton.name));
                return true;
            }

            ControlModeButton controlMode = handler as ControlModeButton;
            if (controlMode != null)
            {
                text = controlMode.controlMode == InputControlMode.Share ? "Control mode, shared controls. Players take turns using this device." : "Control mode, separate controls. Each player uses their own device.";
                return true;
            }

            DifficultyModeButton difficulty = handler as DifficultyModeButton;
            if (difficulty != null)
            {
                text = "Difficulty, " + Humanize(difficulty.difficultyMode.ToString());
                return true;
            }

            LovePlaceComplimentOptionButton compliment = handler as LovePlaceComplimentOptionButton;
            if (compliment != null)
            {
                text = "Compliment choice, " + CleanOrFallback(compliment.label == null ? string.Empty : compliment.label.text, Humanize(compliment.name));
                return true;
            }

            ComicAnswerButton comicAnswer = handler as ComicAnswerButton;
            if (comicAnswer != null)
            {
                int index = GetIntByMethod(comicAnswer, "GetComicEventChoiceIndex", -1);
                text = "Comic choice " + (index + 1) + ", " + CleanOrFallback(comicAnswer.label == null ? string.Empty : comicAnswer.label.text, Humanize(comicAnswer.name));
                return true;
            }

            ComicCustomAnswerButton comicCustom = handler as ComicCustomAnswerButton;
            if (comicCustom != null)
            {
                text = "Comic custom answer. Type your own answer.";
                return true;
            }

            SelectCharacterButton selectCharacter = handler as SelectCharacterButton;
            if (selectCharacter != null)
            {
                text = "Character choice, " + CleanOrFallback(selectCharacter.label == null ? string.Empty : selectCharacter.label.text, Humanize(selectCharacter.name));
                return true;
            }

            ChallengerButton challenger = handler as ChallengerButton;
            if (challenger != null)
            {
                text = "Choose player, " + CleanOrFallback(challenger.nameLabel == null ? string.Empty : challenger.nameLabel.text, "player " + (challenger.playerIndex + 1));
                return true;
            }

            GossipOptionButton gossip = handler as GossipOptionButton;
            if (gossip != null)
            {
                string prefix = "Choice";
                if (gossip is StatOptionButton)
                {
                    prefix = "Stat choice";
                }
                else if (gossip is QualityOptionButton)
                {
                    prefix = "Quality choice";
                }
                else if (gossip is CustomAnswerOptionButton)
                {
                    prefix = "Custom answer";
                }
                text = prefix + ", " + CleanOrFallback(gossip.label == null ? string.Empty : gossip.label.text, Humanize(gossip.name));
                return true;
            }

            ActionButton action = handler as ActionButton;
            if (action != null)
            {
                text = DescribeActionButton(action);
                return true;
            }

            EventChoiceButton choice = handler as EventChoiceButton;
            if (choice != null)
            {
                int choiceNumber = choice.choiceId <= 0 ? 1 : choice.choiceId;
                text = "Choice " + choiceNumber + ", " + CleanOrFallback(choice.label == null ? string.Empty : choice.label.text, Humanize(choice.name));
                return true;
            }

            return false;
        }

        internal static string DescribeChoicesWithStats(EventFlow evt, List<string> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                return "Make your choice.";
            }

            List<string> parts = new List<string>();
            bool tutorial = evt != null && !string.IsNullOrEmpty(evt.eventName) && evt.eventName.IndexOf("tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0;
            parts.Add(tutorial ? "Tutorial choice. The tutorial is pointing out what to look for in each option" : "Make your choice");
            for (int i = 0; i < choices.Count; i++)
            {
                string choice = UiTextReader.Clean(choices[i]);
                if (string.IsNullOrEmpty(choice))
                {
                    continue;
                }

                string text = "Choice " + (i + 1) + ", " + choice;
                StatType stat = GetStatForChoice(evt, i + 1);
                if (stat != StatType.None)
                {
                    text += ". Stat clue, " + Humanize(stat.ToString());
                }
                parts.Add(text);
            }

            if (HasAnyStatChoice(evt, choices.Count))
            {
                parts.Add("Tutorial reminder: in stat-exchange events, the chosen option usually gains its stat and loses the stat tied to the option you did not choose.");
            }

            return string.Join(". ", parts.ToArray());
        }

        internal static string DescribeChoicePrompt(EventFlow evt)
        {
            bool tutorial = evt != null && !string.IsNullOrEmpty(evt.eventName) && evt.eventName.IndexOf("tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (tutorial)
            {
                return "Tutorial choice. Make your choice.";
            }

            return "Make your choice.";
        }

        internal static bool ShouldAllowFocusDuringSpeech(GameObject go)
        {
            try
            {
                if (go == null)
                {
                    return false;
                }

                ButtonHandler handler = go.GetComponent<ButtonHandler>() ?? go.GetComponentInParent<ButtonHandler>();
                if (handler is EventChoiceButton ||
                    handler is ComicAnswerButton ||
                    handler is ComicCustomAnswerButton ||
                    handler is ConPopQuizButton ||
                    handler is PrologueLikeButton ||
                    handler is ProloguePcButton ||
                    handler is ProloguePlayerNameButton ||
                    handler is ProloguePlayerPronounButton ||
                    handler is CustomizePanelButton ||
                    handler is RunModeButton ||
                    handler is LocationActionButton ||
                    handler is DatesNpcSelectorButton ||
                    handler is DatesPhoneNpcButton ||
                    handler is DatesActionButton ||
                    handler is ConShopChoosePlayerButton ||
                    handler is ConShopItemGroupButton ||
                    handler is TitleSubSectionButton)
                {
                    return true;
                }

                if (handler is ActionButton)
                {
                    return true;
                }

                return go.GetComponentInParent<MainTurnLocationWidget>() != null ||
                       go.GetComponentInParent<CafeteriaLocationWidget>() != null ||
                       go.GetComponentInParent<DatesLocationWidget>() != null ||
                       go.GetComponentInParent<PitstopLocationWidget>() != null;
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("AccessibilityDescriber.ShouldAllowFocusDuringSpeech failed: " + ex);
                return false;
            }
        }

        internal static string DescribeConPopQuiz(PrologueConPopQuizData data)
        {
            if (data == null || data.choicesData == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            parts.Add("Top three question, " + UiTextReader.Clean(data.questionText));
            for (int i = 0; i < data.choicesData.Count; i++)
            {
                object choice = data.choicesData[i];
                string option = CleanOrFallback(GetFieldString(choice, "optionText"), "option " + (i + 1));
                string stat = Humanize(GetFieldString(choice, "stat"));
                parts.Add("Option " + (i + 1) + ", " + option + ". Stat, " + stat);
            }
            parts.Add("Choose your first, second, and third favorite. The order shapes your starting stats.");
            return string.Join(". ", parts.ToArray());
        }

        internal static string DescribeConPopQuizView(ConPopQuizView view)
        {
            if (view == null || view.quizButtonGroup == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            string question = view.quizQuestionLabel == null ? string.Empty : UiTextReader.Clean(view.quizQuestionLabel.text);
            parts.Add(string.IsNullOrEmpty(question) ? "Top three question" : "Top three question, " + question);
            List<ButtonHandler> buttons = view.quizButtonGroup.buttonsHandlers;
            for (int i = 0; i < buttons.Count; i++)
            {
                ConPopQuizButton button = buttons[i] as ConPopQuizButton;
                if (button != null)
                {
                    parts.Add("Option " + (i + 1) + ", " + DescribeConPopQuizButton(button));
                }
            }
            parts.Add("Choose your first, second, and third favorite. The order shapes your starting stats.");
            return string.Join(". ", parts.ToArray());
        }

        internal static string DescribeLikes(List<PrologueLikeData> likes)
        {
            if (likes == null || likes.Count == 0)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            parts.Add("Choose three likes. Each like adds one starting stat point and influences which romance options appear more often.");
            for (int i = 0; i < likes.Count; i++)
            {
                PrologueLikeData like = likes[i];
                if (like == null)
                {
                    continue;
                }
                parts.Add("Option " + (i + 1) + ", " + UiTextReader.Clean(like.likeText) + ". Stat, " + Humanize(like.stat.ToString()) + ". Interests, " + DescribeLovables(like.lovables));
            }
            return string.Join(". ", parts.ToArray());
        }

        internal static string DescribeLikesView(PrologueLikesView view)
        {
            if (view == null || view.likesButtonGroup == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            parts.Add(DescribeLikesPrompt(view));
            List<ButtonHandler> buttons = view.likesButtonGroup.buttonsHandlers;
            for (int i = 0; i < buttons.Count; i++)
            {
                PrologueLikeButton button = buttons[i] as PrologueLikeButton;
                if (button != null)
                {
                    parts.Add("Option " + (i + 1) + ", " + DescribePrologueLikeButton(button));
                }
            }
            return string.Join(". ", parts.ToArray());
        }

        internal static string DescribeLikesPrompt(PrologueLikesView view)
        {
            string playerName = string.Empty;
            if (view != null && view.playerNameLabel != null)
            {
                playerName = UiTextReader.Clean(view.playerNameLabel.text);
            }

            string prefix = string.IsNullOrEmpty(playerName) ? "Choose this player's three likes" : "Choose " + playerName + "'s three likes";
            return prefix + ". These likes set starting stat points and influence which romance options appear more often.";
        }

        internal static string DescribeComicQuestion(object comicEvent)
        {
            if (comicEvent == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            object scenesObject = GetField(comicEvent, "eventScenes");
            List<EventScene> scenes = scenesObject as List<EventScene>;
            if (scenes != null && scenes.Count > 0)
            {
                parts.Add("Comic question, " + UiTextReader.Clean(scenes[0].text));
            }
            else
            {
                parts.Add("Comic question");
            }

            object choicesObject = InvokeNoArg<object>(comicEvent, "GetComicChoices");
            System.Collections.IEnumerable choices = choicesObject as System.Collections.IEnumerable;
            int index = 1;
            if (choices != null)
            {
                foreach (object choice in choices)
                {
                    string choiceText = CleanOrFallback(GetFieldString(choice, "choiceText"), "choice " + index);
                    string stat = Humanize(GetFieldString(choice, "statType"));
                    parts.Add("Choice " + index + ", " + choiceText + ". Stat, " + stat);
                    index++;
                }
            }
            parts.Add("The comic stage also changes stats; chosen answers give their listed stat and the unchosen answers can cost stats.");
            return string.Join(". ", parts.ToArray());
        }

        internal static string DescribeComicQuestionPrompt(ComicEvent comicEvent)
        {
            if (comicEvent == null)
            {
                return "Comic question.";
            }

            string question = string.Empty;
            if (comicEvent.eventScenes != null && comicEvent.eventScenes.Count > 0 && comicEvent.eventScenes[0] != null)
            {
                question = UiTextReader.Clean(comicEvent.eventScenes[0].text);
            }

            return string.IsNullOrEmpty(question) ? "Comic question." : question;
        }

        internal static string DescribeComicStatResult(ComicEvent comicEvent, int choiceIndex)
        {
            if (comicEvent == null)
            {
                return string.Empty;
            }

            List<ComicEventChoice> choices = InvokeNoArg<List<ComicEventChoice>>(comicEvent, "GetComicChoices");
            if (choices == null || choices.Count == 0)
            {
                return string.Empty;
            }

            if (choiceIndex >= 0 && choiceIndex < choices.Count)
            {
                List<string> losses = new List<string>();
                for (int i = 0; i < choices.Count; i++)
                {
                    if (i == choiceIndex)
                    {
                        continue;
                    }

                    losses.Add(Humanize(choices[i].statType.ToString()) + " 1");
                }

                string gain = Humanize(choices[choiceIndex].statType.ToString()) + " plus 3.";
                return losses.Count == 0 ? gain : gain + " Lose " + string.Join(", ", losses.ToArray()) + ".";
            }

            return "Comic stats updated.";
        }

        internal static string HumanizePublic(string name)
        {
            return Humanize(name);
        }

        private static string DescribeConShopGroup(ConShopItemGroupProfile group)
        {
            List<string> parts = new List<string>();
            parts.Add("Mystery shop box");
            if (group.itemsInGroup != null && group.itemsInGroup.Count > 0)
            {
                for (int i = 0; i < group.itemsInGroup.Count && i < 4; i++)
                {
                    ItemProfile item = group.itemsInGroup[i];
                    if (item != null)
                    {
                        parts.Add(DescribeShopItem(item));
                    }
                }
            }
            else
            {
                parts.Add(Humanize(group.id));
            }
            return UiTextReader.Clean(string.Join(", ", parts.ToArray()));
        }

        private static string DescribeDatesLocation(DatesLocationWidget widget)
        {
            try
            {
                DatesLocationData data = null;
                if (widget != null && widget.datesView != null)
                {
                    object manager = GetField(widget.datesView, "mDatesManager");
                    data = InvokeNoArg<DatesLocationData>(manager, "GetDatesLocation", widget.locationId);
                }

                if (data == null)
                {
                    return "Date location " + (widget.locationId + 1);
                }

                object isPhoneValue = InvokeNoArg<object>(data, "IsPhoneNpcLocation");
                bool isPhoneNpcLocation = isPhoneValue is bool && (bool)isPhoneValue;
                if (isPhoneNpcLocation)
                {
                    return "Phone date location";
                }

                List<string> npcs = InvokeNoArg<List<string>>(data, "GetNpcs");
                if (npcs == null || npcs.Count == 0)
                {
                    return "Date location " + (widget.locationId + 1);
                }

                List<string> names = new List<string>();
                for (int i = 0; i < npcs.Count; i++)
                {
                    string npc = npcs[i];
                    if (string.IsNullOrEmpty(npc))
                    {
                        continue;
                    }
                    if (npc == "none")
                    {
                        names.Add("go alone");
                    }
                    else
                    {
                        names.Add(GetNpcName(npc));
                    }
                }

                if (names.Count == 0)
                {
                    return "Date location " + (widget.locationId + 1);
                }

                return "Date location, " + string.Join(" and ", names.ToArray());
            }
            catch (System.Exception)
            {
                return "Date location " + (widget == null ? string.Empty : (widget.locationId + 1).ToString());
            }
        }

        private static string DescribeShopItem(ItemProfile item)
        {
            string text = Humanize(item.id);
            if (item.price > 0)
            {
                text += ", price " + item.price;
            }
            if (item.statValues != null && item.statValues.Count > 0)
            {
                List<string> stats = new List<string>();
                for (int i = 0; i < item.statValues.Count && i < 4; i++)
                {
                    object statChange = item.statValues[i];
                    statChange = GetField(statChange, "statChange") ?? statChange;
                    string stat = Humanize(GetFieldString(statChange, "statToModify"));
                    string value = GetFieldString(statChange, "statValue");
                    if (!string.IsNullOrEmpty(stat) && !string.IsNullOrEmpty(value))
                    {
                        stats.Add(stat + " " + Signed(value));
                    }
                }
                if (stats.Count > 0)
                {
                    text += ", " + string.Join(", ", stats.ToArray());
                }
            }
            else if (item.itemEffectType != ConShopItemEffect.DoNothing)
            {
                text += ", effect " + Humanize(item.itemEffectType.ToString());
            }
            else
            {
                text += ", no effect";
            }
            return text;
        }

        private static string DescribeConPopQuizButton(ConPopQuizButton button)
        {
            object data = InvokeNoArg<object>(button, "GetConPopQuizChoiceData");
            string option = CleanOrFallback(GetFieldString(data, "optionText"), button.label == null ? string.Empty : button.label.text);
            string stat = Humanize(GetFieldString(data, "stat"));
            string rank = button.numeralLabel == null ? string.Empty : UiTextReader.Clean(button.numeralLabel.text);
            string text = option + ". Stat, " + stat;
            if (!string.IsNullOrEmpty(rank))
            {
                text += ". Already chosen as " + rank;
            }
            return text;
        }

        private static string DescribePrologueLikeButton(PrologueLikeButton button)
        {
            object data = InvokeNoArg<object>(button, "GetLikeData");
            string option = CleanOrFallback(GetFieldString(data, "likeText"), button.label == null ? string.Empty : button.label.text);
            string stat = Humanize(GetFieldString(data, "stat"));
            object lovablesObject = GetField(data, "lovables");
            string text = option + ". Stat, " + stat;
            List<string> lovables = lovablesObject as List<string>;
            if (lovables != null && lovables.Count > 0)
            {
                text += ". Interests, " + DescribeLovables(lovables);
            }
            return text;
        }

        private static StatType GetStatForChoice(EventFlow evt, int choice)
        {
            try
            {
                return evt == null ? StatType.None : evt.GetStatForChoice(choice);
            }
            catch
            {
                return StatType.None;
            }
        }

        private static bool HasAnyStatChoice(EventFlow evt, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                if (GetStatForChoice(evt, i) != StatType.None)
                {
                    return true;
                }
            }
            return false;
        }

        private static string DescribeLovables(List<string> lovables)
        {
            if (lovables == null || lovables.Count == 0)
            {
                return "none listed";
            }

            List<string> names = new List<string>();
            for (int i = 0; i < lovables.Count && i < 4; i++)
            {
                names.Add(Humanize(lovables[i]));
            }
            return string.Join(", ", names.ToArray());
        }

        private static string Signed(string value)
        {
            if (string.IsNullOrEmpty(value) || value.StartsWith("-") || value.StartsWith("+"))
            {
                return value;
            }
            return "+" + value;
        }

        private static string DescribeNamedAction(string name)
        {
            string human = Humanize(name);
            if (name != null && (name.StartsWith("Music Button") || name.StartsWith("MusicButton")))
            {
                if (name.EndsWith("1")) return "Surf rock music";
                if (name.EndsWith("2")) return "Lo-fi music";
                if (name.EndsWith("3")) return "Kawaii music";
                if (name.EndsWith("4")) return "Retro music";
                if (name.EndsWith("5")) return "Fantasy music";
            }
            return human;
        }

        private static string DescribeActionButton(ActionButton action)
        {
            string label = CleanOrFallback(action.label == null ? string.Empty : action.label.text, DescribeNamedAction(action.name));
            MusicVariationView musicView = Object.FindObjectOfType<MusicVariationView>();
            if (musicView != null &&
                musicView.gameObject.activeInHierarchy &&
                musicView.musicVariationButtons != null &&
                musicView.musicVariationButtons.buttonsHandlers.Contains(action))
            {
                return "Music style, " + label;
            }

            string name = action.name ?? string.Empty;
            if (name.IndexOf("ComicTypeButton", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ComicTypeButton comicType = action as ComicTypeButton;
                string genre = comicType == null ? CleanComicGenreName(name, label) : Humanize(comicType.comicType.ToString());
                return genre;
            }

            return "Action, " + label;
        }

        private static string CleanComicGenreName(string objectName, string label)
        {
            string cleanedLabel = UiTextReader.Clean(label);
            if (!string.IsNullOrEmpty(cleanedLabel) &&
                cleanedLabel.IndexOf("Comic Type Button", System.StringComparison.OrdinalIgnoreCase) < 0 &&
                cleanedLabel.IndexOf("ComicTypeButton", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                return cleanedLabel;
            }

            string text = objectName ?? string.Empty;
            text = text.Replace("ComicTypeButton", string.Empty).Replace("Comic Type Button", string.Empty);
            text = text.Replace("Button", string.Empty).Replace("_", " ").Replace("-", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
            return string.IsNullOrEmpty(text) ? cleanedLabel : Humanize(text);
        }

        private static string GetLocationName(LocationType locationType)
        {
            try
            {
                return Humanize(locationType.ToString());
            }
            catch
            {
                return Humanize(locationType.ToString());
            }
        }

        // "Food court table 2: Vera, Liam. 2 seats free." Each CafeteriaLocation has four
        // seats ("positions"), each holding an NPC id, a PlayerId name ("PlayerA"), or "".
        private static string DescribeCafeteriaLocation(CafeteriaLocationWidget widget)
        {
            string title = widget.isLovePlace ? "Food court love table" : "Food court table " + (widget.locationId + 1);
            try
            {
                object manager = GetField(widget.cafeteriaView, "mCafeteriaManager");
                if (manager == null)
                {
                    return title + ", seats available";
                }

                object location = widget.isLovePlace
                    ? HarmonyLib.Traverse.Create(manager).Method("GetLovePlace").GetValue()
                    : HarmonyLib.Traverse.Create(manager).Method("GetNormalPlace", widget.locationId).GetValue();
                List<string> positions = GetField(location, "positions") as List<string>;
                if (positions == null)
                {
                    return title + ", seats available";
                }

                List<string> names = new List<string>();
                int free = 0;
                for (int i = 0; i < positions.Count; i++)
                {
                    if (string.IsNullOrEmpty(positions[i]))
                    {
                        free++;
                        continue;
                    }

                    names.Add(GetSeatOccupantName(positions[i]));
                }

                string text = title + ": ";
                if (names.Count > 0)
                {
                    text += string.Join(", ", names.ToArray()) + ". ";
                }

                return text + (free == 0 ? "No seats free." : free == 1 ? "1 seat free." : free + " seats free.");
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("DescribeCafeteriaLocation failed: " + ex);
                return title + ", seats available";
            }
        }

        private static string GetSeatOccupantName(string occupant)
        {
            if (System.Enum.IsDefined(typeof(PlayerId), occupant))
            {
                PlayerId playerId = (PlayerId)System.Enum.Parse(typeof(PlayerId), occupant);
                object player = HarmonyLib.Traverse.Create(GetField(GeneralManager.Instance, "sessionManager")).Method("GetPlayer", playerId).GetValue();
                return player != null ? PrettyName(player) : "Player";
            }

            string name = HarmonyLib.Traverse.Create(GetField(GeneralManager.Instance, "textManager")).Method("GetNpcName", occupant).GetValue() as string;
            name = UiTextReader.Clean(name);
            return string.IsNullOrEmpty(name) || name.StartsWith("CHR_") ? GetNpcName(occupant) : name;
        }

        private static string GetNpcName(string npcId)
        {
            try
            {
                return Humanize(npcId);
            }
            catch
            {
                return Humanize(npcId);
            }
        }

        private static string CleanOrFallback(string value, string fallback)
        {
            value = UiTextReader.Clean(value);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        private static string Humanize(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string text = name.Replace("_", " ").Replace("-", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
            return UiTextReader.Clean(text);
        }

        internal static string PrettyName(object player)
        {
            string value = InvokeNoArgString(player, "GetPrettyPrintName");
            if (!string.IsNullOrEmpty(value))
            {
                return UiTextReader.Clean(value);
            }

            value = GetFieldString(player, "name");
            return string.IsNullOrEmpty(value) ? "Player" : UiTextReader.Clean(value);
        }

        internal static string KeyForObject(object value)
        {
            string id = GetFieldString(value, "playerId");
            if (!string.IsNullOrEmpty(id))
            {
                return id;
            }
            return value == null ? "null" : value.GetHashCode().ToString();
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

        private static string GetFieldString(object target, string fieldName)
        {
            object value = GetField(target, fieldName);
            return value == null ? string.Empty : value.ToString();
        }

        private static bool GetBoolByMethod(object target, string methodName, bool fallback)
        {
            object value = InvokeNoArg<object>(target, methodName);
            if (value is bool)
            {
                return (bool)value;
            }
            return fallback;
        }

        private static int GetIntByMethod(object target, string methodName, int fallback)
        {
            object value = InvokeNoArg<object>(target, methodName);
            if (value is int)
            {
                return (int)value;
            }
            return fallback;
        }

        private static bool GetBoolField(object target, string fieldName)
        {
            object value = GetField(target, fieldName);
            return value is bool && (bool)value;
        }

        private static string InvokeNoArgString(object target, string methodName)
        {
            object value = InvokeNoArg<object>(target, methodName);
            return value == null ? string.Empty : value.ToString();
        }

        private static T InvokeNoArg<T>(object target, string methodName, params object[] args) where T : class
        {
            if (target == null)
            {
                return null;
            }

            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                return null;
            }

            return method.Invoke(target, args) as T;
        }
    }
}
