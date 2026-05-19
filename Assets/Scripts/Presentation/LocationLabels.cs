namespace MaratGame.Presentation
{
    /// <summary>
    /// Заголовки локаций для HUD (MVP).
    /// </summary>
    public static class LocationLabels
    {
        public static string GetDisplayName(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
                return string.Empty;

            return locationId switch
            {
                "hall" => "ХОЛЛ БАНКА",
                "canteen" => "СТОЛОВАЯ",
                "cabinet" => "КАБИНЕТ",
                "elevator" => "ЛИФТЫ",
                "planerka" => "ПЛАНЕРКА",
                "krrb" => "КРРБ",
                "toilet" => "ТУАЛЕТ",
                _ => locationId.ToUpperInvariant()
            };
        }
    }
}
