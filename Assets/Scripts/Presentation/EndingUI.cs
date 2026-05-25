using DG.Tweening;
using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Финальный экран дня: статы, достижения, рестарт / главное меню (только вечер).
    /// </summary>
    public sealed class EndingUI : MonoBehaviour
    {
        [SerializeField] StoryRunner storyRunner;
        [SerializeField] CanvasGroup overlayGroup;
        [SerializeField] CanvasGroup rootCanvasGroup;
        [SerializeField] TextMeshProUGUI respectValueText;
        [SerializeField] TextMeshProUGUI calmValueText;
        [SerializeField] TextMeshProUGUI chaosValueText;
        [SerializeField] TextMeshProUGUI decisionsValueText;
        [SerializeField] TextMeshProUGUI achievementsValueText;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] Button replayButton;
        [SerializeField] Button mainMenuButton;

        static readonly Color SolidBackdrop = new(0.04f, 0.06f, 0.1f, 0.94f);

        CanvasGroup _backgroundGroup;
        CanvasGroup _dialogueGroup;
        GameObject _choicesRoot;
        GameObject _navigationRoot;
        GameObject _phoneRoot;
        void Awake()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (replayButton != null)
                replayButton.onClick.AddListener(OnReplayClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            HideLegacyContinueButton();
            CacheGameplayChrome();
            ApplySolidBackdrop();
            NormalizeEndingLayout();
            SetOverlayImmediate(false);
        }

        void CacheGameplayChrome()
        {
            var canvas = overlayGroup != null ? overlayGroup.GetComponentInParent<Canvas>() : null;
            if (canvas == null)
                return;

            var root = canvas.transform;
            _backgroundGroup = root.Find("Background")?.GetComponent<CanvasGroup>();
            _dialogueGroup = root.Find("DialoguePanel")?.GetComponent<CanvasGroup>();
            _choicesRoot = root.Find("ChoicesContainer")?.gameObject;
            _navigationRoot = root.Find("NavigationBar")?.gameObject;
            _phoneRoot = root.Find("PhoneUI")?.gameObject;
        }

        void ApplySolidBackdrop()
        {
            if (overlayGroup == null)
                return;

            var overlayRect = overlayGroup.transform as RectTransform;
            if (overlayRect != null)
            {
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
            }

            var dim = overlayGroup.GetComponent<Image>();
            if (dim != null)
            {
                dim.color = SolidBackdrop;
                dim.raycastTarget = true;
            }

            ApplyButtonLabelColors();
        }

        void ApplyButtonLabelColors()
        {
            SetButtonLabel(replayButton, UiStyle.TextLight);
            SetButtonLabel(mainMenuButton, UiStyle.TextLight);
        }

        static void SetButtonLabel(Button button, Color color)
        {
            if (button == null)
                return;

            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                label.color = color;
        }

        void HideStoryDialogue()
        {
            var dialogue = FindFirstObjectByType<DialogueView>();
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

        void SetGameplayChromeVisible(bool visible)
        {
            if (_backgroundGroup != null)
            {
                _backgroundGroup.alpha = visible ? 1f : 0f;
                _backgroundGroup.gameObject.SetActive(visible);
                if (!visible)
                {
                    foreach (var image in _backgroundGroup.GetComponentsInChildren<Image>(true))
                        image.enabled = false;
                }
            }

            if (_dialogueGroup != null)
            {
                _dialogueGroup.alpha = visible ? 1f : 0f;
                _dialogueGroup.gameObject.SetActive(visible);
            }

            if (_choicesRoot != null)
                _choicesRoot.SetActive(visible);
            if (_navigationRoot != null)
                _navigationRoot.SetActive(visible);
            if (_phoneRoot != null)
                _phoneRoot.SetActive(visible);

            if (!visible)
            {
                var phone = FindFirstObjectByType<PhoneUI>();
                if (phone != null && phone.IsStoryInboxMode)
                    phone.CloseStoryInbox();
            }
        }

        void OnEnable()
        {
            if (storyRunner != null)
            {
                storyRunner.OnNodeChanged += HandleNodeChanged;
                storyRunner.OnNodeComplete += HandleNodeComplete;
            }
        }

        void Start()
        {
            var current = storyRunner?.Engine?.CurrentNode;
            if (IsFinalEndingNode(current))
                ShowEnding(current);
        }

        void OnDisable()
        {
            if (storyRunner != null)
            {
                storyRunner.OnNodeChanged -= HandleNodeChanged;
                storyRunner.OnNodeComplete -= HandleNodeComplete;
            }
        }

        void HandleNodeChanged(StoryNodeData node)
        {
            if (IsFinalEndingNode(node))
            {
                ShowEnding(node);
                return;
            }

            HideEnding();
        }

        void HandleNodeComplete(StoryNodeData node)
        {
            if (IsFinalEndingNode(node))
                ShowEnding(node);
        }

        void ShowEnding(StoryNodeData node)
        {
            var state = GameState.Instance;
            var canShowChaos = Debug.isDebugBuild || Application.isEditor;

            ApplyEndingTitle();
            NormalizeEndingLayout();
            ApplySolidBackdrop();
            ConfigureEndingTexts();
            HideStoryDialogue();
            SetGameplayChromeVisible(false);
            transform.SetAsLastSibling();
            SetStatPlaceholders();
            SetOverlayImmediate(true);

            if (mainMenuButton != null)
                mainMenuButton.gameObject.SetActive(true);
            if (replayButton != null)
                replayButton.gameObject.SetActive(true);
            if (chaosValueText != null && chaosValueText.transform.parent != null)
                chaosValueText.transform.parent.gameObject.SetActive(canShowChaos);

            var duration = UiTweens.Slow;
            if (respectValueText != null)
            {
                UiTweens.Kill(respectValueText);
                UiTweens.Counter(0, state.Stats.Respect, duration, v => respectValueText.text = $"{v}%");
            }

            if (calmValueText != null)
            {
                UiTweens.Kill(calmValueText);
                UiTweens.Counter(0, state.Stats.Calm, duration, v => calmValueText.text = $"{v}%");
            }

            if (chaosValueText != null && canShowChaos)
            {
                UiTweens.Kill(chaosValueText);
                UiTweens.Counter(0, state.Stats.Chaos, duration, v => chaosValueText.text = $"{v}%");
            }

            if (decisionsValueText != null)
            {
                UiTweens.Kill(decisionsValueText);
                UiTweens.Counter(0, state.DecisionsCount, duration, v => decisionsValueText.text = v.ToString());
            }

            if (achievementsValueText != null)
            {
                achievementsValueText.text = BuildAchievementsSummary(state, node);
                var panel = overlayGroup?.transform.Find("StatsPanel") as RectTransform;
                var zone = panel?.Find("AchievementsZone") as RectTransform;
                if (zone != null)
                    EndingUiLayout.CleanupAchievementTextDuplicates(zone, achievementsValueText);
            }
        }

        void ApplyEndingTitle()
        {
            var title = titleText != null
                ? titleText
                : overlayGroup != null
                    ? overlayGroup.transform.Find("StatsPanel/Title")?.GetComponent<TextMeshProUGUI>()
                    : null;
            if (title != null)
                title.text = "ФИНАЛ ДНЯ";
        }

        void SetStatPlaceholders()
        {
            if (respectValueText != null)
                respectValueText.text = "…";
            if (calmValueText != null)
                calmValueText.text = "…";
            if (decisionsValueText != null)
                decisionsValueText.text = "…";
            if (chaosValueText != null)
                chaosValueText.text = "…";
            if (achievementsValueText != null)
                achievementsValueText.text = string.Empty;
        }

        public void OnReplayClicked()
        {
            if (rootCanvasGroup != null)
                UiTweens.Fade(rootCanvasGroup, 0f, UiTweens.Fast, Ease.InQuad);

            var hide = UiTweens.HideOverlay(overlayGroup, UiTweens.Normal, false);
            if (hide != null)
                hide.OnComplete(GameBootstrap.RestartGame);
            else
                GameBootstrap.RestartGame();
        }

        public void OnMainMenuClicked()
        {
            var hide = UiTweens.HideOverlay(overlayGroup, UiTweens.Normal, false);
            if (hide != null)
                hide.OnComplete(GameBootstrap.ResetGame);
            else
                GameBootstrap.ResetGame();
        }

        void HideEnding()
        {
            if (overlayGroup == null || !overlayGroup.gameObject.activeSelf)
                return;

            SetGameplayChromeVisible(true);
            RestoreBackgroundImages();
            UiTweens.HideOverlay(overlayGroup, UiTweens.Fast, false);
        }

        void ConfigureEndingTexts()
        {
            if (achievementsValueText != null)
            {
                achievementsValueText.textWrappingMode = TextWrappingModes.Normal;
                achievementsValueText.overflowMode = TextOverflowModes.Masking;
                achievementsValueText.enableAutoSizing = false;
            }
        }

        void RestoreBackgroundImages()
        {
            if (_backgroundGroup == null)
                return;

            foreach (var image in _backgroundGroup.GetComponentsInChildren<Image>(true))
                image.enabled = true;
        }

        void SetOverlayImmediate(bool visible)
        {
            if (overlayGroup == null)
                return;

            overlayGroup.gameObject.SetActive(visible);
            overlayGroup.alpha = visible ? 1f : 0f;
            overlayGroup.interactable = visible;
            overlayGroup.blocksRaycasts = visible;
        }

        void HideLegacyContinueButton()
        {
            if (overlayGroup == null)
                return;

            var legacy = overlayGroup.transform.Find("StatsPanel/ContinueDayButton");
            if (legacy != null)
                legacy.gameObject.SetActive(false);
        }

        void NormalizeEndingLayout()
        {
            var panel = overlayGroup != null ? overlayGroup.transform.Find("StatsPanel") as RectTransform : null;
            if (panel == null)
                return;

            EndingUiLayout.Apply(panel, BuildLayoutRefs());
        }

        EndingUiRefs BuildLayoutRefs() =>
            new()
            {
                Title = titleText,
                RespectValue = respectValueText,
                CalmValue = calmValueText,
                ChaosValue = chaosValueText,
                DecisionsValue = decisionsValueText,
                AchievementsValue = achievementsValueText,
                ReplayButton = replayButton,
                MainMenuButton = mainMenuButton
            };

        static bool IsFinalEndingNode(StoryNodeData node)
        {
            if (node == null)
                return false;

            return node.id == Chapter4KrrbNodes.BigCongratulationNodeId ||
                   node.id == Chapter4KrrbNodes.EveningGoodEndingNodeId;
        }

        static string BuildAchievementsSummary(GameState state, StoryNodeData node)
        {
            if (state == null)
                return "Без данных";

            var achievements = new System.Collections.Generic.List<string>(6);
            if (state.Flags.HasFlag(Chapter4KrrbFlags.HighSocialPresence))
                achievements.Add("Много общался в течение дня");
            if (state.Flags.HasFlag(Chapter2MiniStoryFlags.HelpedEmployee))
                achievements.Add("Помог сотруднику в мини-сюжете");
            if (state.Flags.HasFlag(Chapter3PlanerkaFlags.BusinessStyle))
                achievements.Add("Планёрка: деловой стиль");
            if (state.Flags.HasFlag(Chapter3PlanerkaFlags.HumorStyle))
                achievements.Add("Планёрка: с юмором");
            if (state.Flags.HasFlag(Chapter3PlanerkaFlags.ObserverStyle))
                achievements.Add("Планёрка: наблюдательный стиль");
            if (state.Flags.HasFlag(Chapter4KrrbFlags.SatNearAlevtina))
                achievements.Add("КРРБ/УК: рядом с Алевтиной");
            if (state.Flags.HasFlag(Chapter4KrrbFlags.SatNearKozlikhin))
                achievements.Add("КРРБ/УК: рядом с Козлихиным");
            if (state.Flags.HasFlag(Chapter4KrrbFlags.SatBackRow))
                achievements.Add("КРРБ/УК: место в конце зала");
            if (state.Flags.HasFlag(Chapter4KrrbFlags.BigCongratulationUnlocked) ||
                (node != null && node.id == Chapter4KrrbNodes.BigCongratulationNodeId))
                achievements.Add("Открыто большое поздравление");

            if (achievements.Count == 0)
                return "Стабильно прошёл рабочий день";

            return string.Join("\n", achievements);
        }

        void OnDestroy()
        {
            if (overlayGroup != null)
                UiTweens.Kill(overlayGroup);
            if (rootCanvasGroup != null)
                UiTweens.Kill(rootCanvasGroup);
            if (respectValueText != null)
                UiTweens.Kill(respectValueText);
            if (calmValueText != null)
                UiTweens.Kill(calmValueText);
            if (chaosValueText != null)
                UiTweens.Kill(chaosValueText);
            if (decisionsValueText != null)
                UiTweens.Kill(decisionsValueText);
        }
    }
}
