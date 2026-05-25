using MaratGame.Data;

namespace MaratGame.Narrative
{
    public readonly struct StoryChoiceAvailability
    {
        public int Index { get; }
        public StoryChoice Choice { get; }
        public bool IsAvailable { get; }
        public string Reason { get; }

        public StoryChoiceAvailability(int index, StoryChoice choice, bool isAvailable, string reason)
        {
            Index = index;
            Choice = choice;
            IsAvailable = isAvailable;
            Reason = reason;
        }
    }
}
