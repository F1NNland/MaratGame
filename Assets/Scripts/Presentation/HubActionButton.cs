using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Холл-hub: ● Действие «Осмотреться» → birthday_scene (MVP шаг 11).
    /// </summary>
    public sealed class HubActionButton : MonoBehaviour
    {
        [SerializeField] StoryRunner storyRunner;
        [SerializeField] GameObject buttonRoot;
        [SerializeField] CanvasGroup buttonGroup;
        [SerializeField] Button actionButton;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] bool autoTriggerWhenReady = true;

        void Awake()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionClicked);

            if (label != null)
                label.text = "●\nОсмотреться";

            SetVisible(false);
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
            var ready = GameState.Instance.DecisionsCount >= BirthdayEndNodes.MinDecisionsForBirthday;
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

        void GoToBirthday()
        {
            CancelInvoke(nameof(TriggerBirthday));

            var state = GameState.Instance;
            if (state.Flags.HasFlag(BirthdayEndFlags.BirthdaySeen))
                return;

            if (state.DecisionsCount < BirthdayEndNodes.MinDecisionsForBirthday)
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
