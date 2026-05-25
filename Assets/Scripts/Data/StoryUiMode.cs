namespace MaratGame.Data
{
    /// <summary>
    /// Режим отображения диалога на UI.
    /// </summary>
    public enum StoryUiMode
    {
        Dialogue = 0,
        Monologue = 1,
        System = 2,
        /// <summary>Список входящих в UI телефона (бабблы, скролл).</summary>
        PhoneInbox = 3
    }
}
