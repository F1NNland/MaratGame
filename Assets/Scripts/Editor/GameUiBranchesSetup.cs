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
        public static void EnsureBranchPortraitsOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var dialogue = Object.FindFirstObjectByType<DialogueView>();
            if (dialogue != null)
                EnsurePortraitImageOnDialogue(dialogue);

            GameUiPhotoBindingsSetup.RefreshAllPhotoBindings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MaratGame] Step 10: dialogue portrait frame + character sprites on Game scene.");
        }

        static void EnsurePortraitImageOnDialogue(DialogueView dialogue)
        {
            var so = new SerializedObject(dialogue);
            if (so.FindProperty("portraitImage").objectReferenceValue != null)
                return;

            var panel = so.FindProperty("panel").objectReferenceValue as RectTransform;
            if (panel == null)
                return;

            var portraitParent = panel.Find("PortraitFrame") ?? panel;
            var portraitGo = portraitParent.Find("PortraitImage")?.gameObject;
            if (portraitGo == null)
            {
                portraitGo = new GameObject("PortraitImage", typeof(RectTransform), typeof(Image));
                portraitGo.transform.SetParent(portraitParent, false);
            }

            var rect = portraitGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.offsetMin = new Vector2(14f, 14f);
            rect.offsetMax = new Vector2(-14f, -14f);

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

    }
}
