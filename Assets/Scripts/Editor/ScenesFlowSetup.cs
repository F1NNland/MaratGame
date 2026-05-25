using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using MaratGame.Presentation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// Шаг 03 MVP: Boot → MainMenu → Game и Build Settings.
    /// </summary>
    static class ScenesFlowSetup
    {
        const string ScenesFolder = "Assets/Scenes";
        const string BootPath = ScenesFolder + "/Boot.unity";
        const string MainMenuPath = ScenesFolder + "/MainMenu.unity";
        const string GamePath = ScenesFolder + "/Game.unity";
        const string DatabasePath = "Assets/Data/Story/StoryDatabase_Test.asset";

        [MenuItem("MaratGame/Scenes/Setup Step 03 (Boot, MainMenu, Game)")]
        public static void SetupStep03()
        {
            StoryMvpSetup.CreateTestContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing. Run Story/Create Test Content first.");
                return;
            }

            EnsureScenesFolder();
            CreateBootScene();
            CreateMainMenuScene();
            CreateGameScene(database);
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            Debug.Log("[MaratGame] Step 03 scenes ready: Boot → MainMenu → Game. Set Boot as first scene and press Play.");
        }

        static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder(ScenesFolder))
                AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var boot = new GameObject("BootLoader");
            boot.AddComponent<BootLoader>();
            SaveScene(scene, BootPath);
        }

        static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var eventSystem = new GameObject("EventSystem");
            AddUiEventSystem();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);
            // Scaler + stretch root configured in EnsureCanvasSupportsProceduralImage

            var menuRoot = new GameObject("MainMenuRoot");
            menuRoot.transform.SetParent(canvasGo.transform, false);
            var menuRect = menuRoot.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;

            var menuGroup = menuRoot.AddComponent<CanvasGroup>();
            menuGroup.alpha = 1f;

            var bgGo = new GameObject("ScreenBackground");
            bgGo.transform.SetParent(canvasGo.transform, false);
            bgGo.transform.SetAsFirstSibling();
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = UiStyle.ScreenBackground;
            bgImage.raycastTarget = false;

            var panelGo = new GameObject("TitlePanel");
            panelGo.transform.SetParent(menuRoot.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.58f);
            panelRect.anchorMax = new Vector2(0.5f, 0.78f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520, 200);
            UiKitFactory.AddRoundedPanel(panelGo, UiStyle.PanelBackground, UiStyle.PanelCornerRadius);

            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.62f);
            titleRect.anchorMax = new Vector2(0.5f, 0.62f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(480, 100);
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.font = TmpUiFactory.DefaultFont;
            title.text = "Обычный\nрабочий день";
            title.fontSize = 40;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = UiStyle.TextLight;

            var subtitleGo = new GameObject("SubtitleText");
            subtitleGo.transform.SetParent(panelGo.transform, false);
            var subtitleRect = subtitleGo.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0.28f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0.28f);
            subtitleRect.pivot = new Vector2(0.5f, 0.5f);
            subtitleRect.sizeDelta = new Vector2(480, 36);
            var subtitle = subtitleGo.AddComponent<TextMeshProUGUI>();
            subtitle.font = TmpUiFactory.DefaultFont;
            subtitle.text = "Офисная бродилка · Банк";
            subtitle.fontSize = 20;
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.color = UiStyle.TextMuted;

            var controller = menuRoot.AddComponent<MainMenuController>();

            var buttonGo = new GameObject("StartGameButton");
            buttonGo.transform.SetParent(menuRoot.transform, false);
            var buttonRect = buttonGo.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.36f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.36f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(300, 52);
            var graphic = UiKitFactory.AddRoundedButtonGraphic(buttonGo, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = graphic;
            buttonGo.AddComponent<CanvasGroup>();

            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.font = TmpUiFactory.DefaultFont;
            label.text = "НАЧАТЬ ИГРУ";
            label.fontSize = 22;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = UiStyle.TextLight;
            label.raycastTarget = false;

            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("menuGroup").objectReferenceValue = menuGroup;
            controllerSo.FindProperty("titleRect").objectReferenceValue = titleRect;
            controllerSo.FindProperty("startButtonRect").objectReferenceValue = buttonRect;
            controllerSo.FindProperty("startButton").objectReferenceValue = button;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(button.onClick, controller.OnStartGameClicked);

            SaveScene(scene, MainMenuPath);
        }

        static void CreateGameScene(StoryDatabase database)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var systems = new GameObject("GameSystems");
            var runner = systems.AddComponent<StoryRunner>();
            systems.AddComponent<UiInputBootstrap>();
            var bootstrap = systems.AddComponent<GameBootstrap>();

            var runnerSo = new SerializedObject(runner);
            var dbProp = runnerSo.FindProperty("database");
            dbProp.objectReferenceValue = database;
            runnerSo.FindProperty("autoStartOnPlay").boolValue = false;
            runnerSo.FindProperty("resetStateOnStart").boolValue = false;
            runnerSo.FindProperty("logNodeChanges").boolValue = true;
            runnerSo.FindProperty("showDebugGui").boolValue = true;
            if (!runnerSo.ApplyModifiedPropertiesWithoutUndo() || dbProp.objectReferenceValue == null)
                Debug.LogError("[MaratGame] Failed to assign StoryDatabase on Game scene StoryRunner.");

            EditorUtility.SetDirty(runner);

            var bootstrapSo = new SerializedObject(bootstrap);
            bootstrapSo.FindProperty("storyRunner").objectReferenceValue = runner;
            bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

            AddUiEventSystem();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);

            SaveScene(scene, GamePath);
        }

        static void AddUiEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        static void SaveScene(Scene scene, string path)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        [MenuItem("MaratGame/Build/Refresh Build Settings Scenes")]
        public static void ConfigureBuildSettingsFromMenu() => ConfigureBuildSettings();

        public static void ConfigureBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(BootPath, true),
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(GamePath, true)
            };

            EditorBuildSettings.scenes = scenes;
            Debug.Log("[MaratGame] Build Settings: Boot (0), MainMenu (1), Game (2).");
        }
    }
}
