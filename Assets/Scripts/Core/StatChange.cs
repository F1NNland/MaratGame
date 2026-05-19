namespace MaratGame.Core
{
    public readonly struct StatChange
    {
        public StatType Stat { get; }
        public int Delta { get; }

        public StatChange(StatType stat, int delta)
        {
            Stat = stat;
            Delta = delta;
        }
    }
}
