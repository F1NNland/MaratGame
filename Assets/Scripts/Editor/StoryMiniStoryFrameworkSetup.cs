using System.Collections.Generic;
using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// Post-MVP шаги 06-07: каркас и контент mini-story "Помочь сотруднику".
    /// </summary>
    static class StoryMiniStoryFrameworkSetup
    {
        const string DataFolder = "Assets/Data/Story";
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";
        const string EntryPath = DataFolder + "/Node_ToiletHelpEmployeeEntry.asset";
        const string ContentPath = DataFolder + "/Node_ToiletHelpEmployeeContent.asset";
        const string ExitPath = DataFolder + "/Node_ToiletHelpEmployeeExit.asset";

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 06 (Mini-story Framework)")]
        public static void SetupPostMvpStep06MiniStoryFramework()
        {
            CreateMiniStoryFrameworkContent();
            Debug.Log("[MaratGame] Post-MVP step 06: mini-story framework setup is ready.");
        }

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 07 (Mini-story Help Employee)")]
        public static void SetupPostMvpStep07MiniStoryHelpEmployee()
        {
            CreateMiniStoryFrameworkContent();
            Debug.Log("[MaratGame] Post-MVP step 07: mini-story 'Помочь сотруднику' is ready.");
        }

        [MenuItem("MaratGame/Story/Create Mini-story Framework Content")]
        public static void CreateMiniStoryFrameworkContent()
        {
            StoryBirthdayEndSetup.CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            EnsureMiniStoryNodes(database, out var entry, out var content, out var exit);

            EditorUtility.SetDirty(entry);
            EditorUtility.SetDirty(content);
            EditorUtility.SetDirty(exit);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void EnsureMiniStoryNodes(
            StoryDatabase database,
            out StoryNodeData entry,
            out StoryNodeData content,
            out StoryNodeData exit)
        {
            if (database == null)
                throw new System.ArgumentNullException(nameof(database));

            entry = StoryMvpSetup.LoadOrCreateNode(EntryPath);
            entry.id = Chapter2CabinetNodes.ToiletHelpEmployeeEntryNodeId;
            entry.locationId = "toilet";
            entry.timeDisplay = "08:25";
            entry.dayBlock = DayBlock.BeforeMeeting;
            entry.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            entry.speaker = string.Empty;
            entry.bodyText =
                "Из средней кабинки снова слышится тихое: «Извините... не подскажете, как быстро восстановить пропуск?». " +
                "Вы останавливаетесь, чтобы разобраться спокойно.";
            entry.portraitCharacterId = string.Empty;
            entry.mediaSlot = MediaSlotType.None;
            entry.mediaPath = string.Empty;
            entry.onEnterEffects = new List<StatChangeEntry>();
            entry.choices = new[]
            {
                new StoryChoice
                {
                    label = "Подойти и выслушать",
                    targetNodeId = Chapter2CabinetNodes.ToiletHelpEmployeeContentNodeId
                }
            };
            ConfigureMiniStoryNode(
                entry,
                MiniStoryFramework.HelpEmployeeMiniStoryId,
                MiniStoryRole.Entry,
                Chapter2CabinetNodes.ToiletHelpEmployeeEntryNodeId,
                Chapter2CabinetNodes.ToiletHelpEmployeeExitNodeId,
                Chapter2CabinetNodes.ToiletHelpEmployeeContentNodeId);

            content = StoryMvpSetup.LoadOrCreateNode(ContentPath);
            content.id = Chapter2CabinetNodes.ToiletHelpEmployeeContentNodeId;
            content.locationId = "toilet";
            content.timeDisplay = "08:26";
            content.dayBlock = DayBlock.BeforeMeeting;
            content.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            content.speaker = "Сотрудник";
            content.bodyText =
                "— Я первый день после перевода. Пропуск не читается, а через пять минут планёрка.\n" +
                "— Сейчас быстро решим. Главное — без суеты.\n\n" +
                "Выбираете, как помочь так, чтобы человек выдохнул и успел на встречу.";
            content.portraitCharacterId = string.Empty;
            content.mediaSlot = MediaSlotType.Gif;
            content.mediaPath = "Assets/Art/placeholder/help_employee.gif";
            content.onEnterEffects = new List<StatChangeEntry>();
            content.choices = new[]
            {
                new StoryChoice
                {
                    label = "Спокойно объяснить порядок и номер поддержки",
                    targetNodeId = Chapter2CabinetNodes.ToiletHelpEmployeeExitNodeId,
                    statChanges = new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 2 },
                        new StatChangeEntry { stat = StatType.Calm, delta = 1 }
                    },
                    flagsToSet = new[]
                    {
                        Chapter2MiniStoryFlags.HelpedEmployee,
                        Chapter2MiniStoryFlags.HandledDelicately
                    }
                },
                new StoryChoice
                {
                    label = "Сразу эскалировать через чат дежурной группы",
                    targetNodeId = Chapter2CabinetNodes.ToiletHelpEmployeeExitNodeId,
                    statChanges = new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 1 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = 1 }
                    },
                    flagsToSet = new[]
                    {
                        Chapter2MiniStoryFlags.HelpedEmployee
                    }
                }
            };
            ConfigureMiniStoryNode(
                content,
                MiniStoryFramework.HelpEmployeeMiniStoryId,
                MiniStoryRole.Content,
                Chapter2CabinetNodes.ToiletHelpEmployeeEntryNodeId,
                Chapter2CabinetNodes.ToiletHelpEmployeeExitNodeId,
                Chapter2CabinetNodes.ToiletHelpEmployeeContentNodeId);

            exit = StoryMvpSetup.LoadOrCreateNode(ExitPath);
            exit.id = Chapter2CabinetNodes.ToiletHelpEmployeeExitNodeId;
            exit.locationId = "toilet";
            exit.timeDisplay = "08:27";
            exit.dayBlock = DayBlock.BeforeMeeting;
            exit.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            exit.speaker = "Марат";
            exit.bodyText =
                "Сотрудник благодарит и заметно успокаивается. Вопрос с пропуском запущен в работу, " +
                "а у вас остаётся минута, чтобы привести мысли в порядок перед планёркой.";
            exit.portraitCharacterId = string.Empty;
            exit.mediaSlot = MediaSlotType.None;
            exit.mediaPath = string.Empty;
            exit.onEnterEffects = new List<StatChangeEntry>();
            exit.choices = new[]
            {
                new StoryChoice
                {
                    label = "Вернуться к зеркалу",
                    targetNodeId = Chapter2CabinetNodes.ToiletIntroNodeId
                }
            };
            ConfigureMiniStoryNode(
                exit,
                MiniStoryFramework.HelpEmployeeMiniStoryId,
                MiniStoryRole.Exit,
                Chapter2CabinetNodes.ToiletHelpEmployeeEntryNodeId,
                Chapter2CabinetNodes.ToiletHelpEmployeeExitNodeId,
                Chapter2CabinetNodes.ToiletHelpEmployeeContentNodeId);

            AppendUniqueNode(database, entry);
            AppendUniqueNode(database, content);
            AppendUniqueNode(database, exit);
        }

        [MenuItem("MaratGame/Story/Run Mini-story Lifecycle Smoke Test")]
        public static void RunMiniStoryLifecycleSmokeTest()
        {
            CreateMiniStoryFrameworkContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            var state = GameState.Instance;
            state.Reset();
            RunMiniStoryPathSmoke(
                database,
                contentChoiceIndex: 0,
                expectDelicateFlag: true,
                expectedRespectDelta: 2,
                expectedCalmDelta: 1,
                expectedChaosDelta: 0);

            RunMiniStoryPathSmoke(
                database,
                contentChoiceIndex: 1,
                expectDelicateFlag: false,
                expectedRespectDelta: 1,
                expectedCalmDelta: 0,
                expectedChaosDelta: 1);

            Debug.Log("[MaratGame] Mini-story lifecycle smoke test passed (2 outcomes).");
        }

        static void ConfigureMiniStoryNode(
            StoryNodeData node,
            string miniStoryId,
            MiniStoryRole role,
            string entryNodeId,
            string exitNodeId,
            params string[] contentNodeIds)
        {
            node.isMiniStory = true;
            node.miniStoryId = miniStoryId;
            node.miniStoryRole = role;
            node.miniStoryEntryNodeId = entryNodeId;
            node.miniStoryExitNodeId = exitNodeId;
            node.miniStoryContentNodeIds = contentNodeIds ?? System.Array.Empty<string>();
        }

        static void AppendUniqueNode(StoryDatabase database, StoryNodeData node)
        {
            foreach (var entry in database.nodes)
            {
                if (entry != null && entry.id == node.id)
                    return;
            }

            database.nodes.Add(node);
        }

        static void RunMiniStoryPathSmoke(
            StoryDatabase database,
            int contentChoiceIndex,
            bool expectDelicateFlag,
            int expectedRespectDelta,
            int expectedCalmDelta,
            int expectedChaosDelta)
        {
            var state = GameState.Instance;
            state.Reset();
            var engine = new StoryEngine(state, database);

            var startRespect = state.Stats.Respect;
            var startCalm = state.Stats.Calm;
            var startChaos = state.Stats.Chaos;

            engine.LoadNode(Chapter2CabinetNodes.ToiletHelpEmployeeEntryNodeId);
            if (engine.CurrentNode == null || engine.CurrentNode.id != Chapter2CabinetNodes.ToiletHelpEmployeeEntryNodeId)
                Debug.LogError("[MaratGame] Mini-story entry node not loaded.");

            engine.SelectChoice(0);
            if (engine.CurrentNode == null || engine.CurrentNode.id != Chapter2CabinetNodes.ToiletHelpEmployeeContentNodeId)
                Debug.LogError("[MaratGame] Mini-story should move to content node.");

            engine.SelectChoice(contentChoiceIndex);
            if (engine.CurrentNode == null || engine.CurrentNode.id != Chapter2CabinetNodes.ToiletHelpEmployeeExitNodeId)
                Debug.LogError("[MaratGame] Mini-story should move to exit node after outcome choice.");

            var enteredFlag = MiniStoryFramework.AnalyticsEntryPrefix + MiniStoryFramework.HelpEmployeeMiniStoryId;
            var exitedFlag = MiniStoryFramework.AnalyticsExitPrefix + MiniStoryFramework.HelpEmployeeMiniStoryId;
            if (!state.Flags.HasFlag(enteredFlag))
                Debug.LogError("[MaratGame] Mini-story entry analytics flag is missing.");
            if (!state.Flags.HasFlag(exitedFlag))
                Debug.LogError("[MaratGame] Mini-story exit analytics flag is missing.");
            if (!state.Flags.HasFlag(Chapter2MiniStoryFlags.HelpedEmployee))
                Debug.LogError("[MaratGame] Mini-story should set helped_employee flag.");
            if (expectDelicateFlag && !state.Flags.HasFlag(Chapter2MiniStoryFlags.HandledDelicately))
                Debug.LogError("[MaratGame] Delicate mini-story path should set handled_delicately flag.");
            if (!expectDelicateFlag && state.Flags.HasFlag(Chapter2MiniStoryFlags.HandledDelicately))
                Debug.LogError("[MaratGame] Escalation mini-story path must not set handled_delicately flag.");

            var respectDelta = state.Stats.Respect - startRespect;
            var calmDelta = state.Stats.Calm - startCalm;
            var chaosDelta = state.Stats.Chaos - startChaos;
            if (respectDelta != expectedRespectDelta || calmDelta != expectedCalmDelta || chaosDelta != expectedChaosDelta)
            {
                Debug.LogError("[MaratGame] Mini-story stats mismatch for choice " + contentChoiceIndex +
                               $": expected ({expectedRespectDelta},{expectedCalmDelta},{expectedChaosDelta}), " +
                               $"got ({respectDelta},{calmDelta},{chaosDelta}).");
            }

            engine.SelectChoice(0);
            if (engine.CurrentNode == null || engine.CurrentNode.id != Chapter2CabinetNodes.ToiletIntroNodeId)
                Debug.LogError("[MaratGame] Mini-story exit should return to toilet_intro.");
        }
    }
}
