using System.Collections.Generic;
using System.Text;
using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using MaratGame.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MaratGame.Agent
{
    /// <summary>
    /// API для AI-агента (Unity MCP invoke_static): состояние игры в Play Mode и семантические действия.
    /// </summary>
    public static class AgentPlayBridge
    {
        const int BodyPreviewMax = 120;
        const int MaxFlagsInState = 64;

        /// <summary>JSON: сцена, узел сценария, статы, список доступных действий.</summary>
        public static string GetPlayState()
        {
            if (!Application.isPlaying)
                return ErrorJson("not_playing", "Enter play mode first (enter_play_mode).");

            var actions = new List<AgentAction>();
            CollectActions(actions);

            var scene = SceneManager.GetActiveScene().name;
            var runner = Object.FindFirstObjectByType<StoryRunner>();
            var node = runner?.Engine?.CurrentNode;
            var state = GameState.Instance;

            var sb = new StringBuilder(512);
            sb.Append('{');
            AppendProp(sb, "ok", "true");
            sb.Append(',');
            AppendProp(sb, "scene", scene);
            sb.Append(',');
            AppendProp(sb, "isPlaying", "true");
            sb.Append(',');
            AppendProp(sb, "nodeId", node?.id);
            sb.Append(',');
            AppendProp(sb, "speaker", node?.speaker);
            sb.Append(',');
            AppendProp(sb, "bodyPreview", Truncate(node?.bodyText, BodyPreviewMax));
            sb.Append(',');
            AppendProp(sb, "chapter", node?.chapterLabel);
            sb.Append(',');
            AppendProp(sb, "time", state?.CurrentTime);
            sb.Append(',');
            AppendProp(sb, "location", state?.CurrentLocationId);
            sb.Append(',');
            AppendProp(sb, "respect", state != null ? state.Stats.Respect.ToString() : "0");
            sb.Append(',');
            AppendProp(sb, "calm", state != null ? state.Stats.Calm.ToString() : "0");
            sb.Append(',');
            AppendProp(sb, "chaos", state != null ? state.Stats.Chaos.ToString() : "0");
            sb.Append(',');
            AppendProp(sb, "dayBlock", state != null ? state.CurrentDayBlock.ToString() : "");
            sb.Append(',');
            AppendProp(sb, "decisions", state != null ? state.DecisionsCount.ToString() : "0");
            sb.Append(',');
            AppendProp(sb, "flagCount", state != null ? state.Flags.Count.ToString() : "0");
            sb.Append(',');
            sb.Append("\"flags\":");
            AppendStringArrayJson(sb, state?.Flags.Snapshot(), MaxFlagsInState);
            sb.Append(',');
            sb.Append("\"actions\":");
            AppendActionsJson(sb, actions);
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// Выполнить действие по id из <see cref="GetPlayState"/> (choice:0, nav:left, phone:open, ui:Canvas/…, menu:start, …).
        /// </summary>
        public static string PerformAction(string actionId)
        {
            if (!Application.isPlaying)
                return ErrorJson("not_playing", "Enter play mode first (enter_play_mode).");

            if (string.IsNullOrWhiteSpace(actionId))
                return ErrorJson("missing_action", "actionId is required.");

            var id = actionId.Trim();

            if (id.StartsWith("ui:", System.StringComparison.Ordinal))
                return InvokeUiButton(id.Substring(3));

            if (TryPerformSemantic(id, out var message, out var nextNodeId, out var ok))
            {
                if (!ok)
                    return ErrorJson("action_failed", message ?? "Action failed.");

                var sb = new StringBuilder(128);
                sb.Append('{');
                AppendProp(sb, "ok", "true");
                sb.Append(',');
                AppendProp(sb, "actionId", id);
                sb.Append(',');
                AppendProp(sb, "message", message);
                sb.Append(',');
                AppendProp(sb, "nodeId", nextNodeId);
                sb.Append('}');
                return sb.ToString();
            }

            return ErrorJson("unknown_action", $"No handler for action: {id}");
        }

        static void CollectActions(List<AgentAction> actions)
        {
            var scene = SceneManager.GetActiveScene().name;
            var seenIds = new HashSet<string>();

            if (scene == SceneNames.MainMenu)
            {
                AddAction(actions, seenIds, "menu:start", "НАЧАТЬ ИГРУ", "menu");
                return;
            }

            var runner = Object.FindFirstObjectByType<StoryRunner>();
            var node = runner?.Engine?.CurrentNode;
            var state = GameState.Instance;
            var hasSingleAvailableChoice = false;
            var singleChoiceLabel = "Продолжить";

            var choiceAvailability = runner?.GetCurrentChoiceAvailability();
            if (choiceAvailability != null)
            {
                var availableCount = 0;
                for (var i = 0; i < choiceAvailability.Length; i++)
                {
                    var availability = choiceAvailability[i];
                    if (availability.Choice == null || !availability.IsAvailable)
                        continue;

                    availableCount++;
                    var label = availability.Choice.label ?? $"Выбор {availability.Index}";
                    AddAction(actions, seenIds, $"choice:{availability.Index}", label, "choice");
                    AddAction(actions, seenIds, BuildStoryChoiceActionId(node?.id, availability.Index), label, "story_choice");
                    AddAction(actions, seenIds, GetChoiceAliasActionId(node?.id, availability.Index), label, "story_choice_alias");
                    singleChoiceLabel = label;
                }

                hasSingleAvailableChoice = availableCount == 1;
            }

            if (node != null && node.id == PhoneUI.HallMorningNodeId && !state.Flags.HasFlag(PhoneUI.PhoneReadFlag))
                AddAction(actions, seenIds, "phone:open", "Телефон (входящие)", "phone");

            var phoneUi = Object.FindFirstObjectByType<PhoneUI>();
            if (phoneUi != null && phoneUi.IsOverlayOpen)
                AddAction(actions, seenIds, "phone:close", "Закрыть телефон → холл", "phone");

            if (node != null && node.id == NavigationBar.HubNodeId)
            {
                if (MorningBranchProgress.CanEnterMorningArea(NavigationBar.CanteenEntryNodeId, state.Flags))
                    AddAction(actions, seenIds, "nav:left", "← Столовая", "navigation");
                if (MorningBranchProgress.CanEnterMorningArea(NavigationBar.CabinetEntryNodeId, state.Flags))
                    AddAction(actions, seenIds, "nav:forward", "↑ Кабинет", "navigation");
                if (MorningBranchProgress.CanEnterMorningArea(NavigationBar.ElevatorEntryNodeId, state.Flags))
                    AddAction(actions, seenIds, "nav:right", "→ Лифты", "navigation");

                if (MorningBranchProgress.IsReadyForHubBirthdayInspect(state)
                    && !state.Flags.HasFlag(BirthdayEndFlags.BirthdaySeen))
                    AddAction(actions, seenIds, "hub:inspect", "● Осмотреться", "hub");
            }

            if (node != null &&
                (node.id == Chapter4KrrbNodes.BigCongratulationNodeId || node.id == Chapter4KrrbNodes.EveningGoodEndingNodeId))
            {
                AddAction(actions, seenIds, "ending:replay", "Сыграть ещё раз", "ending");
                AddAction(actions, seenIds, "ending:menu", "В главное меню", "ending");
            }

            if (hasSingleAvailableChoice)
                AddAction(actions, seenIds, "story:continue", singleChoiceLabel, "story_continue");

            CollectInteractableUiButtons(actions, seenIds);
        }

        static void CollectInteractableUiButtons(List<AgentAction> actions, HashSet<string> seenIds)
        {
            var seen = new HashSet<string>();
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                if (!IsClickable(button))
                    continue;

                var path = AgentUiPaths.GetHierarchyPath(button.gameObject);
                if (string.IsNullOrEmpty(path) || !seen.Add(path))
                    continue;

                var label = GetButtonLabel(button);
                AddAction(actions, seenIds, $"ui:{path}", label, "ui_button");
            }
        }

        static bool TryPerformSemantic(string id, out string message, out string nextNodeId, out bool ok)
        {
            message = null;
            nextNodeId = null;
            ok = false;

            if (TryPerformChoiceSemantic(id, out message, out nextNodeId, out ok))
                return true;

            if (id == "menu:start")
            {
                var menu = Object.FindFirstObjectByType<MainMenuController>();
                if (menu == null)
                {
                    message = "MainMenuController not found.";
                    return true;
                }

                menu.OnStartGameClicked();
                message = "Loaded Game scene.";
                ok = true;
                return true;
            }

            if (id.StartsWith("nav:", System.StringComparison.Ordinal))
            {
                var target = id switch
                {
                    "nav:left" => NavigationBar.CanteenEntryNodeId,
                    "nav:forward" => NavigationBar.CabinetEntryNodeId,
                    "nav:right" => NavigationBar.ElevatorEntryNodeId,
                    _ => null
                };

                if (target == null)
                {
                    message = "Unknown navigation action.";
                    return true;
                }

                var flags = GameState.Instance?.Flags;
                if (flags != null && !MorningBranchProgress.CanEnterMorningArea(target, flags))
                {
                    message = $"Morning area already visited: {target}.";
                    return true;
                }

                var runner = Object.FindFirstObjectByType<StoryRunner>();
                runner?.LoadNode(target);
                nextNodeId = runner?.Engine?.CurrentNode?.id;
                message = $"Navigated to {target}.";
                ok = true;
                return true;
            }

            if (id == "phone:open")
            {
                var phone = Object.FindFirstObjectByType<PhoneUI>();
                if (phone == null)
                {
                    message = "PhoneUI not found.";
                    return true;
                }

                phone.AgentOpenOverlay();
                message = "Phone overlay opened.";
                ok = true;
                return true;
            }

            if (id == "phone:close")
            {
                var phone = Object.FindFirstObjectByType<PhoneUI>();
                if (phone == null)
                {
                    message = "PhoneUI not found.";
                    return true;
                }

                phone.AgentCloseOverlay();
                var runner = Object.FindFirstObjectByType<StoryRunner>();
                nextNodeId = runner?.Engine?.CurrentNode?.id;
                message = "Phone closed, continued to hall hub.";
                ok = true;
                return true;
            }

            if (id == "hub:inspect")
            {
                var hub = Object.FindFirstObjectByType<HubActionButton>();
                if (hub == null)
                {
                    message = "HubActionButton not found.";
                    return true;
                }

                hub.AgentTriggerInspect();
                var runner = Object.FindFirstObjectByType<StoryRunner>();
                nextNodeId = runner?.Engine?.CurrentNode?.id;
                message = "Triggered birthday inspect.";
                ok = true;
                return true;
            }

            if (id == "ending:replay")
            {
                var ending = Object.FindFirstObjectByType<EndingUI>();
                if (ending == null)
                {
                    message = "EndingUI not found.";
                    return true;
                }

                ending.OnReplayClicked();
                message = "Replay started.";
                ok = true;
                return true;
            }

            if (id == "ending:menu")
            {
                var ending = Object.FindFirstObjectByType<EndingUI>();
                if (ending == null)
                {
                    message = "EndingUI not found.";
                    return true;
                }

                ending.OnMainMenuClicked();
                message = "Returned to main menu.";
                ok = true;
                return true;
            }

            return false;
        }

        static bool TryPerformChoiceSemantic(string id, out string message, out string nextNodeId, out bool ok)
        {
            message = null;
            nextNodeId = null;
            ok = false;

            if (id == "story:continue")
            {
                if (!TrySelectFirstAvailableChoice(out var continueIndex, out nextNodeId, out message))
                    return true;

                message = $"Selected first available choice {continueIndex}.";
                ok = true;
                return true;
            }

            if (id.StartsWith("choice:", System.StringComparison.Ordinal))
            {
                if (!int.TryParse(id.Substring("choice:".Length), out var index))
                {
                    message = "Invalid choice index.";
                    return true;
                }

                if (!TrySelectChoice(index, out nextNodeId, out message))
                    return true;

                message = $"Selected choice {index}.";
                ok = true;
                return true;
            }

            if (id.StartsWith("story:", System.StringComparison.Ordinal))
            {
                if (!TrySelectStoryChoice(id, out var storyChoiceIndex, out nextNodeId, out message))
                    return true;

                message = $"Selected story choice {storyChoiceIndex}.";
                ok = true;
                return true;
            }

            if (TryResolveChoiceAlias(id, out var aliasChoiceIndex))
            {
                if (!TrySelectChoice(aliasChoiceIndex, out nextNodeId, out message))
                    return true;

                message = $"Selected aliased choice {aliasChoiceIndex}.";
                ok = true;
                return true;
            }

            return false;
        }

        static bool TrySelectStoryChoice(string actionId, out int index, out string nextNodeId, out string message)
        {
            index = -1;
            nextNodeId = null;
            message = null;

            var runner = Object.FindFirstObjectByType<StoryRunner>();
            if (runner == null)
            {
                message = "StoryRunner not found.";
                return false;
            }

            var parts = actionId.Split(':');
            if (parts.Length != 4 || parts[2] != "choice" || !int.TryParse(parts[3], out index))
            {
                message = "Expected format: story:<nodeId>:choice:<index>.";
                return false;
            }

            var expectedNodeId = parts[1];
            var currentNodeId = runner.Engine?.CurrentNode?.id;
            if (!string.Equals(currentNodeId, expectedNodeId, System.StringComparison.Ordinal))
            {
                message = $"Current node is '{currentNodeId}', expected '{expectedNodeId}'.";
                return false;
            }

            return TrySelectChoice(index, out nextNodeId, out message);
        }

        static bool TrySelectChoice(int index, out string nextNodeId, out string message)
        {
            nextNodeId = null;
            message = null;

            var runner = Object.FindFirstObjectByType<StoryRunner>();
            if (runner == null)
            {
                message = "StoryRunner not found.";
                return false;
            }

            var choices = runner.GetCurrentChoiceAvailability();
            if (choices == null || choices.Length == 0)
            {
                message = "No choices available on current node.";
                return false;
            }

            if (index < 0 || index >= choices.Length)
            {
                message = $"Choice index {index} is out of range.";
                return false;
            }

            var choice = choices[index];
            if (!choice.IsAvailable)
            {
                message = string.IsNullOrWhiteSpace(choice.Reason)
                    ? $"Choice {index} is unavailable."
                    : $"Choice {index} is unavailable: {choice.Reason}";
                return false;
            }

            runner.SelectChoice(index);
            nextNodeId = runner.Engine?.CurrentNode?.id;
            return true;
        }

        static bool TrySelectFirstAvailableChoice(out int selectedIndex, out string nextNodeId, out string message)
        {
            selectedIndex = -1;
            nextNodeId = null;
            message = null;

            var runner = Object.FindFirstObjectByType<StoryRunner>();
            if (runner == null)
            {
                message = "StoryRunner not found.";
                return false;
            }

            var choices = runner.GetCurrentChoiceAvailability();
            if (choices == null || choices.Length == 0)
            {
                message = "No choices available on current node.";
                return false;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                if (!choices[i].IsAvailable)
                    continue;

                selectedIndex = choices[i].Index;
                runner.SelectChoice(selectedIndex);
                nextNodeId = runner.Engine?.CurrentNode?.id;
                return true;
            }

            message = "No available choices on current node.";
            return false;
        }

        static bool TryResolveChoiceAlias(string actionId, out int index)
        {
            index = -1;
            var runner = Object.FindFirstObjectByType<StoryRunner>();
            var nodeId = runner?.Engine?.CurrentNode?.id;
            if (string.IsNullOrWhiteSpace(nodeId))
                return false;

            switch (nodeId)
            {
                case BirthdayEndNodes.PrePlanerkaBridgeNodeId:
                    if (actionId == "chapter2:to_workplace") { index = 0; return true; }
                    break;
                case BirthdayEndNodes.Chapter2CabinetIntroNodeId:
                    if (actionId == "chapter2:inspect_cabinet") { index = 0; return true; }
                    break;
                case Chapter2CabinetNodes.EntryNodeId:
                    if (actionId == "chapter2:mail") { index = 0; return true; }
                    if (actionId == "chapter2:alevtina") { index = 1; return true; }
                    if (actionId == "chapter2:toilet") { index = 2; return true; }
                    if (actionId == "chapter2:meeting_35_44") { index = 3; return true; }
                    if (actionId == "chapter2:planerka") { index = 4; return true; }
                    break;
                case Chapter2CabinetNodes.ToiletIntroNodeId:
                    if (actionId == "toilet:wash_face") { index = 0; return true; }
                    if (actionId == "toilet:phone_messages") { index = 1; return true; }
                    if (actionId == "toilet:silence") { index = 2; return true; }
                    if (actionId == "toilet:middle_stall") { index = 3; return true; }
                    if (actionId == "toilet:return") { index = 4; return true; }
                    break;
                case Chapter3PlanerkaNodes.IntroNodeId:
                    if (actionId == "planerka:business") { index = 0; return true; }
                    if (actionId == "planerka:humor") { index = 1; return true; }
                    if (actionId == "planerka:observer") { index = 2; return true; }
                    break;
                case Chapter4KrrbNodes.IntroNodeId:
                    if (actionId == "krrb:seat_alevtina") { index = 0; return true; }
                    if (actionId == "krrb:seat_kozlikhin") { index = 1; return true; }
                    if (actionId == "krrb:seat_back") { index = 2; return true; }
                    break;
                case NavigationBar.CanteenEntryNodeId:
                    if (actionId == "morning:canteen_kozlikhin") { index = 0; return true; }
                    if (actionId == "morning:canteen_alone") { index = 1; return true; }
                    if (actionId == "morning:canteen_coffee") { index = 2; return true; }
                    break;
                case NavigationBar.ElevatorEntryNodeId:
                    if (actionId == "morning:elevator_nozdrikov") { index = 0; return true; }
                    if (actionId == "morning:elevator_messages") { index = 1; return true; }
                    if (actionId == "morning:elevator_random_floor") { index = 2; return true; }
                    break;
                case Chapter4KrrbNodes.EveningIntroNodeId:
                    if (actionId == "evening:what_next") { index = 0; return true; }
                    break;
                case Chapter4KrrbNodes.EveningBankEmptyNodeId:
                    if (actionId == "evening:big_congratulation") { index = 0; return true; }
                    if (actionId == "evening:good_ending") { index = 1; return true; }
                    break;
            }

            return false;
        }

        static string InvokeUiButton(string objectPath)
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            GameObject go = null;
            foreach (var root in roots)
            {
                go = AgentUiPaths.FindByPath(root.transform, objectPath);
                if (go != null)
                    break;
            }

            if (go == null)
                return ErrorJson("not_found", $"GameObject not found: {objectPath}");

            var button = go.GetComponent<Button>() ?? go.GetComponentInChildren<Button>(true);
            if (button == null)
                return ErrorJson("no_button", $"No Button on: {objectPath}");

            if (!IsClickable(button))
                return ErrorJson("not_interactable", $"Button not clickable: {objectPath}");

            button.onClick.Invoke();

            var runner = Object.FindFirstObjectByType<StoryRunner>();
            var sb = new StringBuilder(128);
            sb.Append('{');
            AppendProp(sb, "ok", "true");
            sb.Append(',');
            AppendProp(sb, "actionId", $"ui:{objectPath}");
            sb.Append(',');
            AppendProp(sb, "message", "Button onClick invoked.");
            sb.Append(',');
            AppendProp(sb, "nodeId", runner?.Engine?.CurrentNode?.id);
            sb.Append('}');
            return sb.ToString();
        }

        static bool IsClickable(Button button)
        {
            if (button == null || !button.isActiveAndEnabled || !button.interactable)
                return false;

            if (!button.gameObject.activeInHierarchy)
                return false;

            var groups = button.GetComponentsInParent<CanvasGroup>(true);
            foreach (var group in groups)
            {
                if (group == null)
                    continue;

                if (!group.interactable || group.alpha < 0.01f || !group.blocksRaycasts)
                    return false;
            }

            return true;
        }

        static void AddAction(List<AgentAction> actions, HashSet<string> seenIds, string id, string label, string kind)
        {
            if (string.IsNullOrWhiteSpace(id) || seenIds == null || !seenIds.Add(id))
                return;

            actions.Add(new AgentAction(id, label, kind));
        }

        static string BuildStoryChoiceActionId(string nodeId, int choiceIndex)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                return null;

            return $"story:{nodeId}:choice:{choiceIndex}";
        }

        static string GetChoiceAliasActionId(string nodeId, int choiceIndex)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                return null;

            switch (nodeId)
            {
                case var n when n == BirthdayEndNodes.PrePlanerkaBridgeNodeId:
                    return choiceIndex == 0 ? "chapter2:to_workplace" : null;
                case var n when n == BirthdayEndNodes.Chapter2CabinetIntroNodeId:
                    return choiceIndex == 0 ? "chapter2:inspect_cabinet" : null;
                case var n when n == Chapter2CabinetNodes.EntryNodeId:
                    return choiceIndex switch
                    {
                        0 => "chapter2:mail",
                        1 => "chapter2:alevtina",
                        2 => "chapter2:toilet",
                        3 => "chapter2:meeting_35_44",
                        4 => "chapter2:planerka",
                        _ => null
                    };
                case var n when n == Chapter2CabinetNodes.ToiletIntroNodeId:
                    return choiceIndex switch
                    {
                        0 => "toilet:wash_face",
                        1 => "toilet:phone_messages",
                        2 => "toilet:silence",
                        3 => "toilet:middle_stall",
                        4 => "toilet:return",
                        _ => null
                    };
                case var n when n == Chapter3PlanerkaNodes.IntroNodeId:
                    return choiceIndex switch
                    {
                        0 => "planerka:business",
                        1 => "planerka:humor",
                        2 => "planerka:observer",
                        _ => null
                    };
                case var n when n == Chapter4KrrbNodes.IntroNodeId:
                    return choiceIndex switch
                    {
                        0 => "krrb:seat_alevtina",
                        1 => "krrb:seat_kozlikhin",
                        2 => "krrb:seat_back",
                        _ => null
                    };
                case var n when n == NavigationBar.CanteenEntryNodeId:
                    return choiceIndex switch
                    {
                        0 => "morning:canteen_kozlikhin",
                        1 => "morning:canteen_alone",
                        2 => "morning:canteen_coffee",
                        _ => null
                    };
                case var n when n == NavigationBar.ElevatorEntryNodeId:
                    return choiceIndex switch
                    {
                        0 => "morning:elevator_nozdrikov",
                        1 => "morning:elevator_messages",
                        2 => "morning:elevator_random_floor",
                        _ => null
                    };
                case var n when n == Chapter4KrrbNodes.EveningIntroNodeId:
                    return choiceIndex == 0 ? "evening:what_next" : null;
                case var n when n == Chapter4KrrbNodes.EveningBankEmptyNodeId:
                    return choiceIndex switch
                    {
                        0 => "evening:big_congratulation",
                        1 => "evening:good_ending",
                        _ => null
                    };
                default:
                    return null;
            }
        }

        static string GetButtonLabel(Button button)
        {
            var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null && !string.IsNullOrWhiteSpace(tmp.text))
                return tmp.text.Replace("\n", " ").Trim();

            return button.gameObject.name;
        }

        static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            text = text.Replace("\n", " ").Trim();
            return text.Length <= max ? text : text.Substring(0, max) + "…";
        }

        static string ErrorJson(string code, string message)
        {
            var sb = new StringBuilder(128);
            sb.Append('{');
            AppendProp(sb, "ok", "false");
            sb.Append(',');
            AppendProp(sb, "error", code);
            sb.Append(',');
            AppendProp(sb, "message", message);
            sb.Append('}');
            return sb.ToString();
        }

        static void AppendStringArrayJson(StringBuilder sb, string[] values, int maxItems)
        {
            sb.Append('[');
            if (values != null)
            {
                var count = values.Length;
                if (maxItems > 0 && count > maxItems)
                    count = maxItems;

                for (var i = 0; i < count; i++)
                {
                    if (i > 0)
                        sb.Append(',');

                    sb.Append(JsonString(values[i]));
                }
            }

            sb.Append(']');
        }

        static void AppendActionsJson(StringBuilder sb, List<AgentAction> actions)
        {
            sb.Append('[');
            for (var i = 0; i < actions.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');

                var a = actions[i];
                sb.Append('{');
                AppendProp(sb, "id", a.Id);
                sb.Append(',');
                AppendProp(sb, "label", a.Label);
                sb.Append(',');
                AppendProp(sb, "kind", a.Kind);
                sb.Append('}');
            }

            sb.Append(']');
        }

        static void AppendProp(StringBuilder sb, string key, string value)
        {
            sb.Append('"').Append(key).Append("\":");
            sb.Append(JsonString(value));
        }

        static string JsonString(string value)
        {
            if (value == null)
                return "null";

            var sb = new StringBuilder(value.Length + 8);
            sb.Append('"');
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32)
                            sb.AppendFormat("\\u{0:X4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }

            sb.Append('"');
            return sb.ToString();
        }

        readonly struct AgentAction
        {
            public readonly string Id;
            public readonly string Label;
            public readonly string Kind;

            public AgentAction(string id, string label, string kind)
            {
                Id = id;
                Label = label;
                Kind = kind;
            }
        }
    }
}
