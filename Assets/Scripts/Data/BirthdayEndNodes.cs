namespace MaratGame.Data
{
    /// <summary>Узлы финала MVP (шаг 11).</summary>
    public static class BirthdayEndNodes
    {
        public const string BirthdaySceneNodeId = "birthday_scene";
        public const string EndingScreenNodeId = "ending_screen";

        /// <summary>Минимум решений для кнопки «Осмотреться» / перехода к поздравлению.</summary>
        public const int MinDecisionsForBirthday = 2;
    }

    public static class BirthdayEndFlags
    {
        public const string BirthdaySeen = "birthday_seen";
    }
}
