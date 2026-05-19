using System.Collections.Generic;
using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] List<LocationBackgroundEntry> locationBackgrounds = new();
        [SerializeField] List<CharacterPortraitEntry> characterPortraits = new();

        Image _activeBackground;
        Image _inactiveBackground;
        string _currentLocationId;
        bool _introPlayed;

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

            _activeBackground = backgroundImageA;
            _inactiveBackground = backgroundImageB;

            if (_activeBackground != null)
            {
                SetImageAlpha(_activeBackground, 1f);
                _activeBackground.gameObject.SetActive(true);
            }

            if (_inactiveBackground != null)
                _inactiveBackground.gameObject.SetActive(false);

            hud?.BindInitialState(GameState.Instance);
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

            if (node.id == BirthdayEndNodes.EndingScreenNodeId)
                return;

            var state = GameState.Instance;

            hud?.SetTime(state.CurrentTime);
            hud?.SetChapter(node.chapterLabel);
            hud?.SetPeriodBadge(ResolvePeriodBadge(node.chapterLabel));
            hud?.AnimateStats(state.Stats.Respect, state.Stats.Calm);

            if (node.id == BirthdayEndNodes.BirthdaySceneNodeId)
            {
                ApplyNodeContent(node, state);
                if (dialogue?.Panel != null && dialogue.CanvasGroup != null)
                    UiTweens.RevealDialogue(dialogue.Panel, dialogue.CanvasGroup, UiTweens.Slow);
                choices?.BuildChoices(node.choices, storyRunner.SelectChoice);
                return;
            }

            UiTweens.TransitionNode(
                backgroundGroup,
                dialogue?.CanvasGroup,
                dialogue?.Panel,
                () => ApplyNodeContent(node, state),
                UiTweens.Normal);

            choices?.BuildChoices(node.choices, storyRunner.SelectChoice);
        }

        void ApplyNodeContent(StoryNodeData node, GameState state)
        {
            if (locationHeader != null)
                locationHeader.text = LocationLabels.GetDisplayName(state.CurrentLocationId);

            dialogue?.SetContent(node.speaker, node.bodyText);
            dialogue?.SetPortrait(ResolvePortrait(node.portraitCharacterId));
            UpdateBackground(state.CurrentLocationId);
        }

        void UpdateBackground(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId) || locationId == _currentLocationId)
                return;

            var sprite = ResolveBackground(locationId);
            if (sprite == null || _inactiveBackground == null || _activeBackground == null)
                return;

            _currentLocationId = locationId;
            _inactiveBackground.sprite = sprite;
            _inactiveBackground.preserveAspect = true;

            UiTweens.CrossFadeImage(_activeBackground, _inactiveBackground);

            (_activeBackground, _inactiveBackground) = (_inactiveBackground, _activeBackground);
        }

        static string ResolvePeriodBadge(string chapterLabel)
        {
            if (string.IsNullOrWhiteSpace(chapterLabel))
                return null;

            return chapterLabel.Contains("УТРО", System.StringComparison.OrdinalIgnoreCase)
                ? "🎂 Утро"
                : null;
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
            if (backgroundGroup != null)
                UiTweens.Kill(backgroundGroup);
            dialogue?.OnDestroyCleanup();
        }
    }
}
