using System;

namespace MaratGame.Core
{
    /// <summary>
    /// Модель состояния игры (без Unity). Инжектируется в StoryRunner на шаге 02.
    /// </summary>
    public sealed class GameState
    {
        public static GameState Instance { get; } = new();

        public GameStats Stats { get; } = new();
        public GameFlags Flags { get; } = new();

        public int DecisionsCount { get; private set; }
        public string CurrentTime { get; private set; } = GameDefaults.StartTime;
        public string CurrentLocationId { get; private set; } = GameDefaults.StartLocationId;

        GameState()
        {
            Reset();
        }

        public void Reset()
        {
            Stats.Reset(GameDefaults.StartRespect, GameDefaults.StartCalm);
            Flags.Clear();
            DecisionsCount = 0;
            CurrentTime = GameDefaults.StartTime;
            CurrentLocationId = GameDefaults.StartLocationId;
        }

        public void RecordDecision() => DecisionsCount++;

        public void SetTime(string timeDisplay)
        {
            if (string.IsNullOrWhiteSpace(timeDisplay))
                throw new ArgumentException("Time display cannot be empty.", nameof(timeDisplay));

            CurrentTime = timeDisplay;
        }

        public void SetLocation(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
                throw new ArgumentException("Location id cannot be empty.", nameof(locationId));

            CurrentLocationId = locationId;
        }

        public void ApplyStatChange(StatChange change) => Stats.ApplyChange(change);
    }
}
