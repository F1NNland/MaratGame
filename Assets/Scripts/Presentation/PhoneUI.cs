using System.Collections;
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

        [SerializeField] StoryRunner storyRunner;
        [SerializeField] GameObject phoneButtonRoot;
        [SerializeField] Button phoneButton;
        [SerializeField] GameObject badgeRoot;
        [SerializeField] TextMeshProUGUI badgeText;
        [SerializeField] CanvasGroup overlayGroup;
        [SerializeField] RectTransform messageListRoot;
        [SerializeField] TextMeshProUGUI messageItemPrefab;
        [SerializeField] Button closeButton;
        [SerializeField] DialogueView dialogueView;

        readonly List<TextMeshProUGUI> _messageItems = new();
        readonly List<Transform> _messageRows = new();
        Tween _pulseTween;
        Tween _dialogueFadeTween;
        Tween _overlayShowTween;
        ScrollRect _messageScroll;
        RectTransform _messageContent;
        Coroutine _storyInboxRoutine;
        Coroutine _overlayRevealRoutine;
        int _inboxRevealGen;
        int _overlayRevealGen;
        bool _overlayOpen;
        bool _storyInboxMode;
        float _dialogueAlphaBeforePhone = 1f;
        Vector2 _defaultListInsetMax;

        public bool IsOverlayOpen => _overlayOpen;
        public bool IsStoryInboxMode => _storyInboxMode;

        /// <summary>Для <see cref="MaratGame.Agent.AgentPlayBridge"/> (MCP).</summary>
        public void AgentOpenOverlay() => OpenOverlay();

        /// <summary>Для <see cref="MaratGame.Agent.AgentPlayBridge"/> (MCP).</summary>
        public void AgentCloseOverlay() => CloseOverlayAndContinue();

        /// <summary>Входящие в сюжетном узле (туалет и т.п.): бабблы, скролл, без кнопки «Закрыть» на панели.</summary>
        public void OpenStoryInbox(IReadOnlyList<string> messages)
        {
            if (overlayGroup == null || messages == null || messages.Count == 0)
                return;

            if (_storyInboxMode && _storyInboxRoutine != null)
                return;

            StopInboxReveal();
            _inboxRevealGen++;
            var gen = _inboxRevealGen;
            ClearMessages();

            _storyInboxMode = true;
            if (closeButton != null)
                closeButton.gameObject.SetActive(false);

            ApplyMessageListInsets(
                UiLayout.PhoneMessageListInsetMaxStory,
                UiLayout.PhoneMessageListInsetMinStory);
            EnsureInboxTitle();
            EnsureMessageListLayout();
            DisablePhoneFrameRaycasts();
            SetDialogueVisibleForPhone(false);
            _overlayOpen = true;

            _storyInboxRoutine = StartCoroutine(OpenStoryInboxRoutine(messages, gen));
        }

        void StopInboxReveal()
        {
            if (_storyInboxRoutine != null)
            {
                StopCoroutine(_storyInboxRoutine);
                _storyInboxRoutine = null;
            }

            StopMessageReveal();
        }

        public void CloseStoryInbox()
        {
            if (!_storyInboxMode)
                return;

            _storyInboxMode = false;
            _inboxRevealGen++;
            StopInboxReveal();
            ClearMessages();
            SetOverlayVisibleImmediate(false);
            ApplyMessageListInsets(_defaultListInsetMax);
            if (closeButton != null)
                closeButton.gameObject.SetActive(true);
            SetDialogueVisibleForPhone(true, immediate: true);
        }

        void Awake()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (dialogueView == null)
                dialogueView = FindFirstObjectByType<DialogueView>();

            if (phoneButton != null)
                phoneButton.onClick.AddListener(OpenOverlay);

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseOverlayAndContinue);

            ClampBadgeLayout();
            SetOverlayVisibleImmediate(false);
            _defaultListInsetMax = UiLayout.PhoneMessageListInsetMax;
            EnsureMessageScroll();
            EnsureOverlayLayout();
            DeactivateMessageTemplate();
            SubscribeStoryEvents();
        }

        void DeactivateMessageTemplate()
        {
            if (messageListRoot == null)
                return;

            foreach (Transform child in messageListRoot)
            {
                if (child.name == "MessageItemTemplate")
                    child.gameObject.SetActive(false);
            }
        }

        void ApplyMessageListInsets(Vector2 insetMax, Vector2? insetMin = null)
        {
            var target = _messageScroll != null && _messageScroll.viewport != null
                ? _messageScroll.viewport
                : messageListRoot;
            if (target == null)
                return;

            target.offsetMin = insetMin ?? UiLayout.PhoneMessageListInsetMin;
            target.offsetMax = insetMax;
        }

        void EnsureCloseButtonLayout()
        {
            if (closeButton == null)
                return;

            var rect = closeButton.GetComponent<RectTransform>();
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, UiLayout.PhoneCloseBottomOffset);
            rect.sizeDelta = UiLayout.SizePhoneClose;

            closeButton.interactable = true;
            closeButton.transform.SetAsLastSibling();

            Graphic target = null;
            foreach (var graphic in closeButton.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic is TextMeshProUGUI label)
                {
                    label.raycastTarget = false;
                    continue;
                }

                graphic.raycastTarget = true;
                target ??= graphic;
            }

            if (target != null)
                closeButton.targetGraphic = target;
        }

        void ClampBadgeLayout()
        {
            if (badgeRoot == null)
                return;

            var rect = badgeRoot.GetComponent<RectTransform>();
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(8f, 8f);
            rect.sizeDelta = UiLayout.SizePhoneBadge;
        }

        void EnsureMessageScroll()
        {
            if (messageListRoot == null || _messageScroll != null)
                return;

            var listParent = messageListRoot.parent;
            if (listParent == null)
                return;

            var viewport = listParent.Find("MessageScrollViewport") as RectTransform;
            if (viewport == null)
            {
                var viewportGo = new GameObject("MessageScrollViewport", typeof(RectTransform));
                viewport = viewportGo.GetComponent<RectTransform>();
                viewport.SetParent(listParent, false);
                viewport.SetSiblingIndex(messageListRoot.GetSiblingIndex());

                viewport.anchorMin = Vector2.zero;
                viewport.anchorMax = Vector2.one;
                viewport.pivot = new Vector2(0.5f, 0.5f);
                viewport.anchoredPosition = Vector2.zero;
                viewport.sizeDelta = Vector2.zero;
                ApplyMessageListInsets(UiLayout.PhoneMessageListInsetMax, UiLayout.PhoneMessageListInsetMin);

                var maskImage = viewportGo.AddComponent<Image>();
                maskImage.color = new Color(1f, 1f, 1f, 0.01f);
                maskImage.raycastTarget = true;
                viewportGo.AddComponent<Mask>().showMaskGraphic = false;

                _messageScroll = viewportGo.AddComponent<ScrollRect>();
                _messageScroll.horizontal = false;
                _messageScroll.vertical = true;
                _messageScroll.movementType = ScrollRect.MovementType.Clamped;
                _messageScroll.scrollSensitivity = 24f;

                messageListRoot.SetParent(viewport, false);
            }
            else
            {
                _messageScroll = viewport.GetComponent<ScrollRect>();
            }

            _messageContent = messageListRoot;
            messageListRoot.anchorMin = new Vector2(0f, 1f);
            messageListRoot.anchorMax = new Vector2(1f, 1f);
            messageListRoot.pivot = new Vector2(0.5f, 1f);
            messageListRoot.anchoredPosition = Vector2.zero;
            messageListRoot.sizeDelta = new Vector2(0f, 0f);

            var contentFitter = messageListRoot.GetComponent<ContentSizeFitter>();
            if (contentFitter == null)
                contentFitter = messageListRoot.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (_messageScroll != null)
            {
                _messageScroll.viewport = viewport;
                _messageScroll.content = _messageContent;
            }

            EnsureTitleDrawOrder();
        }

        void EnsureOverlayLayout()
        {
            if (messageListRoot == null)
                return;

            ApplyMessageListInsets(_defaultListInsetMax);
            EnsureCloseButtonLayout();
            EnsureMessageListLayout();
            DisablePhoneFrameRaycasts();
        }

        void ScrollInboxToStart()
        {
            if (_messageScroll == null || _messageContent == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_messageContent);
            Canvas.ForceUpdateCanvases();
            // 1 = верх списка (первые сообщения), как в утреннем оверлее.
            _messageScroll.verticalNormalizedPosition = 1f;
        }

        void EnsureInboxTitle()
        {
            if (overlayGroup == null)
                return;

            var title = overlayGroup.transform.Find("Panel/Title")?.GetComponent<TextMeshProUGUI>();
            if (title == null)
                return;

            ApplyPhoneTitleLayout(title.rectTransform);
            title.text = "Входящие";
            title.raycastTarget = false;
            title.gameObject.SetActive(true);
            EnsureTitleDrawOrder();
        }

        static void ApplyPhoneTitleLayout(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.offsetMin = new Vector2(UiLayout.PhoneSideInset + 8f, -UiLayout.PhoneTitleBandHeight);
            rect.offsetMax = new Vector2(-UiLayout.PhoneSideInset, -56f);
        }

        void EnsureTitleDrawOrder()
        {
            var panel = overlayGroup != null ? overlayGroup.transform.Find("Panel") : null;
            if (panel == null)
                return;

            var index = 0;
            var sprite = panel.Find("PanelSprite");
            if (sprite != null)
                sprite.SetSiblingIndex(index++);

            var viewport = panel.Find("MessageScrollViewport");
            if (viewport != null)
                viewport.SetSiblingIndex(index++);
            else if (messageListRoot != null && messageListRoot.parent == panel)
                messageListRoot.SetSiblingIndex(index++);

            var title = panel.Find("Title");
            if (title != null)
                title.SetSiblingIndex(index++);

            var close = panel.Find("CloseButton");
            if (close != null)
                close.SetSiblingIndex(index);
        }

        void DisablePhoneFrameRaycasts()
        {
            if (overlayGroup == null)
                return;

            var frame = overlayGroup.transform.Find("Panel/PanelSprite")?.GetComponent<Image>();
            if (frame != null)
                frame.raycastTarget = false;
        }

        static float MeasureMessageTextHeight(TextMeshProUGUI text, float innerWidth)
        {
            text.ForceMeshUpdate(true, true);

            var atWidth = text.GetPreferredValues(innerWidth, 0f).y;
            var textHeight = Mathf.Max(atWidth, text.preferredHeight);

            if (text.renderedHeight > textHeight)
                textHeight = text.renderedHeight;

            var bounds = text.textBounds;
            if (bounds.size.y > textHeight)
                textHeight = bounds.size.y;

            return Mathf.Ceil(textHeight + UiLayout.PhoneMessageHeightBuffer);
        }

        static void FitMessageRow(Transform row, TextMeshProUGUI text, float rowWidth)
        {
            if (row == null || text == null || row is not RectTransform rowRect)
                return;

            var padH = UiLayout.PhoneMessagePadH;
            var padV = UiLayout.PhoneMessagePadV;

            foreach (var fitter in row.GetComponents<ContentSizeFitter>())
                Object.Destroy(fitter);

            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(0f, -padV);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;

            var contentWidth = rowWidth > 20f ? rowWidth : 220f;
            var innerWidth = Mathf.Max(80f, contentWidth - padH * 2f);

            rowRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);

            // Ширина задана — GetPreferredValues + preferredHeight; при sizeDelta.y=0 preferredHeight ≈ одна строка.
            textRect.sizeDelta = new Vector2(-padH * 2f, 1f);
            Canvas.ForceUpdateCanvases();

            var textHeight = MeasureMessageTextHeight(text, innerWidth);
            textHeight = Mathf.Max(text.fontSize, textHeight);
            textRect.sizeDelta = new Vector2(-padH * 2f, textHeight);

            text.ForceMeshUpdate(true, true);
            var refined = MeasureMessageTextHeight(text, innerWidth);
            if (refined > textHeight)
            {
                textHeight = refined;
                textRect.sizeDelta = new Vector2(-padH * 2f, textHeight);
            }

            var totalHeight = textHeight + padV * 2f;
            rowRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

            var layout = row.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.minHeight = totalHeight;
                layout.preferredHeight = totalHeight;
            }

            var bubble = row.Find("Bubble") as RectTransform;
            if (bubble != null)
            {
                bubble.anchorMin = Vector2.zero;
                bubble.anchorMax = Vector2.one;
                bubble.offsetMin = Vector2.zero;
                bubble.offsetMax = Vector2.zero;
            }
        }

        void EnsureMessageListLayout()
        {
            if (messageListRoot == null)
                return;

            var layout = messageListRoot.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
                layout = messageListRoot.gameObject.AddComponent<VerticalLayoutGroup>();

            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        float ResolveMessageRowWidth()
        {
            var list = _messageContent != null ? _messageContent : messageListRoot;
            if (list == null)
                return UiLayout.PhoneMessageRowWidth;

            Canvas.ForceUpdateCanvases();
            var listWidth = list.rect.width - 8f;

            // Пока оверлей скрыт, rect.width часто = ширина всего Canvas → TMP считает одну строку.
            if (listWidth < 80f || listWidth > UiLayout.PhoneMessageListWidth + 24f)
                listWidth = UiLayout.PhoneMessageRowWidth;

            return listWidth;
        }

        void RefitAllMessageRows()
        {
            if (messageListRoot == null)
                return;

            EnsureMessageListLayout();
            var rowWidth = ResolveMessageRowWidth();

            FitAllRows(rowWidth);
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageListRoot);
            FitAllRows(rowWidth);
        }

        void FitAllRows(float listWidth)
        {
            foreach (var row in _messageRows)
            {
                if (row == null)
                    continue;

                if (row.TryGetComponent<PhoneMessageBubbleRow>(out var bubbleRow))
                {
                    bubbleRow.Refit(listWidth);
                    continue;
                }

                var text = row.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    FitMessageRow(row, text, listWidth);
            }
        }

        void OnEnable() => SubscribeStoryEvents();

        void Start() => StartCoroutine(SyncAfterStoryStarted());

        void OnDisable() => UnsubscribeStoryEvents();

        void SubscribeStoryEvents()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (storyRunner == null)
                return;

            storyRunner.OnNodeChanged -= HandleNodeChanged;
            storyRunner.OnNodeChanged += HandleNodeChanged;
        }

        void UnsubscribeStoryEvents()
        {
            if (storyRunner != null)
                storyRunner.OnNodeChanged -= HandleNodeChanged;
        }

        IEnumerator SyncAfterStoryStarted()
        {
            yield return null;
            var current = storyRunner?.Engine?.CurrentNode;
            if (current != null)
                HandleNodeChanged(current);
            else
                SetPhoneChromeActive(false);
        }

        void HandleNodeChanged(StoryNodeData node)
        {
            if (node == null)
                return;

            if (_storyInboxMode && node.uiMode != StoryUiMode.PhoneInbox)
                CloseStoryInbox();

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
            BringPhoneToFront();

            if (badgeRoot != null)
                badgeRoot.SetActive(true);

            if (badgeText != null)
                badgeText.text = IncomingMessagesContent.MorningHall.Length.ToString();

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

        void BringPhoneToFront()
        {
            if (phoneButtonRoot == null)
                return;

            phoneButtonRoot.transform.SetAsLastSibling();
            if (badgeRoot != null && badgeRoot.transform.parent == phoneButtonRoot.transform)
                badgeRoot.transform.SetAsLastSibling();
        }

        void OpenOverlay()
        {
            if (overlayGroup == null)
                return;

            StopPulse();

            if (badgeRoot != null)
                badgeRoot.SetActive(false);

            if (phoneButtonRoot != null)
                phoneButtonRoot.SetActive(false);

            DisablePhoneFrameRaycasts();
            EnsureCloseButtonLayout();
            EnsureInboxTitle();
            EnsureMessageListLayout();
            _overlayOpen = true;
            SetDialogueVisibleForPhone(false);
            ApplyMessageListInsets(UiLayout.PhoneMessageListInsetMax);
            if (closeButton != null)
                closeButton.gameObject.SetActive(true);

            _overlayRevealGen++;
            var gen = _overlayRevealGen;
            if (_overlayRevealRoutine != null)
                StopCoroutine(_overlayRevealRoutine);

            ClearMessages();
            _overlayRevealRoutine = StartCoroutine(OpenOverlayRoutine(gen));
        }

        IEnumerator OpenOverlayRoutine(int gen)
        {
            yield return RevealInboxMessagesRoutine(IncomingMessagesContent.MorningHall, gen);
            if (gen == _overlayRevealGen)
                _overlayRevealRoutine = null;
        }

        void CloseOverlayAndContinue()
        {
            if (overlayGroup == null)
                return;

            _overlayOpen = false;
            _overlayRevealGen++;
            if (_overlayRevealRoutine != null)
            {
                StopCoroutine(_overlayRevealRoutine);
                _overlayRevealRoutine = null;
            }

            ClearMessages();
            StopMessageReveal();
            SetDialogueVisibleForPhone(true);
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

        IEnumerator OpenStoryInboxRoutine(IReadOnlyList<string> messages, int gen)
        {
            yield return RevealInboxMessagesRoutine(messages, gen);
            if (gen == _inboxRevealGen)
                _storyInboxRoutine = null;
        }

        IEnumerator RevealInboxMessagesRoutine(IReadOnlyList<string> messages, int gen)
        {

            if (overlayGroup != null)
            {
                overlayGroup.gameObject.SetActive(true);
                overlayGroup.alpha = 0f;
                overlayGroup.interactable = false;
                overlayGroup.blocksRaycasts = false;
            }

            yield return null;

            if (_overlayShowTween != null)
            {
                _overlayShowTween.Kill();
                _overlayShowTween = null;
            }

            _overlayShowTween = UiTweens.ShowOverlay(overlayGroup);
            if (_overlayShowTween != null)
                yield return _overlayShowTween.WaitForCompletion();

            if (messages == null)
                yield break;

            var stagger = _storyInboxMode
                ? UiTweens.PhoneMessageStagger * 0.12f
                : UiTweens.PhoneMessageStagger * 0.35f;

            foreach (var message in messages)
            {
                if (!IsRevealGenerationActive(gen))
                    yield break;

                var row = SpawnMessageRow(message);
                if (row == null)
                    continue;

                RefitAllMessageRows();
                ScrollInboxToStart();

                var cg = row.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = row.gameObject.AddComponent<CanvasGroup>();

                cg.alpha = 0f;
                row.localScale = Vector3.one * 0.92f;
                cg.DOFade(1f, UiTweens.Fast).SetEase(Ease.OutQuad).SetLink(row.gameObject);
                row.DOScale(1f, UiTweens.Fast).SetEase(Ease.OutBack).SetLink(row.gameObject);

                yield return new WaitForSeconds(stagger);
                RefitAllMessageRows();
                ScrollInboxToStart();
            }

            ScrollInboxToStart();
        }

        Transform SpawnMessageRow(string message)
        {
            if (messageListRoot == null || messageItemPrefab == null)
                return null;

            var templateRoot = messageItemPrefab.transform.parent != null
                && messageItemPrefab.transform.parent.name == "MessageItemTemplate"
                ? messageItemPrefab.transform.parent
                : messageItemPrefab.transform;

            var row = Instantiate(templateRoot, messageListRoot);
            row.gameObject.SetActive(true);

            var rowCg = row.GetComponent<CanvasGroup>();
            if (rowCg == null)
                rowCg = row.gameObject.AddComponent<CanvasGroup>();
            rowCg.alpha = 0f;
            rowCg.blocksRaycasts = false;
            rowCg.interactable = false;
            row.localScale = Vector3.one * 0.92f;
            _messageRows.Add(row);

            var item = row.GetComponentInChildren<TextMeshProUGUI>();
            if (item != null)
            {
                item.text = message;
                item.color = UiStyle.PhoneMessageText;
                item.textWrappingMode = TextWrappingModes.Normal;
                _messageItems.Add(item);
            }

            if (row.TryGetComponent<PhoneMessageBubbleRow>(out var bubbleRow))
                bubbleRow.Refit(ResolveMessageRowWidth());

            return row;
        }

        void ClearMessages()
        {
            StopMessageReveal();

            foreach (var row in _messageRows)
            {
                if (row != null)
                    Destroy(row.gameObject);
            }

            _messageItems.Clear();
            _messageRows.Clear();

            if (messageListRoot == null)
                return;

            for (var i = messageListRoot.childCount - 1; i >= 0; i--)
            {
                var child = messageListRoot.GetChild(i);
                if (child.name == "MessageItemTemplate")
                {
                    child.gameObject.SetActive(false);
                    continue;
                }

                if (child.name.StartsWith("MessageItemTemplate"))
                    Destroy(child.gameObject);
            }
        }

        bool IsRevealGenerationActive(int gen) =>
            gen == _inboxRevealGen || gen == _overlayRevealGen;

        void StopMessageReveal()
        {
            foreach (var row in _messageRows)
            {
                if (row != null)
                    UiTweens.Kill(row);
            }
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

            if (!visible)
                SetDialogueVisibleForPhone(true, immediate: true);
        }

        void SetDialogueVisibleForPhone(bool visible, bool immediate = false)
        {
            var group = dialogueView?.CanvasGroup;
            if (group == null)
                return;

            if (_dialogueFadeTween != null)
            {
                _dialogueFadeTween.Kill();
                _dialogueFadeTween = null;
            }

            if (!visible && group.alpha > 0.01f)
                _dialogueAlphaBeforePhone = group.alpha;

            if (immediate)
            {
                group.alpha = visible ? _dialogueAlphaBeforePhone : 0f;
                return;
            }

            var target = visible ? _dialogueAlphaBeforePhone : 0f;
            _dialogueFadeTween = UiTweens.Fade(group, target, UiTweens.Fast);
        }

        void OnDestroy()
        {
            StopPulse();
            if (_overlayShowTween != null)
                _overlayShowTween.Kill();
            if (overlayGroup != null)
                UiTweens.Kill(overlayGroup);
            if (_dialogueFadeTween != null)
                _dialogueFadeTween.Kill();
            ClearMessages();
        }
    }
}
