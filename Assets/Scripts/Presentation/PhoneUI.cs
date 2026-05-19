using System.Collections.Generic;
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
    /// Телефон: badge, overlay с входящими, переход в hall_hub (MVP шаг 08).
    /// </summary>
    public sealed class PhoneUI : MonoBehaviour
    {
        public const string PhoneReadFlag = "phone_read";
        public const string HallMorningNodeId = "hall_morning";
        public const string HallHubNodeId = "hall_hub";

        public static readonly string[] MorningIncomingMessages =
        {
            "С днем рождения!",
            "Вы на КРРБ будете?",
            "Алевтина просила зайти",
            "Торт привезли",
            "Срочно нужен комментарий"
        };

        [SerializeField] StoryRunner storyRunner;
        [SerializeField] GameObject phoneButtonRoot;
        [SerializeField] Button phoneButton;
        [SerializeField] GameObject badgeRoot;
        [SerializeField] TextMeshProUGUI badgeText;
        [SerializeField] CanvasGroup overlayGroup;
        [SerializeField] RectTransform messageListRoot;
        [SerializeField] TextMeshProUGUI messageItemPrefab;
        [SerializeField] Button closeButton;

        readonly List<TextMeshProUGUI> _messageItems = new();
        Tween _pulseTween;
        bool _overlayOpen;

        void Awake()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (phoneButton != null)
                phoneButton.onClick.AddListener(OpenOverlay);

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseOverlayAndContinue);

            SetOverlayVisibleImmediate(false);
            SetPhoneChromeActive(false);
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

            var state = GameState.Instance;
            var unread = node.id == HallMorningNodeId && !state.Flags.HasFlag(PhoneReadFlag);

            if (unread)
                ShowUnreadPhone();
            else
                HidePhoneChrome();
        }

        void ShowUnreadPhone()
        {
            SetPhoneChromeActive(true);

            if (badgeRoot != null)
                badgeRoot.SetActive(true);

            if (badgeText != null)
                badgeText.text = MorningIncomingMessages.Length.ToString();

            if (phoneButton != null)
            {
                UiTweens.PunchScale(phoneButton.transform);
                StartPulse();
            }
        }

        void HidePhoneChrome()
        {
            StopPulse();
            SetPhoneChromeActive(false);

            if (badgeRoot != null)
                badgeRoot.SetActive(false);

            if (_overlayOpen)
                SetOverlayVisibleImmediate(false);
        }

        void SetPhoneChromeActive(bool active)
        {
            if (phoneButtonRoot != null)
                phoneButtonRoot.SetActive(active);
        }

        void OpenOverlay()
        {
            if (overlayGroup == null)
                return;

            StopPulse();

            if (badgeRoot != null)
                badgeRoot.SetActive(false);

            PopulateMessages();
            _overlayOpen = true;
            UiTweens.ShowOverlay(overlayGroup);
        }

        void CloseOverlayAndContinue()
        {
            if (overlayGroup == null)
                return;

            _overlayOpen = false;
            var hide = UiTweens.HideOverlay(overlayGroup);
            if (hide != null)
            {
                hide.OnComplete(() =>
                {
                    GameState.Instance.Flags.SetFlag(PhoneReadFlag);
                    storyRunner?.LoadNode(HallHubNodeId);
                });
            }
            else
            {
                GameState.Instance.Flags.SetFlag(PhoneReadFlag);
                storyRunner?.LoadNode(HallHubNodeId);
            }
        }

        void PopulateMessages()
        {
            ClearMessages();

            if (messageListRoot == null || messageItemPrefab == null)
                return;

            foreach (var text in MorningIncomingMessages)
            {
                var item = Instantiate(messageItemPrefab, messageListRoot);
                item.gameObject.SetActive(true);
                item.text = text;
                _messageItems.Add(item);
            }
        }

        void ClearMessages()
        {
            foreach (var item in _messageItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _messageItems.Clear();
        }

        void StartPulse()
        {
            if (phoneButton == null)
                return;

            StopPulse();
            phoneButton.transform.localScale = Vector3.one;
            _pulseTween = phoneButton.transform
                .DOScale(1.05f, 0.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(phoneButton.gameObject);
        }

        void StopPulse()
        {
            if (_pulseTween != null)
            {
                _pulseTween.Kill();
                _pulseTween = null;
            }

            if (phoneButton != null)
                UiTweens.Kill(phoneButton.transform);
        }

        void SetOverlayVisibleImmediate(bool visible)
        {
            if (overlayGroup == null)
                return;

            _overlayOpen = visible;
            overlayGroup.gameObject.SetActive(visible);
            overlayGroup.alpha = visible ? 1f : 0f;
            overlayGroup.interactable = visible;
            overlayGroup.blocksRaycasts = visible;
        }

        void OnDestroy()
        {
            StopPulse();
            if (overlayGroup != null)
                UiTweens.Kill(overlayGroup);
            ClearMessages();
        }
    }
}
