using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Подложка слота портрета: светлая заливка + акцентная обводка (ProceduralImage).
    /// </summary>
    public static class PortraitSlotChrome
    {
        const string BorderChildName = "PortraitFrameBorder";

        public static void Apply(RectTransform portraitFrame)
        {
            if (portraitFrame == null)
                return;

            var host = portraitFrame.gameObject;
            EnsureFill(host);
            EnsureBorder(portraitFrame);

            var portraitImage = portraitFrame.Find("PortraitImage");
            if (portraitImage != null)
                portraitImage.SetAsLastSibling();
        }

        public static void LayoutSlot(RectTransform portraitFrame, Image portraitImage)
        {
            if (portraitFrame == null)
                return;

            var sprite = portraitImage != null ? portraitImage.sprite : null;
            LayoutFrame(portraitFrame, UiLayout.ComputePortraitSlotSize(sprite));
            Apply(portraitFrame);

            if (portraitImage == null)
                return;

            LayoutPortraitImage(portraitImage.rectTransform, UiLayout.ComputePortraitContentSize(sprite));
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
        }

        public static void LayoutFrame(RectTransform portraitFrame, Vector2 slotSize)
        {
            if (portraitFrame == null)
                return;

            portraitFrame.anchorMin = Vector2.zero;
            portraitFrame.anchorMax = Vector2.zero;
            portraitFrame.pivot = Vector2.zero;
            portraitFrame.anchoredPosition = Vector2.zero;
            portraitFrame.sizeDelta = slotSize;
        }

        public static void LayoutPortraitImage(RectTransform rect, Vector2 contentSize)
        {
            if (rect == null)
                return;

            var pad = UiLayout.DialoguePortraitPadding;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, pad);
            rect.sizeDelta = contentSize;
        }

        static void EnsureFill(GameObject host)
        {
            var fill = host.GetComponent<ProceduralImage>();
            if (fill == null)
            {
                var legacy = host.GetComponent<Image>();
                if (legacy != null && Application.isPlaying)
                    Object.Destroy(legacy);
#if UNITY_EDITOR
                else if (legacy != null)
                    Object.DestroyImmediate(legacy);
#endif

                fill = host.AddComponent<ProceduralImage>();
            }

            ConfigureRoundedPanel(fill, UiStyle.PortraitSlotBackground, UiStyle.PortraitSlotCornerRadius);
            fill.BorderWidth = 0f;
            fill.raycastTarget = false;
        }

        static void EnsureBorder(RectTransform portraitFrame)
        {
            var borderTransform = portraitFrame.Find(BorderChildName);
            GameObject borderGo;
            if (borderTransform != null)
                borderGo = borderTransform.gameObject;
            else
            {
                borderGo = new GameObject(BorderChildName, typeof(RectTransform));
                borderGo.transform.SetParent(portraitFrame, false);
                borderGo.transform.SetAsFirstSibling();
            }

            var borderRect = borderGo.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;

            var border = borderGo.GetComponent<ProceduralImage>();
            if (border == null)
                border = borderGo.AddComponent<ProceduralImage>();

            ConfigureRoundedPanel(border, UiStyle.PortraitSlotBorder, UiStyle.PortraitSlotCornerRadius);
            border.BorderWidth = UiStyle.PortraitSlotBorderWidth;
            border.raycastTarget = false;
        }

        static void ConfigureRoundedPanel(ProceduralImage image, Color color, float cornerRadius)
        {
            image.color = color;
            image.FalloffDistance = 1f;
            image.ModifierType = typeof(FreeModifier);

            var modifier = image.GetComponent<FreeModifier>();
            if (modifier != null)
            {
                var r = cornerRadius;
                modifier.Radius = new Vector4(r, r, r, r);
            }

            image.SetAllDirty();
        }
    }
}
