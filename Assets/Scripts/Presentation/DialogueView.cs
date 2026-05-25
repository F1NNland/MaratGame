using MaratGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Окно диалога Figma: слева слот портрета, справа имя и текст.
    /// </summary>
    public sealed class DialogueView : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform panel;
        [SerializeField] Graphic panelGraphic;
        [SerializeField] Image portraitImage;
        [SerializeField] TextMeshProUGUI speakerText;
        [SerializeField] TextMeshProUGUI bodyText;

        StoryUiMode _mode = StoryUiMode.Dialogue;
        Sprite _currentPortrait;
        RectTransform _portraitFrameRect;
        RectTransform _textFrameRect;
        bool _hasFigmaChrome;

        public CanvasGroup CanvasGroup => canvasGroup;
        public RectTransform Panel => panel;
        public Image PortraitImage => portraitImage;

        bool ShowPortraitColumn =>
            _mode == StoryUiMode.Dialogue && !string.IsNullOrWhiteSpace(speakerText?.text);

        void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (panelGraphic == null && panel != null)
                panelGraphic = panel.GetComponent<Graphic>();

            _portraitFrameRect = panel != null ? panel.Find("PortraitFrame") as RectTransform : null;
            _textFrameRect = panel != null ? panel.Find("TextFrame") as RectTransform : null;
            _hasFigmaChrome = _portraitFrameRect != null && _textFrameRect != null;

            ResolvePortraitImageReference();
            EnsureTextFrameChrome();
            StripTestPhotoFromPortraitFrame();
        }

        void Start() => RefreshDialogueLayout();

        public void SetContent(string speaker, string body)
        {
            var speakerValue = speaker ?? string.Empty;
            if (speakerText != null)
                speakerText.text = speakerValue;

            if (bodyText != null)
            {
                UiTweens.Kill(bodyText);
                bodyText.text = FormatBody(speakerValue, body ?? string.Empty);
                bodyText.ForceMeshUpdate(true, true);

                if (string.IsNullOrEmpty(bodyText.text))
                    bodyText.maxVisibleCharacters = int.MaxValue;
                else
                    UiTweens.TypewriterDialogue(bodyText);
            }

            RefreshSpeakerState();
            RefreshDialogueLayout();
        }

        public void ApplyUiMode(StoryUiMode mode)
        {
            _mode = NormalizeMode(mode);
            EnsureTextFrameChrome();
            RefreshPanelStyle();
            RefreshSpeakerState();
            RefreshDialogueLayout();
        }

        public void SetPortrait(Sprite sprite)
        {
            _currentPortrait = sprite;
            RefreshPortraitVisibility();
        }

        public void CancelPresentation()
        {
            if (canvasGroup != null)
                UiTweens.Kill(canvasGroup);
            if (panel != null)
                UiTweens.Kill(panel);
            if (bodyText != null)
                UiTweens.Kill(bodyText);
        }

        public void OnDestroyCleanup() => CancelPresentation();

        void OnDestroy() => OnDestroyCleanup();

        void RefreshDialogueLayout()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = _mode == StoryUiMode.PhoneInbox ? 0f : 1f;

            if (panel != null)
                panel.gameObject.SetActive(_mode != StoryUiMode.PhoneInbox);

            RefreshFrameLayout();
            RefreshPortraitVisibility();
            RefreshPortraitFrameStyle();
        }

        void ResolvePortraitImageReference()
        {
            if (portraitImage != null || panel == null)
                return;

            if (_portraitFrameRect != null)
                portraitImage = _portraitFrameRect.Find("PortraitImage")?.GetComponent<Image>();

            if (portraitImage == null)
                portraitImage = panel.Find("PortraitImage")?.GetComponent<Image>();
        }

        static string FormatBody(string speaker, string body)
        {
            if (string.IsNullOrWhiteSpace(speaker))
                return body;

            var trimmed = body.TrimStart();
            while (trimmed.Length > 0 && (trimmed[0] == '—' || trimmed[0] == '-' || trimmed[0] == '\u2013' || trimmed[0] == '\u2014'))
                trimmed = trimmed.Substring(1).TrimStart();

            return trimmed;
        }

        void RefreshPanelStyle()
        {
            if (panelGraphic == null)
                return;

            if (_hasFigmaChrome)
            {
                panelGraphic.color = ResolveTextPanelColor();
                return;
            }

            panelGraphic.color = _mode switch
            {
                StoryUiMode.Monologue => UiStyle.PanelBackgroundMonologue,
                StoryUiMode.System => UiStyle.PanelBackgroundSystem,
                _ => UiStyle.PanelBackground
            };
        }

        void RefreshPortraitFrameStyle()
        {
            if (_portraitFrameRect == null)
                return;

            var procedural = _portraitFrameRect.GetComponent<ProceduralImage>();
            if (procedural != null)
                procedural.color = UiStyle.PortraitSlotBackground;
        }

        void RefreshSpeakerState()
        {
            if (speakerText == null)
                return;

            var isMonologue = _mode == StoryUiMode.Monologue;
            speakerText.gameObject.SetActive(!isMonologue);
            speakerText.color = _mode == StoryUiMode.System
                ? UiStyle.TextMuted
                : UiStyle.TextSpeaker;

            if (bodyText == null)
                return;

            bodyText.color = _mode switch
            {
                StoryUiMode.Monologue => UiStyle.TextMonologue,
                StoryUiMode.System => UiStyle.TextSystem,
                _ => UiStyle.TextLight
            };
        }

        void RefreshPortraitVisibility()
        {
            var showColumn = ShowPortraitColumn;

            if (_portraitFrameRect != null)
            {
                _portraitFrameRect.gameObject.SetActive(showColumn);
                StripTestPhotoFromPortraitFrame();
            }

            if (portraitImage == null)
                return;

            LayoutPortraitImageInFrame();

            var showPhoto = showColumn && _currentPortrait != null;
            if (showPhoto)
            {
                portraitImage.sprite = _currentPortrait;
                portraitImage.preserveAspect = true;
                portraitImage.color = Color.white;
            }

            portraitImage.gameObject.SetActive(showPhoto);
        }

        void RefreshFrameLayout()
        {
            if (panel == null)
                return;

            var showPortraitColumn = ShowPortraitColumn;
            ApplyPanelAnchors(showPortraitColumn);

            if (_textFrameRect != null)
                ApplyTextFrameLayout(_textFrameRect, showPortraitColumn);

            ApplyTextRects(_hasFigmaChrome);
        }

        void ApplyPanelAnchors(bool showPortraitColumn)
        {
            if (showPortraitColumn)
            {
                panel.anchorMin = new Vector2(UiLayout.DialogueAreaMinX, UiLayout.DialogueAreaMinY);
                panel.anchorMax = new Vector2(UiLayout.DialogueAreaMaxX, UiLayout.DialogueAreaMaxY);
            }
            else
            {
                panel.anchorMin = new Vector2(UiLayout.DialogueMonologueAreaMinX, UiLayout.DialogueAreaMinY);
                panel.anchorMax = new Vector2(UiLayout.DialogueMonologueAreaMaxX, UiLayout.DialogueAreaMaxY);
            }

            panel.pivot = new Vector2(0f, 0f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = Vector2.zero;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }

        static void ApplyTextFrameLayout(RectTransform textFrame, bool showPortraitColumn)
        {
            textFrame.anchorMin = Vector2.zero;
            textFrame.anchorMax = Vector2.one;
            textFrame.pivot = new Vector2(0f, 0.5f);
            textFrame.anchoredPosition = Vector2.zero;
            textFrame.sizeDelta = Vector2.zero;
            textFrame.offsetMin = showPortraitColumn
                ? new Vector2(UiLayout.SizeDialoguePortrait.x, 0f)
                : Vector2.zero;
            textFrame.offsetMax = Vector2.zero;
        }

        void ApplyTextRects(bool textsInsideTextFrame)
        {
            var insetLeft = textsInsideTextFrame ? 16f : 16f;

            if (speakerText != null)
            {
                var rect = speakerText.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.offsetMin = new Vector2(insetLeft, -44f);
                rect.offsetMax = new Vector2(-16f, -12f);
            }

            if (bodyText != null)
            {
                var rect = bodyText.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0f, 1f);
                rect.offsetMin = new Vector2(insetLeft, 12f);
                rect.offsetMax = new Vector2(-16f, -48f);
            }
        }

        void EnsureTextFrameChrome()
        {
            if (_textFrameRect == null)
                return;

            var legacy = _textFrameRect.GetComponent<Image>();
            if (legacy != null && legacy.sprite != null)
            {
                legacy.sprite = null;
                legacy.color = ResolveTextPanelColor();
                legacy.type = Image.Type.Simple;
                panelGraphic = legacy;
                _hasFigmaChrome = true;
                return;
            }

            var graphic = _textFrameRect.GetComponent<Graphic>();
            if (graphic != null)
            {
                panelGraphic = graphic;
                _hasFigmaChrome = true;
            }
        }

        Color ResolveTextPanelColor() =>
            _mode switch
            {
                StoryUiMode.Monologue => UiStyle.PanelBackgroundMonologue,
                StoryUiMode.System => UiStyle.PanelBackgroundSystem,
                _ => UiStyle.PanelBackground
            };

        void LayoutPortraitImageInFrame()
        {
            if (portraitImage == null)
                return;

            var rect = portraitImage.rectTransform;
            var pad = UiLayout.DialoguePortraitPadding;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = new Vector2(pad, pad);
            rect.offsetMax = new Vector2(-pad, -pad);
            portraitImage.preserveAspect = true;
        }

        void StripTestPhotoFromPortraitFrame()
        {
            if (_portraitFrameRect == null)
                return;

            var frameImage = _portraitFrameRect.GetComponent<Image>();
            if (frameImage != null && frameImage.sprite != null)
            {
                frameImage.sprite = null;
                frameImage.color = Color.clear;
            }
        }

        static StoryUiMode NormalizeMode(StoryUiMode mode) =>
            mode switch
            {
                StoryUiMode.Monologue => StoryUiMode.Monologue,
                StoryUiMode.System => StoryUiMode.System,
                StoryUiMode.PhoneInbox => StoryUiMode.PhoneInbox,
                _ => StoryUiMode.Dialogue
            };
    }
}
