namespace MaratGame.Data
{
    /// <summary>Флаги утренних веток (MVP шаг 10).</summary>
    public static class MorningBranchFlags
    {
        public const string MetKozlikhin = "met_kozlikhin";
        public const string MetKozlikhinBreakfast = "met_kozlikhin_breakfast";
        public const string AteAloneReflection = "ate_alone_reflection";
        public const string CoffeeToGo = "coffee_to_go";
        public const string MetNozdrikov = "met_nozdrikov";
        public const string MetNozdrikovRoute = "met_nozdrikov_route";
        public const string ElevatorMessagesRead = "elevator_messages_read";
        public const string RandomFloorEvent = "random_floor_event";
        public const string MetAlevtinaCabinet = "met_alevtina_cabinet";
    }

    public static class MorningBranchNodes
    {
        public const string AlevtinaTeaseNodeId = "alevtina_tease";
        public const string CanteenKozlikhinNodeId = "canteen_kozlikhin";
        public const string CanteenAloneNodeId = "canteen_alone";
        public const string CanteenCoffeeNodeId = "canteen_coffee";
        public const string ElevatorNozdrikovNodeId = "elevator_nozdrikov";
        public const string ElevatorMessagesNodeId = "elevator_messages";
        public const string ElevatorRandomFloorNodeId = "elevator_random_floor";
    }

    public static class CharacterIds
    {
        public const string Alevtina = "alevtina";
        public const string Kozlikhin = "kozlikhin";
        public const string Nozdrikov = "nozdrikov";
    }
}
