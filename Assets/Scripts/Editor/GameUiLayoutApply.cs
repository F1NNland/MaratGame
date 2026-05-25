using MaratGame.Presentation;
using MPUIKIT;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MaratGame.Editor
{
    /// <summary>
    /// Подгонка уже собранных сцен под эталон Figma 2001×981 без полного пересоздания UI.
    /// </summary>
    static class GameUiLayoutApply
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";
        const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("MaratGame/UI/Apply Reference Layout 2001×981 (Game scene)")]
        public static void ApplyGameScene() => ApplyScene(GameScenePath);

        [MenuItem("MaratGame/UI/Fix Dialogue Panel (Game scene)")]
        public static void FixDialoguePanelOnGameScene()
        {
            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var dialogue = GameObject.Find("Canvas/DialoguePanel")?.transform;
            if (dialogue == null)
            {
                Debug.LogError("[MaratGame] DialoguePanel not found.");
                return;
            }

            GameUiFigmaDesignApply.ApplyDialogueChromePublic(dialogue, null, null);
            EnsureDialogueHierarchy(dialogue);
            ApplyDialogueLayout();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[MaratGame] DialoguePanel: removed Figma test photos (Human Image, Image.png), procedural panels only.");
        }

        [MenuItem("MaratGame/UI/Fix UI Input (Game scene)")]
        public static void FixUiInputOnGameScene()
        {
            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            EnsureUiCanvasHealthy();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[MaratGame] UI input fixed: EventSystem, Canvas groups, BackgroundCanvas raycasts.");
        }

        [MenuItem("MaratGame/UI/Apply Reference Layout 2001×981 (MainMenu scene)")]
        public static void ApplyMainMenuScene() => ApplyScene(MainMenuScenePath);

        public static void ApplyReferenceResolutionToAllCanvases()
        {
            var resolution = UiLayout.ReferenceResolution;
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null)
                    continue;

                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = resolution;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = UiLayout.ScreenMatch;
                scaler.scaleFactor = 1f;
                EditorUtility.SetDirty(scaler);
            }
        }

        static void ApplyScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
                UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);

            ApplyReferenceResolutionToAllCanvases();

            if (scenePath == GameScenePath)
                ApplyGameHierarchy();

            if (scenePath == MainMenuScenePath)
                ApplyMainMenuHierarchy();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[MaratGame] Reference layout {UiLayout.RefWidth}×{UiLayout.RefHeight} applied: {scenePath}");
        }

        public static void ApplyGameHierarchy()
        {
            EnsureUiCanvasHealthy();
            EnsureBackgroundOnOwnCanvas();
            FixPhotoBackgrounds();
            GameUiNavigationSetup.EnsureLocationBackgrounds();

            ApplyFigmaHudAndHeaderLayout();

            EnsureDialogueFigmaChrome();
            ApplyDialogueLayout();
            ApplyChoicesLayout();
            ApplyNavigationBarLayout();

            foreach (var name in new[] { "NavLeft", "NavForward", "NavRight" })
            {
                var path = $"Canvas/NavigationBar/Bar/{name}";
                SetSize(path, UiLayout.SizeNavButton);
                EnsureLayoutElement(path, UiLayout.SizeNavButton);
                SetFont($"{path}/Label", UiLayout.FontNav);
            }

            GameUiPhoneSetup.ApplyPhoneLayoutToHierarchy();

            ApplyFigmaHubActionLayout();

            SetSize("Canvas/EndingUI/EndingOverlay/StatsPanel", new Vector2(EndingUiLayout.PanelWidth, EndingUiLayout.PanelHeight));
            SetSize("Canvas/EndingUI/EndingOverlay/StatsPanel/ReplayButton", UiLayout.SizeMenuButton);
            SetSize("Canvas/EndingUI/EndingOverlay/StatsPanel/MainMenuButton", UiLayout.SizeMenuButton);
        }

        static void ApplyMainMenuHierarchy()
        {
            SetSize("Canvas/MainMenuRoot/TitlePanel", UiLayout.SizeMenuPanel);
            SetFont("Canvas/MainMenuRoot/TitlePanel/TitleText", UiLayout.FontMenuTitle);
            SetFont("Canvas/MainMenuRoot/TitlePanel/SubtitleText", UiLayout.FontMenuSubtitle);
            SetSize("Canvas/MainMenuRoot/StartGameButton", UiLayout.SizeMenuButton);
            SetFont("Canvas/MainMenuRoot/StartGameButton/Text", UiLayout.FontMenuButton);
        }

        static void TuneChoicePrefab()
        {
            const string path = "Assets/Prefabs/UI/ChoiceButton.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                return;

            var rect = prefab.GetComponent<RectTransform>();
            if (rect != null)
                rect.sizeDelta = UiLayout.SizeChoiceButton;

            UiKitFactory.AddLayoutElement(prefab, UiLayout.SizeChoiceButton);

            var image = prefab.GetComponent<Image>();
            if (image != null)
                image.preserveAspect = false;

            var label = prefab.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.enableAutoSizing = true;
                label.fontSizeMin = UiLayout.FontChoiceAutoMin;
                label.fontSizeMax = UiLayout.FontChoiceAutoMax;
                label.fontSize = UiLayout.FontChoiceAutoMax;
                label.lineSpacing = -2f;
                label.alignment = TextAlignmentOptions.Center;
                label.textWrappingMode = TextWrappingModes.Normal;
                label.overflowMode = TextOverflowModes.Truncate;
                var labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = Vector2.zero;
                labelRect.offsetMin = new Vector2(UiLayout.ChoiceLabelPadH, UiLayout.ChoiceLabelPadV);
                labelRect.offsetMax = new Vector2(-UiLayout.ChoiceLabelPadH, -UiLayout.ChoiceLabelPadV);
            }

            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
        }

        static void TuneTmp(string path, float fontSize, Vector2? size, Vector2? anchoredPos)
        {
            SetFont(path, fontSize);
            if (size.HasValue)
                SetSize(path, size.Value);
            if (anchoredPos.HasValue)
                SetAnchoredPosition(path, anchoredPos.Value);
        }

        public static void EnsureUiCanvasHealthy()
        {
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo != null)
            {
                var rootGroup = canvasGo.GetComponent<CanvasGroup>();
                if (rootGroup != null)
                {
                    rootGroup.blocksRaycasts = true;
                    rootGroup.interactable = true;
                    EditorUtility.SetDirty(rootGroup);
                }
            }

            var canvas = FindRect("Canvas");
            if (canvas == null)
                return;

            canvas.localScale = Vector3.one;
            canvas.anchorMin = Vector2.zero;
            canvas.anchorMax = Vector2.one;
            canvas.pivot = new Vector2(0.5f, 0.5f);
            canvas.anchoredPosition = Vector2.zero;
            canvas.sizeDelta = Vector2.zero;
            canvas.offsetMin = Vector2.zero;
            canvas.offsetMax = Vector2.zero;

            var dialogueGroup = FindComponent<CanvasGroup>("Canvas/DialoguePanel");
            if (dialogueGroup != null)
            {
                dialogueGroup.blocksRaycasts = false;
                dialogueGroup.interactable = false;
                EditorUtility.SetDirty(dialogueGroup);
            }

            DisableRaycastBlocker("Canvas/BottomChrome");
            ConfigureRootCanvasGroup("Canvas");
            ConfigureRootCanvasGroup("Canvas/HUD");
            EnsureButtonRaycastTargets();
            CleanupLegacyHudIcons();
            ApplyFigmaHudAndHeaderLayout();
            SetTmpRaycast("Canvas/DialoguePanel/BodyText", false);
            SetTmpRaycast("Canvas/DialoguePanel/SpeakerText", false);
            SetTmpRaycast("Canvas/LocationHeader", false);
            SetTmpRaycast("Canvas/HUD/TimeText", false);
            SetTmpRaycast("Canvas/HUD/RespectText", false);
            SetTmpRaycast("Canvas/HUD/CalmText", false);
            SetTmpRaycast("Canvas/HUD/ChaosText", false);
            SetTmpRaycast("Canvas/HUD/PeriodBadge", false);
        }

        static void SetTmpRaycast(string path, bool enabled)
        {
            var tmp = FindTmp(path);
            if (tmp != null)
                tmp.raycastTarget = enabled;
        }

        static void DisableRaycastBlocker(string path)
        {
            var image = FindComponent<Image>(path);
            if (image != null)
                image.raycastTarget = false;
        }

        static void ConfigureRootCanvasGroup(string path)
        {
            var group = FindComponent<CanvasGroup>(path);
            if (group == null)
                return;

            group.blocksRaycasts = true;
            group.interactable = true;
            EditorUtility.SetDirty(group);
        }

        static void EnsureButtonRaycastTargets()
        {
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button == null)
                    continue;

                var graphic = button.targetGraphic;
                if (graphic != null)
                    graphic.raycastTarget = true;

                button.interactable = true;
            }
        }

        static void CleanupLegacyHudIcons()
        {
            var legacy = FindTransform("Canvas/HUD/RespectIcon");
            if (legacy != null)
            {
                legacy.gameObject.SetActive(false);
                EditorUtility.SetDirty(legacy.gameObject);
            }
        }

        static void EnsureBackgroundOnOwnCanvas()
        {
            var bg = FindTransform("Canvas/Background");
            if (bg == null)
                bg = FindTransform("BackgroundCanvas/Background");
            if (bg == null)
                return;

            var bgCanvas = UiKitFactory.EnsureBackgroundCanvas(0);
            if (bg.parent != bgCanvas.transform)
                bg.SetParent(bgCanvas.transform, false);

            var uiCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            if (uiCanvas != null)
                uiCanvas.sortingOrder = 10;
        }

        static void FixPhotoBackgrounds()
        {
            foreach (var path in new[] {
                "BackgroundCanvas/Background/BackgroundImageA",
                "BackgroundCanvas/Background/BackgroundImageB",
                "Canvas/Background/BackgroundImageA",
                "Canvas/Background/BackgroundImageB" })
            {
                var image = FindComponent<Image>(path);
                if (image == null)
                    continue;

                image.color = UiStyle.PhotoBackground;
                image.preserveAspect = false;
                EditorUtility.SetDirty(image);
            }

            var bgGroup = FindComponent<CanvasGroup>("BackgroundCanvas/Background");
            if (bgGroup != null)
            {
                bgGroup.alpha = 1f;
                var so = new SerializedObject(bgGroup);
                so.FindProperty("m_BlocksRaycasts").boolValue = false;
                so.FindProperty("m_Interactable").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bgGroup);
            }

            foreach (var path in new[]
            {
                "BackgroundCanvas/Background/BackgroundImageA",
                "BackgroundCanvas/Background/BackgroundImageB",
                "BackgroundCanvas/ScreenVignette"
            })
                DisableRaycastBlocker(path);
        }

        static void EnsureLayoutElement(string path, Vector2 size)
        {
            var t = FindTransform(path);
            if (t == null)
                return;

            UiKitFactory.AddLayoutElement(t.gameObject, size);
            EditorUtility.SetDirty(t.gameObject);
        }

        static T FindComponent<T>(string path) where T : Component
        {
            var t = FindTransform(path);
            return t != null ? t.GetComponent<T>() : null;
        }

        static void SetFont(string path, float fontSize)
        {
            var tmp = FindTmp(path);
            if (tmp == null)
                return;

            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = false;
            EditorUtility.SetDirty(tmp);
        }

        static void SetSize(string path, Vector2 size)
        {
            var rect = FindRect(path);
            if (rect == null)
                return;

            if (rect.anchorMin == rect.anchorMax)
                rect.sizeDelta = size;
        }

        static void SetAnchoredPosition(string path, Vector2 pos)
        {
            var rect = FindRect(path);
            if (rect == null)
                return;

            rect.anchoredPosition = pos;
        }

        static void StretchAnchors(string path, Vector2 min, Vector2 max)
        {
            var rect = FindRect(path);
            if (rect == null)
                return;

            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
        {
            if (rect == null)
                return;

            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        public static void LayoutPortraitImageRect(RectTransform rect)
        {
            if (rect == null)
                return;

            var pad = UiLayout.DialoguePortraitPadding;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = new Vector2(pad, pad);
            rect.offsetMax = new Vector2(-pad, -pad);

            var image = rect.GetComponent<Image>();
            if (image != null)
            {
                image.preserveAspect = true;
                image.raycastTarget = false;
            }
        }

        static void SetRectOffsets(string path, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = FindRect(path);
            if (rect == null)
                return;

            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        static Transform FindTransform(string path)
        {
            var parts = path.Split('/');
            if (parts.Length == 0)
                return null;

            var root = GameObject.Find(parts[0]);
            if (root == null)
                return null;

            var current = root.transform;
            for (var i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null)
                    return null;
            }

            return current;
        }

        static RectTransform FindRect(string path)
        {
            var t = FindTransform(path);
            return t != null ? t.GetComponent<RectTransform>() : null;
        }

        static TextMeshProUGUI FindTmp(string path)
        {
            var t = FindTransform(path);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        public static void ApplyFigmaHudAndHeaderLayout()
        {
            TuneTmp("Canvas/HUD/TimeText", UiLayout.FontHudTime, UiLayout.SizeHudTime, new Vector2(80f, -54f));
            TuneTmp("Canvas/LocationHeader", UiLayout.FontLocationHeader, UiLayout.SizeLocationHeader, new Vector2(0f, -62f));
            SetFont("Canvas/LocationHeader", UiLayout.FontLocationHeader);
            var locationHeader = FindTmp("Canvas/LocationHeader");
            if (locationHeader != null)
                locationHeader.fontStyle = FontStyles.Bold;

            var chapter = FindTmp("Canvas/HUD/ChapterText");
            if (chapter != null)
                chapter.gameObject.SetActive(false);

            var period = FindTmp("Canvas/HUD/PeriodBadge");
            if (period != null)
            {
                SetAnchors(period.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), UiLayout.SizeHudPeriod);
                period.fontSize = UiLayout.FontHudPeriod;
            }

            LayoutHudStatSlot(0, "ChaosText", "StatChaosIcon");
            LayoutHudStatSlot(1, "CalmText", "StatCalmIcon");
            LayoutHudStatSlot(2, "RespectText", "StatRespectIcon");
        }

        /// <param name="slotFromRight">0 — крайний справа слот (хаос), далее влево.</param>
        static void LayoutHudStatSlot(int slotFromRight, string textPath, string iconPath)
        {
            var iconRightX = -(UiLayout.HudStatRightMargin + slotFromRight * UiLayout.HudStatSlotWidth);
            var textRightX = iconRightX - UiLayout.SizeHudStatIcon.x - UiLayout.HudStatIconTextGap;
            LayoutHudStat(textPath, iconPath, iconRightX, textRightX);
        }

        static void LayoutHudStat(string textPath, string iconPath, float iconRightX, float textRightX)
        {
            var text = FindTmp($"Canvas/HUD/{textPath}");
            if (text == null)
                return;

            text.fontSize = UiLayout.FontHudStat;
            text.enableAutoSizing = false;
            text.alignment = TextAlignmentOptions.MidlineRight;
            text.raycastTarget = false;

            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(1f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(1f, 0.5f);
            textRect.anchoredPosition = new Vector2(textRightX, -30f);
            textRect.sizeDelta = UiLayout.SizeHudStatValue;

            var icon = FindTransform($"Canvas/HUD/{iconPath}");
            if (icon == null)
                return;

            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(1f, 1f);
            iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.pivot = new Vector2(1f, 0.5f);
            iconRect.anchoredPosition = new Vector2(iconRightX, -30f);
            iconRect.sizeDelta = UiLayout.SizeHudStatIcon;

            var iconImage = icon.GetComponent<Image>();
            if (iconImage != null)
                iconImage.raycastTarget = false;
        }

        const string FigmaExportFolder = "Assets/Art/UI/FigmaExport";

        public static void EnsureDialogueFigmaChrome()
        {
            var dialogue = GameObject.Find("Canvas/DialoguePanel")?.transform;
            if (dialogue == null)
                return;

            GameUiFigmaDesignApply.ApplyDialogueChromePublic(dialogue, null, null);
            EnsureDialogueHierarchy(dialogue);
        }

        public static void EnsureDialogueHierarchy(Transform dialogue)
        {
            if (dialogue == null)
                return;

            var portraitFrame = dialogue.Find("PortraitFrame");
            if (portraitFrame == null)
                return;

            var portraitImage = dialogue.Find("PortraitImage");
            if (portraitImage != null)
            {
                if (portraitImage.parent != portraitFrame)
                    portraitImage.SetParent(portraitFrame, false);

                LayoutPortraitImageRect(portraitImage.GetComponent<RectTransform>());
            }

            var textFrame = dialogue.Find("TextFrame");
            if (textFrame == null)
                return;

            foreach (var name in new[] { "SpeakerText", "BodyText" })
            {
                var text = dialogue.Find(name);
                if (text == null || text.parent == textFrame)
                    continue;

                text.SetParent(textFrame, false);
            }
        }

        public static void ApplyDialogueLayout()
        {
            var panel = FindRect("Canvas/DialoguePanel");
            if (panel == null)
                return;

            EnsureDialogueFigmaChrome();

            // Старт игры — hall_morning (монолог): одна панель по центру снизу.
            panel.anchorMin = new Vector2(UiLayout.DialogueMonologueAreaMinX, UiLayout.DialogueAreaMinY);
            panel.anchorMax = new Vector2(UiLayout.DialogueMonologueAreaMaxX, UiLayout.DialogueAreaMaxY);
            panel.pivot = new Vector2(0f, 0f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = Vector2.zero;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var portraitImage = panel.Find("PortraitFrame/PortraitImage") ?? panel.Find("PortraitImage");
            if (portraitImage != null)
                LayoutPortraitImageRect(portraitImage.GetComponent<RectTransform>());

            var speakerPath = "Canvas/DialoguePanel/TextFrame/SpeakerText";
            var bodyPath = "Canvas/DialoguePanel/TextFrame/BodyText";
            if (FindRect(speakerPath) == null)
                speakerPath = "Canvas/DialoguePanel/SpeakerText";
            if (FindRect(bodyPath) == null)
                bodyPath = "Canvas/DialoguePanel/BodyText";

            var textLeft = panel.Find("TextFrame") != null
                ? 16f
                : UiLayout.SizeDialoguePortrait.x + 16f;
            LayoutDialogueTextBlock(speakerPath, textLeft, 40f, true);
            LayoutDialogueTextBlock(bodyPath, textLeft, 12f, false);

            TuneTmp(speakerPath, UiLayout.FontDialogueSpeaker, null, null);
            SetFont(speakerPath, UiLayout.FontDialogueSpeaker);
            SetFont(bodyPath, UiLayout.FontDialogueBody);

            var textFrame = panel.Find("TextFrame") as RectTransform;
            if (textFrame != null)
            {
                textFrame.anchorMin = Vector2.zero;
                textFrame.anchorMax = Vector2.one;
                textFrame.pivot = new Vector2(0f, 0.5f);
                textFrame.anchoredPosition = Vector2.zero;
                textFrame.sizeDelta = Vector2.zero;
                textFrame.offsetMin = Vector2.zero;
                textFrame.offsetMax = Vector2.zero;
            }

            var portraitFrame = panel.Find("PortraitFrame");
            if (portraitFrame != null)
            {
                portraitFrame.gameObject.SetActive(false);
                EditorUtility.SetDirty(portraitFrame.gameObject);
            }

            EditorUtility.SetDirty(panel.gameObject);
        }

        static void LayoutDialogueTextBlock(string path, float insetLeft, float insetBottom, bool topBand)
        {
            var rect = FindRect(path);
            var tmp = FindTmp(path);
            if (rect == null)
                return;

            const float padRight = 16f;
            const float padTop = 12f;

            if (topBand)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.offsetMin = new Vector2(insetLeft, -44f);
                rect.offsetMax = new Vector2(-padRight, -padTop);
            }
            else
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0f, 1f);
                rect.offsetMin = new Vector2(insetLeft, insetBottom);
                rect.offsetMax = new Vector2(-padRight, -48f);
            }

            if (tmp == null)
                return;

            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableAutoSizing = false;
        }

        public static void ApplyChoicesLayout()
        {
            StretchAnchors(
                "Canvas/ChoicesContainer",
                new Vector2(0.04f, UiLayout.ChoicesAreaMinY),
                new Vector2(0.96f, UiLayout.ChoicesAreaMaxY));
            EnsureChoicesVerticalLayout();
            TuneChoicePrefab();
        }

        public static void ApplyNavigationBarLayout()
        {
            var bottomUi = UiLayout.BottomChromeAnchorMaxY;
            StretchAnchors(
                "Canvas/NavigationBar",
                new Vector2(UiLayout.NavBarMinX, 0.02f),
                new Vector2(UiLayout.NavBarMaxX, bottomUi - 0.02f));
            StretchAnchors("Canvas/NavigationBar/Bar", Vector2.zero, Vector2.one);

            var barLayout = FindComponent<HorizontalLayoutGroup>("Canvas/NavigationBar/Bar");
            if (barLayout != null)
            {
                barLayout.childForceExpandHeight = false;
                barLayout.childForceExpandWidth = false;
                barLayout.childControlWidth = false;
                barLayout.childAlignment = TextAnchor.MiddleLeft;
                barLayout.spacing = 24f;
            }

            foreach (var name in new[] { "NavLeft", "NavForward", "NavRight" })
            {
                var path = $"Canvas/NavigationBar/Bar/{name}";
                SetSize(path, UiLayout.SizeNavButton);
                EnsureLayoutElement(path, UiLayout.SizeNavButton);
            }
        }

        static void EnsureChoicesVerticalLayout()
        {
            // Раскладка выбирается в рантайме в ChoicesView (ряд до 3 кнопок, столбец при 4+).
            var container = FindTransform("Canvas/ChoicesContainer");
            if (container == null)
                return;

            foreach (var layout in container.GetComponents<LayoutGroup>())
                Object.DestroyImmediate(layout);

            var fitter = container.GetComponent<ContentSizeFitter>();
            if (fitter != null)
                Object.DestroyImmediate(fitter);
        }

        static void ApplyFigmaHubActionLayout()
        {
            var bottomUi = UiLayout.BottomChromeAnchorMaxY;
            StretchAnchors("Canvas/HubActionButton", new Vector2(UiLayout.HubActionMinX, 0.04f), new Vector2(UiLayout.HubActionMaxX, bottomUi - 0.04f));

            var button = FindRect("Canvas/HubActionButton/ActionButton");
            if (button == null)
                return;

            button.anchorMin = Vector2.zero;
            button.anchorMax = Vector2.one;
            button.offsetMin = Vector2.zero;
            button.offsetMax = Vector2.zero;

            var layout = button.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
                layout = button.gameObject.AddComponent<HorizontalLayoutGroup>();

            layout.spacing = 12f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            SetSize("Canvas/HubActionButton/ActionButton", new Vector2(380f, 144f));
            var dot = FindRect("Canvas/HubActionButton/ActionButton/DotGraphic");
            if (dot != null)
                dot.sizeDelta = UiLayout.SizeHubDotButton;
        }
    }

    /// <summary>
    /// Спрайты из Figma (Downloads/Projects) → HUD, нижняя панель, навигация, телефон, кнопки выбора.
    /// </summary>
    static class GameUiFigmaDesignApply
    {
        const string ExportFolder = "Assets/Art/UI/FigmaExport";
        const string GameScenePath = "Assets/Scenes/Game.unity";
        const string ChoicePrefabPath = "Assets/Prefabs/UI/ChoiceButton.prefab";
        const string DownloadsSourceV2 = @"C:\Users\finnl\Downloads\Projects (2)";
        const string DownloadsSourceLegacy = @"C:\Users\finnl\Downloads\Projects";

        [MenuItem("MaratGame/UI/Import Figma UI from Downloads")]
        public static void ImportFromDownloads()
        {
            var source = ResolveDownloadsSource();
            if (source == null)
            {
                Debug.LogError("[MaratGame] Figma export not found. Expected: " + DownloadsSourceV2);
                return;
            }

            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/UI");
            EnsureFolder(ExportFolder);

            foreach (var file in System.IO.Directory.GetFiles(source, "*.png"))
            {
                var name = System.IO.Path.GetFileName(file);
                System.IO.File.Copy(file, System.IO.Path.Combine(
                    System.IO.Path.GetFullPath(ExportFolder), name), true);
            }

            AssetDatabase.Refresh();
            ForceReimportExportFolder();
            ConfigureAllSprites();
            Debug.Log($"[MaratGame] Figma UI ({UiLayout.RefWidth}×{UiLayout.RefHeight}) copied from {source} → {ExportFolder}");
        }

        static void ForceReimportExportFolder()
        {
            var fullPath = System.IO.Path.GetFullPath(ExportFolder);
            if (!System.IO.Directory.Exists(fullPath))
                return;

            foreach (var file in System.IO.Directory.GetFiles(fullPath, "*.png"))
            {
                var name = System.IO.Path.GetFileName(file);
                var assetPath = $"{ExportFolder}/{name}";
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.Refresh();
        }

        static string ResolveDownloadsSource()
        {
            if (System.IO.Directory.Exists(DownloadsSourceV2))
                return DownloadsSourceV2;
            if (System.IO.Directory.Exists(DownloadsSourceLegacy))
                return DownloadsSourceLegacy;
            return null;
        }

        static Canvas FindUiCanvas()
        {
            var go = GameObject.Find("Canvas");
            return go != null ? go.GetComponent<Canvas>() : null;
        }

        static void ReparentMisplacedChrome(Transform uiCanvas)
        {
            var bottomOnBg = GameObject.Find("BackgroundCanvas/BottomChrome");
            if (bottomOnBg == null)
                return;

            bottomOnBg.transform.SetParent(uiCanvas, false);
            bottomOnBg.transform.SetAsFirstSibling();
        }

        [MenuItem("MaratGame/UI/Apply Figma Design (Game scene)")]
        public static void ApplyGameScene()
        {
            ConfigureAllSprites();

            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var canvas = FindUiCanvas();
            if (canvas == null)
            {
                Debug.LogError("[MaratGame] Game scene has no UI Canvas.");
                return;
            }

            foreach (var sceneCanvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                UiKitFactory.EnsureCanvasSupportsProceduralImage(sceneCanvas);
                var scaler = sceneCanvas.GetComponent<CanvasScaler>();
                if (scaler == null)
                    continue;

                scaler.referenceResolution = UiLayout.ReferenceResolution;
                scaler.matchWidthOrHeight = UiLayout.ScreenMatch;
                EditorUtility.SetDirty(scaler);
            }

            var bgCanvasRoot = GameObject.Find("BackgroundCanvas")?.transform;

            var topPanel = LoadSprite("Top panel.png");
            var bottomPanel = LoadSprite("Bottom panel.png");
            var vignette = LoadSprite("Bg.png");
            var choiceBg = LoadSprite("Button.png");
            var navLeft = LoadSprite("Button Arrow Left.png");
            var navDot = LoadSprite("Button Dot.png");
            var navRight = LoadSprite("Button Arrow Left-1.png") ?? navLeft;
            var phoneIcon = LoadSprite("Phone.png");
            var phonePanel = LoadSprite("Phone panel.png");
            var badgeCircle = LoadSprite("Notification circle.png");
            var humanPortrait = LoadSprite("Human Image.png");
            var humanIcon = LoadSprite("Human Icon.png");
            var heartIcon = LoadSprite("Icon heart.png");

            if (bgCanvasRoot != null)
                ApplyScreenVignette(bgCanvasRoot, vignette);
            ReparentMisplacedChrome(canvas.transform);
            ApplyHudChrome(canvas.transform.Find("HUD"), topPanel, heartIcon);
            ApplyBottomChrome(canvas.transform, bottomPanel);
            ApplyDialogueChrome(canvas.transform.Find("DialoguePanel"), null, null);
            ApplyNavigationSprites(navLeft, navRight);
            ApplyPhoneSprites(phoneIcon, phonePanel, badgeCircle);
            FixUiOverlayRoot("Canvas/PhoneUI");
            ApplyHudStatIcons(LoadSprite("Icon.png"), LoadSprite("Icon-1.png"), LoadSprite("Icon heart.png"));
            ApplyHubActionFigmaSprites(navDot, LoadSprite("Осмотреться.png"));
            ApplyChoicePrefab(choiceBg, humanIcon, humanPortrait);
            TuneHudTypography();
            PostProcessFigmaLayoutFix(canvas.transform);
            GameUiLayoutApply.ApplyGameHierarchy();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[MaratGame] Figma design ({UiLayout.RefWidth}×{UiLayout.RefHeight}) applied on Game scene.");
        }

        static void PostProcessFigmaLayoutFix(Transform uiCanvas)
        {
            var bottom = uiCanvas.Find("BottomChrome")?.GetComponent<RectTransform>();
            if (bottom != null)
            {
                bottom.transform.SetAsFirstSibling();
                bottom.anchorMin = Vector2.zero;
                bottom.anchorMax = new Vector2(1f, UiLayout.BottomChromeAnchorMaxY);
                bottom.pivot = new Vector2(0.5f, 0f);
                bottom.anchoredPosition = Vector2.zero;
                bottom.sizeDelta = Vector2.zero;
                bottom.offsetMin = Vector2.zero;
                bottom.offsetMax = Vector2.zero;
            }

            GameUiLayoutApply.ApplyDialogueLayout();
            GameUiLayoutApply.ApplyChoicesLayout();
            GameUiLayoutApply.ApplyNavigationBarLayout();
            FixUiOverlayRoot("Canvas/PhoneUI");

            var dialogue = uiCanvas.Find("DialoguePanel");
            if (dialogue != null)
            {
                var bad = dialogue.Find("PanelSprite");
                if (bad != null)
                    DestroyEditorObject(bad.gameObject);
            }

            var overlay = GameObject.Find("Canvas/PhoneUI/PhoneOverlay")?.GetComponent<CanvasGroup>();
            if (overlay != null)
            {
                overlay.alpha = 0f;
                overlay.interactable = false;
                overlay.blocksRaycasts = false;
            }

            var badge = uiCanvas.Find("PhoneUI/PhoneButton/Badge")?.GetComponent<RectTransform>();
            if (badge != null)
            {
                badge.sizeDelta = UiLayout.SizePhoneBadge;
                badge.anchoredPosition = new Vector2(8f, 8f);
            }

            GameUiLayoutApply.EnsureUiCanvasHealthy();
        }

        static void ConfigureAllSprites()
        {
            if (!AssetDatabase.IsValidFolder(ExportFolder))
                return;

            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ExportFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.spritePixelsPerUnit = 1f;
                importer.SaveAndReimport();
            }

            AssetDatabase.Refresh();
        }

        static void ApplyReferenceLayoutOnOpenScene()
        {
            // Layout уже применён в ApplyGameScene выше.
        }

        static Sprite LoadSprite(string fileName)
        {
            var path = $"{ExportFolder}/{fileName}";
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                    return sprite;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
                return null;

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        static void ApplyScreenVignette(Transform canvas, Sprite vignette)
        {
            if (vignette == null)
                return;

            var root = EnsureChild(canvas, "ScreenVignette");
            root.transform.SetAsFirstSibling();
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, UiLayout.BottomChromeAnchorMaxY + 0.02f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = GetOrAddImage(root);
            image.sprite = vignette;
            image.color = Color.white;
            image.raycastTarget = false;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
        }

        static void ApplyHudChrome(Transform hud, Sprite topPanel, Sprite heartIcon)
        {
            if (hud == null || topPanel == null)
                return;

            var chrome = EnsureChild(hud, "TopChrome");
            chrome.transform.SetAsFirstSibling();
            var rect = chrome.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, UiLayout.TopChromeHeight);
            rect.offsetMin = new Vector2(0f, -UiLayout.TopChromeHeight);
            rect.offsetMax = Vector2.zero;

            var image = GetOrAddImage(chrome);
            image.sprite = topPanel;
            image.color = Color.white;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;

            var legacy = hud.Find("RespectIcon");
            if (legacy != null)
                legacy.gameObject.SetActive(false);
        }

        static void EnsureStatIcon(Transform hud, string name, Sprite sprite, Vector2 anchor, Vector2 pos)
        {
            var go = EnsureChild(hud, name);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(28f, 28f);
            var image = GetOrAddImage(go);
            image.sprite = sprite;
            image.color = Color.white;
            image.raycastTarget = false;
        }

        static void ApplyBottomChrome(Transform canvas, Sprite bottomPanel)
        {
            if (bottomPanel == null)
                return;

            var chrome = EnsureChild(canvas, "BottomChrome");
            chrome.transform.SetAsFirstSibling();
            var rect = chrome.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, UiLayout.BottomChromeAnchorMaxY);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = GetOrAddImage(chrome);
            image.sprite = bottomPanel;
            image.color = Color.white;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
        }

        static void FixUiOverlayRoot(string hierarchyPath)
        {
            var rect = GameObject.Find(hierarchyPath)?.GetComponent<RectTransform>();
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void ApplyDialogueChromePublic(Transform dialogue, Sprite portraitFrame, Sprite textPanel) =>
            ApplyDialogueChrome(dialogue, portraitFrame, textPanel);

        static void ApplyDialogueChrome(Transform dialogue, Sprite portraitFrame, Sprite textPanel)
        {
            if (dialogue == null)
                return;

            var legacySprite = dialogue.Find("PanelSprite");
            if (legacySprite != null)
                DestroyEditorObject(legacySprite.gameObject);

            var procedural = dialogue.GetComponent<ProceduralImage>();
            if (procedural != null)
            {
                procedural.color = new Color(1f, 1f, 1f, 0f);
                procedural.raycastTarget = false;
            }

            ApplyPortraitFrameChrome(dialogue);
            ApplyTextFrameChrome(dialogue);

            GameUiLayoutApply.EnsureDialogueHierarchy(dialogue);
        }

        /// <summary>
        /// Текстовая панель без Image.png из Figma (в экспорте вшито тестовое фото фона).
        /// </summary>
        public static void ApplyTextFrameChrome(Transform dialogue)
        {
            if (dialogue == null)
                return;

            var textBack = EnsureChild(dialogue, "TextFrame");
            textBack.transform.SetSiblingIndex(1);
            var textRect = textBack.GetComponent<RectTransform>();
            var portraitW = UiLayout.SizeDialoguePortrait.x;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.offsetMin = new Vector2(portraitW, 0f);
            textRect.offsetMax = Vector2.zero;

            var legacyPhoto = textBack.GetComponent<Image>();
            if (legacyPhoto != null)
                DestroyEditorObject(legacyPhoto);

            var procedural = textBack.GetComponent<ProceduralImage>();
            if (procedural == null)
                UiKitFactory.AddRoundedPanel(textBack, UiStyle.PanelBackground, UiStyle.PanelCornerRadius);
            else
                procedural.color = UiStyle.PanelBackground;

            if (textBack.GetComponent<ProceduralImage>() != null)
                textBack.GetComponent<ProceduralImage>().raycastTarget = false;
        }

        /// <summary>
        /// Рамка слота портрета (без Human Image.png — в экспорте Figma там тестовое фото фона).
        /// </summary>
        public static void ApplyPortraitFrameChrome(Transform dialogue)
        {
            if (dialogue == null)
                return;

            var portraitBack = EnsureChild(dialogue, "PortraitFrame");
            portraitBack.transform.SetAsFirstSibling();
            var portraitRect = portraitBack.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0f, 0f);
            portraitRect.anchorMax = new Vector2(0f, 1f);
            portraitRect.pivot = new Vector2(0f, 0.5f);
            portraitRect.sizeDelta = new Vector2(UiLayout.SizeDialoguePortrait.x, 0f);
            portraitRect.anchoredPosition = Vector2.zero;

            var legacyPhoto = portraitBack.GetComponent<Image>();
            if (legacyPhoto != null)
                DestroyEditorObject(legacyPhoto);

            var procedural = portraitBack.GetComponent<ProceduralImage>();
            if (procedural == null)
                UiKitFactory.AddRoundedPanel(portraitBack, UiStyle.PortraitSlotBackground, 12f);
            else
                procedural.color = UiStyle.PortraitSlotBackground;

            if (portraitBack.GetComponent<ProceduralImage>() != null)
                portraitBack.GetComponent<ProceduralImage>().raycastTarget = false;
        }

        static void ApplyNavigationSprites(Sprite left, Sprite right)
        {
            var hasRightSprite = right != null && right != left;
            ApplyNavButton("Canvas/NavigationBar/Bar/NavLeft", left, false);
            ApplyNavButton("Canvas/NavigationBar/Bar/NavForward", left, false, -90f);
            ApplyNavButton("Canvas/NavigationBar/Bar/NavRight", right != null ? right : left, !hasRightSprite);

            NavigationBar.ApplyDirectionLabels(
                GameObject.Find("Canvas/NavigationBar/Bar/NavLeft/Label")?.GetComponent<TextMeshProUGUI>(),
                GameObject.Find("Canvas/NavigationBar/Bar/NavForward/Label")?.GetComponent<TextMeshProUGUI>(),
                GameObject.Find("Canvas/NavigationBar/Bar/NavRight/Label")?.GetComponent<TextMeshProUGUI>());

            GameUiLayoutApply.ApplyNavigationBarLayout();

            var phoneButton = GameObject.Find("Canvas/PhoneUI/PhoneButton")?.GetComponent<RectTransform>();
            if (phoneButton != null)
            {
                phoneButton.anchorMin = new Vector2(1f, 0f);
                phoneButton.anchorMax = new Vector2(1f, 0f);
                phoneButton.pivot = new Vector2(1f, 0f);
                phoneButton.anchoredPosition = new Vector2(-72f, 56f);
                phoneButton.sizeDelta = UiLayout.SizePhoneButton;
            }
        }

        static void ApplyHudStatIcons(Sprite respectIcon, Sprite calmIcon, Sprite chaosIcon)
        {
            var hud = GameObject.Find("Canvas/HUD")?.transform;
            if (hud == null)
                return;

            EnsureHudStatIcon(hud, "StatRespectIcon", respectIcon);
            EnsureHudStatIcon(hud, "StatCalmIcon", calmIcon);
            EnsureChaosStat(hud, chaosIcon);
            EnsureHudStatIcon(hud, "StatChaosIcon", chaosIcon);
            GameUiLayoutApply.ApplyFigmaHudAndHeaderLayout();
        }

        static void EnsureChaosStat(Transform hud, Sprite chaosIcon)
        {
            var chaosText = hud.Find("ChaosText");
            if (chaosText == null)
            {
                var go = new GameObject("ChaosText", typeof(RectTransform));
                go.transform.SetParent(hud, false);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.font = TmpUiFactory.DefaultFont;
                tmp.fontSize = UiLayout.FontHudStat;
                tmp.color = UiStyle.TextLight;
                tmp.alignment = TextAlignmentOptions.MidlineRight;
                chaosText = go.transform;
            }

            var hudView = hud.GetComponent<GameHudView>();
            if (hudView != null)
            {
                var so = new SerializedObject(hudView);
                so.FindProperty("chaosText").objectReferenceValue = chaosText.GetComponent<TextMeshProUGUI>();
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void EnsureHudStatIcon(Transform hud, string iconName, Sprite sprite)
        {
            if (sprite == null)
                return;

            var iconGo = hud.Find(iconName)?.gameObject;
            if (iconGo == null)
            {
                iconGo = new GameObject(iconName, typeof(RectTransform));
                iconGo.transform.SetParent(hud, false);
            }

            var image = iconGo.GetComponent<Image>();
            if (image == null)
                image = iconGo.AddComponent<Image>();

            image.sprite = sprite;
            image.color = Color.white;
            image.raycastTarget = false;
            image.preserveAspect = true;
            iconGo.GetComponent<RectTransform>().sizeDelta = UiLayout.SizeHudStatIcon;
        }

        static void ApplyHubActionFigmaSprites(Sprite dotSprite, Sprite inspectLabelSprite)
        {
            var button = GameObject.Find("Canvas/HubActionButton/ActionButton");
            if (button == null || dotSprite == null)
                return;

            foreach (var proc in button.GetComponents<ProceduralImage>())
                DestroyEditorObject(proc);
            foreach (var mp in button.GetComponents<MPImage>())
                DestroyEditorObject(mp);

            var legacyImage = button.GetComponent<Image>();
            if (legacyImage != null)
                DestroyEditorObject(legacyImage);

            var dotGo = EnsureChild(button.transform, "DotGraphic");
            var dotRect = dotGo.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0f, 0.5f);
            dotRect.anchorMax = new Vector2(0f, 0.5f);
            dotRect.pivot = new Vector2(0f, 0.5f);
            dotRect.anchoredPosition = new Vector2(0f, 0f);
            var dotImage = GetOrAddImage(dotGo);
            dotImage.sprite = dotSprite;
            ApplyUiSpriteSize(dotImage, UiLayout.SizeHubDotButton);

            var label = button.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.text = "Осмотреться";
                label.fontSize = UiLayout.FontHubAction;
                label.color = UiStyle.TextLight;
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.raycastTarget = false;
                var labelRect = label.rectTransform;
                labelRect.anchorMin = new Vector2(0f, 0.5f);
                labelRect.anchorMax = new Vector2(0f, 0.5f);
                labelRect.pivot = new Vector2(0f, 0.5f);
                labelRect.anchoredPosition = new Vector2(UiLayout.SizeHubDotButton.x + 12f, 0f);
                labelRect.sizeDelta = UiLayout.SizeHubInspectLabel;
            }

            var inspectGraphic = button.transform.Find("InspectLabel");
            if (inspectGraphic != null)
                DestroyEditorObject(inspectGraphic.gameObject);

            var btn = button.GetComponent<Button>();
            if (btn != null)
                btn.targetGraphic = dotImage;
        }

        static void ApplyNavButton(string path, Sprite sprite, bool mirror, float zRotation = 0f)
        {
            var go = GameObject.Find(path);
            if (go == null || sprite == null)
                return;

            var size = UiLayout.SizeNavButton;
            var rect = go.GetComponent<RectTransform>();
            if (rect != null)
                rect.sizeDelta = size;

            UiKitFactory.AddLayoutElement(go, size);
            var image = ReplaceWithSprite(go, sprite);
            image.raycastTarget = true;
            var button = go.GetComponent<Button>();
            if (button != null)
            {
                button.targetGraphic = image;
                button.interactable = true;
            }

            image.rectTransform.localScale = mirror ? new Vector3(-1f, 1f, 1f) : Vector3.one;
            image.rectTransform.localEulerAngles = new Vector3(0f, 0f, zRotation);

            var label = go.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.fontSize = UiLayout.FontNav;
                label.color = UiStyle.TextMuted;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                var labelRect = label.rectTransform;
                labelRect.anchorMin = new Vector2(0.5f, 0f);
                labelRect.anchorMax = new Vector2(0.5f, 0f);
                labelRect.pivot = new Vector2(0.5f, 1f);
                labelRect.anchoredPosition = new Vector2(0f, -78f);
                labelRect.sizeDelta = new Vector2(150f, 36f);
            }
        }

        static void ApplyPhoneSprites(Sprite phoneIcon, Sprite phonePanel, Sprite badgeCircle)
        {
            var button = GameObject.Find("Canvas/PhoneUI/PhoneButton");
            if (button != null && phoneIcon != null)
            {
                var rect = button.GetComponent<RectTransform>();
                if (rect != null)
                    rect.sizeDelta = UiLayout.SizePhoneButton;

                var image = ReplaceWithSprite(button, phoneIcon);
                image.preserveAspect = true;
                image.raycastTarget = true;
                image.rectTransform.sizeDelta = UiLayout.SizePhoneButton;
                var btn = button.GetComponent<Button>();
                if (btn != null)
                {
                    btn.targetGraphic = image;
                    btn.interactable = true;
                }

                var iconText = button.transform.Find("Icon")?.GetComponent<TextMeshProUGUI>();
                if (iconText != null)
                    iconText.gameObject.SetActive(false);
            }

            var badge = GameObject.Find("Canvas/PhoneUI/PhoneButton/Badge");
            if (badge != null && badgeCircle != null)
            {
                var procedural = badge.GetComponent<ProceduralImage>();
                if (procedural != null)
                    DestroyEditorObject(procedural);

                var badgeRect = badge.GetComponent<RectTransform>();
                if (badgeRect != null)
                {
                    badgeRect.anchorMin = new Vector2(1f, 1f);
                    badgeRect.anchorMax = new Vector2(1f, 1f);
                    badgeRect.pivot = new Vector2(0.5f, 0.5f);
                    badgeRect.anchoredPosition = new Vector2(8f, 8f);
                    badgeRect.sizeDelta = UiLayout.SizePhoneBadge;
                }

                var image = GetOrAddImage(badge);
                image.sprite = badgeCircle;
                image.color = Color.white;
                image.preserveAspect = true;
                image.rectTransform.sizeDelta = UiLayout.SizePhoneBadge;
            }

            var panel = GameObject.Find("Canvas/PhoneUI/PhoneOverlay/Panel");
            if (panel != null && phonePanel != null)
            {
                var panelRect = panel.GetComponent<RectTransform>();
                if (panelRect != null)
                    panelRect.sizeDelta = UiLayout.SizePhonePanel;

                var procedural = panel.GetComponent<ProceduralImage>();
                if (procedural != null)
                    procedural.color = new Color(1f, 1f, 1f, 0f);

                var bg = EnsureChild(panel.transform, "PanelSprite");
                bg.transform.SetAsFirstSibling();
                var bgRect = bg.GetComponent<RectTransform>();
                bgRect.anchorMin = new Vector2(0.5f, 0.5f);
                bgRect.anchorMax = new Vector2(0.5f, 0.5f);
                bgRect.pivot = new Vector2(0.5f, 0.5f);
                bgRect.anchoredPosition = Vector2.zero;
                var image = GetOrAddImage(bg);
                image.sprite = phonePanel;
                image.color = Color.white;
                image.raycastTarget = false;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                ApplySpriteNativeSize(image);
                bgRect.sizeDelta = image.rectTransform.sizeDelta;
            }
        }

        static void ApplySpriteNativeSize(Image image) =>
            ApplyUiSpriteSize(image, null);

        /// <summary>SetNativeSize из Figma-экспорта может дать тысячи px — всегда ограничиваем эталоном UI.</summary>
        static void ApplyUiSpriteSize(Image image, Vector2? maxSize)
        {
            if (image == null || image.sprite == null)
                return;

            image.preserveAspect = true;
            if (maxSize.HasValue)
            {
                image.rectTransform.sizeDelta = maxSize.Value;
                return;
            }

            image.SetNativeSize();
            var size = image.rectTransform.sizeDelta;
            if (size.x > 2048f || size.y > 2048f)
                image.rectTransform.sizeDelta = new Vector2(
                    Mathf.Min(size.x, 512f),
                    Mathf.Min(size.y, 512f));
        }

        static void ApplyChoicePrefab(Sprite buttonBg, Sprite humanIcon, Sprite humanPortrait)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ChoicePrefabPath) == null)
                return;

            var root = PrefabUtility.LoadPrefabContents(ChoicePrefabPath);
            if (root == null)
                return;

            try
            {
                if (buttonBg != null)
                {
                    var image = ReplaceWithSprite(root, buttonBg);
                    var button = root.GetComponent<Button>();
                    if (button != null)
                        button.targetGraphic = image;
                }

                var iconGo = root.transform.Find("ChoiceIcon");
                if (iconGo != null)
                    DestroyEditorObject(iconGo.gameObject);

                var label = root.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    var labelRect = label.rectTransform;
                    labelRect.offsetMin = new Vector2(20f, 12f);
                    labelRect.offsetMax = new Vector2(-20f, -12f);
                    label.alignment = TextAlignmentOptions.Center;
                }

                var portraitSlot = root.transform.Find("PortraitSlot");
                if (humanPortrait != null && portraitSlot == null)
                {
                    var portraitGo = new GameObject("PortraitSlot", typeof(RectTransform));
                    portraitGo.transform.SetParent(root.transform, false);
                    portraitGo.SetActive(false);
                    var portraitRect = portraitGo.GetComponent<RectTransform>();
                    portraitRect.anchorMin = new Vector2(1f, 0.5f);
                    portraitRect.anchorMax = new Vector2(1f, 0.5f);
                    portraitRect.pivot = new Vector2(1f, 0.5f);
                    portraitRect.anchoredPosition = new Vector2(-16f, 0f);
                    portraitRect.sizeDelta = new Vector2(56f, 56f);
                    var portraitImage = GetOrAddImage(portraitGo);
                    portraitImage.sprite = humanPortrait;
                    portraitImage.preserveAspect = true;
                }

                PrefabUtility.SaveAsPrefabAsset(root, ChoicePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void TuneHudTypography()
        {
            SetTmpColor("Canvas/HUD/TimeText", UiStyle.TextLight);
            SetTmpColor("Canvas/HUD/RespectText", UiStyle.TextLight);
            SetTmpColor("Canvas/HUD/CalmText", UiStyle.TextLight);
            SetTmpColor("Canvas/HUD/ChapterText", new Color(0.55f, 0.75f, 0.95f, 1f));
            SetTmpColor("Canvas/HUD/PeriodBadge", new Color(0.45f, 0.82f, 1f, 1f));
            SetTmpColor("Canvas/LocationHeader", UiStyle.TextLight);
        }

        static void SetTmpColor(string path, Color color)
        {
            var tmp = GameObject.Find(path)?.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                tmp.color = color;
        }

        static Image ReplaceWithSprite(GameObject host, Sprite sprite)
        {
            foreach (var mp in host.GetComponents<MPImage>())
                DestroyEditorObject(mp);

            foreach (var proc in host.GetComponents<ProceduralImage>())
            {
                DestroyEditorObject(proc);
                var modifier = host.GetComponent<FreeModifier>();
                if (modifier != null)
                    DestroyEditorObject(modifier);
            }

            var image = host.GetComponent<Image>();
            if (image == null)
                image = host.AddComponent<Image>();

            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.raycastTarget = true;
            image.preserveAspect = true;
            return image;
        }

        static void DestroyEditorObject(Object obj)
        {
            if (obj == null)
                return;

            Object.DestroyImmediate(obj, true);
        }

        static Image GetOrAddImage(GameObject host)
        {
            var image = host.GetComponent<Image>();
            return image != null ? image : host.AddComponent<Image>();
        }

        static GameObject EnsureChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
                return existing.gameObject;

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
    }
}
