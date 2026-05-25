using MaratGame.Data;
using MaratGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MaratGame.Editor
{
    /// <summary>
    /// Post-MVP шаг 12: фоны/портреты/mediaSlot для полного дня.
    /// </summary>
    public static class StoryVisualMediaPassSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";
        const string StoryDatabasePath = "Assets/Data/Story/StoryDatabase_Test.asset";
        const string MeetingImagePath = "Assets/Фото Марат/Планерка/photo_2026-05-15_15-10-08.jpg";
        const string BigCongratulationVideoPath = "Assets/Art/placeholder/big_congratulation.mp4";
        const string ToiletWashGifPath = "Assets/Art/placeholder/toilet_wash_face.gif";
        const string HelpEmployeeGifPath = "Assets/Art/placeholder/help_employee.gif";

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 12 (Visual & Media Pass)")]
        public static void SetupPostMvpStep12VisualAndMediaPass()
        {
            StoryBirthdayEndSetup.CreateBirthdayEndContent();
            ApplyNodeMediaPass();
            GameUiNavigationSetup.EnsureNavigationUiOnGameScene();
            GameUiBranchesSetup.EnsureBranchPortraitsOnGameScene();
            EnsureMediaOverlayOnGameScene();
            Debug.Log("[MaratGame] Post-MVP step 12: visual/media pass ready.");
        }

        static void ApplyNodeMediaPass()
        {
            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(StoryDatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            var meeting = FindNode(database, Chapter2CabinetNodes.Meeting3544NodeId);
            var toiletWash = FindNode(database, Chapter2CabinetNodes.ToiletWashFaceNodeId);
            var helpEmployee = FindNode(database, Chapter2CabinetNodes.ToiletHelpEmployeeContentNodeId);
            var eveningIntro = FindNode(database, Chapter4KrrbNodes.EveningIntroNodeId);
            var eveningBank = FindNode(database, Chapter4KrrbNodes.EveningBankEmptyNodeId);
            var bigCongratulation = FindNode(database, Chapter4KrrbNodes.BigCongratulationNodeId);
            var eveningGood = FindNode(database, Chapter4KrrbNodes.EveningGoodEndingNodeId);

            if (meeting != null)
            {
                meeting.mediaSlot = MediaSlotType.Image;
                meeting.mediaPath = MeetingImagePath;
                EditorUtility.SetDirty(meeting);
            }

            if (toiletWash != null)
            {
                toiletWash.mediaSlot = MediaSlotType.Gif;
                toiletWash.mediaPath = ToiletWashGifPath;
                EditorUtility.SetDirty(toiletWash);
            }

            if (helpEmployee != null)
            {
                helpEmployee.mediaSlot = MediaSlotType.Gif;
                helpEmployee.mediaPath = HelpEmployeeGifPath;
                EditorUtility.SetDirty(helpEmployee);
            }

            if (bigCongratulation != null)
            {
                bigCongratulation.mediaSlot = MediaSlotType.Video;
                bigCongratulation.mediaPath = BigCongratulationVideoPath;
                EditorUtility.SetDirty(bigCongratulation);
            }

            SetEveningLocation(eveningIntro);
            SetEveningLocation(eveningBank);
            SetEveningLocation(bigCongratulation);
            SetEveningLocation(eveningGood);

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void EnsureMediaOverlayOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            var controller = Object.FindFirstObjectByType<GameUIController>();
            if (canvas == null || controller == null)
            {
                Debug.LogWarning("[MaratGame] Missing Canvas or GameUIController in Game scene.");
                return;
            }

            var mediaImage = EnsureMediaImage(canvas.transform);
            var (mediaVideo, videoPlayer) = EnsureMediaVideo(canvas.transform);
            var (fallbackGroup, fallbackText) = EnsureMediaFallback(canvas.transform);

            var so = new SerializedObject(controller);
            so.FindProperty("mediaImageOverlay").objectReferenceValue = mediaImage;
            so.FindProperty("mediaVideoOverlay").objectReferenceValue = mediaVideo;
            so.FindProperty("mediaVideoPlayer").objectReferenceValue = videoPlayer;
            so.FindProperty("mediaFallbackGroup").objectReferenceValue = fallbackGroup;
            so.FindProperty("mediaFallbackText").objectReferenceValue = fallbackText;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static Image EnsureMediaImage(Transform canvas)
        {
            var root = FindOrCreateUiObject(canvas, "MediaImageOverlay");
            Stretch(root.GetComponent<RectTransform>());

            var image = root.GetComponent<Image>();
            if (image == null)
                image = root.AddComponent<Image>();

            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            root.SetActive(false);
            return image;
        }

        static (RawImage image, VideoPlayer player) EnsureMediaVideo(Transform canvas)
        {
            var root = FindOrCreateUiObject(canvas, "MediaVideoOverlay");
            Stretch(root.GetComponent<RectTransform>());

            var image = root.GetComponent<RawImage>();
            if (image == null)
                image = root.AddComponent<RawImage>();
            image.color = Color.white;
            image.raycastTarget = false;

            var player = root.GetComponent<VideoPlayer>();
            if (player == null)
                player = root.AddComponent<VideoPlayer>();

            player.playOnAwake = false;
            player.isLooping = true;
            player.waitForFirstFrame = true;
            root.SetActive(false);
            return (image, player);
        }

        static (CanvasGroup group, TextMeshProUGUI text) EnsureMediaFallback(Transform canvas)
        {
            var root = FindOrCreateUiObject(canvas, "MediaFallback");
            Stretch(root.GetComponent<RectTransform>());

            var group = root.GetComponent<CanvasGroup>();
            if (group == null)
                group = root.AddComponent<CanvasGroup>();

            var dim = root.GetComponent<Image>();
            if (dim == null)
                dim = root.AddComponent<Image>();
            dim.color = new Color(0.04f, 0.06f, 0.1f, 0.55f);
            dim.raycastTarget = false;

            var textGo = root.transform.Find("Text") as RectTransform;
            if (textGo == null)
            {
                textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<RectTransform>();
                textGo.SetParent(root.transform, false);
            }

            textGo.anchorMin = new Vector2(0.1f, 0.45f);
            textGo.anchorMax = new Vector2(0.9f, 0.55f);
            textGo.offsetMin = Vector2.zero;
            textGo.offsetMax = Vector2.zero;

            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.font = TmpUiFactory.DefaultFont;
            text.fontSize = 28f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = UiStyle.TextLight;
            text.text = "MEDIA placeholder";
            text.enableWordWrapping = true;
            text.raycastTarget = false;

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            root.SetActive(false);
            return (group, text);
        }

        static GameObject FindOrCreateUiObject(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Stretch(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static StoryNodeData FindNode(StoryDatabase database, string nodeId)
        {
            if (database?.nodes == null || string.IsNullOrWhiteSpace(nodeId))
                return null;

            foreach (var node in database.nodes)
            {
                if (node != null && node.id == nodeId)
                    return node;
            }

            return null;
        }

        static void SetEveningLocation(StoryNodeData node)
        {
            if (node == null)
                return;

            node.locationId = "evening";
            EditorUtility.SetDirty(node);
        }
    }
}
