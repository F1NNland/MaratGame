using UnityEngine;



namespace MaratGame.Presentation

{

    /// <summary>

    /// Размеры UI под эталон Figma 2001×981. См. docs/UI_KIT.md и макет Projects.

    /// </summary>

    public static class UiLayout

    {

        public const float RefWidth = 2001f;

        public const float RefHeight = 981f;



        public static readonly Vector2 ReferenceResolution = new(RefWidth, RefHeight);

        public const float ScreenMatch = 0.5f;



        public const float TopChromeHeight = 108f;

        public const float BottomChromeHeight = 204f;

        public static float BottomChromeAnchorMaxY => BottomChromeHeight / RefHeight;



        public const float FontHudTime = 28f;

        public const float FontHudStat = 24f;

        public const float FontHudChapter = 22f;

        public const float FontHudPeriod = 18f;

        public const float FontLocationHeader = 30f;

        public const float FontDialogueSpeaker = 27f;

        public const float FontDialogueBody = 25f;

        public const float FontChoice = 24f;

        /// <summary>Autosize подписи кнопки выбора (1–2 строки в ряду).</summary>
        public const float FontChoiceAutoMin = 17f;

        public const float FontChoiceAutoMax = 22f;

        /// <summary>Autosize в сетке 4+ (уже ячейка).</summary>
        public const float FontChoiceGridAutoMin = 15f;

        public const float FontChoiceGridAutoMax = 19f;

        public const float FontNav = 18f;

        public const float FontPhoneTitle = 27f;

        public const float FontPhoneMessage = 22f;

        public const float FontPhoneClose = 24f;

        public const float FontHubAction = 22f;

        public const float FontMenuTitle = 47f;

        public const float FontMenuSubtitle = 24f;

        public const float FontMenuButton = 25f;



        public static readonly Vector2 SizeHudTime = new(120f, 40f);

        public static readonly Vector2 SizeHudStatValue = new(56f, 32f);
        public static readonly Vector2 SizeHudStat = SizeHudStatValue;
        public static readonly Vector2 SizeHudStatIcon = new(32f, 32f);
        public const float HudStatRightMargin = 24f;
        public const float HudStatIconTextGap = 8f;
        /// <summary>Ширина слота «иконка + %» справа налево.</summary>
        public const float HudStatSlotWidth = 90f;
        public static readonly Vector2 SizeHudChapter = new(667f, 29f);
        public static readonly Vector2 SizeLocationHeader = new(900f, 40f);
        public const float FontPhoneIcon = 36f;

        public static readonly Vector2 SizeHudPeriod = new(180f, 28f);

        public static readonly Vector2 SizeChoiceButton = new(360f, 102f);

        /// <summary>До 3 выборов в один ряд (flex по ширине контейнера).</summary>
        public static readonly Vector2 SizeChoiceButtonRow = new(-1f, 102f);

        /// <summary>Кнопка в сетке 4+ выборов (несколько строк текста).</summary>
        public static readonly Vector2 SizeChoiceButtonGrid = new(-1f, 96f);

        /// <summary>Одна кнопка в столбце (3 длинных варианта).</summary>
        public static readonly Vector2 SizeChoiceButtonStacked = new(-1f, 72f);

        public const float ChoiceLabelPadH = 18f;
        public const float ChoiceLabelPadV = 12f;

        public const float ChoiceGridSpacingX = 14f;
        public const float ChoiceGridSpacingY = 10f;
        public const float ChoiceGridPaddingH = 12f;
        public const float ChoiceGridPaddingV = 10f;

        /// <summary>Ширина полосы выбора (доля Canvas по якорям контейнера).</summary>
        public static float ChoicesAreaWidth => (0.96f - 0.04f) * RefWidth;

        public static readonly Vector2 SizeNavButton = new(144f, 144f);

        public static readonly Vector2 SizeHubDotButton = new(144f, 144f);

        public static readonly Vector2 SizeHubInspectLabel = new(217f, 33f);
        public static readonly Vector2 SizeHubAction = new(380f, 144f);

        public static readonly Vector2 SizePhoneButton = new(53f, 97f);

        public static readonly Vector2 SizePhoneBadge = new(28f, 28f);

        /// <summary>Макет телефона <c>Phone_big.png</c> (318×580).</summary>
        public static readonly Vector2 SizePhonePanel = new(318f, 580f);

        public const float PhoneSideInset = 28f;

        /// <summary>Высота полосы «Входящие» под верхом панели (см. <c>SetPhoneTitleLayout</c>).</summary>
        public const float PhoneTitleBandHeight = 100f;

