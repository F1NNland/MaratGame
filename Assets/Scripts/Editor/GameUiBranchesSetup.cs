using MaratGame.Data;
using MaratGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// MVP шаг 10: портрет в DialoguePanel + привязка Алевтины на Game.
    /// </summary>
    static class GameUiBranchesSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";
        const string AlevtinaPhotoPath = "Assets/Фото Марат/Люди/Екатерина.jpg";

        public static void EnsureBranchPortraitsOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var dialogue = Object.FindFirstObjectByType<DialogueView>();
            if (dialogue != null)
                EnsurePortraitImageOnDialogue(dialogue);

            var controller = Object.FindFirstObjectByType<GameUIController>();
            if (controller != null)
            {
                var sprite = LoadSpriteFromPhoto(AlevtinaPhotoPath);
                if (sprite != null)
                    SetCharacterPortrait(controller, CharacterIds.Alevtina, sprite);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Step 10: dialogue portrait + alevtina sprite on Game scene.");
        }

        static void EnsurePortraitImageOnDialogue(DialogueView dialogue)
        {
            var so = new SerializedObject(dialogue);
            if (so.FindProperty("portraitImage").objectReferenceValue != null)
                return;

            var panel = so.FindProperty("panel").objectReferenceValue as RectTransform;
            if (panel == null)
                return;

            var portraitGo = new GameObject("PortraitImage", typeof(RectTransform), typeof(Image));
            portraitGo.transform.SetParent(panel, false);

            var rect = portraitGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(12f, 0f);
            rect.sizeDelta = new Vector2(96f, 96f);

            var image = portraitGo.GetComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
            portraitGo.SetActive(false);

            so.FindProperty("portraitImage").objectReferenceValue = image;

            var speaker = so.FindProperty("speakerText").objectReferenceValue as TMPro.TextMeshProUGUI;
            if (speaker != null)
            {
                var speakerRect = speaker.rectTransform;
                speakerRect.offsetMin = new Vector2(120f, speakerRect.offsetMin.y);
            }

            var body = so.FindProperty("bodyText").objectReferenceValue as TMPro.TextMeshProUGUI;
            if (body != null)
                body.rectTransform.offsetMin = new Vector2(120f, body.rectTransform.offsetMin.y);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetCharacterPortrait(GameUIController controller, string characterId, Sprite sprite)
        {
            var so = new SerializedObject(controller);
            var entries = so.FindProperty("characterPortraits");
            var index = -1;

            for (var i = 0; i < entries.arraySize; i++)
            {
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("characterId").stringValue == characterId)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                index = entries.arraySize;
                entries.InsertArrayElementAtIndex(index);
                entries.GetArrayElementAtIndex(index).FindPropertyRelative("characterId").stringValue = characterId;
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
    }
}
