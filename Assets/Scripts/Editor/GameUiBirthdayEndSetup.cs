using MaratGame.Presentation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 11: EndingUI overlay + HubActionButton на сцене Game.
    /// </summary>
    static class GameUiBirthdayEndSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("MaratGame/UI/Setup Step 11 (Birthday & Ending)")]
        public static void SetupStep11Ui() => EnsureBirthdayEndUiOnGameScene();

        public static void EnsureBirthdayEndUiOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MaratGame] Game scene has no Canvas. Run Setup Step 04 first.");
                return;
            }

            UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);

            if (Object.FindFirstObjectByType<HubActionButton>() == null)
                BuildHubActionButton(canvas.transform);

            if (Object.FindFirstObjectByType<EndingUI>() == null)
                BuildEndingUi(canvas.transform);

            var canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();

            var ending = Object.FindFirstObjectByType<EndingUI>();
            if (ending != null)
            {
                var so = new SerializedObject(ending);
                so.FindProperty("rootCanvasGroup").objectReferenceValue = canvasGroup;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Step 11: HubActionButton + EndingUI on Game scene.");
        }

        static void BuildHubActionButton(Transform canvas)
        {
            var root = CreateUiObject("HubActionButton", canvas);
            var view = root.AddComponent<HubActionButton>();

            var buttonRoot = CreateUiObject("ActionButton", root.transform);
            var rect = buttonRoot.GetComponent<RectTransform>();
            SetAnchors(rect, new Vector2(0.5f, 0.02f), new Vector2(0.5f, 0.02f), Vector2.zero, new Vector2(140f, 72f));
            var group = buttonRoot.AddComponent<CanvasGroup>();

            var graphic = UiKitFactory.AddRoundedButtonGraphic(buttonRoot, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var button = buttonRoot.AddComponent<Button>();
            button.targetGraphic = graphic;

            var label = TmpUiFactory.CreateText(
                "Label",
                buttonRoot.transform,
                17f,
                TMPro.TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(label.gameObject);
            label.lineSpacing = -4f;
            label.text = "●\nОсмотреться";

            var runner = Object.FindFirstObjectByType<Narrative.StoryRunner>();
            var so = new SerializedObject(view);
            so.FindProperty("storyRunner").objectReferenceValue = runner;
            so.FindProperty("buttonRoot").objectReferenceValue = buttonRoot;
            so.FindProperty("buttonGroup").objectReferenceValue = group;
            so.FindProperty("actionButton").objectReferenceValue = button;
            so.FindProperty("label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            buttonRoot.SetActive(false);
        }

        static void BuildEndingUi(Transform canvas)
        {
            var root = CreateUiObject("EndingUI", canvas);
            var view = root.AddComponent<EndingUI>();

            var overlay = CreateUiObject("EndingOverlay", root.transform);
            StretchFull(overlay);
            var overlayGroup = overlay.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 0f;
            overlayGroup.blocksRaycasts = false;
            overlayGroup.interactable = false;

            var dim = overlay.AddComponent<Image>();
            dim.color = new Color(0.04f, 0.06f, 0.1f, 0.92f);
            dim.raycastTarget = true;

            var panel = CreateUiObject("StatsPanel", overlay.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(420f, 360f);
            UiKitFactory.AddRoundedPanel(panel, UiStyle.PanelBackground, UiStyle.PanelCornerRadius);

            var title = TmpUiFactory.CreateText(
                "Title",
                panel.transform,
                28f,
                TMPro.TextAlignmentOptions.Center,
                UiStyle.TextLight);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.88f);
            titleRect.anchorMax = new Vector2(0.5f, 0.88f);
            titleRect.sizeDelta = new Vector2(380f, 48f);
            title.fontStyle = TMPro.FontStyles.Bold;
            title.text = "ГЛАВА 1";

            var respectLabel = CreateStatRow(panel.transform, "RespectRow", "Уважение", 0.62f, out var respectValue);
            var calmLabel = CreateStatRow(panel.transform, "CalmRow", "Спокойствие", 0.46f, out var calmValue);
            var decisionsLabel = CreateStatRow(panel.transform, "DecisionsRow", "Решений принято", 0.30f, out var decisionsValue);

            var replayGo = CreateUiObject("ReplayButton", panel.transform);
            var replayRect = replayGo.GetComponent<RectTransform>();
            replayRect.anchorMin = new Vector2(0.5f, 0.1f);
            replayRect.anchorMax = new Vector2(0.5f, 0.1f);
            replayRect.sizeDelta = new Vector2(300f, 52f);
            var replayGraphic = UiKitFactory.AddRoundedButtonGraphic(replayGo, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var replayButton = replayGo.AddComponent<Button>();
            replayButton.targetGraphic = replayGraphic;

            var replayLabel = TmpUiFactory.CreateText(
                "Text",
                replayGo.transform,
                20f,
                TMPro.TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(replayLabel.gameObject);
            replayLabel.fontStyle = TMPro.FontStyles.Bold;
            replayLabel.text = "↺ СЫГРАТЬ ЕЩЁ РАЗ";

            var runner = Object.FindFirstObjectByType<Narrative.StoryRunner>();
            var so = new SerializedObject(view);
            so.FindProperty("storyRunner").objectReferenceValue = runner;
            so.FindProperty("overlayGroup").objectReferenceValue = overlayGroup;
            so.FindProperty("respectValueText").objectReferenceValue = respectValue;
            so.FindProperty("calmValueText").objectReferenceValue = calmValue;
            so.FindProperty("decisionsValueText").objectReferenceValue = decisionsValue;
            so.FindProperty("replayButton").objectReferenceValue = replayButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(replayButton.onClick, view.OnReplayClicked);

            overlay.SetActive(false);
        }

        static TMPro.TextMeshProUGUI CreateStatRow(
            Transform parent,
            string rowName,
            string labelText,
            float anchorY,
            out TMPro.TextMeshProUGUI valueText)
        {
            var row = CreateUiObject(rowName, parent);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, anchorY);
            rowRect.anchorMax = new Vector2(0.5f, anchorY);
            rowRect.sizeDelta = new Vector2(360f, 40f);

            var label = TmpUiFactory.CreateText(
                "Label",
                row.transform,
                20f,
                TMPro.TextAlignmentOptions.MidlineLeft,
                UiStyle.TextMuted);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0.55f, 0.5f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.text = labelText;

            valueText = TmpUiFactory.CreateText(
                "Value",
                row.transform,
                24f,
                TMPro.TextAlignmentOptions.MidlineRight,
                UiStyle.TextLight);
            var valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.55f, 0.5f);
            valueRect.anchorMax = new Vector2(1f, 0.5f);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            valueText.text = "—";

            return valueText;
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

        static void SetAnchors(RectTransform rect, Vector2 anchor, Vector2 pivotAnchor, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = pivotAnchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }
    }
}
