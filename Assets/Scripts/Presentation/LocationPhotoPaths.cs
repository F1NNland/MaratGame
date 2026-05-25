namespace MaratGame.Presentation
{
    /// <summary>Пути к временным фото локаций (см. docs/ASSETS.md).</summary>
    public static class LocationPhotoPaths
    {
        public const string Hall = "Assets/Фото Марат/Холл/photo_2026-05-15_15-09-51.jpg";
        public const string Canteen = "Assets/Фото Марат/Столовая/photo_2026-05-15_15-09-40.jpg";
        public const string Planerka = "Assets/Фото Марат/Планерка/photo_2026-05-15_15-10-12.jpg";
        public const string Toilet = "Assets/Фото Марат/Туалет/photo_2026-05-15_15-10-22.jpg";
        public const string Krrb = "Assets/Фото Марат/КРРБ/photo_2026-05-15_15-11-42.jpg";

        public static string ResolveAssetPath(string locationId) =>
            locationId switch
            {
                "hall" => Hall,
                "canteen" => Canteen,
                "planerka" or "meeting_room" => Planerka,
                "toilet" => Toilet,
                "krrb" => Krrb,
                "cabinet" or "elevator" or "evening" => Hall,
                _ => null
            };
    }
}
