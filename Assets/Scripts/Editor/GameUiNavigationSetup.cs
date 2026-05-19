using MaratGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 09: NavigationBar на сцене Game + фоны локаций.
    /// </summary>
    static class GameUiNavigationSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";
        const string CanteenPhotoPath = "Assets/Фото Марат/Столовая/photo_2026-05-15_15-09-40.jpg";

        [MenuItem("MaratGame/UI/Setup Step 09 (Navigation Bar)")]
        public static void SetupStep09NavigationUi() => EnsureNavigationUiOnGameScene();

        public static void EnsureNavigationUiOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MaratGame] Game scene has no Canvas. Run Setup Step 04 first.");
                return;
            }

            UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);

            if (Object.FindFirstObjectByType<NavigationBar>() == null)
                BuildNavigationBar(canvas.transform);

            EnsureLocationBackgrounds();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Step 09 NavigationBar on Game scene.");
        }

        static NavigationBar BuildNavigationBar(Transform canvas)
        {
            var root = CreateUiObject("NavigationBar", canvas);
            var view = root.AddComponent<NavigationBar>();

            var barRoot = CreateUiObject("Bar", root.transform);
            var barRect = barRoot.GetComponent<RectTransform>();
            SetAnchors(barRect, new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.14f), Vector2.zero, Vector2.zero);
            var barGroup = barRoot.AddComponent<CanvasGroup>();

            var layout = barRoot.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var left = BuildNavButton(barRoot.transform, "NavLeft");
            var forward = BuildNavButton(barRoot.transform, "NavForward");
            var right = BuildNavButton(barRoot.transform, "NavRight");

            NavigationBar.ApplyDirectionLabels(left.label, forward.label, right.label);

            var so = new SerializedObject(view);
            so.FindProperty("barRoot").objectReferenceValue = barRoot;
            so.FindProperty("barGroup").objectReferenceValue = barGroup;
            so.FindProperty("leftButton").objectReferenceValue = left.button;
            so.FindProperty("forwardButton").objectReferenceValue = forward.button;
            so.FindProperty("rightButton").objectReferenceValue = right.button;
            so.ApplyModifiedPropertiesWithoutUndo();

            var runner = Object.FindFirstObjectByType<Narrative.StoryRunner>();
            so = new SerializedObject(view);
            so.FindProperty("storyRunner").objectReferenceValue = runner;
            so.ApplyModifiedPropertiesWithoutUndo();

            barRoot.SetActive(false);
            return view;
        }

        static (Button button, TextMeshProUGUI label) BuildNavButton(Transform parent, string name)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 72f);

            var graphic = UiKitFactory.AddRoundedButtonGraphic(go, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
            var button = go.AddComponent<Button>();
            button.targetGraphic = graphic;

            var label = TmpUiFactory.CreateText(
                "Label",
                go.transform,
                17f,
                TextAlignmentOptions.Center,
                UiStyle.TextLight);
            StretchFull(label.gameObject);
            label.lineSpacing = -4f;

            return (button, label);
        }

        public static void EnsureLocationBackgrounds()
        {
            var hallSprite = GameUiShellSetup.LoadHallSprite();
            var canteenSprite = LoadSpriteFromPhoto(CanteenPhotoPath) ?? hallSprite;

            var controller = Object.FindFirstObjectByType<GameUIController>();
            if (controller == null)
            {
                Debug.LogWarning("[MaratGame] No GameUIController — skip location backgrounds.");
                return;
            }

            SetLocationBackground(controller, "hall", hallSprite);
            SetLocationBackground(controller, "canteen", canteenSprite);
            SetLocationBackground(controller, "cabinet", hallSprite);
            SetLocationBackground(controller, "elevator", hallSprite);
        }

        static void SetLocationBackground(GameUIController controller, string locationId, Sprite sprite)
        {
            if (sprite == null)
                return;

            var so = new SerializedObject(controller);
            var entries = so.FindProperty("locationBackgrounds");
            var index = -1;

            for (var i = 0; i < entries.arraySize; i++)
            {
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("locationId").stringValue == locationId)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                index = entries.arraySize;
                entries.InsertArrayElementAtIndex(index);
                entries.GetArrayElementAtIndex(index).FindPropertyRelative("locationId").stringValue = locationId;
            }

            entries.GetArrayElementAtIndex(index).FindPropertyRelative("sprite").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Sprite LoadSpriteFromPhoto(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                    return sprite;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return tex != null
                ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f)
                : null;
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
