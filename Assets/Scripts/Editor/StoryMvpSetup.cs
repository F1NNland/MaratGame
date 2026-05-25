using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace MaratGame.Editor
{
    static class StoryMvpSetup
    {
        const string DataFolder = "Assets/Data/Story";
        const string NodeHallPath = DataFolder + "/Node_TestHall.asset";
        const string NodeHall2Path = DataFolder + "/Node_TestHall2.asset";
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";
        const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("MaratGame/Story/Create Test Content (Step 02)")]
        public static void CreateTestContent()
        {
            EnsureStoryDataFolder();

            var hallIntro = LoadOrCreateNode(NodeHallPath);
            hallIntro.id = "hall_intro";
            hallIntro.locationId = "hall";
            hallIntro.timeDisplay = "07:58";
            hallIntro.dayBlock = DayBlock.Morning;
            hallIntro.chapterLabel = "Глава 1 — тест";
            hallIntro.speaker = "Марат";
            hallIntro.bodyText = "Холл банка. День рождения только начинается (тестовый узел).";
            hallIntro.onEnterEffects = new System.Collections.Generic.List<StatChangeEntry>();
            hallIntro.choices = new[]
            {
                new StoryChoice
                {
                    label = "Далее",
                    targetNodeId = "hall_intro_2",
                    statChanges = new[] { new StatChangeEntry { stat = StatType.Respect, delta = 5 } },
                    flagsToSet = new[] { "hall_intro_seen" }
                },
                new StoryChoice
                {
                    label = "Только с пропуском",
                    targetNodeId = "hall_intro_2",
                    requiredFlags = new[] { "staff_pass" },
                    unavailableReason = "Нужен пропуск сотрудника."
                }
            };

            var hallIntro2 = LoadOrCreateNode(NodeHall2Path);
            hallIntro2.id = "hall_intro_2";
            hallIntro2.locationId = "hall";
            hallIntro2.timeDisplay = "07:59";
            hallIntro2.dayBlock = DayBlock.BeforeMeeting;
            hallIntro2.chapterLabel = "Глава 1 — тест";
            hallIntro2.speaker = "Марат";
            hallIntro2.bodyText = "Второй тестовый узел. Выборов нет — сработает OnNodeComplete.";
            hallIntro2.requiredFlags = new[] { "hall_intro_seen" };
            hallIntro2.mediaSlot = MediaSlotType.Image;
            hallIntro2.mediaPath = "Assets/Art/placeholder/hall_intro_2.png";
            hallIntro2.onEnterEffects = new System.Collections.Generic.List<StatChangeEntry>
            {
                new StatChangeEntry { stat = StatType.Calm, delta = -3 },
                new StatChangeEntry { stat = StatType.Chaos, delta = 7 }
            };
            hallIntro2.choices = System.Array.Empty<StoryChoice>();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<StoryDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.startNodeId = "hall_intro";
            database.nodes = new System.Collections.Generic.List<StoryNodeData> { hallIntro, hallIntro2 };

            EditorUtility.SetDirty(hallIntro);
            EditorUtility.SetDirty(hallIntro2);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaratGame] Test story content created at " + DataFolder);
        }

        [MenuItem("MaratGame/Story/Setup SampleScene StoryRunner (Step 02)")]
        public static void SetupSampleScene()
        {
            CreateTestContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing. Run Create Test Content first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
            var runner = Object.FindFirstObjectByType<StoryRunner>();
            if (runner == null)
            {
                var go = new GameObject("StoryRunner");
                runner = go.AddComponent<StoryRunner>();
            }

            var so = new SerializedObject(runner);
            so.FindProperty("database").objectReferenceValue = database;
            so.FindProperty("resetStateOnStart").boolValue = true;
            so.FindProperty("logNodeChanges").boolValue = true;
            so.FindProperty("showDebugGui").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] SampleScene: StoryRunner configured with StoryDatabase_Test.", runner);
        }

        [MenuItem("MaratGame/Story/Run StoryRunner Smoke Test")]
        public static void RunSmokeTest()
        {
            CreateTestContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Fail("StoryDatabase_Test not found.");
                return;
            }

            var state = GameState.Instance;
            state.Reset();

            var engine = new StoryEngine(state, database);
            StoryNodeData lastNode = null;
            var nodeChangeCount = 0;
            var nodeCompleteFired = false;

            engine.OnNodeChanged += n =>
            {
                lastNode = n;
                nodeChangeCount++;
            };
            engine.OnNodeComplete += _ => nodeCompleteFired = true;

            engine.LoadNode("hall_intro");

            if (lastNode == null || lastNode.id != "hall_intro")
                Fail($"Expected hall_intro, got {lastNode?.id}");

            if (state.CurrentTime != "07:58" || state.CurrentLocationId != "hall")
                Fail($"After hall_intro: time={state.CurrentTime}, loc={state.CurrentLocationId}");

            var initialChoices = engine.GetChoiceAvailability();
            if (initialChoices.Length != 2)
                Fail($"Expected 2 choices, got {initialChoices.Length}");

            if (initialChoices[1].IsAvailable)
                Fail("Second choice should be unavailable without staff_pass.");

            if (initialChoices[1].Reason != "Нужен пропуск сотрудника.")
                Fail($"Unavailable reason mismatch: {initialChoices[1].Reason}");

            var loadLockedDirectlyFailed = false;
            try
            {
                engine.LoadNode("hall_intro_2");
            }
            catch (System.InvalidOperationException)
            {
                loadLockedDirectlyFailed = true;
            }

            if (!loadLockedDirectlyFailed)
                Fail("Expected hall_intro_2 to be unavailable before flag hall_intro_seen.");

            engine.SelectChoice(0);

            if (state.Stats.Respect != 55)
                Fail($"Respect after choice +5: {state.Stats.Respect}");

            if (!state.Flags.HasFlag("hall_intro_seen"))
                Fail("Flag hall_intro_seen not set");

            if (state.DecisionsCount != 1)
                Fail($"DecisionsCount: {state.DecisionsCount}");

            if (lastNode.id != "hall_intro_2")
                Fail($"Expected hall_intro_2, got {lastNode.id}");

            if (state.Stats.Calm != 57)
                Fail($"Calm after onEnter -3: {state.Stats.Calm}");

            if (state.Stats.Chaos != 7)
                Fail($"Chaos after onEnter +7: {state.Stats.Chaos}");

            if (state.CurrentDayBlock != DayBlock.BeforeMeeting)
                Fail($"Expected day block BeforeMeeting, got {state.CurrentDayBlock}");

            if (lastNode.mediaSlot != MediaSlotType.Image || string.IsNullOrWhiteSpace(lastNode.mediaPath))
                Fail("Expected mediaSlot image and non-empty mediaPath on hall_intro_2.");

            if (!nodeCompleteFired)
                Fail("OnNodeComplete did not fire for hall_intro_2");

            if (nodeChangeCount != 2)
                Fail($"OnNodeChanged count: {nodeChangeCount}");

            Debug.Log("[MaratGame] StoryRunner smoke test passed.");
        }

        internal static StoryNodeData LoadOrCreateNode(string path)
        {
            var node = AssetDatabase.LoadAssetAtPath<StoryNodeData>(path);
            if (node != null)
                return node;

            node = ScriptableObject.CreateInstance<StoryNodeData>();
            AssetDatabase.CreateAsset(node, path);
            return node;
        }

        internal static void EnsureStoryDataFolder()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(DataFolder);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }

        static void Fail(string message) => Debug.LogError("[MaratGame] StoryRunner smoke test FAILED: " + message);
    }
}
