using System.Collections.Generic;
using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 10: утренние ветки столовая / лифты / кабинет + alevtina_tease.
    /// </summary>
    static class StoryBranchesMorningSetup
    {
        const string DataFolder = "Assets/Data/Story";
        const string NodeMorningPath = DataFolder + "/Node_HallMorning.asset";
        const string NodeHubPath = DataFolder + "/Node_HallHub.asset";
        const string NodeCanteenPath = DataFolder + "/Node_CanteenEntry.asset";
        const string NodeCabinetPath = DataFolder + "/Node_CabinetEntry.asset";
        const string NodeElevatorPath = DataFolder + "/Node_ElevatorEntry.asset";
        const string NodeAlevtinaPath = DataFolder + "/Node_AlevtinaTease.asset";
        const string NodeCanteenKozlikhinPath = DataFolder + "/Node_CanteenKozlikhin.asset";
        const string NodeCanteenKozlikhinFollowupPath = DataFolder + "/Node_CanteenKozlikhinFollowup.asset";
        const string NodeCanteenAlonePath = DataFolder + "/Node_CanteenAlone.asset";
        const string NodeCanteenAloneFollowupPath = DataFolder + "/Node_CanteenAloneFollowup.asset";
        const string NodeCanteenCoffeePath = DataFolder + "/Node_CanteenCoffee.asset";
        const string NodeCanteenCoffeeFollowupPath = DataFolder + "/Node_CanteenCoffeeFollowup.asset";
        const string NodeElevatorNozdrikovPath = DataFolder + "/Node_ElevatorNozdrikov.asset";
        const string NodeElevatorNozdrikovFollowupPath = DataFolder + "/Node_ElevatorNozdrikovFollowup.asset";
        const string NodeElevatorMessagesPath = DataFolder + "/Node_ElevatorMessages.asset";
        const string NodeElevatorMessagesFollowupPath = DataFolder + "/Node_ElevatorMessagesFollowup.asset";
        const string NodeElevatorRandomFloorPath = DataFolder + "/Node_ElevatorRandomFloor.asset";
        const string NodeElevatorRandomFloorFollowupPath = DataFolder + "/Node_ElevatorRandomFloorFollowup.asset";
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";

        [MenuItem("MaratGame/Story/Setup Step 10 (Morning Branches)")]
        public static void SetupStep10()
        {
            CreateBranchesMorningContent();
            GameUiBranchesSetup.EnsureBranchPortraitsOnGameScene();
            Debug.Log("[MaratGame] Step 10: morning branches ready. Play: hall_hub → ←↑→ → choices → back.");
        }

        [MenuItem("MaratGame/Story/Setup Full Morning Branches")]
        public static void SetupFullMorningBranches()
        {
            SetupStep10();
            Debug.Log("[MaratGame] Full morning branches setup applied.");
        }

        [MenuItem("MaratGame/Story/Create Morning Branches Content")]
        public static void CreateBranchesMorningContent()
        {
            StoryMvpSetup.EnsureStoryDataFolder();
            StoryNavigationSetup.CreateNavigationStoryContent();

            var canteen = StoryMvpSetup.LoadOrCreateNode(NodeCanteenPath);
            canteen.id = NavigationBar.CanteenEntryNodeId;
            canteen.locationId = "canteen";
            canteen.timeDisplay = "08:03";
            canteen.chapterLabel = "ГЛАВА 1 · УТРО";
            canteen.speaker = string.Empty;
            canteen.bodyText =
                "Завтрак на подносе, запах свежего кофе — в столовой уже шумно, как в любой понедельник. " +
                "Выбирайте, как начать день.";
            canteen.choices = new[]
            {
                MakeChoice("Сесть к Козлихину", MorningBranchNodes.CanteenKozlikhinNodeId),
                MakeChoice("Сесть одному", MorningBranchNodes.CanteenAloneNodeId),
                MakeChoice("Кофе с собой", MorningBranchNodes.CanteenCoffeeNodeId)
            };
            canteen.onEnterEffects = new List<StatChangeEntry>();

            var elevator = StoryMvpSetup.LoadOrCreateNode(NodeElevatorPath);
            elevator.id = NavigationBar.ElevatorEntryNodeId;
            elevator.locationId = "elevator";
            elevator.timeDisplay = "08:06";
            elevator.chapterLabel = "ГЛАВА 1 · УТРО";
            elevator.speaker = string.Empty;
            elevator.bodyText =
                "Лифтовая зона гудит: один кабинет только что уехал наверх. Ноздриков ждёт у кнопки вызова.";
            elevator.choices = new[]
            {
                MakeChoice("Разговор с Ноздриковым", MorningBranchNodes.ElevatorNozdrikovNodeId),
                MakeChoice("Читать сообщения", MorningBranchNodes.ElevatorMessagesNodeId),
                MakeChoice("Случайный этаж", MorningBranchNodes.ElevatorRandomFloorNodeId)
            };
            elevator.onEnterEffects = new List<StatChangeEntry>();

            var cabinet = StoryMvpSetup.LoadOrCreateNode(NodeCabinetPath);
            cabinet.id = NavigationBar.CabinetEntryNodeId;
            cabinet.locationId = "cabinet";
            cabinet.timeDisplay = "08:10";
            cabinet.chapterLabel = "ГЛАВА 1 · УТРО";
            cabinet.bodyText =
                "Ваш стол: монитор, горящая лампа и стопка из 43 непрочитанных писем. " +
                "Алевтина заглядывает из соседнего кабинета.";
            cabinet.portraitCharacterId = CharacterIds.Alevtina;
            cabinet.choices = new[]
            {
                new StoryChoice
                {
                    label = "Пойти к Алевтине",
                    targetNodeId = MorningBranchNodes.AlevtinaTeaseNodeId
                }
            };
            cabinet.onEnterEffects = new List<StatChangeEntry>();

            var alevtina = StoryMvpSetup.LoadOrCreateNode(NodeAlevtinaPath);
            alevtina.id = MorningBranchNodes.AlevtinaTeaseNodeId;
            alevtina.locationId = "cabinet";
            alevtina.timeDisplay = "08:11";
            alevtina.chapterLabel = "ГЛАВА 1 · УТРО";
            alevtina.speaker = "Алевтина";
            alevtina.portraitCharacterId = CharacterIds.Alevtina;
            alevtina.bodyText =
                "— Ну что, именинник, кофе уже пил или сразу к тосту побежишь? " +
                "Письма подождут, день рождения — нет.";
            alevtina.onEnterEffects = new List<StatChangeEntry>();
            alevtina.choices = new[]
            {
                MakeChoice(
                    "В холл",
                    NavigationBar.HubNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Respect, delta = 2 } },
                    new[] { MorningBranchFlags.MetAlevtinaCabinet })
            };

            var canteenKozlikhin = StoryMvpSetup.LoadOrCreateNode(NodeCanteenKozlikhinPath);
            canteenKozlikhin.id = MorningBranchNodes.CanteenKozlikhinNodeId;
            canteenKozlikhin.locationId = "canteen";
            canteenKozlikhin.timeDisplay = "08:04";
            canteenKozlikhin.chapterLabel = "ГЛАВА 1 · УТРО";
            canteenKozlikhin.speaker = "Козлихин";
            canteenKozlikhin.uiMode = StoryUiMode.Dialogue;
            canteenKozlikhin.portraitCharacterId = CharacterIds.Kozlikhin;
            canteenKozlikhin.bodyText =
                "— Марат, с днём рождения! На КРРБ сегодня опять будут ругаться за маршрут. " +
                "Если что, я тебя подстрахую по отчёту.";
            canteenKozlikhin.onEnterEffects = new List<StatChangeEntry>();
            canteenKozlikhin.choices = new[]
            {
                MakeChoice("Обсудить маршрут на КРРБ", "canteen_kozlikhin_followup")
            };

            var canteenKozlikhinFollowup = StoryMvpSetup.LoadOrCreateNode(NodeCanteenKozlikhinFollowupPath);
            canteenKozlikhinFollowup.id = "canteen_kozlikhin_followup";
            canteenKozlikhinFollowup.locationId = "canteen";
            canteenKozlikhinFollowup.timeDisplay = "08:05";
            canteenKozlikhinFollowup.chapterLabel = "ГЛАВА 1 · УТРО";
            canteenKozlikhinFollowup.speaker = "Марат";
            canteenKozlikhinFollowup.uiMode = StoryUiMode.Dialogue;
            canteenKozlikhinFollowup.bodyText =
                "Козлихин набрасывает схему: кто едет в КРРБ, а кто остаётся на подхвате в банке. " +
                "Вы договариваетесь держать связь в мессенджере.";
            canteenKozlikhinFollowup.onEnterEffects = new List<StatChangeEntry>();
            canteenKozlikhinFollowup.choices = new[]
            {
                MakeChoice(
                    "В холл",
                    NavigationBar.HubNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Respect, delta = 4 } },
                    new[] { MorningBranchFlags.MetKozlikhin, MorningBranchFlags.MetKozlikhinBreakfast })
            };

            var canteenAlone = StoryMvpSetup.LoadOrCreateNode(NodeCanteenAlonePath);
            canteenAlone.id = MorningBranchNodes.CanteenAloneNodeId;
            canteenAlone.locationId = "canteen";
            canteenAlone.timeDisplay = "08:04";
            canteenAlone.chapterLabel = "ГЛАВА 1 · УТРО";
            canteenAlone.speaker = "Марат";
            canteenAlone.uiMode = StoryUiMode.Monologue;
            canteenAlone.bodyText =
                "Вы занимаете столик у окна. В наушниках тихо играет плейлист, и впервые за утро появляется пауза.";
            canteenAlone.onEnterEffects = new List<StatChangeEntry>();
            canteenAlone.choices = new[]
            {
                MakeChoice("Собраться с мыслями", "canteen_alone_followup")
            };

            var canteenAloneFollowup = StoryMvpSetup.LoadOrCreateNode(NodeCanteenAloneFollowupPath);
            canteenAloneFollowup.id = "canteen_alone_followup";
            canteenAloneFollowup.locationId = "canteen";
            canteenAloneFollowup.timeDisplay = "08:05";
            canteenAloneFollowup.chapterLabel = "ГЛАВА 1 · УТРО";
            canteenAloneFollowup.speaker = string.Empty;
            canteenAloneFollowup.uiMode = StoryUiMode.Monologue;
            canteenAloneFollowup.bodyText =
                "Пять спокойных минут помогают разложить в голове задачи до планёрки и не суетиться.";
            canteenAloneFollowup.onEnterEffects = new List<StatChangeEntry>();
            canteenAloneFollowup.choices = new[]
            {
                MakeChoice(
                    "В холл",
                    NavigationBar.HubNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Calm, delta = 3 } },
                    new[] { MorningBranchFlags.AteAloneReflection })
            };

            var canteenCoffee = StoryMvpSetup.LoadOrCreateNode(NodeCanteenCoffeePath);
            canteenCoffee.id = MorningBranchNodes.CanteenCoffeeNodeId;
            canteenCoffee.locationId = "canteen";
            canteenCoffee.timeDisplay = "08:04";
            canteenCoffee.chapterLabel = "ГЛАВА 1 · УТРО";
            canteenCoffee.speaker = "Бариста";
            canteenCoffee.bodyText =
                "— Двойной эспрессо и в путь? Сегодня у нас пол-офиса так стартует.";
            canteenCoffee.onEnterEffects = new List<StatChangeEntry>();
            canteenCoffee.choices = new[]
            {
                MakeChoice("Забрать стакан и идти", "canteen_coffee_followup")
            };

            var canteenCoffeeFollowup = StoryMvpSetup.LoadOrCreateNode(NodeCanteenCoffeeFollowupPath);
            canteenCoffeeFollowup.id = "canteen_coffee_followup";
            canteenCoffeeFollowup.locationId = "hall";
            canteenCoffeeFollowup.timeDisplay = "08:05";
            canteenCoffeeFollowup.chapterLabel = "ГЛАВА 1 · УТРО";
            canteenCoffeeFollowup.speaker = "Марат";
            canteenCoffeeFollowup.bodyText =
                "Кофе на ходу бодрит, но темп утра снова ускоряется: задач много, времени мало.";
            canteenCoffeeFollowup.onEnterEffects = new List<StatChangeEntry>();
            canteenCoffeeFollowup.choices = new[]
            {
                MakeChoice(
                    "В холл",
                    NavigationBar.HubNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Calm, delta = 1 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = 2 }
                    },
                    new[] { MorningBranchFlags.CoffeeToGo })
            };

            var elevatorNozdrikov = StoryMvpSetup.LoadOrCreateNode(NodeElevatorNozdrikovPath);
            elevatorNozdrikov.id = MorningBranchNodes.ElevatorNozdrikovNodeId;
            elevatorNozdrikov.locationId = "elevator";
            elevatorNozdrikov.timeDisplay = "08:07";
            elevatorNozdrikov.chapterLabel = "ГЛАВА 1 · УТРО";
            elevatorNozdrikov.speaker = "Ноздриков";
            elevatorNozdrikov.portraitCharacterId = CharacterIds.Nozdrikov;
            elevatorNozdrikov.bodyText =
                "— Если поедем на КРРБ через старый маршрут, точно опоздаем. " +
                "Есть обход через службу сопровождения, но надо согласовать заранее.";
            elevatorNozdrikov.onEnterEffects = new List<StatChangeEntry>();
            elevatorNozdrikov.choices = new[]
            {
                MakeChoice("Сверить маршруты", "elevator_nozdrikov_followup")
            };

            var elevatorNozdrikovFollowup = StoryMvpSetup.LoadOrCreateNode(NodeElevatorNozdrikovFollowupPath);
            elevatorNozdrikovFollowup.id = "elevator_nozdrikov_followup";
            elevatorNozdrikovFollowup.locationId = "elevator";
            elevatorNozdrikovFollowup.timeDisplay = "08:08";
            elevatorNozdrikovFollowup.chapterLabel = "ГЛАВА 1 · УТРО";
            elevatorNozdrikovFollowup.speaker = "Марат";
            elevatorNozdrikovFollowup.bodyText =
                "Вы фиксируете ключевые точки маршрута и решаете перепроверить их после планёрки.";
            elevatorNozdrikovFollowup.onEnterEffects = new List<StatChangeEntry>();
            elevatorNozdrikovFollowup.choices = new[]
            {
                MakeChoice(
                    "В холл",
                    NavigationBar.HubNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 2 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = 1 }
                    },
                    new[] { MorningBranchFlags.MetNozdrikov, MorningBranchFlags.MetNozdrikovRoute })
            };

            var elevatorMessages = StoryMvpSetup.LoadOrCreateNode(NodeElevatorMessagesPath);
            elevatorMessages.id = MorningBranchNodes.ElevatorMessagesNodeId;
            elevatorMessages.locationId = "elevator";
            elevatorMessages.timeDisplay = "08:07";
            elevatorMessages.chapterLabel = "ГЛАВА 1 · УТРО";
            elevatorMessages.speaker = string.Empty;
            elevatorMessages.uiMode = StoryUiMode.PhoneInbox;
            elevatorMessages.bodyText = string.Empty;
            elevatorMessages.onEnterEffects = new List<StatChangeEntry>();
            elevatorMessages.choices = new[]
            {
                MakeChoice("Закрыть чат", "elevator_messages_followup")
            };

            var elevatorMessagesFollowup = StoryMvpSetup.LoadOrCreateNode(NodeElevatorMessagesFollowupPath);
            elevatorMessagesFollowup.id = "elevator_messages_followup";
            elevatorMessagesFollowup.locationId = "hall";
            elevatorMessagesFollowup.timeDisplay = "08:08";
            elevatorMessagesFollowup.chapterLabel = "ГЛАВА 1 · УТРО";
            elevatorMessagesFollowup.speaker = "Марат";
            elevatorMessagesFollowup.bodyText =
                "Сообщения дают контекст по дню: понятно, где вы нужны в первую очередь.";
            elevatorMessagesFollowup.onEnterEffects = new List<StatChangeEntry>();
            elevatorMessagesFollowup.choices = new[]
            {
                MakeChoice(
                    "В холл",
                    NavigationBar.HubNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Calm, delta = 2 } },
                    new[] { MorningBranchFlags.ElevatorMessagesRead })
            };

            var elevatorRandomFloor = StoryMvpSetup.LoadOrCreateNode(NodeElevatorRandomFloorPath);
            elevatorRandomFloor.id = MorningBranchNodes.ElevatorRandomFloorNodeId;
            elevatorRandomFloor.locationId = "elevator";
            elevatorRandomFloor.timeDisplay = "08:07";
            elevatorRandomFloor.chapterLabel = "ГЛАВА 1 · УТРО";
            elevatorRandomFloor.speaker = string.Empty;
            elevatorRandomFloor.bodyText =
                "Вы машинально нажимаете не ту кнопку и выходите этажом выше. На двери табличка: «Служба КРРБ».";
            elevatorRandomFloor.onEnterEffects = new List<StatChangeEntry>();
            elevatorRandomFloor.choices = new[]
            {
                MakeChoice("Спуститься обратно", "elevator_random_floor_followup")
            };

            var elevatorRandomFloorFollowup = StoryMvpSetup.LoadOrCreateNode(NodeElevatorRandomFloorFollowupPath);
            elevatorRandomFloorFollowup.id = "elevator_random_floor_followup";
            elevatorRandomFloorFollowup.locationId = "hall";
            elevatorRandomFloorFollowup.timeDisplay = "08:09";
            elevatorRandomFloorFollowup.chapterLabel = "ГЛАВА 1 · УТРО";
            elevatorRandomFloorFollowup.speaker = "Марат";
            elevatorRandomFloorFollowup.bodyText =
                "Случайный крюк добавляет суматохи, зато вы уже представляете, где потом искать нужный кабинет.";
            elevatorRandomFloorFollowup.onEnterEffects = new List<StatChangeEntry>();
            elevatorRandomFloorFollowup.choices = new[]
            {
                MakeChoice(
                    "В холл",
                    NavigationBar.HubNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Calm, delta = -2 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = 4 }
                    },
                    new[] { MorningBranchFlags.RandomFloorEvent })
            };

            var hallMorning = AssetDatabase.LoadAssetAtPath<StoryNodeData>(NodeMorningPath);
            var hallHub = AssetDatabase.LoadAssetAtPath<StoryNodeData>(NodeHubPath);

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<StoryDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.startNodeId = PhoneUI.HallMorningNodeId;
            var nodes = new List<StoryNodeData>
            {
                hallMorning,
                hallHub,
                canteen,
                cabinet,
                elevator,
                alevtina,
                canteenKozlikhin,
                canteenKozlikhinFollowup,
                canteenAlone,
                canteenAloneFollowup,
                canteenCoffee,
                canteenCoffeeFollowup,
                elevatorNozdrikov,
                elevatorNozdrikovFollowup,
                elevatorMessages,
                elevatorMessagesFollowup,
                elevatorRandomFloor,
                elevatorRandomFloorFollowup
            };
            database.nodes = nodes;

            foreach (var node in nodes)
            {
                if (node != null)
                    EditorUtility.SetDirty(node);
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaratGame] Morning branches expanded: canteen/elevator 3 variants each + cabinet.");
        }

        [MenuItem("MaratGame/Story/Run Morning Branches Smoke Test")]
        public static void RunBranchesSmokeTest()
        {
            CreateBranchesMorningContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            var state = GameState.Instance;
            state.Reset();

            var engine = new MaratGame.Narrative.StoryEngine(state, database);
            engine.LoadNode(NavigationBar.HubNodeId);

            engine.LoadNode(NavigationBar.CanteenEntryNodeId);
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            if (!state.Flags.HasFlag(MorningBranchFlags.MetKozlikhinBreakfast) || state.Stats.Respect != 54)
                Debug.LogError("[MaratGame] Canteen/Kozlikhin: expected breakfast flag and Respect 54.");
            if (engine.CurrentNode.id != NavigationBar.HubNodeId)
                Debug.LogError("[MaratGame] Canteen should return to hall_hub.");

            engine.LoadNode(NavigationBar.CanteenEntryNodeId);
            engine.SelectChoice(1);
            if (engine.CurrentNode.uiMode != StoryUiMode.Monologue)
                Debug.LogError("[MaratGame] Canteen/alone should use Monologue UI mode.");
            engine.SelectChoice(0);
            if (engine.CurrentNode.uiMode != StoryUiMode.Monologue)
                Debug.LogError("[MaratGame] Canteen/alone followup should use Monologue UI mode.");
            engine.SelectChoice(0);
            if (!state.Flags.HasFlag(MorningBranchFlags.AteAloneReflection) || state.Stats.Calm != 63)
                Debug.LogError("[MaratGame] Canteen/alone: expected reflection flag and Calm 63.");

            engine.LoadNode(NavigationBar.CanteenEntryNodeId);
            engine.SelectChoice(2);
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            if (!state.Flags.HasFlag(MorningBranchFlags.CoffeeToGo) || state.Stats.Chaos != 2)
                Debug.LogError("[MaratGame] Canteen/coffee: expected coffee flag and Chaos 2.");

            engine.LoadNode(NavigationBar.ElevatorEntryNodeId);
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            if (!state.Flags.HasFlag(MorningBranchFlags.MetNozdrikovRoute))
                Debug.LogError("[MaratGame] Elevator/nozdrikov: expected route flag.");

            engine.LoadNode(NavigationBar.ElevatorEntryNodeId);
            engine.SelectChoice(1);
            if (engine.CurrentNode.uiMode != StoryUiMode.PhoneInbox)
                Debug.LogError("[MaratGame] Elevator/messages should use PhoneInbox UI mode.");
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            if (!state.Flags.HasFlag(MorningBranchFlags.ElevatorMessagesRead))
                Debug.LogError("[MaratGame] Elevator/messages: expected messages flag.");

            engine.LoadNode(NavigationBar.ElevatorEntryNodeId);
            engine.SelectChoice(2);
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            if (!state.Flags.HasFlag(MorningBranchFlags.RandomFloorEvent))
                Debug.LogError("[MaratGame] Elevator/random floor: expected random floor flag.");

            engine.LoadNode(NavigationBar.CabinetEntryNodeId);
            engine.SelectChoice(0);
            if (engine.CurrentNode.id != MorningBranchNodes.AlevtinaTeaseNodeId)
                Debug.LogError("[MaratGame] Cabinet should go to alevtina_tease.");

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != NavigationBar.HubNodeId)
                Debug.LogError("[MaratGame] Alevtina tease should return to hall_hub.");
            if (!state.Flags.HasFlag(MorningBranchFlags.MetAlevtinaCabinet))
                Debug.LogError("[MaratGame] Alevtina tease should set met_alevtina_cabinet.");

            if (state.Stats.Respect != 58 || state.Stats.Calm != 64 || state.Stats.Chaos != 7)
                Debug.LogError("[MaratGame] Unexpected final stats after all morning branches.");

            state.Reset();
            engine = new MaratGame.Narrative.StoryEngine(state, database);
            engine.LoadNode(NavigationBar.HubNodeId);
            engine.LoadNode(NavigationBar.ElevatorEntryNodeId);
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            engine.SelectChoice(0);
            if (MorningBranchProgress.IsReadyForHubBirthdayInspect(state))
                Debug.LogError("[MaratGame] One elevator branch must not unlock «Осмотреться» / birthday.");

            Debug.Log("[MaratGame] Morning branches smoke test passed. Decisions=" + state.DecisionsCount +
                      ", Respect=" + state.Stats.Respect + ", Calm=" + state.Stats.Calm + ", Chaos=" + state.Stats.Chaos);
        }

        static StoryChoice MakeChoice(
            string label,
            string targetNodeId,
            StatChangeEntry[] statChanges = null,
            string[] flagsToSet = null)
        {
            return new StoryChoice
            {
                label = label,
                targetNodeId = targetNodeId,
                statChanges = statChanges ?? System.Array.Empty<StatChangeEntry>(),
                flagsToSet = flagsToSet ?? System.Array.Empty<string>()
            };
        }
    }
}
