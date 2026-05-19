using UnityEngine;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Общие цвета и размеры UI (MVP). См. docs/UI_KIT.md.
    /// </summary>
    public static class UiStyle
    {
        public const float PanelCornerRadius = 16f;
        public const float ButtonCornerRadius = 12f;

        public static readonly Color PanelBackground = new(0.08f, 0.09f, 0.12f, 0.92f);
        public static readonly Color ScreenBackground = new(0.05f, 0.06f, 0.08f, 1f);
        public static readonly Color AccentButton = new(0.18f, 0.38f, 0.62f, 1f);
        public static readonly Color TextLight = new(0.94f, 0.95f, 0.97f, 1f);
        public static readonly Color TextMuted = new(0.72f, 0.76f, 0.82f, 1f);
    }
}
