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

            ApplyDirectionLabels(
                FindNavLabel(leftButton),
                FindNavLabel(forwardButton),
                FindNavLabel(rightButton));
            ApplyNavDirectionGlyphs();

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

            if (!showHub)
                return;

            RefreshHubNavButtons(GameState.Instance?.Flags);
            if (barRoot != null)
                UiTweens.PunchScale(barRoot.transform);
        }

        static TextMeshProUGUI FindNavLabel(Button button)
        {
            return button != null ? button.transform.Find("Label")?.GetComponent<TextMeshProUGUI>() : null;
        }

        void NavigateTo(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                return;

            var flags = GameState.Instance?.Flags;
            if (flags != null && !MorningBranchProgress.CanEnterMorningArea(nodeId, flags))
                return;

            storyRunner?.LoadNode(nodeId);
        }

        void RefreshHubNavButtons(GameFlags flags)
        {
            SetNavAvailable(leftButton, MorningBranchProgress.CanEnterMorningArea(CanteenEntryNodeId, flags));
            SetNavAvailable(forwardButton, MorningBranchProgress.CanEnterMorningArea(CabinetEntryNodeId, flags));
            SetNavAvailable(rightButton, MorningBranchProgress.CanEnterMorningArea(ElevatorEntryNodeId, flags));
        }

        static void SetNavAvailable(Button button, bool available)
        {
            if (button == null)
                return;

            button.interactable = available;

            var cg = button.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = button.gameObject.AddComponent<CanvasGroup>();

            cg.alpha = available ? 1f : 0.4f;
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

        void ApplyNavDirectionGlyphs()
        {
            var leftSprite = leftButton != null ? leftButton.GetComponent<Image>()?.sprite : null;
            var rightSprite = rightButton != null ? rightButton.GetComponent<Image>()?.sprite : null;
            var hasDedicatedRight = rightSprite != null && rightSprite != leftSprite;

            ApplyNavGlyph(leftButton, leftSprite, 0f, mirrorX: false);
            ApplyNavGlyph(forwardButton, leftSprite, -90f, mirrorX: false);
            ApplyNavGlyph(
                rightButton,
                hasDedicatedRight ? rightSprite : leftSprite,
                0f,
                mirrorX: !hasDedicatedRight);
        }

        static void ApplyNavGlyph(Button button, Sprite forwardSprite, float zRotation, bool mirrorX)
        {
            if (button == null)
                return;

            var image = button.GetComponent<Image>();
            if (image == null)
                return;

            if (forwardSprite != null)
                image.sprite = forwardSprite;

            var rect = image.rectTransform;
            rect.localEulerAngles = new Vector3(0f, 0f, zRotation);
            rect.localScale = mirrorX ? new Vector3(-1f, 1f, 1f) : Vector3.one;
        }
    }
}
