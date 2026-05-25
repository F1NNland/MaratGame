namespace MaratGame.Data
{
    /// <summary>Узлы перехода утра → полного дня и вечернего финала.</summary>
    public static class BirthdayEndNodes
    {
        public const string BirthdaySceneNodeId = "birthday_scene";
        public const string PrePlanerkaBridgeNodeId = "pre_planerka_bridge";
        public const string Chapter2CabinetIntroNodeId = "chapter2_cabinet_intro";

        /// <summary>Минимум сюжетных выборов (ветки), не кликов навигации.</summary>
        public const int MinDecisionsForBirthday = 3;
    }

    public static class BirthdayEndFlags
    {
        public const string BirthdaySeen = "birthday_seen";
    }

    /// <summary>Узлы главы 2 (кабинет до планерки).</summary>
    public static class Chapter2CabinetNodes
    {
        public const string EntryNodeId = "chapter2_cabinet_entry";
        public const string MailNodeId = "chapter2_cabinet_mail";
        public const string AlevtinaNodeId = "chapter2_cabinet_alevtina";
        public const string ToiletIntroNodeId = "toilet_intro";
        public const string ToiletWashFaceNodeId = "toilet_wash_face";
        public const string ToiletPhoneMessagesNodeId = "toilet_phone_messages";
        public const string ToiletSilenceMonologueNodeId = "toilet_silence_monologue";
        public const string ToiletMiddleStallNodeId = "toilet_middle_stall";
        public const string ToiletHelpEmployeeExitNodeId = "toilet_help_employee_exit";
        public const string Meeting3544NodeId = "meeting_35_44_intro";
        public const string ToiletHelpEmployeeEntryNodeId = "toilet_help_employee_entry";
        public const string ToiletHelpEmployeeContentNodeId = "toilet_help_employee_content";
    }

    /// <summary>Флаги ветвления главы 2 для следующих глав.</summary>
    public static class Chapter2CabinetFlags
    {
        public const string MailChecked = "cabinet_mail_checked";
        public const string AlevtinaBriefed = "cabinet_alevtina_briefed";
        public const string KrrbClosedMeetingHint = "krrb_closed_meeting_hint";
        public const string ToiletBreakTaken = "cabinet_toilet_break_taken";
        public const string Meeting3544Visited = "meeting_35_44_visited";
    }

    /// <summary>Флаги локации "Туалет" (post-MVP шаг 05).</summary>
    public static class Chapter2ToiletFlags
    {
        public const string WashedFace = "toilet_washed_face";
        public const string PhoneMessagesChecked = "toilet_phone_messages_checked";
        public const string SilenceMonologueHeard = "toilet_silence_monologue_heard";
        public const string MiddleStallEntered = "toilet_middle_stall_entered";
    }

    /// <summary>Флаги мини-сюжета "Помочь сотруднику" (post-MVP шаг 07).</summary>
    public static class Chapter2MiniStoryFlags
    {
        public const string HelpedEmployee = "helped_employee";
        public const string HandledDelicately = "handled_delicately";
    }

    /// <summary>Узлы главы 3 (планерка).</summary>
    public static class Chapter3PlanerkaNodes
    {
        public const string IntroNodeId = "planerka_intro";
        public const string BusinessReactionNodeId = "planerka_business_reaction";
        public const string BusinessFollowupNodeId = "planerka_business_followup";
        public const string HumorReactionNodeId = "planerka_humor_reaction";
        public const string HumorFollowupNodeId = "planerka_humor_followup";
        public const string ObserverReactionNodeId = "planerka_observer_reaction";
        public const string ObserverFollowupNodeId = "planerka_observer_followup";
    }

    /// <summary>Флаги выбора стиля поведения на планерке (post-MVP шаг 08).</summary>
    public static class Chapter3PlanerkaFlags
    {
        public const string BusinessStyle = "planerka_business_style";
        public const string HumorStyle = "planerka_humor_style";
        public const string ObserverStyle = "planerka_observer_style";
    }

    /// <summary>Узлы главы 4 (КРРБ/УК).</summary>
    public static class Chapter4KrrbNodes
    {
        public const string IntroNodeId = "krrb_intro";
        public const string SeatAlevtinaReactionNodeId = "krrb_seat_alevtina_reaction";
        public const string SeatAlevtinaFollowupNodeId = "krrb_seat_alevtina_followup";
        public const string SeatKozlikhinReactionNodeId = "krrb_seat_kozlikhin_reaction";
        public const string SeatKozlikhinFollowupNodeId = "krrb_seat_kozlikhin_followup";
        public const string SeatBackReactionNodeId = "krrb_seat_back_reaction";
        public const string SeatBackFollowupNodeId = "krrb_seat_back_followup";
        public const string EveningIntroNodeId = "evening_intro";
    public const string EveningBankEmptyNodeId = "evening_bank_empty";
    public const string BigCongratulationNodeId = "big_congratulation";
    public const string EveningGoodEndingNodeId = "evening_good_ending";
    }

    /// <summary>Флаги главы 4 (КРРБ/УК).</summary>
    public static class Chapter4KrrbFlags
    {
        public const string SatNearAlevtina = "krrb_sat_near_alevtina";
        public const string SatNearKozlikhin = "krrb_sat_near_kozlikhin";
        public const string SatBackRow = "krrb_sat_back_row";
        public const string HighSocialPresence = "high_social_presence";
    public const string BigCongratulationUnlocked = "big_congratulation_unlocked";
    }

    /// <summary>Технические константы mini-story framework (post-MVP step 06).</summary>
    public static class MiniStoryFramework
    {
        public const string HelpEmployeeMiniStoryId = "mini_help_employee";
        public const string AnalyticsEntryPrefix = "ministory_entered:";
        public const string AnalyticsExitPrefix = "ministory_exited:";
    }
}
