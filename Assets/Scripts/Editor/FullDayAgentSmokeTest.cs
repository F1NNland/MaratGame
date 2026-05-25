using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using MaratGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    static class FullDayAgentSmokeTest
    {
        const string DatabasePath = "Assets/Data/Story/StoryDatabase_Test.asset";

        [MenuItem("MaratGame/Agent/Run Full Day Smoke (2 routes)")]
        public static void Run()
        {
            StoryBirthdayEndSetup.CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame][AgentSmoke] StoryDatabase_Test missing.");
                return;
            }

            RunRoute(
                "route_a_big_congratulation",
                database,
                morningNodeId: NavigationBar.CanteenEntryNodeId,
                morningChoiceIndex: 0,
                planerkaChoiceIndex: 0,
                krrbSeatChoiceIndex: 0,
                expectedFinalNodeId: Chapter4KrrbNodes.BigCongratulationNodeId,
                expectBigCongratulationUnlocked: true);

            RunRoute(
                "route_b_positive_fallback",
                database,
                morningNodeId: NavigationBar.ElevatorEntryNodeId,
                morningChoiceIndex: 2,
                planerkaChoiceIndex: 2,
                krrbSeatChoiceIndex: 2,
                expectedFinalNodeId: Chapter4KrrbNodes.EveningGoodEndingNodeId,
                expectBigCongratulationUnlocked: false);

            Debug.Log("[MaratGame][AgentSmoke] Full-day smoke passed: 2 routes reached valid positive finals.");
        }

        static void RunRoute(
            string routeId,
            StoryDatabase database,
            string morningNodeId,
            int morningChoiceIndex,
            int planerkaChoiceIndex,
            int krrbSeatChoiceIndex,
            string expectedFinalNodeId,
            bool expectBigCongratulationUnlocked)
        {
            var state = GameState.Instance;
            state.Reset();
            state.Flags.SetFlag(PhoneUI.PhoneReadFlag); // Phone overlay closes into hall_hub in runtime flow.

            var engine = new StoryEngine(state, database);
            engine.LoadNode(PhoneUI.HallHubNodeId);
            AssertNode(engine, PhoneUI.HallHubNodeId, routeId, "hall hub entry");
            AssertDayBlock(state, DayBlock.Morning, routeId, "morning hub");

            // Morning branch #1 from hub.
            engine.LoadNode(morningNodeId);
            SelectAvailable(engine, morningChoiceIndex, routeId, "morning branch entry choice");
            SelectFirstAvailable(engine, routeId, "morning branch follow-up");
            SelectFirstAvailable(engine, routeId, "morning branch return to hub");
            AssertNode(engine, NavigationBar.HubNodeId, routeId, "after morning branch");

            // Morning branch #2 to guarantee enough decisions for birthday trigger.
            engine.LoadNode(NavigationBar.ElevatorEntryNodeId);
            SelectAvailable(engine, 1, routeId, "second morning branch choice");
            SelectFirstAvailable(engine, routeId, "second morning branch follow-up");
            SelectFirstAvailable(engine, routeId, "second morning branch return to hub");
            AssertNode(engine, NavigationBar.HubNodeId, routeId, "before birthday trigger");

            if (!MorningBranchProgress.IsReadyForHubBirthdayInspect(state))
                Fail(routeId, "Expected two morning areas explored before birthday trigger.");

            // Runtime hub inspect action sets flag and loads birthday_scene.
            state.Flags.SetFlag(BirthdayEndFlags.BirthdaySeen);
            engine.LoadNode(BirthdayEndNodes.BirthdaySceneNodeId);
            SelectFirstAvailable(engine, routeId, "birthday -> bridge");
            AssertNode(engine, BirthdayEndNodes.PrePlanerkaBridgeNodeId, routeId, "pre-planerka bridge");
            AssertDayBlock(state, DayBlock.BeforeMeeting, routeId, "pre-planerka");

            SelectFirstAvailable(engine, routeId, "bridge -> chapter2 intro");
            AssertNode(engine, BirthdayEndNodes.Chapter2CabinetIntroNodeId, routeId, "chapter2 intro");
            AssertDayBlock(state, DayBlock.BeforeMeeting, routeId, "chapter2 intro");

            SelectFirstAvailable(engine, routeId, "chapter2 intro -> cabinet");
            AssertNode(engine, Chapter2CabinetNodes.EntryNodeId, routeId, "chapter2 cabinet entry");
            SelectAvailable(engine, 4, routeId, "chapter2 -> planerka");
            AssertNode(engine, Chapter3PlanerkaNodes.IntroNodeId, routeId, "planerka intro");
            AssertDayBlock(state, DayBlock.AfterMeeting, routeId, "planerka");

            SelectAvailable(engine, planerkaChoiceIndex, routeId, "planerka style");
            SelectFirstAvailable(engine, routeId, "planerka reaction");
            SelectFirstAvailable(engine, routeId, "planerka follow-up");
            AssertNode(engine, Chapter4KrrbNodes.IntroNodeId, routeId, "krrb intro");
            AssertDayBlock(state, DayBlock.KrrbUk, routeId, "krrb");

            SelectAvailable(engine, krrbSeatChoiceIndex, routeId, "krrb seat");
            SelectFirstAvailable(engine, routeId, "krrb reaction");
            SelectFirstAvailable(engine, routeId, "krrb follow-up");
            AssertNode(engine, Chapter4KrrbNodes.EveningIntroNodeId, routeId, "evening intro");
            AssertDayBlock(state, DayBlock.Evening, routeId, "evening");

            SelectFirstAvailable(engine, routeId, "evening intro -> evening bank");
            AssertNode(engine, Chapter4KrrbNodes.EveningBankEmptyNodeId, routeId, "evening bank empty");

            if (expectBigCongratulationUnlocked)
            {
                if (!IsChoiceAvailable(engine, 0))
                    Fail(routeId, "Expected big congratulation option to be available.");
            }
            else if (IsChoiceAvailable(engine, 0))
            {
                Fail(routeId, "Big congratulation option should be unavailable on fallback route.");
            }

            var endingChoice = IsChoiceAvailable(engine, 0) ? 0 : 1;
            SelectAvailable(engine, endingChoice, routeId, "final evening choice");
            AssertNode(engine, expectedFinalNodeId, routeId, "final node");
            AssertDayBlock(state, DayBlock.Final, routeId, "final day block");
        }

        static void SelectFirstAvailable(StoryEngine engine, string routeId, string step)
        {
            var availability = engine.GetChoiceAvailability();
            if (availability == null || availability.Length == 0)
                Fail(routeId, $"Soft-lock at step '{step}': no choices.");

            for (var i = 0; i < availability.Length; i++)
            {
                if (!availability[i].IsAvailable)
                    continue;

                engine.SelectChoice(availability[i].Index);
                return;
            }

            Fail(routeId, $"Soft-lock at step '{step}': all choices are unavailable.");
        }

        static void SelectAvailable(StoryEngine engine, int index, string routeId, string step)
        {
            var availability = engine.GetChoiceAvailability();
            if (availability == null || index < 0 || index >= availability.Length)
                Fail(routeId, $"Step '{step}': choice index {index} out of range.");

            if (!availability[index].IsAvailable)
            {
                var reason = string.IsNullOrWhiteSpace(availability[index].Reason)
                    ? "no reason provided"
                    : availability[index].Reason;
                Fail(routeId, $"Step '{step}': choice {index} unavailable ({reason}).");
            }

            engine.SelectChoice(index);
        }

        static bool IsChoiceAvailable(StoryEngine engine, int index)
        {
            var availability = engine.GetChoiceAvailability();
            return availability != null &&
                   index >= 0 &&
                   index < availability.Length &&
                   availability[index].IsAvailable;
        }

        static void AssertNode(StoryEngine engine, string expectedNodeId, string routeId, string step)
        {
            var actualNodeId = engine.CurrentNode?.id ?? "<null>";
            if (actualNodeId != expectedNodeId)
                Fail(routeId, $"Step '{step}': expected node '{expectedNodeId}', got '{actualNodeId}'.");
        }

        static void AssertDayBlock(GameState state, DayBlock expected, string routeId, string step)
        {
            if (state.CurrentDayBlock != expected)
                Fail(routeId, $"Step '{step}': expected day block {expected}, got {state.CurrentDayBlock}.");
        }

        static void Fail(string routeId, string message)
        {
            throw new System.InvalidOperationException($"[MaratGame][AgentSmoke][{routeId}] {message}");
        }
    }
}
