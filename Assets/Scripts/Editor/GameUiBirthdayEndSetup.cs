using MaratGame.Presentation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// EndingUI (вечерний финал) + HubActionButton на сцене Game.
    /// </summary>
    static class GameUiBirthdayEndSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("MaratGame/UI/Setup Step 11 (Birthday & Ending)")]
        public static void SetupStep11Ui() => EnsureBirthdayEndUiOnGameScene();

        [MenuItem("MaratGame/UI/Refresh Ending Screen Layout")]
        public static void RefreshEndingScreenLayout()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var ending = Object.FindFirstObjectByType<EndingUI>();
            if (ending == null)
            {
                Debug.LogWarning("[MaratGame] EndingUI not found on Game scene.");
                return;
            }

            EnsureFullDayBridgeControls(ending);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Ending screen layout refreshed.");
        }

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

            var hubAction = Object.FindFirstObjectByType<HubActionButton>();
            if (hubAction != null)
            {
                var hubSo = new SerializedObject(hubAction);
                hubSo.FindProperty("autoTriggerWhenReady").boolValue = false;
                hubSo.ApplyModifiedPropertiesWithoutUndo();
            }

            if (Object.FindFirstObjectByType<EndingUI>() == null)
                BuildEndingUi(canvas.transform);

            var canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();

            var ending = Object.FindFirstObjectByType<EndingUI>();
            if (ending != null)
            {
                EnsureFullDayBridgeControls(ending);
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
            SetAnchors(rect, new Vector2(0.5f, 0.02f), new Vector2(0.5f, 0.02f), Vector2.zero, UiLayout.SizeHubAction);
            var group = buttonRoot.AddComponent<CanvasGroup>();

            var graphic = UiKitFactory.AddRoundedButtonGraphic(buttonRoot, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var button = buttonRoot.AddComponent<Button>();
            button.targetGraphic = graphic;

            var label = TmpUiFactory.CreateText(
                "Label",
                buttonRoot.transform,
                UiLayout.FontHubAction,
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
            dim.color = new Color(0.05f, 0.08f, 0.12f, 1f);
            dim.raycastTarget = true;

            var panel = CreateUiObject("StatsPanel", overlay.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(EndingUiLayout.PanelWidth, EndingUiLayout.PanelHeight);
            UiKitFactory.AddRoundedPanel(panel, UiStyle.PanelBackground, UiStyle.PanelCornerRadius);

            var title = TmpUiFactory.CreateText(
                "Title",
                panel.transform,
                28f,
                TMPro.TextAlignmentOptions.Center,
                UiStyle.TextSpeaker);
            title.fontStyle = TMPro.FontStyles.Bold;
            title.text = "ФИНАЛ ДНЯ";

            var statsZone = CreateZonePanel(panel.transform, "StatsZone", new Color(0.1f, 0.13f, 0.18f, 0.92f));
            var achievementsZone = CreateZonePanel(panel.transform, "AchievementsZone", new Color(0.08f, 0.11f, 0.15f, 0.88f));
            var actionsZone = CreateZonePanel(panel.transform, "ActionsZone", new Color(0.07f, 0.09f, 0.13f, 0.75f));

            CreateStatRow(statsZone, "RespectRow", "Уважение", 0.70f, out var respectValue);
            CreateStatRow(statsZone, "CalmRow", "Спокойствие", 0.58f, out var calmValue);
            CreateStatRow(statsZone, "ChaosRow", "Хаос", 0.46f, out var chaosValue);
            CreateStatRow(statsZone, "DecisionsRow", "Решений принято", 0.34f, out var decisionsValue);
            CreateAchievementsSection(achievementsZone, out var achievementsValue);

            var replayGo = CreateUiObject("ReplayButton", actionsZone);
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
            replayLabel.text = "СЫГРАТЬ ЕЩЁ РАЗ";

            var mainMenuButton = BuildMainMenuButton(actionsZone);

            var refs = new EndingUiRefs
            {
                Title = title,
                RespectValue = respectValue,
                CalmValue = calmValue,
                ChaosValue = chaosValue,
                DecisionsValue = decisionsValue,
                AchievementsValue = achievementsValue,
                ReplayButton = replayButton,
                MainMenuButton = mainMenuButton
            };
            EndingUiLayout.Apply(panelRect, refs);
            EndingUiLayout.DestroyDuplicateEndingObjects(panelRect, refs);

            var runner = Object.FindFirstObjectByType<Narrative.StoryRunner>();
            var so = new SerializedObject(view);
            so.FindProperty("storyRunner").objectReferenceValue = runner;
            so.FindProperty("overlayGroup").objectReferenceValue = overlayGroup;
            so.FindProperty("respectValueText").objectReferenceValue = respectValue;
            so.FindProperty("calmValueText").objectReferenceValue = calmValue;
            so.FindProperty("chaosValueText").objectReferenceValue = chaosValue;
            so.FindProperty("decisionsValueText").objectReferenceValue = decisionsValue;
            so.FindProperty("achievementsValueText").objectReferenceValue = achievementsValue;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("replayButton").objectReferenceValue = replayButton;
            so.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(replayButton.onClick, view.OnReplayClicked);
            UnityEventTools.AddPersistentListener(mainMenuButton.onClick, view.OnMainMenuClicked);

            overlay.SetActive(false);
        }

        static void EnsureFullDayBridgeControls(EndingUI view)
        {
            if (view == null)
                return;

            var root = view.transform;
            var panel = root.Find("EndingOverlay/StatsPanel");
            if (panel == null)
                return;
            if (panel is RectTransform panelRect)
                panelRect.sizeDelta = new Vector2(EndingUiLayout.PanelWidth, EndingUiLayout.PanelHeight);

            var legacyContinue = panel.Find("ContinueDayButton");
            if (legacyContinue != null)
                legacyContinue.gameObject.SetActive(false);

            var title = panel.Find("Title")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (title != null)
                title.text = "ФИНАЛ ДНЯ";

            var replayButton = panel.Find("ReplayButton")?.GetComponent<Button>();
            var mainMenuButton = panel.Find("MainMenuButton")?.GetComponent<Button>();
            var statsZone = panel.Find("StatsZone") ?? panel;
            var achievementsZone = panel.Find("AchievementsZone") ?? panel;
            var chaosValue = statsZone.Find("ChaosRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>()
                ?? panel.Find("ChaosRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>();
            var achievementsValue = achievementsZone.Find("AchievementsBody")?.GetComponent<TMPro.TextMeshProUGUI>()
                ?? achievementsZone.Find("Value")?.GetComponent<TMPro.TextMeshProUGUI>()
                ?? panel.Find("AchievementsRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>();

            if (mainMenuButton == null)
                mainMenuButton = BuildMainMenuButton(panel);
            if (chaosValue == null)
                CreateStatRow(statsZone, "ChaosRow", "Хаос", 0.46f, out chaosValue);
            if (achievementsValue == null)
                CreateAchievementsSection(achievementsZone, out achievementsValue);

            if (replayButton != null && !HasPersistentListener(replayButton, view, nameof(EndingUI.OnReplayClicked)))
                UnityEventTools.AddPersistentListener(replayButton.onClick, view.OnReplayClicked);
            if (mainMenuButton != null && !HasPersistentListener(mainMenuButton, view, nameof(EndingUI.OnMainMenuClicked)))
                UnityEventTools.AddPersistentListener(mainMenuButton.onClick, view.OnMainMenuClicked);

            var replayText = replayButton != null
                ? replayButton.GetComponentInChildren<TMPro.TextMeshProUGUI>(true)
                : null;
            if (replayText != null)
                replayText.text = "СЫГРАТЬ ЕЩЁ РАЗ";
            var mainMenuText = mainMenuButton != null
                ? mainMenuButton.GetComponentInChildren<TMPro.TextMeshProUGUI>(true)
                : null;
            if (mainMenuText != null)
                mainMenuText.text = "В ГЛАВНОЕ МЕНЮ";

            var so = new SerializedObject(view);
            if (title != null)
                so.FindProperty("titleText").objectReferenceValue = title;
            if (replayButton != null)
                so.FindProperty("replayButton").objectReferenceValue = replayButton;
            if (mainMenuButton != null)
                so.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
            if (chaosValue != null)
                so.FindProperty("chaosValueText").objectReferenceValue = chaosValue;
            if (achievementsValue != null)
                so.FindProperty("achievementsValueText").objectReferenceValue = achievementsValue;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (panel is RectTransform endingPanel)
            {
                var refs = BuildEndingRefs(panel, title, chaosValue, achievementsValue, replayButton, mainMenuButton);
                EndingUiLayout.Apply(endingPanel, refs);
                EndingUiLayout.DestroyDuplicateEndingObjects(endingPanel, refs);
            }
        }

        static EndingUiRefs BuildEndingRefs(
            Transform panel,
            TMPro.TextMeshProUGUI title,
            TMPro.TextMeshProUGUI chaosValue,
            TMPro.TextMeshProUGUI achievementsValue,
            Button replayButton,
            Button mainMenuButton) =>
            new()
            {
                Title = title,
                RespectValue = panel.Find("StatsZone/RespectRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>()
                    ?? panel.Find("RespectRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>(),
                CalmValue = panel.Find("StatsZone/CalmRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>()
                    ?? panel.Find("CalmRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>(),
                ChaosValue = chaosValue,
                DecisionsValue = panel.Find("StatsZone/DecisionsRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>()
                    ?? panel.Find("DecisionsRow/Value")?.GetComponent<TMPro.TextMeshProUGUI>(),
                AchievementsValue = achievementsValue,
                ReplayButton = replayButton,
                MainMenuButton = mainMenuButton
            };

        static Transform CreateZonePanel(Transform parent, string zoneName, Color color)
        {
            var existing = parent.Find(zoneName);
            if (existing != null)
                return existing;

            var zone = CreateUiObject(zoneName, parent);
            UiKitFactory.AddRoundedPanel(zone, color, 12f);
            return zone.transform;
        }

        static TMPro.TextMeshProUGUI CreateAchievementsSection(Transform zone, out TMPro.TextMeshProUGUI valueText)
        {
            if (zone.Find("AchievementsHeader") == null)
            {
                var header = TmpUiFactory.CreateText(
                    "AchievementsHeader",
                    zone,
                    20f,
                    TMPro.TextAlignmentOptions.MidlineLeft,
                    UiStyle.TextMuted);
                header.fontStyle = TMPro.FontStyles.Bold;
                header.text = "Достижения";
            }

            valueText = zone.Find("AchievementsBody")?.GetComponent<TMPro.TextMeshProUGUI>()
                ?? zone.Find("Value")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (valueText == null)
            {
                valueText = TmpUiFactory.CreateText(
                    "AchievementsBody",
                    zone,
                    17f,
                    TMPro.TextAlignmentOptions.TopLeft,
                    UiStyle.TextLight);
                valueText.enableWordWrapping = true;
            }

            valueText.text = string.Empty;
            return valueText;
        }

        static Button BuildMainMenuButton(Transform panel)
        {
            var menuGo = CreateUiObject("MainMenuButton", panel);
            var menuRect = menuGo.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.5f, 0.0f);
            menuRect.anchorMax = new Vector2(0.5f, 0.0f);
            menuRect.sizeDelta = new Vector2(300f, 52f);
            var menuGraphic = UiKitFactory.AddRoundedButtonGraphic(menuGo, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var menuButton = menuGo.AddComponent<Button>();
            menuButton.targetGraphic = menuGraphic;

            var menuLabel = TmpUiFactory.CreateText(
                "Text",
                menuGo.transform,
                20f,
                TMPro.TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(menuLabel.gameObject);
            menuLabel.fontStyle = TMPro.FontStyles.Bold;
            menuLabel.text = "В ГЛАВНОЕ МЕНЮ";
            return menuButton;
        }

        static TMPro.TextMeshProUGUI CreateAchievementsRow(
            Transform parent,
            out TMPro.TextMeshProUGUI valueText)
        {
            var row = CreateUiObject("AchievementsRow", parent);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.24f);
            rowRect.anchorMax = new Vector2(0.5f, 0.24f);
            rowRect.sizeDelta = new Vector2(420f, 72f);

            var label = TmpUiFactory.CreateText(
                "Label",
                row.transform,
                18f,
                TMPro.TextAlignmentOptions.TopLeft,
                UiStyle.TextMuted);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.4f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.text = "Достижения";

            valueText = TmpUiFactory.CreateText(
                "Value",
                row.transform,
                18f,
                TMPro.TextAlignmentOptions.TopRight,
                UiStyle.TextLight);
            var valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.4f, 0f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            valueText.text = "—";
            valueText.enableWordWrapping = true;
            return valueText;
        }

        static bool HasPersistentListener(Button button, Object target, string methodName)
        {
            if (button == null || target == null || string.IsNullOrWhiteSpace(methodName))
                return false;

            var count = button.onClick.GetPersistentEventCount();
            for (var i = 0; i < count; i++)
            {
                if (button.onClick.GetPersistentTarget(i) == target &&
                    button.onClick.GetPersistentMethodName(i) == methodName)
                    return true;
            }

            return false;
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
