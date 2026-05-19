using MaratGame.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 07: узел hall_morning, старт игры, фон холла на сцене Game.
    /// </summary>
    static class StoryHallMorningSetup
    {
        const string DataFolder = "Assets/Data/Story";
        const string NodeMorningPath = DataFolder + "/Node_HallMorning.asset";
        const string NodeHubPath = DataFolder + "/Node_HallHub.asset";
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";

        public const string HallMorningNodeId = "hall_morning";

        [MenuItem("MaratGame/Story/Setup Step 07 (Hall Morning)")]
        public static void SetupStep07()
        {
            CreateHallMorningContent();
            GameUiShellSetup.EnsureHallBackgroundOnGameScene();
            Debug.Log("[MaratGame] Step 07: hall_morning ready. Start game from Main Menu → Game.");
        }

        [MenuItem("MaratGame/Story/Create Hall Morning Content")]
        public static void CreateHallMorningContent()
        {
            StoryMvpSetup.EnsureStoryDataFolder();

            var hallMorning = StoryMvpSetup.LoadOrCreateNode(NodeMorningPath);
            hallMorning.id = HallMorningNodeId;
            hallMorning.locationId = "hall";
            hallMorning.timeDisplay = "07:58";
            hallMorning.chapterLabel = "ГЛАВА 1 · УТРО";
            hallMorning.speaker = string.Empty;
            hallMorning.bodyText =
                "07:58. Турникеты, пропуск, холл банка — как каждый понедельник. " +
                "Только сегодня день рождения, и телефон в кармане уже не молчит.";
            hallMorning.onEnterEffects = new System.Collections.Generic.List<StatChangeEntry>();
            hallMorning.choices = System.Array.Empty<StoryChoice>();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<StoryDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.startNodeId = HallMorningNodeId;
            var nodes = new System.Collections.Generic.List<StoryNodeData> { hallMorning };
            var hallHub = AssetDatabase.LoadAssetAtPath<StoryNodeData>(NodeHubPath);
            if (hallHub != null)
                nodes.Add(hallHub);

            database.nodes = nodes;

            EditorUtility.SetDirty(hallMorning);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaratGame] Hall morning story: startNodeId=" + HallMorningNodeId);
        }
    }
}
