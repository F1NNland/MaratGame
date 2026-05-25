using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Подписка на StoryRunner.OnNodeChanged и анимация UI через UiTweens.
    /// </summary>
    public sealed class GameUIController : MonoBehaviour
    {
        [SerializeField] StoryRunner storyRunner;
        [SerializeField] GameHudView hud;
        [SerializeField] TextMeshProUGUI locationHeader;
        [SerializeField] CanvasGroup backgroundGroup;
        [SerializeField] Image backgroundImageA;
        [SerializeField] Image backgroundImageB;
        [SerializeField] DialogueView dialogue;
        [SerializeField] ChoicesView choices;
        [SerializeField] PhoneUI phoneUi;
        [SerializeField] CanvasGroup mediaFallbackGroup;
        [SerializeField] TextMeshProUGUI mediaFallbackText;
        [SerializeField] Image mediaImageOverlay;
        [SerializeField] RawImage mediaVideoOverlay;
        [SerializeField] VideoPlayer mediaVideoPlayer;

        [SerializeField] List<LocationBackgroundEntry> locationBackgrounds = new();
        [SerializeField] List<CharacterPortraitEntry> characterPortraits = new();

        Image _activeBackground;
        Image _inactiveBackground;
        string _currentLocationId;
        bool _introPlayed;
        Texture2D _runtimeMediaTexture;
        Sprite _runtimeMediaSprite;
        RenderTexture _mediaRenderTexture;
        bool _videoHandlersBound;

        [System.Serializable]
        public struct LocationBackgroundEntry
        {
            public string locationId;
            public Sprite sprite;
        }

        [System.Serializable]
        public struct CharacterPortraitEntry
        {
            public string characterId;
            public Sprite sprite;
        }

        void Awake()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            UiInputBootstrap.EnsureUiInput();

            _activeBackground = backgroundImageA;
            _inactiveBackground = backgroundImageB;

            if (_activeBackground != null)
            {
                ApplyPhotoTint(_activeBackground);
                SetImageAlpha(_activeBackground, 1f);
                _activeBackground.gameObject.SetActive(true);
            }

            if (_inactiveBackground != null)
            {
                ApplyPhotoTint(_inactiveBackground);
                _inactiveBackground.gameObject.SetActive(false);
            }

            if (backgroundGroup != null)
                backgroundGroup.alpha = 1f;
            NormalizeBackgroundRootRect();
            EnsureLocationBackgroundFallbacks();
            EnsureBackgroundImagesEnabled();

            hud?.BindInitialState(GameState.Instance);
            ApplyInitialBackground(GameState.Instance?.CurrentLocationId ?? GameDefaults.StartLocationId);
        }

        void OnEnable()
        {
            if (storyRunner != null)
                storyRunner.OnNodeChanged += HandleNodeChanged;
        }

        void OnDisable()
        {
            if (storyRunner != null)
                storyRunner.OnNodeChanged -= HandleNodeChanged;
        }

        void Start()
        {
            if (!_introPlayed && hud != null)
            {
                _introPlayed = true;
                hud.PlayIntroFade();
            }
        }

        void HandleNodeChanged(StoryNodeData node)
        {
            if (node == null)
                return;

            if (phoneUi == null)
                phoneUi = FindFirstObjectByType<PhoneUI>();

            SyncPhoneInboxForNode(node);

            var state = GameState.Instance;

            hud?.SetTime(state.CurrentTime);
            hud?.SetChapter(node.chapterLabel);
            hud?.SetPeriodBadge(ResolveDayBlockBadge(state.CurrentDayBlock));
            hud?.AnimateStats(state.Stats.Respect, state.Stats.Calm, state.Stats.Chaos);

            if (IsFinalEndingNode(node))
            {
                choices?.ClearChoices();
                HideDialogueForEnding();
                ClearStoryMedia();

                if (locationHeader != null)
                    locationHeader.text = LocationLabels.FormatChapterLocation(node.chapterLabel, state.CurrentLocationId);

                return;
            }

            if (node.id == BirthdayEndNodes.BirthdaySceneNodeId)
            {
                EnsureChoicesVisible();
                ApplyNodeContent(node, state);
                if (dialogue?.Panel != null && dialogue.CanvasGroup != null)
                    UiTweens.RevealDialogue(dialogue.Panel, dialogue.CanvasGroup, ResolveRevealDuration(node));
                choices?.BuildChoices(storyRunner.GetCurrentChoiceAvailability(), storyRunner.SelectChoice);
                return;
            }

            if (node.uiMode == StoryUiMode.PhoneInbox)
            {
                EnsureChoicesVisible();
                ApplyNodeContent(node, state);
                choices?.BuildChoices(storyRunner.GetCurrentChoiceAvailability(), storyRunner.SelectChoice);
                return;
            }

            EnsureChoicesVisible();
            dialogue?.CancelPresentation();

            if (dialogue?.CanvasGroup != null)
                dialogue.CanvasGroup.gameObject.SetActive(true);

            var availability = storyRunner.GetCurrentChoiceAvailability();
            ApplyNodeContent(node, state);
            choices?.BuildChoices(availability, storyRunner.SelectChoice);

            UiTweens.TransitionNode(
                backgroundGroup,
                dialogue?.CanvasGroup,
                dialogue?.Panel,
                null,
                ResolveRevealDuration(node));
        }

        void EnsureChoicesVisible()
        {
            if (choices == null)
                return;

            choices.gameObject.SetActive(true);
            var cg = choices.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        void ApplyNodeContent(StoryNodeData node, GameState state)
        {
            if (locationHeader != null)
                locationHeader.text = LocationLabels.FormatChapterLocation(node.chapterLabel, state.CurrentLocationId);

            hud?.SetChapter(string.Empty);

            var uiMode = ResolveUiMode(node);
            if (uiMode == StoryUiMode.PhoneInbox)
                dialogue?.SetContent(string.Empty, string.Empty);
            else
                dialogue?.SetContent(node.speaker, node.bodyText);

            dialogue?.ApplyUiMode(uiMode);
            dialogue?.SetPortrait(ResolvePortrait(node.portraitCharacterId));
            ApplyMedia(node);
            UpdateBackground(state.CurrentLocationId);
        }

        static bool IsFinalEndingNode(StoryNodeData node) =>
            node != null &&
            (node.id == Chapter4KrrbNodes.BigCongratulationNodeId ||
             node.id == Chapter4KrrbNodes.EveningGoodEndingNodeId);

        void HideDialogueForEnding()
        {
            if (dialogue == null)
                return;

            if (dialogue.CanvasGroup != null)
                UiTweens.Kill(dialogue.CanvasGroup);
            if (dialogue.Panel != null)
                UiTweens.Kill(dialogue.Panel);

            dialogue.SetContent(string.Empty, string.Empty);

            if (dialogue.CanvasGroup != null)
            {
                dialogue.CanvasGroup.alpha = 0f;
                dialogue.CanvasGroup.gameObject.SetActive(false);
            }
        }

        void ClearStoryMedia() => ResetMediaPresentation();

        static StoryUiMode ResolveUiMode(StoryNodeData node)
        {
            if (node == null)
                return StoryUiMode.Dialogue;

            if (node.uiMode == StoryUiMode.PhoneInbox)
                return StoryUiMode.PhoneInbox;

            if (node.uiMode == StoryUiMode.Monologue)
                return StoryUiMode.Monologue;

            if (node.uiMode == StoryUiMode.System)
                return StoryUiMode.System;

            if (!string.IsNullOrWhiteSpace(node.speaker))
                return StoryUiMode.Dialogue;

            return StoryUiMode.Monologue;
        }

        void SyncPhoneInboxForNode(StoryNodeData node)
        {
            if (phoneUi == null)
                return;

            if (node.uiMode == StoryUiMode.PhoneInbox)
            {
                var messages = ResolveInboxMessages(node);
                if (messages != null && messages.Count > 0 && !phoneUi.IsStoryInboxMode)
                    phoneUi.OpenStoryInbox(messages);
                return;
            }

            if (phoneUi.IsStoryInboxMode)
                phoneUi.CloseStoryInbox();
        }

        static IReadOnlyList<string> ResolveInboxMessages(StoryNodeData node)
        {
            if (node == null)
                return null;

            return IncomingMessagesContent.TryGetForNode(node.id, out var messages)
                ? messages
                : null;
        }

        static float ResolveRevealDuration(StoryNodeData node)
        {
            return ResolveUiMode(node) switch
            {
                StoryUiMode.Monologue => UiTweens.Slow,
                StoryUiMode.System => UiTweens.Fast,
                _ => UiTweens.Normal
            };
        }

        void ApplyInitialBackground(string locationId)
        {
            if (_activeBackground == null)
                return;

            var sprite = ResolveBackground(locationId) ?? _activeBackground.sprite;
            if (sprite == null)
                return;

            _currentLocationId = locationId ?? string.Empty;
            _activeBackground.sprite = sprite;
            _activeBackground.preserveAspect = false;
            ApplyPhotoTint(_activeBackground);
            SetImageAlpha(_activeBackground, 1f);
        }

        void UpdateBackground(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
                return;

            EnsureBackgroundImagesEnabled();

            var sprite = ResolveBackground(locationId) ?? ResolveBackground("hall");
            if (sprite == null || _activeBackground == null)
                return;

            if (locationId == _currentLocationId && _activeBackground.sprite == sprite)
                return;

            _currentLocationId = locationId;

            if (_inactiveBackground == null || _activeBackground.sprite == sprite)
            {
                ApplyBackgroundImmediate(sprite);
                return;
            }

            _inactiveBackground.sprite = sprite;
            _inactiveBackground.preserveAspect = false;
            ApplyPhotoTint(_inactiveBackground);
            SetImageAlpha(_inactiveBackground, 1f);
            _inactiveBackground.gameObject.SetActive(true);

            UiTweens.CrossFadeImage(_activeBackground, _inactiveBackground);

            (_activeBackground, _inactiveBackground) = (_inactiveBackground, _activeBackground);
        }

        void ApplyBackgroundImmediate(Sprite sprite)
        {
            if (sprite == null || _activeBackground == null)
                return;

            UiTweens.Kill(_activeBackground);
            if (_inactiveBackground != null)
                UiTweens.Kill(_inactiveBackground);

            _activeBackground.sprite = sprite;
            _activeBackground.preserveAspect = false;
            ApplyPhotoTint(_activeBackground);
            SetImageAlpha(_activeBackground, 1f);
            _activeBackground.gameObject.SetActive(true);

            if (_inactiveBackground == null)
                return;

            _inactiveBackground.gameObject.SetActive(false);
            SetImageAlpha(_inactiveBackground, 1f);
        }

        void EnsureBackgroundImagesEnabled()
        {
            if (backgroundGroup != null)
            {
                backgroundGroup.gameObject.SetActive(true);
                backgroundGroup.alpha = 1f;
            }

            foreach (var image in new[] { _activeBackground, _inactiveBackground })
            {
                if (image == null)
                    continue;

                image.enabled = true;
            }
        }

        static void ApplyPhotoTint(Image image)
        {
            if (image == null)
                return;

            image.color = UiStyle.PhotoBackground;
        }

        void NormalizeBackgroundRootRect()
        {
            var rootRect = backgroundGroup != null ? backgroundGroup.GetComponent<RectTransform>() : null;
            if (rootRect == null)
                return;

            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = Vector2.zero;
        }

        void EnsureLocationBackgroundFallbacks()
        {
            var hall = ResolveBackground("hall") ?? _activeBackground?.sprite ?? _inactiveBackground?.sprite;
            if (hall == null)
                return;

            var canteen = ResolveBackground("canteen") ?? hall;
            var toilet = ResolveBackground("toilet") ?? hall;
            var planerka = ResolveBackground("planerka") ?? hall;
            var krrb = ResolveBackground("krrb") ?? hall;

            EnsureLocationBackground("hall", hall);
            EnsureLocationBackground("canteen", canteen);
            EnsureLocationBackground("cabinet", hall);
            EnsureLocationBackground("elevator", hall);
            EnsureLocationBackground("toilet", toilet);
            EnsureLocationBackground("planerka", planerka);
            EnsureLocationBackground("meeting_room", planerka);
            EnsureLocationBackground("krrb", krrb);
            EnsureLocationBackground("evening", hall);

#if UNITY_EDITOR
            PopulateMissingLocationSpritesFromPhotos();
            PopulateMissingCharacterPortraitsFromPhotos();
#endif
        }

        void EnsureLocationBackground(string locationId, Sprite sprite)
        {
            if (string.IsNullOrWhiteSpace(locationId) || sprite == null)
                return;

            for (var i = 0; i < locationBackgrounds.Count; i++)
            {
                if (locationBackgrounds[i].locationId != locationId)
                    continue;

                if (locationBackgrounds[i].sprite == sprite)
                    return;

                locationBackgrounds[i] = new LocationBackgroundEntry
                {
                    locationId = locationId,
                    sprite = sprite
                };
                return;
            }

            locationBackgrounds.Add(new LocationBackgroundEntry
            {
                locationId = locationId,
                sprite = sprite
            });
        }

        void ApplyMedia(StoryNodeData node)
        {
            ResetMediaPresentation();
            if (node == null || node.mediaSlot == MediaSlotType.None)
                return;

            switch (node.mediaSlot)
            {
                case MediaSlotType.Image:
                    if (TryShowImageMedia(node.mediaPath))
                        return;

                    ShowMediaFallback(node.mediaSlot, node.mediaPath, "image unavailable");
                    return;
                case MediaSlotType.Gif:
                    if (TryShowGifMedia(node.mediaPath))
                        return;

                    ShowMediaFallback(node.mediaSlot, node.mediaPath, "gif placeholder");
                    return;
                case MediaSlotType.Video:
                    if (TryShowVideoMedia(node.mediaPath))
                        return;

                    ShowMediaFallback(node.mediaSlot, node.mediaPath, "video unavailable");
                    return;
                default:
                    ShowMediaFallback(node.mediaSlot, node.mediaPath, "media unavailable");
                    return;
            }
        }

        void ResetMediaPresentation()
        {
            if (mediaImageOverlay != null)
            {
                mediaImageOverlay.gameObject.SetActive(false);
                mediaImageOverlay.raycastTarget = false;
            }

            if (mediaVideoOverlay != null)
            {
                mediaVideoOverlay.gameObject.SetActive(false);
                mediaVideoOverlay.raycastTarget = false;
                mediaVideoOverlay.texture = null;
            }

            if (mediaVideoPlayer != null)
                mediaVideoPlayer.Stop();

            HideMediaFallback();
            ClearRuntimeMediaSprite();
        }

        bool TryShowImageMedia(string mediaPath)
        {
            if (mediaImageOverlay == null)
                return false;

            if (!TryCreateRuntimeSprite(mediaPath, out var sprite))
                return false;

            mediaImageOverlay.sprite = sprite;
            mediaImageOverlay.preserveAspect = true;
            mediaImageOverlay.color = Color.white;
            mediaImageOverlay.gameObject.SetActive(true);
            return true;
        }

        bool TryShowGifMedia(string mediaPath)
        {
            // GIF-анимация пока не подключена: если это не GIF-файл, показываем как обычное изображение.
            if (!string.IsNullOrWhiteSpace(mediaPath) && mediaPath.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase))
                return false;

            return TryShowImageMedia(mediaPath);
        }

        bool TryShowVideoMedia(string mediaPath)
        {
            if (mediaVideoOverlay == null || mediaVideoPlayer == null)
                return false;

            var fullPath = ResolveAbsoluteMediaPath(mediaPath);
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            {
                Debug.LogWarning($"[GameUIController] Video file missing: {mediaPath}", this);
                return false;
            }

            EnsureVideoHandlersBound();
            EnsureVideoRenderTexture();

            if (_mediaRenderTexture == null)
                return false;

            mediaVideoOverlay.texture = _mediaRenderTexture;
            mediaVideoOverlay.gameObject.SetActive(true);

            mediaVideoPlayer.source = VideoSource.Url;
            mediaVideoPlayer.url = BuildFileVideoUrl(fullPath);
            mediaVideoPlayer.playOnAwake = false;
            mediaVideoPlayer.isLooping = true;
            mediaVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            mediaVideoPlayer.targetTexture = _mediaRenderTexture;
            mediaVideoPlayer.Play();
            return true;
        }

        void ShowMediaFallback(MediaSlotType slot, string mediaPath, string reason)
        {
            if (mediaFallbackGroup == null)
                return;

            mediaFallbackGroup.gameObject.SetActive(true);
            mediaFallbackGroup.alpha = 1f;
            mediaFallbackGroup.interactable = false;
            mediaFallbackGroup.blocksRaycasts = false;

            if (mediaFallbackText != null)
                mediaFallbackText.text = $"{ResolveMediaLabel(slot)} placeholder: {reason}";

            if (!string.IsNullOrWhiteSpace(mediaPath))
                Debug.LogWarning($"[GameUIController] {ResolveMediaLabel(slot)} fallback for path: {mediaPath}", this);
        }

        void HideMediaFallback()
        {
            if (mediaFallbackGroup == null)
                return;

            mediaFallbackGroup.gameObject.SetActive(false);
            mediaFallbackGroup.alpha = 0f;
            mediaFallbackGroup.interactable = false;
            mediaFallbackGroup.blocksRaycasts = false;
        }

        bool TryCreateRuntimeSprite(string mediaPath, out Sprite sprite)
        {
            sprite = null;
            var fullPath = ResolveAbsoluteMediaPath(mediaPath);
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            {
                Debug.LogWarning($"[GameUIController] Media file missing: {mediaPath}", this);
                return false;
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(fullPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameUIController] Failed to read media file '{mediaPath}': {ex.Message}", this);
                return false;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes, markNonReadable: false))
            {
                Destroy(texture);
                return false;
            }

            ClearRuntimeMediaSprite();
            _runtimeMediaTexture = texture;
            _runtimeMediaSprite = Sprite.Create(
                _runtimeMediaTexture,
                new Rect(0f, 0f, _runtimeMediaTexture.width, _runtimeMediaTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite = _runtimeMediaSprite;
            return true;
        }

        static string ResolveMediaLabel(MediaSlotType slot)
        {
            return slot switch
            {
                MediaSlotType.Image => "IMAGE",
                MediaSlotType.Gif => "GIF",
                MediaSlotType.Video => "VIDEO",
                _ => "MEDIA"
            };
        }

        static string ResolveDayBlockBadge(DayBlock dayBlock)
        {
            return dayBlock switch
            {
                DayBlock.Morning => "Утро",
                DayBlock.BeforeMeeting => "До планерки",
                DayBlock.AfterMeeting => "После планерки",
                DayBlock.KrrbUk => "КРРБ / УК",
                DayBlock.Evening => "Вечер",
                DayBlock.Final => "Финал",
                _ => null
            };
        }

        Sprite ResolveBackground(string locationId)
        {
            foreach (var entry in locationBackgrounds)
            {
                if (entry.locationId == locationId && entry.sprite != null)
                    return entry.sprite;
            }

            return null;
        }

#if UNITY_EDITOR
        void PopulateMissingLocationSpritesFromPhotos()
        {
            foreach (var locationId in new[]
                     {
                         "hall", "canteen", "cabinet", "elevator", "toilet", "planerka", "meeting_room", "krrb",
                         "evening"
                     })
            {
                if (ResolveBackground(locationId) != null)
                    continue;

                var path = LocationPhotoPaths.ResolveAssetPath(locationId);
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                var sprite = LoadSpriteFromAssetPath(path);
                if (sprite != null)
                    EnsureLocationBackground(locationId, sprite);
            }
        }

        void PopulateMissingCharacterPortraitsFromPhotos()
        {
            foreach (var characterId in new[] { CharacterIds.Alevtina, CharacterIds.Kozlikhin, CharacterIds.Nozdrikov })
            {
                if (ResolvePortrait(characterId) != null)
                    continue;

                var path = CharacterPhotoPaths.ResolveAssetPath(characterId);
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                var sprite = LoadSpriteFromAssetPath(path);
                if (sprite != null)
                    EnsureCharacterPortrait(characterId, sprite);
            }
        }

        void EnsureCharacterPortrait(string characterId, Sprite sprite)
        {
            if (string.IsNullOrWhiteSpace(characterId) || sprite == null)
                return;

            for (var i = 0; i < characterPortraits.Count; i++)
            {
                if (characterPortraits[i].characterId != characterId)
                    continue;

                if (characterPortraits[i].sprite == sprite)
                    return;

                characterPortraits[i] = new CharacterPortraitEntry
                {
                    characterId = characterId,
                    sprite = sprite
                };
                return;
            }

            characterPortraits.Add(new CharacterPortraitEntry
            {
                characterId = characterId,
                sprite = sprite
            });
        }

        static Sprite LoadSpriteFromAssetPath(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                    return sprite;
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return texture != null
                ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f)
                : null;
        }
