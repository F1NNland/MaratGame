using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Холл-hub: ● «Осмотреться» → birthday_scene → глава 2 (большое поздравление — вечер).
    /// </summary>
    public sealed class HubActionButton : MonoBehaviour
    {
        [SerializeField] StoryRunner storyRunner;
        [SerializeField] GameObject buttonRoot;
        [SerializeField] CanvasGroup buttonGroup;
        [SerializeField] Button actionButton;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] bool autoTriggerWhenReady;

        void Awake()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionClicked);

            if (label != null)
                label.text = "Осмотреться";

            ClampHubDotGraphicSize();
            SetVisible(false);
        }

        void ClampHubDotGraphicSize()
        {
            if (actionButton == null)
                return;

            var dot = actionButton.transform.Find("DotGraphic") as RectTransform;
            if (dot == null)
                return;

            dot.sizeDelta = UiLayout.SizeHubDotButton;
            dot.anchorMin = new Vector2(0f, 0.5f);
            dot.anchorMax = new Vector2(0f, 0.5f);
            dot.pivot = new Vector2(0f, 0.5f);
            dot.anchoredPosition = Vector2.zero;
        }

        void OnEnable()
        {
            if (storyRunner != null)
                storyRunner.OnNodeChanged += HandleNodeChanged;
        }

        void Start()
        {
            var current = storyRunner?.Engine?.CurrentNode;
            if (current != null)
                HandleNodeChanged(current);
        }

        void OnDisable()
        {
            if (storyRunner != null)
                storyRunner.OnNodeChanged -= HandleNodeChanged;
        }

        void HandleNodeChanged(StoryNodeData node)
        {
            if (node == null)
                return;

            var onHub = node.id == NavigationBar.HubNodeId;
            var ready = MorningBranchProgress.IsReadyForHubBirthdayInspect(GameState.Instance);
            var show = onHub && ready && !GameState.Instance.Flags.HasFlag(BirthdayEndFlags.BirthdaySeen);
            SetVisible(show);

            if (show && autoTriggerWhenReady)
                TryAutoTriggerBirthday();
        }

        void TryAutoTriggerBirthday()
        {
            if (GameState.Instance.Flags.HasFlag(BirthdayEndFlags.BirthdaySeen))
                return;

            CancelInvoke(nameof(TriggerBirthday));
            Invoke(nameof(TriggerBirthday), 0.6f);
        }

        void TriggerBirthday() => GoToBirthday();

        void OnActionClicked() => GoToBirthday();

        /// <summary>Для <see cref="MaratGame.Agent.AgentPlayBridge"/> (MCP).</summary>
        public void AgentTriggerInspect() => GoToBirthday();

        void GoToBirthday()
        {
            CancelInvoke(nameof(TriggerBirthday));

            var state = GameState.Instance;
            if (state.Flags.HasFlag(BirthdayEndFlags.BirthdaySeen))
                return;

            if (!MorningBranchProgress.IsReadyForHubBirthdayInspect(state))
                return;

            state.Flags.SetFlag(BirthdayEndFlags.BirthdaySeen);
            storyRunner?.LoadNode(BirthdayEndNodes.BirthdaySceneNodeId);
        }

        void SetVisible(bool visible)
        {
            if (buttonRoot != null)
                buttonRoot.SetActive(visible);

            if (buttonGroup == null)
                return;

            buttonGroup.alpha = visible ? 1f : 0f;
            buttonGroup.interactable = visible;
            buttonGroup.blocksRaycasts = visible;
        }

        void OnDestroy()
        {
            CancelInvoke(nameof(TriggerBirthday));
            if (buttonRoot != null)
                UiTweens.Kill(buttonRoot.transform);
            if (buttonGroup != null)
                UiTweens.Kill(buttonGroup);
        }
    }
}
