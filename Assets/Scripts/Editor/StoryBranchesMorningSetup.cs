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
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";

        [MenuItem("MaratGame/Story/Setup Step 10 (Morning Branches)")]
        public static void SetupStep10()
        {
            CreateBranchesMorningContent();
            GameUiBranchesSetup.EnsureBranchPortraitsOnGameScene();
            Debug.Log("[MaratGame] Step 10: morning branches ready. Play: hall_hub → ←↑→ → choices → back.");
        }

        [MenuItem("MaratGame/Story/Create Morning Branches Content")]
        public static void CreateBranchesMorningContent()
        {
            StoryMvpSetup.EnsureStoryDataFolder();
            StoryNavigationSetup.CreateNavigationStoryContent();

            var canteen = StoryMvpSetup.LoadOrCreateNode(NodeCanteenPath);
            canteen.bodyText =
                "Завтрак на подносе, запах свежего кофе — в столовой уже шумно, как в любой понедельник.";
            canteen.choices = new[]
            {
                new StoryChoice
                {
                    label = "Сесть к Козлихину",
                    targetNodeId = NavigationBar.HubNodeId,
                    statChanges = new[] { new StatChangeEntry { stat = StatType.Respect, delta = 5 } },
                    flagsToSet = new[] { MorningBranchFlags.MetKozlikhin }
                },
                new StoryChoice
                {
                    label = "Взять кофе с собой",
                    targetNodeId = NavigationBar.HubNodeId,
                    statChanges = new[] { new StatChangeEntry { stat = StatType.Calm, delta = 5 } }
                }
            };

            var elevator = StoryMvpSetup.LoadOrCreateNode(NodeElevatorPath);
            elevator.bodyText =
                "Лифтовая зона гудит: один кабинет только что уехал наверх. Ноздриков ждёт у кнопки вызова.";
            elevator.choices = new[]
            {
                new StoryChoice
                {
                    label = "Заговорить с Ноздриковым",
                    targetNodeId = NavigationBar.HubNodeId,
                    flagsToSet = new[] { MorningBranchFlags.MetNozdrikov }
                },
                new StoryChoice
                {
                    label = "Читать сообщения",
                    targetNodeId = NavigationBar.HubNodeId,
                    statChanges = new[] { new StatChangeEntry { stat = StatType.Calm, delta = 3 } }
                }
            };

            var cabinet = StoryMvpSetup.LoadOrCreateNode(NodeCabinetPath);
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

            var alevtina = StoryMvpSetup.LoadOrCreateNode(NodeAlevtinaPath);
            alevtina.id = MorningBranchNodes.AlevtinaTeaseNodeId;
            alevtina.locationId = "cabinet";
            alevtina.timeDisplay = "07:58";
            alevtina.chapterLabel = "ГЛАВА 1 · УТРО";
            alevtina.speaker = "Алевтина";
            alevtina.portraitCharacterId = CharacterIds.Alevtina;
            alevtina.bodyText =
                "— Ну что, именинник, кофе уже пил или сразу к тосту побежишь? " +
                "Письма подождут, день рождения — нет.";
            alevtina.onEnterEffects = new List<StatChangeEntry>();
            alevtina.choices = new[]
            {
                new StoryChoice
                {
                    label = "В холл",
                    targetNodeId = NavigationBar.HubNodeId
                }
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
            database.nodes = new List<StoryNodeData>
            {
                hallMorning,
                hallHub,
                canteen,
                cabinet,
                elevator,
                alevtina
            };

            EditorUtility.SetDirty(canteen);
            EditorUtility.SetDirty(elevator);
            EditorUtility.SetDirty(cabinet);
            EditorUtility.SetDirty(alevtina);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaratGame] Morning branches: canteen/elevator choices, cabinet → " +
                      MorningBranchNodes.AlevtinaTeaseNodeId + " → hall_hub.");
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
            if (!state.Flags.HasFlag(MorningBranchFlags.MetKozlikhin) || state.Stats.Respect != 55)
                Debug.LogError("[MaratGame] Canteen/Kozlikhin: expected met_kozlikhin and Respect 55.");
            if (engine.CurrentNode.id != NavigationBar.HubNodeId)
                Debug.LogError("[MaratGame] Canteen should return to hall_hub.");

            engine.LoadNode(NavigationBar.ElevatorEntryNodeId);
            engine.SelectChoice(0);
            if (!state.Flags.HasFlag(MorningBranchFlags.MetNozdrikov))
                Debug.LogError("[MaratGame] Elevator: expected met_nozdrikov.");

            engine.LoadNode(NavigationBar.CabinetEntryNodeId);
            engine.SelectChoice(0);
            if (engine.CurrentNode.id != MorningBranchNodes.AlevtinaTeaseNodeId)
                Debug.LogError("[MaratGame] Cabinet should go to alevtina_tease.");

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != NavigationBar.HubNodeId)
                Debug.LogError("[MaratGame] Alevtina tease should return to hall_hub.");

            Debug.Log("[MaratGame] Morning branches smoke test passed. Decisions=" + state.DecisionsCount);
        }
    }
}
