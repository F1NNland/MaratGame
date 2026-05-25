using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Строка входящего в телефоне: белый MPImage-баббл + TMP, высота по preferredHeight / lineCount.
    /// </summary>
    [RequireComponent(typeof(LayoutElement))]
    public sealed class PhoneMessageBubbleRow : MonoBehaviour
    {
        TextMeshProUGUI _text;
        LayoutElement _layout;
        RectTransform _rowRect;
        RectTransform _bubbleRect;

        void Awake() => CacheRefs();

        void CacheRefs()
        {
            _rowRect = (RectTransform)transform;
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _layout = GetComponent<LayoutElement>();
            var bubble = transform.Find("Bubble");
            _bubbleRect = bubble as RectTransform;
        }

        /// <summary>Подгоняет баббл и TMP под заданную ширину строки (контент списка).</summary>
        public void Refit(float rowContentWidth)
        {
            if (_text == null)
                CacheRefs();

            if (_text == null || _rowRect == null)
                return;

            foreach (var fitter in GetComponents<ContentSizeFitter>())
                Destroy(fitter);

            var padH = UiLayout.PhoneMessagePadH;
            var padV = UiLayout.PhoneMessagePadV;
            var contentWidth = rowContentWidth > 20f ? rowContentWidth : UiLayout.PhoneMessageRowWidth;
            var innerWidth = Mathf.Max(80f, contentWidth - padH * 2f);

            _rowRect.anchorMin = new Vector2(0f, 1f);
            _rowRect.anchorMax = new Vector2(1f, 1f);
            _rowRect.pivot = new Vector2(0.5f, 1f);
            _rowRect.sizeDelta = new Vector2(0f, _layout != null && _layout.preferredHeight > 0f ? _layout.preferredHeight : 32f);

            _text.textWrappingMode = TextWrappingModes.Normal;
            _text.overflowMode = TextOverflowModes.Overflow;
            _text.raycastTarget = false;

            var textRect = _text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.pivot = new Vector2(0f, 1f);
            textRect.anchoredPosition = new Vector2(padH, -padV);
            textRect.sizeDelta = new Vector2(innerWidth, 0f);

            var textHeight = MeasureTextHeight(_text, innerWidth);
            textHeight = Mathf.Max(_text.fontSize, textHeight);
            textRect.sizeDelta = new Vector2(innerWidth, textHeight);

            var totalHeight = textHeight + padV * 2f;
            _rowRect.sizeDelta = new Vector2(0f, totalHeight);

            if (_layout != null)
            {
                _layout.minHeight = totalHeight;
                _layout.preferredHeight = totalHeight;
            }

            if (_bubbleRect != null)
            {
                _bubbleRect.anchorMin = Vector2.zero;
                _bubbleRect.anchorMax = Vector2.one;
                _bubbleRect.offsetMin = Vector2.zero;
                _bubbleRect.offsetMax = Vector2.zero;
            }

            LayoutRebuilder.MarkLayoutForRebuild(_rowRect);
        }

        static float MeasureTextHeight(TextMeshProUGUI text, float innerWidth)
        {
            var rect = text.rectTransform;
            rect.sizeDelta = new Vector2(innerWidth, 0f);
            text.ForceMeshUpdate(true, true);
            Canvas.ForceUpdateCanvases();

            var preferred = text.GetPreferredValues(innerWidth, 0f).y;
            var textHeight = Mathf.Max(preferred, text.preferredHeight);

            var lineCount = text.textInfo.lineCount;
            if (lineCount > 1)
            {
                var linesHeight = 0f;
                for (var i = 0; i < lineCount; i++)
                {
                    var line = text.textInfo.lineInfo[i];
                    linesHeight += line.lineHeight;
                }

                if (linesHeight > textHeight)
                    textHeight = linesHeight;
            }

            if (text.renderedHeight > textHeight)
                textHeight = text.renderedHeight;

            var bounds = text.textBounds;
            if (bounds.size.y > textHeight)
                textHeight = bounds.size.y;

            return Mathf.Ceil(textHeight + UiLayout.PhoneMessageHeightBuffer);
        }
    }
}