        public static readonly Vector2 PhoneMessageListInsetMin = new(PhoneSideInset, 52f);

        /// <summary>Утренний оверлей: место под заголовок сверху и «Закрыть» снизу.</summary>
        public static readonly Vector2 PhoneMessageListInsetMax = new(-PhoneSideInset, -PhoneTitleBandHeight);

        /// <summary>Сюжетные входящие: тот же отступ под заголовок, меньше снизу (без кнопки на панели).</summary>
        public static readonly Vector2 PhoneMessageListInsetMinStory = new(PhoneSideInset, 36f);

        public static readonly Vector2 PhoneMessageListInsetMaxStory = new(-PhoneSideInset, -PhoneTitleBandHeight);

        public const float PhoneMessagePadH = 14f;
        public const float PhoneMessagePadV = 10f;

        /// <summary>Запас под перенос строк и нижние выносные (кириллица).</summary>
        public const float PhoneMessageHeightBuffer = 6f;

        /// <summary>Ширина области списка сообщений (панель 318 − боковые inset 28+28).</summary>
        public static float PhoneMessageListWidth =>
            SizePhonePanel.x - PhoneMessageListInsetMin.x + PhoneMessageListInsetMax.x;

        /// <summary>Ширина строки баббла (список − padding VLG 4+4).</summary>
        public static float PhoneMessageRowWidth => PhoneMessageListWidth - 8f;

        public static readonly Vector2 SizePhoneClose = new(252f, 44f);

        /// <summary>Отступ кнопки «Закрыть» от низа панели телефона (выше белой рамки).</summary>
        public const float PhoneCloseBottomOffset = 68f;

        public static readonly Vector2 SizeMenuPanel = new(667f, 236f);

        public static readonly Vector2 SizeMenuButton = new(438f, 65f);



        public static readonly Vector2 SizeDialoguePortrait = new(336f, 303f);
        public static readonly Vector2 SizeDialogueTextPanel = new(667f, 327f);
        public const float DialoguePortraitWidth = 336f;
        public const float DialoguePortraitPadding = 14f;

        public const float DialogueAreaMinX = 0.04f;

        public static float DialogueAreaMaxX =>
            DialogueAreaMinX + (SizeDialoguePortrait.x + SizeDialogueTextPanel.x) / RefWidth;

        /// <summary>Монолог: одна текстовая панель по центру снизу (без колонки портрета).</summary>
        public static float DialogueMonologueAreaMinX =>
            (RefWidth - SizeDialogueTextPanel.x) * 0.5f / RefWidth;

        public static float DialogueMonologueAreaMaxX =>
            DialogueMonologueAreaMinX + SizeDialogueTextPanel.x / RefWidth;

        /// <summary>Нижняя полоса UI: кнопки выбора внутри BottomChrome (как в макете).</summary>
        public const float ChoicesAreaPaddingBottom = 0.028f;

        public const float ChoicesAreaPaddingTop = 0.018f;

        public static float ChoicesAreaMinY => ChoicesAreaPaddingBottom;

        /// <summary>Верхняя граница ряда кнопок (внутри нижней полосы).</summary>
        public static float ChoicesAreaMaxY =>
            BottomChromeAnchorMaxY - ChoicesAreaPaddingTop;

        /// <summary>Верхняя граница для 2+ рядов кнопок — строго внутри BottomChrome.</summary>
        public static float ChoicesAreaMaxYInsideChrome =>
            BottomChromeAnchorMaxY - 0.006f;

        /// <summary>Высота одного ряда кнопок, чтобы N рядов влезли в нижнюю подложку.</summary>
        public static float ComputeChoiceRowHeight(int rowCount)
        {
            var rows = Mathf.Max(1, rowCount);
            var bandPx = (ChoicesAreaMaxYInsideChrome - ChoicesAreaMinY) * RefHeight;
            var inner = bandPx - ChoiceGridPaddingV * 2f - ChoiceGridSpacingY * (rows - 1);
            return Mathf.Clamp(inner / rows, 56f, SizeChoiceButtonGrid.y);
        }

        public static float DialogueAreaMinY => BottomChromeAnchorMaxY + 0.012f;

        public static float DialogueAreaMaxY => DialogueAreaMinY + SizeDialogueTextPanel.y / RefHeight;

        public const float NavBarMinX = 0.04f;

        public const float NavBarMaxX = 0.40f;

        public const float HubActionMinX = 0.58f;

        public const float HubActionMaxX = 0.82f;

        public const float PhoneAnchorX = 0.94f;

    }

}


