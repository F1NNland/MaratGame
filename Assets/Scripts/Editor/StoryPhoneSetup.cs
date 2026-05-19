using MaratGame.Data;
using MaratGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 08: телефон, hall_hub, контент сообщений.
    /// </summary>
    static class StoryPhoneSetup
    {
        const string DataFolder = "Assets/Data/Story";
        const string NodeMorningPath = DataFolder + "/Node_HallMorning.asset";
        const string NodeHubPath = DataFolder + "/Node_HallHub.asset";
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";

        public const string HallMorningNodeId = PhoneUI.HallMorningNodeId;
        public const string HallHubNodeId = PhoneUI.HallHubNodeId;

        [MenuItem("MaratGame/Story/Setup Step 08 (Phone)")]
        public static void SetupStep08()
        {
            CreatePhoneStoryContent();
            GameUiPhoneSetup.EnsurePhoneUiOnGameScene();
            Debug.Log("[MaratGame] Step 08: phone + hall_hub ready. Play from Main Menu → Game.");
        }

        [MenuItem("MaratGame/Story/Create Phone Story Content")]
        public static void CreatePhoneStoryContent()
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

            var hallHub = StoryMvpSetup.LoadOrCreateNode(NodeHubPath);
            hallHub.id = HallHubNodeId;
            hallHub.locationId = "hall";
            hallHub.timeDisplay = "07:58";
            hallHub.chapterLabel = "ГЛАВА 1 · УТРО";
            hallHub.speaker = string.Empty;
            hallHub.bodyText = "Телефон стихает. Куда дальше?";
            hallHub.onEnterEffects = new System.Collections.Generic.List<StatChangeEntry>();
            hallHub.choices = System.Array.Empty<StoryChoice>();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<StoryDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.startNodeId = HallMorningNodeId;
            database.nodes = new System.Collections.Generic.List<StoryNodeData> { hallMorning, hallHub };

            EditorUtility.SetDirty(hallMorning);
            EditorUtility.SetDirty(hallHub);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaratGame] Phone story: start=" + HallMorningNodeId + " → overlay → " + HallHubNodeId);
        }
    }
}
