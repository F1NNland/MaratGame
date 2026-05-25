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
                "meeting_room" => "ПЕРЕГОВОРКА",
                "evening" => "ВЕЧЕР",
                _ => locationId.ToUpperInvariant()
            };
        }

        /// <summary>Строка как в Figma: «ГЛАВА 1 — ХОЛЛ БАНКА».</summary>
        public static string FormatChapterLocation(string chapterLabel, string locationId)
        {
            var location = GetDisplayName(locationId);
            if (string.IsNullOrWhiteSpace(chapterLabel))
                return location;

            var chapter = chapterLabel;
            var dot = chapter.IndexOf('·');
            if (dot >= 0)
                chapter = chapter.Substring(0, dot).Trim();

            if (string.IsNullOrWhiteSpace(location))
                return chapter;

            return $"{chapter} — {location}";
        }
    }
}
