using MaratGame.Presentation;
using MPUIKIT;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

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
    }
}