#endif

        Sprite ResolvePortrait(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return null;

            foreach (var entry in characterPortraits)
            {
                if (entry.characterId == characterId && entry.sprite != null)
                    return entry.sprite;
            }

            return null;
        }

        static void SetImageAlpha(Image image, float alpha)
        {
            if (image == null)
                return;

            var c = image.color;
            c.a = alpha;
            image.color = c;
        }

        void OnDestroy()
        {
            if (_videoHandlersBound && mediaVideoPlayer != null)
                mediaVideoPlayer.errorReceived -= HandleVideoError;

            if (_mediaRenderTexture != null)
                Destroy(_mediaRenderTexture);

            ClearRuntimeMediaSprite();

            if (backgroundGroup != null)
                UiTweens.Kill(backgroundGroup);
            dialogue?.OnDestroyCleanup();
        }

        void EnsureVideoHandlersBound()
        {
            if (_videoHandlersBound || mediaVideoPlayer == null)
                return;

            mediaVideoPlayer.errorReceived += HandleVideoError;
            _videoHandlersBound = true;
        }

        void EnsureVideoRenderTexture()
        {
            if (_mediaRenderTexture != null)
                return;

            _mediaRenderTexture = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32)
            {
                name = "MediaVideoOverlayRT"
            };
            _mediaRenderTexture.Create();
        }

        static string ResolveAbsoluteMediaPath(string mediaPath)
        {
            if (string.IsNullOrWhiteSpace(mediaPath) || !mediaPath.StartsWith("Assets/", System.StringComparison.Ordinal))
                return null;

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
                return null;

            var relativePath = mediaPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(projectRoot, "Assets", relativePath);
        }

        static string BuildFileVideoUrl(string fullPath)
        {
            var normalizedPath = fullPath.Replace('\\', '/');
            return $"file:///{normalizedPath}";
        }

        void HandleVideoError(VideoPlayer source, string message)
        {
            if (source == null || source != mediaVideoPlayer)
                return;

            Debug.LogWarning($"[GameUIController] Video playback failed: {message}", this);
            ShowMediaFallback(MediaSlotType.Video, source.url, "video playback failed");
        }

        void ClearRuntimeMediaSprite()
        {
            if (_runtimeMediaSprite != null)
            {
                Destroy(_runtimeMediaSprite);
                _runtimeMediaSprite = null;
            }

            if (_runtimeMediaTexture != null)
            {
                Destroy(_runtimeMediaTexture);
                _runtimeMediaTexture = null;
            }
        }

    }
}
