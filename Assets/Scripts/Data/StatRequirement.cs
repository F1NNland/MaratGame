using System;
using MaratGame.Core;

namespace MaratGame.Data
{
    [Serializable]
    public struct StatRequirement
    {
        public StatType stat;
        public bool useMin;
        public int minValue;
        public bool useMax;
        public int maxValue;
    }
}
