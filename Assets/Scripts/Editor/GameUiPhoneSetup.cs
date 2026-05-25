using MaratGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 08: PhoneButton + PhoneOverlay (Phone_big + пузыри MPImage).
    /// </summary>
    static class GameUiPhoneSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";
        const string PhoneFrameSpritePath = "Assets/Art/UI/Phone_big.png";

        [MenuItem("MaratGame/UI/Setup Step 08 (Phone UI)")]
        public static void SetupStep08PhoneUi() => EnsurePhoneUiOnGameScene();

        [MenuItem("MaratGame/UI/Upgrade Phone UI (Phone_big + bubbles)")]
        public static void UpgradePhoneUiKitOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);

            ApplyPhoneKitToHierarchy();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Phone UI upgraded: Phone_big frame + white MPImage message bubbles.");
        }

        [MenuItem("MaratGame/UI/Fix Phone Layout (Game scene)")]
        public static void FixPhoneLayoutOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);
            ApplyPhoneKitToHierarchy();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Phone UI layout + kit (Phone_big, bubbles) applied on Game scene.");
        }

        public static void ApplyPhoneLayoutToHierarchy()
        {
            var panel = GameObject.Find("Canvas/PhoneUI/PhoneOverlay/Panel")?.GetComponent<RectTransform>();
            var title = GameObject.Find("Canvas/PhoneUI/PhoneOverlay/Panel/Title")?.GetComponent<RectTransform>();
            var list = GameObject.Find("Canvas/PhoneUI/PhoneOverlay/Panel/MessageList")?.GetComponent<RectTransform>();
            var close = GameObject.Find("Canvas/PhoneUI/PhoneOverlay/Panel/CloseButton")?.GetComponent<RectTransform>();
            var frame = GameObject.Find("Canvas/PhoneUI/PhoneOverlay/Panel/PanelSprite")?.GetComponent<RectTransform>();
            var button = GameObject.Find("Canvas/PhoneUI/PhoneButton")?.GetComponent<RectTransform>();

            if (panel != null)
                SetAnchors(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, UiLayout.SizePhonePanel);

            if (frame != null)
                StretchFull(frame.gameObject);

            if (title != null)
                SetPhoneTitleLayout(title);

            if (list != null)
            {
                SetStretchInsets(list, UiLayout.PhoneMessageListInsetMin, UiLayout.PhoneMessageListInsetMax);
                var layout = list.GetComponent<VerticalLayoutGroup>();
                if (layout != null)
                {
                    layout.padding = new RectOffset(4, 4, 4, 4);
                    layout.spacing = 8f;
                    layout.childAlignment = TextAnchor.UpperLeft;
                    layout.childControlWidth = true;
                    layout.childControlHeight = false;
                    layout.childForceExpandWidth = true;
                    layout.childForceExpandHeight = false;
                }
            }

            if (close != null)
                SetAnchors(close, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, UiLayout.PhoneCloseBottomOffset), UiLayout.SizePhoneClose);

            if (button != null)
            {
                button.anchorMin = new Vector2(1f, 0f);
                button.anchorMax = new Vector2(1f, 0f);
                button.pivot = new Vector2(1f, 0f);
                button.anchoredPosition = new Vector2(-72f, 56f);
                button.sizeDelta = UiLayout.SizePhoneButton;
            }
        }

        public static void ApplyPhoneKitToHierarchy()
        {
            ApplyPhoneLayoutToHierarchy();

            var panel = GameObject.Find("Canvas/PhoneUI/PhoneOverlay/Panel");
            if (panel == null)
            {
                Debug.LogWarning("[MaratGame] Phone Panel not found. Run Setup Step 08 first.");
                return;
            }

            EnsurePhoneFrameSprite(panel);
            StripLegacyPanelBackground(panel);

            var list = panel.transform.Find("MessageList");
            if (list != null)
            {
                var template = BuildMessageItemTemplate(list);
                WireMessageTemplateToPhoneUi(template);
                EnsureMessageListMask(list);
                UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(list.gameObject);
            }

            var title = panel.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.text = "Входящие";
                title.color = UiStyle.PhoneInboxTitle;
                title.fontStyle = FontStyles.Bold;
                title.raycastTarget = false;
            }

            FixCloseButtonRaycasts();
        }

        static void EnsureMessageListMask(Transform list)
        {
            if (list.GetComponent<RectMask2D>() == null)
                list.gameObject.AddComponent<RectMask2D>();
        }

        static void FixCloseButtonRaycasts()
        {
            var close = GameObject.Find("Canvas/PhoneUI/PhoneOverlay/Panel/CloseButton");
            if (close == null)
                return;

            close.transform.SetAsLastSibling();

            foreach (var label in close.GetComponentsInChildren<TextMeshProUGUI>(true))
                label.raycastTarget = false;

            var button = close.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;
                var graphic = button.targetGraphic;
                if (graphic != null)
                    graphic.raycastTarget = true;
            }
        }

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
                ApplyPhoneKitToHierarchy();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[MaratGame] PhoneUI already on scene — kit upgraded in place.");
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
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-72f, 56f);
            rect.sizeDelta = UiLayout.SizePhoneButton;

            var graphic = UiKitFactory.AddRoundedButtonGraphic(root, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var button = root.AddComponent<Button>();
            button.targetGraphic = graphic;

            var icon = TmpUiFactory.CreateText(
                "Icon",
                root.transform,
                UiLayout.FontPhoneIcon,
                TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(icon.gameObject);
            icon.text = "Тел";

            var badge = CreateUiObject("Badge", root.transform);
            var badgeRect = badge.GetComponent<RectTransform>();
            SetAnchors(badgeRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(8f, 8f), UiLayout.SizePhoneBadge);
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
            SetAnchors(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, UiLayout.SizePhonePanel);

            var frameGo = CreateUiObject("PanelSprite", panel.transform);
            StretchFull(frameGo);
            var frameImage = frameGo.AddComponent<Image>();
            frameImage.preserveAspect = true;
            frameImage.raycastTarget = false;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PhoneFrameSpritePath);
            if (sprite != null)
                frameImage.sprite = sprite;

            var title = TmpUiFactory.CreateText(
                "Title",
                panel.transform,
                UiLayout.FontPhoneTitle,
                TextAlignmentOptions.TopLeft,
                UiStyle.PhoneInboxTitle);
            SetPhoneTitleLayout(title.rectTransform);
            title.fontStyle = FontStyles.Bold;
            title.text = "Входящие";

            var listRoot = CreateUiObject("MessageList", panel.transform);
            var listRect = listRoot.GetComponent<RectTransform>();
            SetStretchInsets(listRect, UiLayout.PhoneMessageListInsetMin, UiLayout.PhoneMessageListInsetMax);

            var layout = listRoot.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var template = BuildMessageItemTemplate(listRoot.transform);

            var closeGo = CreateUiObject("CloseButton", panel.transform);
            var closeRect = closeGo.GetComponent<RectTransform>();
            SetAnchors(closeRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, UiLayout.PhoneCloseBottomOffset), UiLayout.SizePhoneClose);
            var closeGraphic = UiKitFactory.AddRoundedButtonGraphic(closeGo, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var closeButton = closeGo.AddComponent<Button>();
            closeButton.targetGraphic = closeGraphic;

            var closeLabel = TmpUiFactory.CreateText(
                "Label",
                closeGo.transform,
                UiLayout.FontPhoneClose,
                TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(closeLabel.gameObject);
            closeLabel.raycastTarget = false;
            closeLabel.text = "Закрыть";

            return (group, listRect, template, closeButton);
        }

        static TextMeshProUGUI BuildMessageItemTemplate(Transform listParent)
        {
            var existing = listParent.Find("MessageItemTemplate");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var root = CreateUiObject("MessageItemTemplate", listParent);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.sizeDelta = Vector2.zero;

            var rowGroup = root.AddComponent<CanvasGroup>();
            rowGroup.alpha = 0f;
            rowGroup.blocksRaycasts = false;
            rowGroup.interactable = false;

            var layout = root.AddComponent<LayoutElement>();
            layout.minHeight = 32f;
            layout.flexibleWidth = 1f;

            root.AddComponent<PhoneMessageBubbleRow>();

            var bubble = CreateUiObject("Bubble", root.transform);
            StretchFull(bubble);
            var bubbleGraphic = UiKitFactory.AddRoundedButtonGraphic(bubble, UiStyle.PhoneMessageBubble, UiStyle.PhoneMessageBubbleRadius);
            bubbleGraphic.raycastTarget = false;

            var text = TmpUiFactory.CreateText(
                "Text",
                root.transform,
                UiLayout.FontPhoneMessage,
                TextAlignmentOptions.TopLeft,
                UiStyle.PhoneMessageText);
            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.pivot = new Vector2(0f, 1f);
            textRect.anchoredPosition = new Vector2(UiLayout.PhoneMessagePadH, -UiLayout.PhoneMessagePadV);
            textRect.sizeDelta = new Vector2(UiLayout.PhoneMessageRowWidth - UiLayout.PhoneMessagePadH * 2f, 0f);
            text.margin = Vector4.zero;
            text.raycastTarget = false;
            text.text = "…";
            root.SetActive(false);

            return text;
        }

        static void WireMessageTemplateToPhoneUi(TextMeshProUGUI template)
        {
            var phoneUi = Object.FindFirstObjectByType<PhoneUI>();
            if (phoneUi == null || template == null)
                return;

            var so = new SerializedObject(phoneUi);
            so.FindProperty("messageItemPrefab").objectReferenceValue = template;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsurePhoneFrameSprite(GameObject panel)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PhoneFrameSpritePath);
            if (sprite == null)
            {
                Debug.LogWarning($"[MaratGame] Sprite not found: {PhoneFrameSpritePath}. Reimport Phone_big.png.");
                return;
            }

            var frame = panel.transform.Find("PanelSprite");
            if (frame == null)
            {
                frame = CreateUiObject("PanelSprite", panel.transform).transform;
                StretchFull(frame.gameObject);
                frame.SetAsFirstSibling();
            }

            var image = frame.GetComponent<Image>() ?? frame.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = false;
            StretchFull(frame.gameObject);
        }

        static void StripLegacyPanelBackground(GameObject panel)
        {
            foreach (var proc in panel.GetComponents<ProceduralImage>())
                Object.DestroyImmediate(proc);

            foreach (var mod in panel.GetComponents<FreeModifier>())
                Object.DestroyImmediate(mod);

            var legacy = panel.GetComponent<Image>();
            if (legacy != null && panel.transform.Find("PanelSprite") != null)
                Object.DestroyImmediate(legacy);
        }

        static void SetPhoneTitleLayout(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = new Vector2(UiLayout.PhoneSideInset + 8f, -UiLayout.PhoneTitleBandHeight);
            rect.offsetMax = new Vector2(-UiLayout.PhoneSideInset, -56f);
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
            rect.pivot = new Vector2(0.5f, 0.5f);
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

        static void SetStretchInsets(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}

