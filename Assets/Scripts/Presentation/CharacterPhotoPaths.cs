namespace MaratGame.Presentation
{
    /// <summary>Портреты персонажей (PNG, VN-стиль). См. docs/ASSETS.md.</summary>
    public static class CharacterPhotoPaths
    {
        public const string Marat = "Assets/Art/Generated/Characters/marat_portrait.png";
        public const string Alevtina = "Assets/Art/Generated/Characters/alevtina_portrait.png";
        public const string Kozlikhin = "Assets/Art/Generated/Characters/kozlikhin_portrait.png";
        public const string Nozdrikov = "Assets/Art/Generated/Characters/nozdrikov_portrait.png";

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
