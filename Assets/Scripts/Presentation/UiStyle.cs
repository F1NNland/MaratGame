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
        public const float PhoneMessageBubbleRadius = 14f;

        public static readonly Color PanelBackground = new(0.08f, 0.09f, 0.12f, 0.96f);
        /// <summary>Слот портрета слева в окне диалога (Figma).</summary>
        public static readonly Color PortraitSlotBackground = new(0.1f, 0.15f, 0.22f, 0.96f);
        public static readonly Color PanelBackgroundMonologue = new(0.08f, 0.09f, 0.12f, 0.96f);
        public static readonly Color TextSpeaker = new(0.45f, 0.78f, 0.95f, 1f);
        public static readonly Color PanelBackgroundSystem = new(0.07f, 0.11f, 0.15f, 0.88f);
        public static readonly Color ScreenBackground = new(0.05f, 0.06f, 0.08f, 1f);
        public static readonly Color AccentButton = new(0.18f, 0.38f, 0.62f, 1f);
        public static readonly Color TextLight = new(0.94f, 0.95f, 0.97f, 1f);
        public static readonly Color PhoneMessageBubble = Color.white;
        public static readonly Color PhoneMessageText = new(0.12f, 0.14f, 0.18f, 1f);
        public static readonly Color PhoneInboxTitle = new(0.94f, 0.95f, 0.97f, 1f);
        public static readonly Color TextMuted = new(0.72f, 0.76f, 0.82f, 1f);
        public static readonly Color TextMonologue = new(0.88f, 0.91f, 0.95f, 1f);
        public static readonly Color TextSystem = new(0.80f, 0.90f, 0.96f, 1f);

        /// <summary>Фото-фоны локаций: белый tint, иначе JPEG почти чёрный.</summary>
        public static readonly Color PhotoBackground = Color.white;
    }
}
