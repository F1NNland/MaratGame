using MaratGame.Core;

namespace MaratGame.Data
{
    /// <summary>
    /// Прогресс утра по ТЗ: из холла — столовая, лифты, кабинет; «Осмотреться»
    /// только после нескольких веток, не после одного диалога.
    /// </summary>
    public static class MorningBranchProgress
    {
        /// <summary>Минимум зон (из трёх: столовая / лифты / кабинет).</summary>
        public const int MinAreasExploredForHubInspect = 2;

        public static bool HasCanteenActivity(GameFlags flags) =>
            flags.HasFlag(MorningBranchFlags.MetKozlikhin)
            || flags.HasFlag(MorningBranchFlags.MetKozlikhinBreakfast)
            || flags.HasFlag(MorningBranchFlags.AteAloneReflection)
            || flags.HasFlag(MorningBranchFlags.CoffeeToGo);

        public static bool HasElevatorActivity(GameFlags flags) =>
            flags.HasFlag(MorningBranchFlags.MetNozdrikov)
            || flags.HasFlag(MorningBranchFlags.MetNozdrikovRoute)
            || flags.HasFlag(MorningBranchFlags.ElevatorMessagesRead)
            || flags.HasFlag(MorningBranchFlags.RandomFloorEvent);

        public static bool HasCabinetActivity(GameFlags flags) =>
            flags.HasFlag(MorningBranchFlags.MetAlevtinaCabinet);

        public static int CountExploredAreas(GameFlags flags)
        {
            var count = 0;
            if (HasCanteenActivity(flags))
                count++;
            if (HasElevatorActivity(flags))
                count++;
            if (HasCabinetActivity(flags))
                count++;
            return count;
        }

        public static bool IsReadyForHubBirthdayInspect(GameState state)
        {
            if (state == null)
                return false;

            if (state.DecisionsCount < BirthdayEndNodes.MinDecisionsForBirthday)
                return false;

            return CountExploredAreas(state.Flags) >= MinAreasExploredForHubInspect;
        }

        /// <summary>Повторный заход в утреннюю зону из холла (после выбора и возврата).</summary>
        public static bool CanEnterMorningArea(string entryNodeId, GameFlags flags)
        {
            if (string.IsNullOrEmpty(entryNodeId) || flags == null)
                return true;

            return entryNodeId switch
            {
                "canteen_entry" => !HasCanteenActivity(flags),
                "elevator_entry" => !HasElevatorActivity(flags),
                "cabinet_entry" => !HasCabinetActivity(flags),
                _ => true
            };
        }
    }
}
