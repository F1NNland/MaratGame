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
    /// Экран итогов MVP: статы, число решений, «Сыграть ещё раз» (шаг 11).
    /// </summary>
    public sealed class EndingUI : MonoBehaviour
    {
        [SerializeField] StoryRunner storyRunner;
        [SerializeField] CanvasGroup overlayGroup;
        [SerializeField] CanvasGroup rootCanvasGroup;
        [SerializeField] TextMeshProUGUI respectValueText;
        [SerializeField] TextMeshProUGUI calmValueText;
        [SerializeField] TextMeshProUGUI decisionsValueText;
        [SerializeField] Button replayButton;

        void Awake()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (replayButton != null)
                replayButton.onClick.AddListener(OnReplayClicked);

            SetOverlayImmediate(false);
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
            if (current != null && current.id == BirthdayEndNodes.EndingScreenNodeId)
                ShowEnding();
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
            if (node != null && node.id == BirthdayEndNodes.EndingScreenNodeId)
                ShowEnding();
        }

        void HandleNodeComplete(StoryNodeData node)
        {
            if (node != null && node.id == BirthdayEndNodes.EndingScreenNodeId)
                ShowEnding();
        }

        void ShowEnding()
        {
            var state = GameState.Instance;

            SetStatPlaceholders();
            SetOverlayImmediate(true);
            UiTweens.ShowOverlay(overlayGroup, UiTweens.Slow);

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

            if (decisionsValueText != null)
            {
                UiTweens.Kill(decisionsValueText);
                UiTweens.Counter(0, state.DecisionsCount, duration, v => decisionsValueText.text = v.ToString());
            }
        }

        void SetStatPlaceholders()
        {
            if (respectValueText != null)
                respectValueText.text = "—";
            if (calmValueText != null)
                calmValueText.text = "—";
            if (decisionsValueText != null)
                decisionsValueText.text = "—";
        }

        public void OnReplayClicked()
        {
            if (rootCanvasGroup != null)
                UiTweens.Fade(rootCanvasGroup, 0f, UiTweens.Fast, Ease.InQuad);

            var hide = UiTweens.HideOverlay(overlayGroup, UiTweens.Normal, false);
            if (hide != null)
                hide.OnComplete(GameBootstrap.ResetGame);
            else
                GameBootstrap.ResetGame();
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
            if (decisionsValueText != null)
                UiTweens.Kill(decisionsValueText);
        }
    }
}
