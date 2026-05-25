using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using MaratGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// Шаг 04 MVP: Canvas HUD, диалог, выборы на сцене Game. UI ref: 2001×981.
    /// </summary>
    static class GameUiShellSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";
        const string HallPhotoPath = "Assets/Фото Марат/Холл/photo_2026-05-15_15-09-51.jpg";
        const string ChoicePrefabPath = "Assets/Prefabs/UI/ChoiceButton.prefab";

        [MenuItem("MaratGame/UI/Setup Step 04 (Game Canvas)")]
        public static void SetupStep04()
        {
            StoryMvpSetup.CreateTestContent();

            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MaratGame] Game scene has no Canvas. Run Scenes/Setup Step 03 first.");
                return;
            }

            UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);
            canvas.sortingOrder = 10;
            ClearUiChildren(canvas.gameObject);

            var hallSprite = LoadHallSprite();
            var choicePrefab = GetOrCreateChoicePrefab();

            var bgCanvas = UiKitFactory.EnsureBackgroundCanvas(0);
            var hud = BuildHud(canvas.transform);
            var locationHeader = BuildLocationHeader(canvas.transform);
            var (bgGroup, imageA, imageB) = BuildBackground(bgCanvas.transform, hallSprite);
            var dialogue = BuildDialogue(canvas.transform);
            var choices = BuildChoices(canvas.transform, choicePrefab);
            var controller = canvas.gameObject.GetComponent<GameUIController>();
            if (controller == null)
                controller = canvas.gameObject.AddComponent<GameUIController>();

            WireController(controller, hud, locationHeader, bgGroup, imageA, imageB, dialogue, choices, hallSprite);
            ConfigureStoryRunner();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Step 04 UI shell ready (TMP + Procedural panel + MPImage buttons).");
        }

        static void ClearUiChildren(GameObject canvas)
        {
            for (var i = canvas.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        }

        public static Sprite LoadHallSprite()
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(HallPhotoPath))
            {
                if (asset is Sprite sprite)
                    return sprite;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(HallPhotoPath);
            return tex != null
                ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f)
                : null;
        }

        /// <summary>
        /// Без пересборки Canvas: фон холла и locationBackgrounds на Game.
        /// </summary>
        public static void EnsureHallBackgroundOnGameScene()
        {
            var hallSprite = LoadHallSprite();
            if (hallSprite == null)
            {
                Debug.LogWarning("[MaratGame] Hall photo not found at " + HallPhotoPath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var controller = Object.FindFirstObjectByType<GameUIController>();
            if (controller == null)
            {
                Debug.LogWarning("[MaratGame] Game scene has no GameUIController. Run Setup Step 04 first.");
                return;
            }

            var so = new SerializedObject(controller);
            var imageA = so.FindProperty("backgroundImageA").objectReferenceValue as Image;
            var imageB = so.FindProperty("backgroundImageB").objectReferenceValue as Image;
            if (imageA != null)
            {
                imageA.sprite = hallSprite;
                imageA.preserveAspect = true;
            }

            if (imageB != null)
            {
                imageB.sprite = hallSprite;
                imageB.preserveAspect = true;
            }

            var entries = so.FindProperty("locationBackgrounds");
            var hallIndex = -1;
            for (var i = 0; i < entries.arraySize; i++)
            {
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("locationId").stringValue == "hall")
                {
                    hallIndex = i;
                    break;
                }
            }

            if (hallIndex < 0)
            {
                hallIndex = entries.arraySize;
                entries.InsertArrayElementAtIndex(hallIndex);
                entries.GetArrayElementAtIndex(hallIndex).FindPropertyRelative("locationId").stringValue = "hall";
            }

            entries.GetArrayElementAtIndex(hallIndex).FindPropertyRelative("sprite").objectReferenceValue = hallSprite;
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsurePeriodBadgeOnHud();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static void EnsurePeriodBadgeOnHud()
        {
            var hud = Object.FindFirstObjectByType<GameHudView>();
            if (hud == null)
                return;

            var so = new SerializedObject(hud);
            if (so.FindProperty("periodBadgeText").objectReferenceValue != null)
            {
                hud.SetPeriodBadge("🎂 Утро");
                return;
            }

            var hudTransform = hud.transform;
            var periodBadge = CreateTmpText("PeriodBadge", hudTransform, (int)UiLayout.FontHudPeriod, TextAnchor.UpperCenter, UiStyle.TextMuted);
            SetAnchors(periodBadge.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), UiLayout.SizeHudPeriod);
            periodBadge.text = "🎂 Утро";
            so.FindProperty("periodBadgeText").objectReferenceValue = periodBadge;
            so.ApplyModifiedPropertiesWithoutUndo();
            hud.SetPeriodBadge("🎂 Утро");
        }

        static GameHudView BuildHud(Transform parent)
        {
            var root = CreateUiObject("HUD", parent);
            StretchFull(root);
            var group = root.AddComponent<CanvasGroup>();

            var time = CreateTmpText("TimeText", root.transform, (int)UiLayout.FontHudTime, TextAnchor.UpperLeft, UiStyle.TextLight);
            SetAnchors(time.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -20f), UiLayout.SizeHudTime);
            time.text = GameDefaults.StartTime;

            var respect = CreateTmpText("RespectText", root.transform, (int)UiLayout.FontHudStat, TextAnchor.UpperRight, UiStyle.TextMuted);
            SetAnchors(respect.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -20f), UiLayout.SizeHudStat);
            respect.text = $"Уважение {GameDefaults.StartRespect}%";

            var calm = CreateTmpText("CalmText", root.transform, (int)UiLayout.FontHudStat, TextAnchor.UpperRight, UiStyle.TextMuted);
            SetAnchors(calm.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -60f), UiLayout.SizeHudStat);
            calm.text = $"Спокойствие {GameDefaults.StartCalm}%";

            var chapter = CreateTmpText("ChapterText", root.transform, (int)UiLayout.FontHudChapter, TextAnchor.UpperCenter, UiStyle.TextMuted);
            SetAnchors(chapter.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), UiLayout.SizeHudChapter);

            var periodBadge = CreateTmpText("PeriodBadge", root.transform, (int)UiLayout.FontHudPeriod, TextAnchor.UpperCenter, UiStyle.TextMuted);
            SetAnchors(periodBadge.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), UiLayout.SizeHudPeriod);
            periodBadge.text = "🎂 Утро";

            var view = root.AddComponent<GameHudView>();
            var so = new SerializedObject(view);
            so.FindProperty("canvasGroup").objectReferenceValue = group;
            so.FindProperty("timeText").objectReferenceValue = time;
            so.FindProperty("respectText").objectReferenceValue = respect;
            so.FindProperty("calmText").objectReferenceValue = calm;
            so.FindProperty("chapterText").objectReferenceValue = chapter;
            so.FindProperty("periodBadgeText").objectReferenceValue = periodBadge;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        static TextMeshProUGUI BuildLocationHeader(Transform parent)
        {
            var go = CreateUiObject("LocationHeader", parent);
            var rect = go.GetComponent<RectTransform>();
            SetAnchors(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), UiLayout.SizeLocationHeader);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = TmpUiFactory.DefaultFont;
            text.fontSize = UiLayout.FontLocationHeader;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = UiStyle.TextLight;
            text.text = LocationLabels.GetDisplayName(GameDefaults.StartLocationId);
            return text;
        }

        static (CanvasGroup group, Image a, Image b) BuildBackground(Transform parent, Sprite hallSprite)
        {
            var root = CreateUiObject("Background", parent);
            StretchFull(root);
            root.transform.SetAsFirstSibling();
            var group = root.AddComponent<CanvasGroup>();
            group.alpha = 1f;

            var imageA = CreateBackgroundImage("BackgroundImageA", root.transform, hallSprite);
            var imageB = CreateBackgroundImage("BackgroundImageB", root.transform, hallSprite);
            imageB.gameObject.SetActive(false);
            return (group, imageA, imageB);
        }

        static Image CreateBackgroundImage(string name, Transform parent, Sprite sprite)
        {
            var go = CreateUiObject(name, parent);
            StretchFull(go);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = UiStyle.PhotoBackground;
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
            return image;
        }

        static DialogueView BuildDialogue(Transform parent)
        {
            var root = CreateUiObject("DialoguePanel", parent);
            var panelRect = root.GetComponent<RectTransform>();
            SetAnchors(panelRect, new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.4f), Vector2.zero, Vector2.zero);
            UiKitFactory.AddRoundedPanel(root, UiStyle.PanelBackground, UiStyle.PanelCornerRadius);
            var group = root.AddComponent<CanvasGroup>();

            var portraitGo = CreateUiObject("PortraitImage", root.transform);
            var portraitRect = portraitGo.GetComponent<RectTransform>();
            SetAnchors(portraitRect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(16f, 0f), new Vector2(UiLayout.DialoguePortraitWidth, 0f));
            portraitRect.pivot = new Vector2(0f, 0.5f);
            var portraitImage = portraitGo.AddComponent<Image>();
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;
            portraitGo.SetActive(false);

            var speaker = CreateTmpText("SpeakerText", root.transform, (int)UiLayout.FontDialogueSpeaker, TextAnchor.UpperLeft, UiStyle.TextLight);
            SetAnchors(speaker.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(140f, -14f), new Vector2(-32f, 44f));
            speaker.fontStyle = FontStyles.Bold;

            var body = CreateTmpText("BodyText", root.transform, (int)UiLayout.FontDialogueBody, TextAnchor.UpperLeft, UiStyle.TextLight);
            SetAnchors(body.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(140f, 20f), new Vector2(-32f, -56f));

            var view = root.AddComponent<DialogueView>();
            var so = new SerializedObject(view);
            so.FindProperty("canvasGroup").objectReferenceValue = group;
            so.FindProperty("panel").objectReferenceValue = panelRect;
            so.FindProperty("portraitImage").objectReferenceValue = portraitImage;
            so.FindProperty("speakerText").objectReferenceValue = speaker;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        static ChoicesView BuildChoices(Transform parent, Button choicePrefab)
        {
            var root = CreateUiObject("ChoicesContainer", parent);
            var rect = root.GetComponent<RectTransform>();
            SetAnchors(rect, new Vector2(0.15f, 0.02f), new Vector2(0.85f, 0.15f), Vector2.zero, Vector2.zero);

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = root.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var view = root.AddComponent<ChoicesView>();
            var so = new SerializedObject(view);
            so.FindProperty("container").objectReferenceValue = rect;
            so.FindProperty("choiceButtonPrefab").objectReferenceValue = choicePrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        static Button GetOrCreateChoicePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<Button>(ChoicePrefabPath) != null)
                AssetDatabase.DeleteAsset(ChoicePrefabPath);

            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/UI");

            var go = new GameObject("ChoiceButton");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = UiLayout.SizeChoiceButton;
            UiKitFactory.AddLayoutElement(go, UiLayout.SizeChoiceButton);

            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            var graphic = UiKitFactory.AddRoundedButtonGraphic(go, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);

            var button = go.AddComponent<Button>();
            button.targetGraphic = graphic;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            StretchFull(labelRect.gameObject);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.font = TmpUiFactory.DefaultFont;
            label.fontSize = UiLayout.FontChoice;
            label.alignment = TextAlignmentOptions.Center;
            label.color = UiStyle.TextLight;
            label.text = "Выбор";

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ChoicePrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Button>();
        }

        static void WireController(
            GameUIController controller,
            GameHudView hud,
            TextMeshProUGUI locationHeader,
            CanvasGroup bgGroup,
            Image imageA,
            Image imageB,
            DialogueView dialogue,
            ChoicesView choices,
            Sprite hallSprite)
        {
            var runner = Object.FindFirstObjectByType<StoryRunner>();
            var so = new SerializedObject(controller);
            so.FindProperty("storyRunner").objectReferenceValue = runner;
            so.FindProperty("hud").objectReferenceValue = hud;
            so.FindProperty("locationHeader").objectReferenceValue = locationHeader;
            so.FindProperty("backgroundGroup").objectReferenceValue = bgGroup;
            so.FindProperty("backgroundImageA").objectReferenceValue = imageA;
            so.FindProperty("backgroundImageB").objectReferenceValue = imageB;
            so.FindProperty("dialogue").objectReferenceValue = dialogue;
            so.FindProperty("choices").objectReferenceValue = choices;

            var entries = so.FindProperty("locationBackgrounds");
            entries.arraySize = 1;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("locationId").stringValue = "hall";
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("sprite").objectReferenceValue = hallSprite;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ConfigureStoryRunner()
        {
            var runner = Object.FindFirstObjectByType<StoryRunner>();
            if (runner == null)
                return;

            var so = new SerializedObject(runner);
            so.FindProperty("showDebugGui").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static TextMeshProUGUI CreateTmpText(string name, Transform parent, int fontSize, TextAnchor anchor, Color color)
        {
            return TmpUiFactory.CreateText(
                name,
                parent,
                fontSize,
                TmpUiFactory.FromLegacyAnchor(anchor),
                color);
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
