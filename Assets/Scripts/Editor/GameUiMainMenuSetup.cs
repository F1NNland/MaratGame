using MaratGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// Шаг 06 MVP: экран «НАЧАТЬ ИГРУ» как веб-MVP (docs/mvp/step-06-main-menu.md).
    /// </summary>
    static class GameUiMainMenuSetup
    {
        const string MainMenuPath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("MaratGame/UI/Setup Step 06 (Main Menu)")]
        public static void SetupStep06()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MaratGame] MainMenu: Canvas not found.");
                return;
            }

            UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);
            EnsureScreenBackground(canvas.transform);

            var menuRoot = GameObject.Find("MainMenuRoot");
            if (menuRoot == null)
            {
                Debug.LogError("[MaratGame] MainMenu: MainMenuRoot not found.");
                return;
            }

            RemoveLegacyDebugLoader(menuRoot);

            var menuGroup = menuRoot.GetComponent<CanvasGroup>();
            if (menuGroup == null)
                menuGroup = menuRoot.AddComponent<CanvasGroup>();
            menuGroup.alpha = 1f;
            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;

            var titleBlock = EnsureTitleBlock(menuRoot.transform);
            var startButton = EnsureStartButton(menuRoot.transform);

            var controller = menuRoot.GetComponent<MainMenuController>();
            if (controller == null)
                controller = menuRoot.AddComponent<MainMenuController>();

            WireController(controller, menuGroup, titleBlock.titleRect, startButton.rect, startButton.button);
            WireStartButton(startButton.button, controller);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Step 06: Main menu ready (НАЧАТЬ ИГРУ → Game).");
        }

        static void RemoveLegacyDebugLoader(GameObject menuRoot)
        {
            var loaders = menuRoot.GetComponents<Component>();
            foreach (var c in loaders)
            {
                if (c == null)
                    continue;
                if (c.GetType().Name == "MainMenuDebugLoad")
                    Object.DestroyImmediate(c);
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(menuRoot);
        }

        static void EnsureScreenBackground(Transform canvas)
        {
            var existing = canvas.Find("ScreenBackground");
            if (existing != null)
                return;

            var bgGo = new GameObject("ScreenBackground", typeof(RectTransform));
            bgGo.transform.SetParent(canvas, false);
            bgGo.transform.SetAsFirstSibling();

            var rect = bgGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = bgGo.AddComponent<Image>();
            image.color = UiStyle.ScreenBackground;
            image.raycastTarget = false;
        }

        static (RectTransform titleRect, TextMeshProUGUI subtitle) EnsureTitleBlock(Transform menuRoot)
        {
            var panelGo = FindChild(menuRoot, "TitlePanel");
            if (panelGo == null)
            {
                panelGo = new GameObject("TitlePanel", typeof(RectTransform));
                panelGo.transform.SetParent(menuRoot, false);
            }

            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.58f);
            panelRect.anchorMax = new Vector2(0.5f, 0.78f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520, 200);
            panelRect.anchoredPosition = Vector2.zero;

            UiKitFactory.AddRoundedPanel(panelGo, UiStyle.PanelBackground, UiStyle.PanelCornerRadius);

            var titleGo = FindChild(panelGo.transform, "TitleText");
            if (titleGo == null)
            {
                titleGo = new GameObject("TitleText", typeof(RectTransform));
                titleGo.transform.SetParent(panelGo.transform, false);
            }

            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.62f);
            titleRect.anchorMax = new Vector2(0.5f, 0.62f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(480, 100);
            titleRect.anchoredPosition = Vector2.zero;

            var title = titleGo.GetComponent<TextMeshProUGUI>();
            if (title == null)
                title = titleGo.AddComponent<TextMeshProUGUI>();
            title.font = TmpUiFactory.DefaultFont;
            title.text = "Обычный\nрабочий день";
            title.fontSize = 40;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = UiStyle.TextLight;
            title.textWrappingMode = TextWrappingModes.Normal;

            var subtitleGo = FindChild(panelGo.transform, "SubtitleText");
            if (subtitleGo == null)
            {
                subtitleGo = new GameObject("SubtitleText", typeof(RectTransform));
                subtitleGo.transform.SetParent(panelGo.transform, false);
            }

            var subtitleRect = subtitleGo.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0.28f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0.28f);
            subtitleRect.pivot = new Vector2(0.5f, 0.5f);
            subtitleRect.sizeDelta = new Vector2(480, 36);
            subtitleRect.anchoredPosition = Vector2.zero;

            var subtitle = subtitleGo.GetComponent<TextMeshProUGUI>();
            if (subtitle == null)
                subtitle = subtitleGo.AddComponent<TextMeshProUGUI>();
            subtitle.font = TmpUiFactory.DefaultFont;
            subtitle.text = "Офисная бродилка · Банк";
            subtitle.fontSize = 20;
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.color = UiStyle.TextMuted;
            subtitle.textWrappingMode = TextWrappingModes.Normal;

            var legacyTitle = menuRoot.Find("TitleText");
            if (legacyTitle != null && legacyTitle.parent == menuRoot)
                Object.DestroyImmediate(legacyTitle.gameObject);

            return (titleRect, subtitle);
        }

        static (RectTransform rect, Button button) EnsureStartButton(Transform menuRoot)
        {
            var buttonGo = FindChild(menuRoot, "StartGameButton");
            if (buttonGo == null)
                buttonGo = FindChild(menuRoot, "DebugLoadGameButton");

            if (buttonGo == null)
            {
                buttonGo = new GameObject("StartGameButton", typeof(RectTransform));
                buttonGo.transform.SetParent(menuRoot, false);
            }
            else
            {
                buttonGo.name = "StartGameButton";
            }

            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.36f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.36f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(300, 52);
            buttonRect.anchoredPosition = Vector2.zero;

            var graphic = UiKitFactory.AddRoundedButtonGraphic(buttonGo, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var button = buttonGo.GetComponent<Button>();
            if (button == null)
                button = buttonGo.AddComponent<Button>();
            button.targetGraphic = graphic;

            var labelGo = FindChild(buttonGo.transform, "Text");
            if (labelGo == null)
            {
                labelGo = new GameObject("Text", typeof(RectTransform));
                labelGo.transform.SetParent(buttonGo.transform, false);
            }

            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelGo.GetComponent<TextMeshProUGUI>();
            if (label == null)
                label = labelGo.AddComponent<TextMeshProUGUI>();
            label.font = TmpUiFactory.DefaultFont;
            label.text = "НАЧАТЬ ИГРУ";
            label.fontSize = 22;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = UiStyle.TextLight;
            label.raycastTarget = false;

            if (buttonGo.GetComponent<CanvasGroup>() == null)
                buttonGo.AddComponent<CanvasGroup>();

            return (buttonRect, button);
        }

        static void WireController(
            MainMenuController controller,
            CanvasGroup menuGroup,
            RectTransform titleRect,
            RectTransform startButtonRect,
            Button startButton)
        {
            var so = new SerializedObject(controller);
            so.FindProperty("menuGroup").objectReferenceValue = menuGroup;
            so.FindProperty("titleRect").objectReferenceValue = titleRect;
            so.FindProperty("startButtonRect").objectReferenceValue = startButtonRect;
            so.FindProperty("startButton").objectReferenceValue = startButton;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireStartButton(Button button, MainMenuController controller)
        {
            while (button.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(button.onClick, 0);

            UnityEventTools.AddPersistentListener(button.onClick, controller.OnStartGameClicked);
        }

        static GameObject FindChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            return child != null ? child.gameObject : null;
        }
    }
}
