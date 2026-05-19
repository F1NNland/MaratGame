using MaratGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    internal static class TmpUiFactory
    {
        public static TMP_FontAsset DefaultFont =>
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpAssets.DefaultFontPath);

        public static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            float fontSize,
            TextAlignmentOptions alignment,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.richText = true;
            return text;
        }

        public static TextAlignmentOptions FromLegacyAnchor(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.MidlineLeft,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.MidlineRight,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.TopLeft
            };
        }
    }
}
