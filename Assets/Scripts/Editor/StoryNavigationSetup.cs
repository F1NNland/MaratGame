using System.Collections.Generic;
using MaratGame.Data;
using MaratGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 09: hall_hub + заглушки canteen / cabinet / elevator.
    /// </summary>
    static class StoryNavigationSetup
    {
        const string DataFolder = "Assets/Data/Story";
        const string NodeMorningPath = DataFolder + "/Node_HallMorning.asset";
        const string NodeHubPath = DataFolder + "/Node_HallHub.asset";
        const string NodeCanteenPath = DataFolder + "/Node_CanteenEntry.asset";
        const string NodeCabinetPath = DataFolder + "/Node_CabinetEntry.asset";
        const string NodeElevatorPath = DataFolder + "/Node_ElevatorEntry.asset";
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";

        [MenuItem("MaratGame/Story/Setup Step 09 (Navigation Hub)")]
        public static void SetupStep09()
        {
            CreateNavigationStoryContent();
            GameUiNavigationSetup.EnsureNavigationUiOnGameScene();
            Debug.Log("[MaratGame] Step 09: navigation hub + entry stubs ready. Play: phone → hall_hub → arrows.");
        }

        [MenuItem("MaratGame/Story/Create Navigation Story Content")]
        public static void CreateNavigationStoryContent()
        {
            StoryMvpSetup.EnsureStoryDataFolder();

            var hallMorning = StoryMvpSetup.LoadOrCreateNode(NodeMorningPath);
            hallMorning.id = PhoneUI.HallMorningNodeId;
            hallMorning.locationId = "hall";
            hallMorning.timeDisplay = "07:58";
            hallMorning.chapterLabel = "ГЛАВА 1 · УТРО";
            hallMorning.speaker = string.Empty;
            hallMorning.bodyText =
                "07:58. Турникеты, пропуск, холл банка — как каждый понедельник. " +
                "Только сегодня день рождения, и телефон в кармане уже не молчит.";
            hallMorning.onEnterEffects = new List<StatChangeEntry>();
            hallMorning.choices = System.Array.Empty<StoryChoice>();

            var hallHub = StoryMvpSetup.LoadOrCreateNode(NodeHubPath);
            hallHub.id = NavigationBar.HubNodeId;
            hallHub.locationId = "hall";
            hallHub.timeDisplay = "07:58";
            hallHub.chapterLabel = "ГЛАВА 1 · УТРО";
            hallHub.speaker = string.Empty;
            hallHub.bodyText = "Телефон стихает. Куда дальше?";
            hallHub.onEnterEffects = new List<StatChangeEntry>();
            hallHub.choices = System.Array.Empty<StoryChoice>();

            var canteen = StoryMvpSetup.LoadOrCreateNode(NodeCanteenPath);
            canteen.id = NavigationBar.CanteenEntryNodeId;
            canteen.locationId = "canteen";
            canteen.timeDisplay = "07:58";
            canteen.chapterLabel = "ГЛАВА 1 · УТРО";
            canteen.speaker = string.Empty;
            canteen.bodyText =
                "В столовой пахнет свежей выпечкой. Очередь за кофе уже выстроилась — " +
                "типичное утро понедельника.";
            canteen.onEnterEffects = new List<StatChangeEntry>();
            canteen.choices = new[]
            {
                new StoryChoice
                {
                    label = "В холл",
                    targetNodeId = NavigationBar.HubNodeId
                }
            };

            var cabinet = StoryMvpSetup.LoadOrCreateNode(NodeCabinetPath);
            cabinet.id = NavigationBar.CabinetEntryNodeId;
            cabinet.locationId = "cabinet";
            cabinet.timeDisplay = "07:58";
            cabinet.chapterLabel = "ГЛАВА 1 · УТРО";
            cabinet.speaker = string.Empty;
            cabinet.bodyText =
                "Дверь кабинета приоткрыта: внутри щёлкает клавиатура — кто-то уже начал понедельник раньше вас.";
            cabinet.onEnterEffects = new List<StatChangeEntry>();
            cabinet.choices = new[]
            {
                new StoryChoice
                {
                    label = "В холл",
                    targetNodeId = NavigationBar.HubNodeId
                }
            };

            var elevator = StoryMvpSetup.LoadOrCreateNode(NodeElevatorPath);
            elevator.id = NavigationBar.ElevatorEntryNodeId;
            elevator.locationId = "elevator";
            elevator.timeDisplay = "07:58";
            elevator.chapterLabel = "ГЛАВА 1 · УТРО";
            elevator.speaker = string.Empty;
            elevator.bodyText =
                "Лифтовая зона гудит: один кабинет только что уехал наверх, на табло мигает «2».";
            elevator.onEnterEffects = new List<StatChangeEntry>();
            elevator.choices = new[]
            {
                new StoryChoice
                {
                    label = "В холл",
                    targetNodeId = NavigationBar.HubNodeId
                }
            };

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
                elevator
            };

            EditorUtility.SetDirty(hallMorning);
            EditorUtility.SetDirty(hallHub);
            EditorUtility.SetDirty(canteen);
            EditorUtility.SetDirty(cabinet);
            EditorUtility.SetDirty(elevator);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaratGame] Navigation story: " + NavigationBar.HubNodeId + " → ←↑→ entries, «В холл» back.");
        }

        [MenuItem("MaratGame/Story/Run Navigation Smoke Test")]
        public static void RunNavigationSmokeTest()
        {
            CreateNavigationStoryContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            var state = MaratGame.Core.GameState.Instance;
            state.Reset();

            var engine = new MaratGame.Narrative.StoryEngine(state, database);
            engine.LoadNode(NavigationBar.HubNodeId);

            engine.LoadNode(NavigationBar.CanteenEntryNodeId);
            if (state.CurrentLocationId != "canteen")
                Debug.LogError("[MaratGame] Expected canteen location.");

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != NavigationBar.HubNodeId)
                Debug.LogError("[MaratGame] Expected hall_hub after «В холл».");

            engine.LoadNode(NavigationBar.CabinetEntryNodeId);
            engine.SelectChoice(0);
            engine.LoadNode(NavigationBar.ElevatorEntryNodeId);
            engine.SelectChoice(0);

            Debug.Log("[MaratGame] Navigation smoke test passed.");
        }
    }
}
