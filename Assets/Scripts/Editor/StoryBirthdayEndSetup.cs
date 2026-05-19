using System.Collections.Generic;
using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using MaratGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 11: birthday_scene + ending_screen.
    /// </summary>
    static class StoryBirthdayEndSetup
    {
        const string DataFolder = "Assets/Data/Story";
        const string NodeBirthdayPath = DataFolder + "/Node_BirthdayScene.asset";
        const string NodeEndingPath = DataFolder + "/Node_EndingScreen.asset";
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";

        [MenuItem("MaratGame/Story/Setup Step 11 (Birthday & End)")]
        public static void SetupStep11()
        {
            CreateBirthdayEndContent();
            GameUiBirthdayEndSetup.EnsureBirthdayEndUiOnGameScene();
            BuildSetup.ConfigureWindowsBuild();
            Debug.Log("[MaratGame] Step 11: birthday → ending → replay. Build: Builds/Windows/MaratGame.exe");
        }

        [MenuItem("MaratGame/Story/Create Birthday & End Content")]
        public static void CreateBirthdayEndContent()
        {
            StoryBranchesMorningSetup.CreateBranchesMorningContent();

            var birthday = StoryMvpSetup.LoadOrCreateNode(NodeBirthdayPath);
            birthday.id = BirthdayEndNodes.BirthdaySceneNodeId;
            birthday.locationId = "hall";
            birthday.timeDisplay = "07:58";
            birthday.chapterLabel = "ГЛАВА 1 · УТРО";
            birthday.speaker = string.Empty;
            birthday.bodyText =
                "С днём рождения, Марат!\n\n«Наверное, ради этого всё и работает.»";
            birthday.portraitCharacterId = string.Empty;
            birthday.onEnterEffects = new List<StatChangeEntry>();
            birthday.choices = new[]
            {
                new StoryChoice
                {
                    label = "Далее",
                    targetNodeId = BirthdayEndNodes.EndingScreenNodeId
                }
            };

            var ending = StoryMvpSetup.LoadOrCreateNode(NodeEndingPath);
            ending.id = BirthdayEndNodes.EndingScreenNodeId;
            ending.locationId = "hall";
            ending.timeDisplay = "07:58";
            ending.chapterLabel = "ГЛАВА 1 · УТРО";
            ending.speaker = string.Empty;
            ending.bodyText = string.Empty;
            ending.onEnterEffects = new List<StatChangeEntry>();
            ending.choices = System.Array.Empty<StoryChoice>();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            if (!ContainsNode(database, birthday))
                database.nodes.Add(birthday);
            if (!ContainsNode(database, ending))
                database.nodes.Add(ending);

            EditorUtility.SetDirty(birthday);
            EditorUtility.SetDirty(ending);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaratGame] Birthday end: hub ● → " + BirthdayEndNodes.BirthdaySceneNodeId +
                      " → " + BirthdayEndNodes.EndingScreenNodeId);
        }

        static bool ContainsNode(StoryDatabase database, StoryNodeData node)
        {
            foreach (var entry in database.nodes)
            {
                if (entry != null && entry.id == node.id)
                    return true;
            }

            return false;
        }

        [MenuItem("MaratGame/Story/Run Birthday End Smoke Test")]
        public static void RunBirthdayEndSmokeTest()
        {
            CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            var state = GameState.Instance;
            state.Reset();

            var engine = new StoryEngine(state, database);
            engine.LoadNode(NavigationBar.HubNodeId);

            state.RecordDecision();
            state.RecordDecision();
            if (state.DecisionsCount < BirthdayEndNodes.MinDecisionsForBirthday)
                Debug.LogError("[MaratGame] Expected DecisionsCount >= 2 for birthday.");

            engine.LoadNode(BirthdayEndNodes.BirthdaySceneNodeId);
            if (engine.CurrentNode.id != BirthdayEndNodes.BirthdaySceneNodeId)
                Debug.LogError("[MaratGame] Expected birthday_scene.");

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != BirthdayEndNodes.EndingScreenNodeId)
                Debug.LogError("[MaratGame] Expected ending_screen after «Далее».");

            Debug.Log("[MaratGame] Birthday end smoke test passed. Respect=" + state.Stats.Respect +
                      "% Calm=" + state.Stats.Calm + "% Decisions=" + state.DecisionsCount);
        }
    }
}
