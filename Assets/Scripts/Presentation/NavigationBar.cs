using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Холл-hub: ← Столовая · ↑ Кабинет · → Лифты (MVP шаг 09).
    /// </summary>
    public sealed class NavigationBar : MonoBehaviour
    {
        public const string HubNodeId = PhoneUI.HallHubNodeId;
        public const string CanteenEntryNodeId = "canteen_entry";
        public const string CabinetEntryNodeId = "cabinet_entry";
        public const string ElevatorEntryNodeId = "elevator_entry";

        [SerializeField] StoryRunner storyRunner;
        [SerializeField] GameObject barRoot;
        [SerializeField] CanvasGroup barGroup;
        [SerializeField] Button leftButton;
        [SerializeField] Button forwardButton;
        [SerializeField] Button rightButton;

        void Awake()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (leftButton != null)
                leftButton.onClick.AddListener(() => NavigateTo(CanteenEntryNodeId));
            if (forwardButton != null)
                forwardButton.onClick.AddListener(() => NavigateTo(CabinetEntryNodeId));
            if (rightButton != null)
                rightButton.onClick.AddListener(() => NavigateTo(ElevatorEntryNodeId));

            SetBarVisible(false);
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
            var showHub = node != null && node.id == HubNodeId;
            SetBarVisible(showHub);

            if (showHub && barRoot != null)
                UiTweens.PunchScale(barRoot.transform);
        }

        void NavigateTo(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                return;

            GameState.Instance.RecordDecision();
            storyRunner?.LoadNode(nodeId);
        }

        void SetBarVisible(bool visible)
        {
            if (barRoot != null)
                barRoot.SetActive(visible);

            if (barGroup == null)
                return;

            barGroup.alpha = visible ? 1f : 0f;
            barGroup.interactable = visible;
            barGroup.blocksRaycasts = visible;
        }

        void OnDestroy()
        {
            if (barRoot != null)
                UiTweens.Kill(barRoot.transform);
            if (barGroup != null)
                UiTweens.Kill(barGroup);
        }

        public static void ApplyDirectionLabels(
            TextMeshProUGUI leftLabel,
            TextMeshProUGUI forwardLabel,
            TextMeshProUGUI rightLabel)
        {
            if (leftLabel != null)
                leftLabel.text = "←\nСтоловая";
            if (forwardLabel != null)
                forwardLabel.text = "↑\nКабинет";
            if (rightLabel != null)
                rightLabel.text = "→\nЛифты";
        }
    }
}
