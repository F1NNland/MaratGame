using MaratGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 08: PhoneButton + PhoneOverlay на сцене Game.
    /// </summary>
    static class GameUiPhoneSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("MaratGame/UI/Setup Step 08 (Phone UI)")]
        public static void SetupStep08PhoneUi() => EnsurePhoneUiOnGameScene();

        public static void EnsurePhoneUiOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MaratGame] Game scene has no Canvas. Run Setup Step 04 first.");
                return;
            }

            UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);

            var existing = Object.FindFirstObjectByType<PhoneUI>();
            if (existing != null)
            {
                Debug.Log("[MaratGame] PhoneUI already on scene — skipped rebuild.");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return;
            }

            var phoneRoot = BuildPhoneUi(canvas.transform);
            var runner = Object.FindFirstObjectByType<Narrative.StoryRunner>();
            var so = new SerializedObject(phoneRoot);
            so.FindProperty("storyRunner").objectReferenceValue = runner;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Step 08 Phone UI added to Game scene.");
        }

        static PhoneUI BuildPhoneUi(Transform canvas)
        {
            var root = CreateUiObject("PhoneUI", canvas);
            var view = root.AddComponent<PhoneUI>();

            var buttonRoot = BuildPhoneButton(root.transform);
            var overlay = BuildPhoneOverlay(root.transform);

            var so = new SerializedObject(view);
            so.FindProperty("phoneButtonRoot").objectReferenceValue = buttonRoot;
            so.FindProperty("phoneButton").objectReferenceValue = buttonRoot.GetComponentInChildren<Button>();
            so.FindProperty("badgeRoot").objectReferenceValue = buttonRoot.transform.Find("Badge")?.gameObject;
            so.FindProperty("badgeText").objectReferenceValue =
                buttonRoot.transform.Find("Badge/BadgeText")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("overlayGroup").objectReferenceValue = overlay.group;
            so.FindProperty("messageListRoot").objectReferenceValue = overlay.list;
            so.FindProperty("messageItemPrefab").objectReferenceValue = overlay.messageTemplate;
            so.FindProperty("closeButton").objectReferenceValue = overlay.closeButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            buttonRoot.SetActive(false);
            overlay.group.gameObject.SetActive(false);

            return view;
        }

        static GameObject BuildPhoneButton(Transform parent)
        {
            var root = CreateUiObject("PhoneButton", parent);
            var rect = root.GetComponent<RectTransform>();
            SetAnchors(rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(72f, 72f));

            var graphic = UiKitFactory.AddRoundedButtonGraphic(root, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var button = root.AddComponent<Button>();
            button.targetGraphic = graphic;

            var icon = TmpUiFactory.CreateText(
                "Icon",
                root.transform,
                32f,
                TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(icon.gameObject);
            icon.text = "Тел";

            var badge = CreateUiObject("Badge", root.transform);
            var badgeRect = badge.GetComponent<RectTransform>();
            SetAnchors(badgeRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(28f, 28f));
            UiKitFactory.AddRoundedPanel(badge, new Color(0.85f, 0.22f, 0.18f, 1f), 14f);

            var badgeText = TmpUiFactory.CreateText(
                "BadgeText",
                badge.transform,
                16f,
                TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(badgeText.gameObject);
            badgeText.text = "5";

            return root;
        }

        static (CanvasGroup group, RectTransform list, TextMeshProUGUI messageTemplate, Button closeButton) BuildPhoneOverlay(
            Transform parent)
        {
            var root = CreateUiObject("PhoneOverlay", parent);
            StretchFull(root);
            var group = root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var dim = CreateUiObject("Dimmer", root.transform);
            StretchFull(dim);
            var dimImage = dim.AddComponent<Image>();
            dimImage.color = new Color(0f, 0f, 0f, 0.55f);
            dimImage.raycastTarget = true;

            var panel = CreateUiObject("Panel", root.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            SetAnchors(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 480f));
            UiKitFactory.AddRoundedPanel(panel, UiStyle.PanelBackground, UiStyle.PanelCornerRadius);

            var title = TmpUiFactory.CreateText(
                "Title",
                panel.transform,
                24f,
                TextAlignmentOptions.TopLeft,
                UiStyle.TextLight);
            SetAnchors(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -16f), new Vector2(-40f, 36f));
            title.fontStyle = FontStyles.Bold;
            title.text = "Входящие";

            var listRoot = CreateUiObject("MessageList", panel.transform);
            var listRect = listRoot.GetComponent<RectTransform>();
            SetAnchors(listRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 72f), new Vector2(-40f, -56f));

            var layout = listRoot.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var template = TmpUiFactory.CreateText(
                "MessageItemTemplate",
                listRoot.transform,
                18f,
                TextAlignmentOptions.TopLeft,
                UiStyle.TextLight);
            template.gameObject.SetActive(false);
            template.text = "…";

            var closeGo = CreateUiObject("CloseButton", panel.transform);
            var closeRect = closeGo.GetComponent<RectTransform>();
            SetAnchors(closeRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(200f, 44f));
            var closeGraphic = UiKitFactory.AddRoundedButtonGraphic(closeGo, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var closeButton = closeGo.AddComponent<Button>();
            closeButton.targetGraphic = closeGraphic;

            var closeLabel = TmpUiFactory.CreateText(
                "Label",
                closeGo.transform,
                20f,
                TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(closeLabel.gameObject);
            closeLabel.text = "Закрыть";

            return (group, listRect, template, closeButton);
        }

        static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void StretchFull(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }
    }
}
