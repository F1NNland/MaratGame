namespace MaratGame.Data
{
    /// <summary>Тексты входящих для <see cref="StoryUiMode.PhoneInbox"/> и утреннего оверлея телефона.</summary>
    public static class IncomingMessagesContent
    {
        /// <summary>Утро в холле (кнопка телефона на hall_morning).</summary>
        public static readonly string[] MorningHall =
        {
            "С днём рождения!",
            "Вы на КРРБ будете?",
            "Алевтина просила зайти",
            "Торт привезли",
            "Срочно нужен комментарий"
        };

        /// <summary>Лифт — «Читать сообщения» (ТЗ: HR, встреча 35.44, шутки).</summary>
        public static readonly string[] ElevatorRide =
        {
            "HR: не забудьте поздравить отдел с днём рождения",
            "Напоминание: встреча 35.44 через несколько минут",
            "Коллега: торт уже в переговорке — не съедайте раньше",
            "Алевтина: зайдите после утренней сводки",
            "КРРБ: короткий апдейт по согласованию",
            "Из чата: «Марат, ты уже старше банковской лицензии»",
            "HR: поздравление отдела отправим в 11:00"
        };

        /// <summary>Туалет — 15 входящих (ТЗ).</summary>
        public static readonly string[] ToiletMidday =
        {
            "С днём рождения!",
            "Где ты?",
            "В 35.44 через 6 минут",
            "Возьми отчёт",
            "У КРРБ новая правка",
            "Тортик уже в переговорке",
            "Ноздриков ищет тебя",
            "Подтверди маршрут",
            "Алевтина online",
            "Нужна подпись",
            "Напомни про пропуск",
            "Козлихин: держим связь",
            "Поздравляю ещё раз",
            "Созвонимся после планёрки",
            "Не опоздай :)"
        };

        public static bool TryGetForNode(string nodeId, out string[] messages)
        {
            messages = null;
            if (string.IsNullOrEmpty(nodeId))
                return false;

            if (nodeId == MorningBranchNodes.ElevatorMessagesNodeId)
            {
                messages = ElevatorRide;
                return true;
            }

            if (nodeId == Chapter2CabinetNodes.ToiletPhoneMessagesNodeId)
            {
                messages = ToiletMidday;
                return true;
            }

            return false;
        }
    }
}
