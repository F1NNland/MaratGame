using MaratGame.Presentation;
using MPUIKIT;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using CanvasScaler = UnityEngine.UI.CanvasScaler;

namespace MaratGame.Editor
{
    /// <summary>
    /// MPUIKit + Procedural UI Image для панелей и кнопок. См. docs/UI_KIT.md.
    /// </summary>
    internal static class UiKitFactory
    {
        public static void EnsureCanvasSupportsProceduralImage(Canvas canvas)
        {
            if (canvas == null)
                return;

            var channels = canvas.additionalShaderChannels;
            channels |= AdditionalCanvasShaderChannels.TexCoord1;
            channels |= AdditionalCanvasShaderChannels.TexCoord2;
            channels |= AdditionalCanvasShaderChannels.TexCoord3;
            canvas.additionalShaderChannels = channels;

            ConfigureCanvasScaler(canvas);
            StretchCanvasRoot(canvas);
        }

        /// <summary>
        /// Фото-фоны на отдельном Canvas без TexCoord1–3 (иначе Default UI Image не рисуется).
        /// </summary>
        public static Canvas EnsureBackgroundCanvas(int sortingOrder = 0)
        {
            var existing = GameObject.Find("BackgroundCanvas");
            if (existing != null)
            {
                var existingCanvas = existing.GetComponent<Canvas>();
                if (existingCanvas != null)
                    existingCanvas.sortingOrder = sortingOrder;
                return existingCanvas;
            }

            var canvasGo = new GameObject("BackgroundCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            ConfigureCanvasScaler(canvas);
            StretchCanvasRoot(canvas);
            return canvas;
        }

        public static void ConfigureCanvasScaler(Canvas canvas)
        {
            if (canvas == null)
                return;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UiLayout.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = UiLayout.ScreenMatch;
            scaler.scaleFactor = 1f;
        }

        static void StretchCanvasRoot(Canvas canvas)
        {
            var rect = canvas.GetComponent<RectTransform>();
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static ProceduralImage AddRoundedPanel(GameObject host, Color color, float cornerRadius)
        {
            var image = host.GetComponent<ProceduralImage>();
            if (image == null)
            {
                var legacy = host.GetComponent<Image>();
                if (legacy != null)
                    Object.DestroyImmediate(legacy);

                image = host.AddComponent<ProceduralImage>();
            }

            image.color = color;
            image.raycastTarget = true;
            image.FalloffDistance = 1f;
            image.BorderWidth = 0f;
            image.ModifierType = typeof(FreeModifier);

            var modifier = host.GetComponent<FreeModifier>();
            if (modifier != null)
            {
                var r = cornerRadius;
                modifier.Radius = new Vector4(r, r, r, r);
            }

            image.SetAllDirty();
            return image;
        }

        public static void ApplyPortraitSlotChrome(GameObject portraitFrame)
        {
            if (portraitFrame == null)
                return;

            PortraitSlotChrome.Apply(portraitFrame.GetComponent<RectTransform>());
        }

        public static MPImage AddMessageBubbleGraphic(GameObject host, Color color, float cornerRadius)
        {
            var image = AddRoundedButtonGraphic(host, color, cornerRadius);
            image.raycastTarget = false;
            return image;
        }

        public static MPImage AddRoundedButtonGraphic(GameObject host, Color color, float cornerRadius)
        {
            var image = host.GetComponent<MPImage>();
            if (image == null)
            {
                var legacy = host.GetComponent<Image>();
                if (legacy != null)
                    Object.DestroyImmediate(legacy);

                image = host.AddComponent<MPImage>();
            }

            image.color = color;
            image.raycastTarget = true;
            image.DrawShape = DrawShape.Rectangle;
            image.MaterialMode = MaterialMode.Dynamic;

            var rectangle = image.Rectangle;
            var radius = new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius);
            rectangle.CornerRadius = radius;
            image.Rectangle = rectangle;

            image.SetAllDirty();
            return image;
        }

        public static void ReplaceGraphicWithRoundedButton(GameObject buttonRoot)
        {
            AddRoundedButtonGraphic(buttonRoot, UiStyle.AccentButton, UiStyle.ButtonCornerRadius);
        }

        public static LayoutElement AddLayoutElement(GameObject host, Vector2 size)
        {
            var layout = host.GetComponent<LayoutElement>();
            if (layout == null)
                layout = host.AddComponent<LayoutElement>();

            layout.minWidth = size.x;
            layout.minHeight = size.y;
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
            layout.flexibleWidth = 1f;
            layout.flexibleHeight = 0f;
            return layout;
        }
    }
}
