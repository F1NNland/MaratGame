namespace MaratGame.Presentation
{
    /// <summary>Фоны локаций (иллюстрации VN). См. docs/ASSETS.md.</summary>
    public static class LocationPhotoPaths
    {
        public const string Hall = "Assets/Art/Generated/Locations/hall_bg.png";
        public const string Canteen = "Assets/Art/Generated/Locations/canteen_bg.png";
        public const string Planerka = "Assets/Art/Generated/Locations/planerka_bg.png";
        public const string Toilet = "Assets/Art/Generated/Locations/toilet_bg.png";
        public const string Krrb = "Assets/Art/Generated/Locations/krrb_bg.png";
        public const string Cabinet = "Assets/Art/Generated/Locations/cabinet_bg.png";
        public const string Elevator = "Assets/Art/Generated/Locations/elevator_bg.png";
        public const string Evening = "Assets/Art/Generated/Locations/evening_bg.png";

        public static string ResolveAssetPath(string locationId) =>
            locationId switch
            {
                "hall" => Hall,
                "canteen" => Canteen,
                "planerka" or "meeting_room" => Planerka,
                "toilet" => Toilet,
                "krrb" => Krrb,
                "cabinet" => Cabinet,
                "elevator" => Elevator,
                "evening" => Evening,
                _ => null
            };
    }
}
