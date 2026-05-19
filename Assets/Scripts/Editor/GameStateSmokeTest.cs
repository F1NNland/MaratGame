using MaratGame.Core;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    static class GameStateSmokeTest
    {
        [MenuItem("MaratGame/Run GameState Smoke Test")]
        static void Run()
        {
            var state = GameState.Instance;
            state.Reset();

            if (state.Stats.Respect != 50 || state.Stats.Calm != 60)
                Fail($"Start stats: respect={state.Stats.Respect}, calm={state.Stats.Calm}");

            if (state.CurrentTime != "07:58" || state.CurrentLocationId != "hall")
                Fail($"Start time/location: {state.CurrentTime} @ {state.CurrentLocationId}");

            state.ApplyStatChange(new StatChange(StatType.Respect, 25));
            if (state.Stats.Respect != 75)
                Fail($"Respect after +25: {state.Stats.Respect}");

            state.ApplyStatChange(new StatChange(StatType.Respect, 50));
            if (state.Stats.Respect != 100)
                Fail($"Respect should clamp to 100: {state.Stats.Respect}");

            state.Flags.SetFlag("phone_read");
            if (!state.Flags.HasFlag("phone_read"))
                Fail("Flag phone_read not set");

            state.RecordDecision();
            state.RecordDecision();
            if (state.DecisionsCount != 2)
                Fail($"DecisionsCount: {state.DecisionsCount}");

            state.Reset();
            if (state.Flags.HasFlag("phone_read") || state.DecisionsCount != 0)
                Fail("Reset did not clear flags/decisions");

            Debug.Log("[MaratGame] GameState smoke test passed.");
        }

        static void Fail(string message)
        {
            Debug.LogError("[MaratGame] GameState smoke test FAILED: " + message);
        }
    }
}
