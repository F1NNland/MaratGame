namespace MaratGame.Presentation
{
    /// <summary>Пути к портретам (см. docs/ASSETS.md, characterId).</summary>
    public static class CharacterPhotoPaths
    {
        public const string Marat = "Assets/Фото Марат/Люди/Марат.jpg";
        public const string Alevtina = "Assets/Фото Марат/Люди/Екатерина.jpg";
        public const string Kozlikhin = "Assets/Фото Марат/Люди/Козыреа.jpg";
        public const string Nozdrikov = "Assets/Фото Марат/Люди/Назаров.jpg";

        public static string ResolveAssetPath(string characterId) =>
            characterId switch
            {
                "marat" => Marat,
                "alevtina" => Alevtina,
                "kozlikhin" => Kozlikhin,
                "nozdrikov" => Nozdrikov,
                _ => null
            };
    }
}
