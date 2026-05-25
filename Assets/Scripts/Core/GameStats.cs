using System;

namespace MaratGame.Core
{
    public sealed class GameStats
    {
        public const int MinValue = 0;
        public const int MaxValue = 100;

        public int Respect { get; private set; }
        public int Calm { get; private set; }
        public int Chaos { get; private set; }

        public void Reset(int respect, int calm, int chaos)
        {
            Respect = Clamp(respect);
            Calm = Clamp(calm);
            Chaos = Clamp(chaos);
        }

        public void AddRespect(int delta) => Respect = Clamp(Respect + delta);

        public void AddCalm(int delta) => Calm = Clamp(Calm + delta);

        public void AddChaos(int delta) => Chaos = Clamp(Chaos + delta);

        public void ApplyChange(StatChange change)
        {
            switch (change.Stat)
            {
                case StatType.Respect:
                    AddRespect(change.Delta);
                    break;
                case StatType.Calm:
                    AddCalm(change.Delta);
                    break;
                case StatType.Chaos:
                    AddChaos(change.Delta);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change.Stat), change.Stat, null);
            }
        }

        public static int Clamp(int value) => Math.Clamp(value, MinValue, MaxValue);
    }
}
