using System;
using MaratGame.Core;

namespace MaratGame.Data
{
    [Serializable]
    public struct StatChangeEntry
    {
        public StatType stat;
        public int delta;

        public StatChange ToCore() => new(stat, delta);
    }
}
