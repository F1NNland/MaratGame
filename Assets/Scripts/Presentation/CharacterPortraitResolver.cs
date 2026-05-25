using MaratGame.Data;

namespace MaratGame.Presentation
{
    /// <summary>characterId для слота портрета: из узла или по отображаемому имени говорящего.</summary>
    public static class CharacterPortraitResolver
    {
        public static string ResolveCharacterId(StoryNodeData node)
        {
            if (node == null)
                return null;

            if (!string.IsNullOrWhiteSpace(node.portraitCharacterId))
                return node.portraitCharacterId.Trim();

            return ResolveCharacterIdFromSpeaker(node.speaker);
        }

        public static string ResolveCharacterIdFromSpeaker(string speaker)
        {
            if (string.IsNullOrWhiteSpace(speaker))
                return null;

            return speaker.Trim() switch
            {
                "Марат" => CharacterIds.Marat,
                "Алевтина" => CharacterIds.Alevtina,
                "Козлихин" => CharacterIds.Kozlikhin,
                "Ноздриков" => CharacterIds.Nozdrikov,
                _ => null
            };
        }
    }
}
