using System.Collections.Generic;
using MaratGame.Core;
using MaratGame.Data;
using MaratGame.Narrative;
using MaratGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>
    /// Полный день: birthday_scene → глава 2…4 → вечерний финал.
    /// </summary>
    static class StoryBirthdayEndSetup
    {
        const string DataFolder = "Assets/Data/Story";
        const string NodeBirthdayPath = DataFolder + "/Node_BirthdayScene.asset";
        const string NodePrePlanerkaBridgePath = DataFolder + "/Node_PrePlanerkaBridge.asset";
        const string LegacyEndingScreenNodeId = "ending_screen";
        const string NodeChapter2CabinetIntroPath = DataFolder + "/Node_Chapter2CabinetIntro.asset";
        const string NodeChapter2CabinetEntryPath = DataFolder + "/Node_Chapter2CabinetEntry.asset";
        const string NodeChapter2CabinetMailPath = DataFolder + "/Node_Chapter2CabinetMail.asset";
        const string NodeChapter2CabinetAlevtinaPath = DataFolder + "/Node_Chapter2CabinetAlevtina.asset";
        const string NodeChapter2ToiletIntroPath = DataFolder + "/Node_ToiletIntro.asset";
        const string NodeChapter2ToiletWashFacePath = DataFolder + "/Node_ToiletWashFace.asset";
        const string NodeChapter2ToiletPhoneMessagesPath = DataFolder + "/Node_ToiletPhoneMessages.asset";
        const string NodeChapter2ToiletSilenceMonologuePath = DataFolder + "/Node_ToiletSilenceMonologue.asset";
        const string NodeChapter2ToiletMiddleStallPath = DataFolder + "/Node_ToiletMiddleStall.asset";
        const string NodeChapter2Meeting3544Path = DataFolder + "/Node_Meeting3544Intro.asset";
        const string NodeChapter3PlanerkaIntroPath = DataFolder + "/Node_Chapter3PlanerkaIntro.asset";
        const string NodeChapter3PlanerkaBusinessReactionPath = DataFolder + "/Node_Chapter3PlanerkaBusinessReaction.asset";
        const string NodeChapter3PlanerkaBusinessFollowupPath = DataFolder + "/Node_Chapter3PlanerkaBusinessFollowup.asset";
        const string NodeChapter3PlanerkaHumorReactionPath = DataFolder + "/Node_Chapter3PlanerkaHumorReaction.asset";
        const string NodeChapter3PlanerkaHumorFollowupPath = DataFolder + "/Node_Chapter3PlanerkaHumorFollowup.asset";
        const string NodeChapter3PlanerkaObserverReactionPath = DataFolder + "/Node_Chapter3PlanerkaObserverReaction.asset";
        const string NodeChapter3PlanerkaObserverFollowupPath = DataFolder + "/Node_Chapter3PlanerkaObserverFollowup.asset";
        const string NodeChapter4KrrbIntroPath = DataFolder + "/Node_KrrbIntro.asset";
        const string NodeChapter4KrrbSeatAlevtinaReactionPath = DataFolder + "/Node_KrrbSeatAlevtinaReaction.asset";
        const string NodeChapter4KrrbSeatAlevtinaFollowupPath = DataFolder + "/Node_KrrbSeatAlevtinaFollowup.asset";
        const string NodeChapter4KrrbSeatKozlikhinReactionPath = DataFolder + "/Node_KrrbSeatKozlikhinReaction.asset";
        const string NodeChapter4KrrbSeatKozlikhinFollowupPath = DataFolder + "/Node_KrrbSeatKozlikhinFollowup.asset";
        const string NodeChapter4KrrbSeatBackReactionPath = DataFolder + "/Node_KrrbSeatBackReaction.asset";
        const string NodeChapter4KrrbSeatBackFollowupPath = DataFolder + "/Node_KrrbSeatBackFollowup.asset";
        const string NodeChapter4EveningIntroPath = DataFolder + "/Node_EveningIntro.asset";
        const string NodeChapter4EveningBankEmptyPath = DataFolder + "/Node_EveningBankEmpty.asset";
        const string NodeChapter4BigCongratulationPath = DataFolder + "/Node_BigCongratulation.asset";
        const string NodeChapter4EveningGoodEndingPath = DataFolder + "/Node_EveningGoodEnding.asset";
        const string DatabasePath = DataFolder + "/StoryDatabase_Test.asset";

        [MenuItem("MaratGame/Story/Setup Step 11 (Birthday & End)")]
        public static void SetupStep11()
        {
            CreateBirthdayEndContent();
            GameUiBirthdayEndSetup.EnsureBirthdayEndUiOnGameScene();
            BuildSetup.ConfigureWindowsBuild();
            Debug.Log("[MaratGame] Step 11: birthday → ending. Build: Builds/Windows/MaratGame.exe");
        }

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 01 (Full Day Bridge)")]
        public static void SetupPostMvpStep01FullDayBridge()
        {
            CreateBirthdayEndContent();
            GameUiBirthdayEndSetup.EnsureBirthdayEndUiOnGameScene();
            Debug.Log("[MaratGame] Post-MVP step 01: birthday_scene → pre_planerka_bridge → chapter2_cabinet_intro.");
        }

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 04 (Chapter 2 Cabinet)")]
        public static void SetupPostMvpStep04Chapter2Cabinet()
        {
            CreateBirthdayEndContent();
            GameUiBirthdayEndSetup.EnsureBirthdayEndUiOnGameScene();
            Debug.Log("[MaratGame] Post-MVP step 04: chapter2 cabinet with 4 playable choices is ready.");
        }

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 05 (Chapter 2 Toilet)")]
        public static void SetupPostMvpStep05Chapter2Toilet()
        {
            CreateBirthdayEndContent();
            GameUiBirthdayEndSetup.EnsureBirthdayEndUiOnGameScene();
            Debug.Log("[MaratGame] Post-MVP step 05: toilet location has 4 actions and returns to chapter2_cabinet_entry.");
        }

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 08 (Chapter 3 Planerka)")]
        public static void SetupPostMvpStep08Chapter3Planerka()
        {
            CreateBirthdayEndContent();
            GameUiBirthdayEndSetup.EnsureBirthdayEndUiOnGameScene();
            Debug.Log("[MaratGame] Post-MVP step 08: planerka has 3 styles and transition to krrb_intro.");
        }

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 09 (Chapter 4 KRRB/UK)")]
        public static void SetupPostMvpStep09Chapter4Krrb()
        {
            CreateBirthdayEndContent();
            GameUiBirthdayEndSetup.EnsureBirthdayEndUiOnGameScene();
            Debug.Log("[MaratGame] Post-MVP step 09: krrb/uk has 3 seat branches and transition to evening_intro.");
        }

        [MenuItem("MaratGame/Story/Setup Post-MVP Step 10 (Evening & Big Congratulation)")]
        public static void SetupPostMvpStep10EveningBigCongratulation()
        {
            CreateBirthdayEndContent();
            GameUiBirthdayEndSetup.EnsureBirthdayEndUiOnGameScene();
            Debug.Log("[MaratGame] Post-MVP step 10: evening block has conditional big_congratulation and positive fallback ending.");
        }

        [MenuItem("MaratGame/Story/Create Birthday & End Content")]
        public static void CreateBirthdayEndContent()
        {
            StoryBranchesMorningSetup.CreateBranchesMorningContent();

            var birthday = StoryMvpSetup.LoadOrCreateNode(NodeBirthdayPath);
            birthday.id = BirthdayEndNodes.BirthdaySceneNodeId;
            birthday.locationId = "hall";
            birthday.timeDisplay = "07:58";
            birthday.chapterLabel = "ГЛАВА 1 · УТРО";
            birthday.speaker = string.Empty;
            birthday.bodyText =
                "С днём рождения, Марат! Коллеги машут из open space — рабочий день только начинается.";
            birthday.portraitCharacterId = string.Empty;
            birthday.onEnterEffects = new List<StatChangeEntry>();
            birthday.choices = new[]
            {
                new StoryChoice
                {
                    label = "Далее",
                    targetNodeId = BirthdayEndNodes.PrePlanerkaBridgeNodeId
                }
            };

            var bridge = StoryMvpSetup.LoadOrCreateNode(NodePrePlanerkaBridgePath);
            bridge.id = BirthdayEndNodes.PrePlanerkaBridgeNodeId;
            bridge.locationId = "hall";
            bridge.timeDisplay = "08:20";
            bridge.dayBlock = DayBlock.BeforeMeeting;
            bridge.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            bridge.speaker = string.Empty;
            bridge.bodyText = "Утренние поздравления позади. До планёрки есть время собраться и зайти в кабинет.";
            bridge.portraitCharacterId = string.Empty;
            bridge.onEnterEffects = new List<StatChangeEntry>();
            bridge.choices = new[]
            {
                new StoryChoice
                {
                    label = "К рабочему месту",
                    targetNodeId = BirthdayEndNodes.Chapter2CabinetIntroNodeId
                }
            };

            var chapter2Entry = StoryMvpSetup.LoadOrCreateNode(NodeChapter2CabinetIntroPath);
            chapter2Entry.id = BirthdayEndNodes.Chapter2CabinetIntroNodeId;
            chapter2Entry.locationId = "cabinet";
            chapter2Entry.timeDisplay = "08:20";
            chapter2Entry.dayBlock = DayBlock.BeforeMeeting;
            chapter2Entry.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            chapter2Entry.speaker = "Марат";
            chapter2Entry.bodyText = "Ещё немного времени до планёрки. Можно осмотреть кабинет и собраться с мыслями.";
            chapter2Entry.portraitCharacterId = string.Empty;
            chapter2Entry.onEnterEffects = new List<StatChangeEntry>();
            chapter2Entry.choices = new[]
            {
                MakeChoice("Осмотреть кабинет", Chapter2CabinetNodes.EntryNodeId)
            };

            var chapter2CabinetEntry = StoryMvpSetup.LoadOrCreateNode(NodeChapter2CabinetEntryPath);
            chapter2CabinetEntry.id = Chapter2CabinetNodes.EntryNodeId;
            chapter2CabinetEntry.locationId = "cabinet";
            chapter2CabinetEntry.timeDisplay = "08:21";
            chapter2CabinetEntry.dayBlock = DayBlock.BeforeMeeting;
            chapter2CabinetEntry.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            chapter2CabinetEntry.speaker = string.Empty;
            chapter2CabinetEntry.bodyText =
                "На столе кружка, стопка документов и 43 непрочитанных письма. " +
                "До планёрки остаётся несколько минут — на что пустить внимание?";
            chapter2CabinetEntry.portraitCharacterId = string.Empty;
            chapter2CabinetEntry.onEnterEffects = new List<StatChangeEntry>();
            chapter2CabinetEntry.choices = new[]
            {
                MakeChoice(
                    "Разобрать почту",
                    Chapter2CabinetNodes.MailNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 5 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = -3 }
                    },
                    new[] { Chapter2CabinetFlags.MailChecked }),
                MakeChoice(
                    "К Алевтине",
                    Chapter2CabinetNodes.AlevtinaNodeId,
                    null,
                    new[] { Chapter2CabinetFlags.AlevtinaBriefed, Chapter2CabinetFlags.KrrbClosedMeetingHint }),
                MakeChoice(
                    "5 минут в туалет",
                    Chapter2CabinetNodes.ToiletIntroNodeId,
                    null,
                    new[] { Chapter2CabinetFlags.ToiletBreakTaken }),
                MakeChoice(
                    "Встреча 35.44",
                    Chapter2CabinetNodes.Meeting3544NodeId,
                    new[] { new StatChangeEntry { stat = StatType.Calm, delta = -1 } },
                    new[] { Chapter2CabinetFlags.Meeting3544Visited }),
                MakeChoice("Пора на планёрку", Chapter3PlanerkaNodes.IntroNodeId)
            };

            var chapter2Mail = StoryMvpSetup.LoadOrCreateNode(NodeChapter2CabinetMailPath);
            chapter2Mail.id = Chapter2CabinetNodes.MailNodeId;
            chapter2Mail.locationId = "cabinet";
            chapter2Mail.timeDisplay = "08:23";
            chapter2Mail.dayBlock = DayBlock.BeforeMeeting;
            chapter2Mail.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            chapter2Mail.speaker = "Марат";
            chapter2Mail.bodyText =
                "43 письма — без сюрпризов: отчёты, согласования, пара срочных уточнений по повестке. " +
                "Рабочий ритм возвращается.";
            chapter2Mail.portraitCharacterId = string.Empty;
            chapter2Mail.onEnterEffects = new List<StatChangeEntry>();
            chapter2Mail.choices = new[]
            {
                MakeChoice("Вернуться к столу", Chapter2CabinetNodes.EntryNodeId)
            };

            var chapter2Alevtina = StoryMvpSetup.LoadOrCreateNode(NodeChapter2CabinetAlevtinaPath);
            chapter2Alevtina.id = Chapter2CabinetNodes.AlevtinaNodeId;
            chapter2Alevtina.locationId = "cabinet";
            chapter2Alevtina.timeDisplay = "08:22";
            chapter2Alevtina.dayBlock = DayBlock.BeforeMeeting;
            chapter2Alevtina.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            chapter2Alevtina.speaker = "Алевтина";
            chapter2Alevtina.bodyText =
                "— На 35.44 не задерживайся: после неё будет закрытая сверка по КРРБ. " +
                "Нужен короткий, но точный отчёт.";
            chapter2Alevtina.portraitCharacterId = CharacterIds.Alevtina;
            chapter2Alevtina.onEnterEffects = new List<StatChangeEntry>();
            chapter2Alevtina.choices = new[]
            {
                MakeChoice(
                    "Принял, вернуться к столу",
                    Chapter2CabinetNodes.EntryNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Respect, delta = 2 } })
            };

            var toiletIntro = StoryMvpSetup.LoadOrCreateNode(NodeChapter2ToiletIntroPath);
            toiletIntro.id = Chapter2CabinetNodes.ToiletIntroNodeId;
            toiletIntro.locationId = "toilet";
            toiletIntro.timeDisplay = "08:23";
            toiletIntro.dayBlock = DayBlock.BeforeMeeting;
            toiletIntro.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            toiletIntro.speaker = "Марат";
            toiletIntro.bodyText =
                "Тихо. Пахнет мылом и чуть слышно гудит вентиляция. До планёрки ещё есть пара минут.";
            toiletIntro.portraitCharacterId = string.Empty;
            toiletIntro.onEnterEffects = new List<StatChangeEntry>();
            toiletIntro.choices = new[]
            {
                MakeChoice("Умыться", Chapter2CabinetNodes.ToiletWashFaceNodeId),
                MakeChoice("Проверить телефон", Chapter2CabinetNodes.ToiletPhoneMessagesNodeId),
                MakeChoice("Постоять в тишине", Chapter2CabinetNodes.ToiletSilenceMonologueNodeId),
                MakeChoice("Зайти в среднюю кабинку", Chapter2CabinetNodes.ToiletMiddleStallNodeId),
                MakeChoice("Вернуться в кабинет", Chapter2CabinetNodes.EntryNodeId)
            };

            var toiletWashFace = StoryMvpSetup.LoadOrCreateNode(NodeChapter2ToiletWashFacePath);
            toiletWashFace.id = Chapter2CabinetNodes.ToiletWashFaceNodeId;
            toiletWashFace.locationId = "toilet";
            toiletWashFace.timeDisplay = "08:24";
            toiletWashFace.dayBlock = DayBlock.BeforeMeeting;
            toiletWashFace.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            toiletWashFace.speaker = "Марат";
            toiletWashFace.bodyText =
                "Холодная вода быстро собирает мысли. Дыхание выравнивается, в голове становится тише.";
            toiletWashFace.portraitCharacterId = string.Empty;
            toiletWashFace.mediaSlot = MediaSlotType.Gif;
            toiletWashFace.mediaPath = "Assets/Art/placeholder/toilet_wash_face.gif";
            toiletWashFace.onEnterEffects = new List<StatChangeEntry>();
            toiletWashFace.choices = new[]
            {
                MakeChoice(
                    "Вернуться в кабинет",
                    Chapter2CabinetNodes.EntryNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Calm, delta = 3 } },
                    new[] { Chapter2ToiletFlags.WashedFace })
            };

            var toiletPhoneMessages = StoryMvpSetup.LoadOrCreateNode(NodeChapter2ToiletPhoneMessagesPath);
            toiletPhoneMessages.id = Chapter2CabinetNodes.ToiletPhoneMessagesNodeId;
            toiletPhoneMessages.locationId = "toilet";
            toiletPhoneMessages.timeDisplay = "08:24";
            toiletPhoneMessages.dayBlock = DayBlock.BeforeMeeting;
            toiletPhoneMessages.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            toiletPhoneMessages.speaker = string.Empty;
            toiletPhoneMessages.bodyText = string.Empty;
            toiletPhoneMessages.uiMode = StoryUiMode.PhoneInbox;
            toiletPhoneMessages.portraitCharacterId = string.Empty;
            toiletPhoneMessages.onEnterEffects = new List<StatChangeEntry>();
            toiletPhoneMessages.choices = new[]
            {
                MakeChoice(
                    "Спрятать телефон и вернуться",
                    Chapter2CabinetNodes.EntryNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Respect, delta = 1 } },
                    new[] { Chapter2ToiletFlags.PhoneMessagesChecked })
            };

            var toiletSilenceMonologue = StoryMvpSetup.LoadOrCreateNode(NodeChapter2ToiletSilenceMonologuePath);
            toiletSilenceMonologue.id = Chapter2CabinetNodes.ToiletSilenceMonologueNodeId;
            toiletSilenceMonologue.locationId = "toilet";
            toiletSilenceMonologue.timeDisplay = "08:24";
            toiletSilenceMonologue.dayBlock = DayBlock.BeforeMeeting;
            toiletSilenceMonologue.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            toiletSilenceMonologue.speaker = "Марат";
            toiletSilenceMonologue.bodyText =
                "«Три вдоха. Сегодня мой день — но это ещё и обычный рабочий четверг. " +
                "Сделать по-человечески, без суеты — и этого достаточно».";
            toiletSilenceMonologue.portraitCharacterId = string.Empty;
            toiletSilenceMonologue.onEnterEffects = new List<StatChangeEntry>();
            toiletSilenceMonologue.choices = new[]
            {
                MakeChoice(
                    "Вернуться в кабинет",
                    Chapter2CabinetNodes.EntryNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Calm, delta = 2 } },
                    new[] { Chapter2ToiletFlags.SilenceMonologueHeard })
            };

            var toiletMiddleStall = StoryMvpSetup.LoadOrCreateNode(NodeChapter2ToiletMiddleStallPath);
            toiletMiddleStall.id = Chapter2CabinetNodes.ToiletMiddleStallNodeId;
            toiletMiddleStall.locationId = "toilet";
            toiletMiddleStall.timeDisplay = "08:24";
            toiletMiddleStall.dayBlock = DayBlock.BeforeMeeting;
            toiletMiddleStall.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            toiletMiddleStall.speaker = string.Empty;
            toiletMiddleStall.bodyText =
                "Из средней кабинки слышится сдержанное: «Извините... можно помощь?». " +
                "Похоже, у коллеги срочный вопрос с пропуском перед планёркой.";
            toiletMiddleStall.portraitCharacterId = string.Empty;
            toiletMiddleStall.onEnterEffects = new List<StatChangeEntry>();
            toiletMiddleStall.choices = new[]
            {
                MakeChoice(
                    "Подойти и разобраться",
                    Chapter2CabinetNodes.ToiletHelpEmployeeEntryNodeId,
                    null,
                    new[] { Chapter2ToiletFlags.MiddleStallEntered })
            };

            var meeting3544 = StoryMvpSetup.LoadOrCreateNode(NodeChapter2Meeting3544Path);
            meeting3544.id = Chapter2CabinetNodes.Meeting3544NodeId;
            meeting3544.locationId = "meeting_room";
            meeting3544.timeDisplay = "08:24";
            meeting3544.dayBlock = DayBlock.BeforeMeeting;
            meeting3544.chapterLabel = "ГЛАВА 2 · ДО ПЛАНЕРКИ";
            meeting3544.speaker = string.Empty;
            meeting3544.bodyText =
                "Открываете дверь 35.44 — и понимаете, что это не то совещание. " +
                "Мини-сюжет пока оставлен как заглушка, но вход в ветку уже работает.";
            meeting3544.portraitCharacterId = string.Empty;
            meeting3544.onEnterEffects = new List<StatChangeEntry>();
            meeting3544.choices = new[]
            {
                MakeChoice("Вернуться в кабинет", Chapter2CabinetNodes.EntryNodeId)
            };

            var planerkaIntro = StoryMvpSetup.LoadOrCreateNode(NodeChapter3PlanerkaIntroPath);
            planerkaIntro.id = Chapter3PlanerkaNodes.IntroNodeId;
            planerkaIntro.locationId = "planerka";
            planerkaIntro.timeDisplay = "08:30";
            planerkaIntro.dayBlock = DayBlock.AfterMeeting;
            planerkaIntro.chapterLabel = "ГЛАВА 3 · ПЛАНЕРКА";
            planerkaIntro.speaker = "Руководитель";
            planerkaIntro.bodyText =
                "Проектор снова не запускается, в переговорке повисает пауза. " +
                "Команда смотрит на вас — нужно быстро задать тон встречи.";
            planerkaIntro.portraitCharacterId = string.Empty;
            planerkaIntro.onEnterEffects = new List<StatChangeEntry>();
            planerkaIntro.choices = new[]
            {
                MakeChoice(
                    "Деловой стиль: собрать факты и план действий",
                    Chapter3PlanerkaNodes.BusinessReactionNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 3 }
                    },
                    new[] { Chapter3PlanerkaFlags.BusinessStyle }),
                MakeChoice(
                    "Снять напряжение шуткой и перевести в работу",
                    Chapter3PlanerkaNodes.HumorReactionNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Calm, delta = 3 },
                        new StatChangeEntry { stat = StatType.Respect, delta = 1 }
                    },
                    new[] { Chapter3PlanerkaFlags.HumorStyle }),
                MakeChoice(
                    "Наблюдать и фиксировать динамику команды",
                    Chapter3PlanerkaNodes.ObserverReactionNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Calm, delta = 1 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = -1 }
                    },
                    new[] { Chapter3PlanerkaFlags.ObserverStyle })
            };

            var planerkaBusinessReaction = StoryMvpSetup.LoadOrCreateNode(NodeChapter3PlanerkaBusinessReactionPath);
            planerkaBusinessReaction.id = Chapter3PlanerkaNodes.BusinessReactionNodeId;
            planerkaBusinessReaction.locationId = "planerka";
            planerkaBusinessReaction.timeDisplay = "08:31";
            planerkaBusinessReaction.dayBlock = DayBlock.AfterMeeting;
            planerkaBusinessReaction.chapterLabel = "ГЛАВА 3 · ПЛАНЕРКА";
            planerkaBusinessReaction.speaker = "Руководитель";
            planerkaBusinessReaction.bodyText =
                "— Окей, без техники не теряем темп: у нас три блока и десять минут. " +
                "Вы быстро распределяете вопросы по приоритету, и в комнате становится собраннее.";
            planerkaBusinessReaction.portraitCharacterId = string.Empty;
            planerkaBusinessReaction.onEnterEffects = new List<StatChangeEntry>();
            planerkaBusinessReaction.choices = new[]
            {
                MakeChoice("Поддержать деловой темп", Chapter3PlanerkaNodes.BusinessFollowupNodeId)
            };

            var planerkaBusinessFollowup = StoryMvpSetup.LoadOrCreateNode(NodeChapter3PlanerkaBusinessFollowupPath);
            planerkaBusinessFollowup.id = Chapter3PlanerkaNodes.BusinessFollowupNodeId;
            planerkaBusinessFollowup.locationId = "planerka";
            planerkaBusinessFollowup.timeDisplay = "08:33";
            planerkaBusinessFollowup.dayBlock = DayBlock.AfterMeeting;
            planerkaBusinessFollowup.chapterLabel = "ГЛАВА 3 · ПЛАНЕРКА";
            planerkaBusinessFollowup.speaker = "Алевтина";
            planerkaBusinessFollowup.bodyText =
                "— Спасибо, так намного понятнее. Дальше можно переходить к блоку КРРБ/УК. " +
                "Коллеги кивают: встреча вышла короткой и предметной.";
            planerkaBusinessFollowup.portraitCharacterId = CharacterIds.Alevtina;
            planerkaBusinessFollowup.onEnterEffects = new List<StatChangeEntry>();
            planerkaBusinessFollowup.choices = new[]
            {
                MakeChoice("Перейти к КРРБ/УК", Chapter4KrrbNodes.IntroNodeId)
            };

            var planerkaHumorReaction = StoryMvpSetup.LoadOrCreateNode(NodeChapter3PlanerkaHumorReactionPath);
            planerkaHumorReaction.id = Chapter3PlanerkaNodes.HumorReactionNodeId;
            planerkaHumorReaction.locationId = "planerka";
            planerkaHumorReaction.timeDisplay = "08:31";
            planerkaHumorReaction.dayBlock = DayBlock.AfterMeeting;
            planerkaHumorReaction.chapterLabel = "ГЛАВА 3 · ПЛАНЕРКА";
            planerkaHumorReaction.speaker = "Руководитель";
            planerkaHumorReaction.bodyText =
                "— Отлично, стало легче дышать. Давайте в таком же тоне и разберём повестку. " +
                "После короткой шутки команда улыбается, напряжение заметно падает.";
            planerkaHumorReaction.portraitCharacterId = string.Empty;
            planerkaHumorReaction.onEnterEffects = new List<StatChangeEntry>();
            planerkaHumorReaction.choices = new[]
            {
                MakeChoice("Перевести шутку в рабочий ритм", Chapter3PlanerkaNodes.HumorFollowupNodeId)
            };

            var planerkaHumorFollowup = StoryMvpSetup.LoadOrCreateNode(NodeChapter3PlanerkaHumorFollowupPath);
            planerkaHumorFollowup.id = Chapter3PlanerkaNodes.HumorFollowupNodeId;
            planerkaHumorFollowup.locationId = "planerka";
            planerkaHumorFollowup.timeDisplay = "08:33";
            planerkaHumorFollowup.dayBlock = DayBlock.AfterMeeting;
            planerkaHumorFollowup.chapterLabel = "ГЛАВА 3 · ПЛАНЕРКА";
            planerkaHumorFollowup.speaker = "Козлихин";
            planerkaHumorFollowup.bodyText =
                "— Отлично разрядили обстановку, теперь давайте по делу. " +
                "Несколько коротких реплик, и команда синхронизируется перед следующим блоком.";
            planerkaHumorFollowup.portraitCharacterId = string.Empty;
            planerkaHumorFollowup.onEnterEffects = new List<StatChangeEntry>();
            planerkaHumorFollowup.choices = new[]
            {
                MakeChoice("Перейти к КРРБ/УК", Chapter4KrrbNodes.IntroNodeId)
            };

            var planerkaObserverReaction = StoryMvpSetup.LoadOrCreateNode(NodeChapter3PlanerkaObserverReactionPath);
            planerkaObserverReaction.id = Chapter3PlanerkaNodes.ObserverReactionNodeId;
            planerkaObserverReaction.locationId = "planerka";
            planerkaObserverReaction.timeDisplay = "08:31";
            planerkaObserverReaction.dayBlock = DayBlock.AfterMeeting;
            planerkaObserverReaction.chapterLabel = "ГЛАВА 3 · ПЛАНЕРКА";
            planerkaObserverReaction.speaker = "Козлихин";
            planerkaObserverReaction.bodyText =
                "— Интересно, ты почти ничего не сказал, но всех внимательно слушал. " +
                "По реакции коллег становится видно, у кого сегодня перегруз, а кто держит ритм.";
            planerkaObserverReaction.portraitCharacterId = string.Empty;
            planerkaObserverReaction.onEnterEffects = new List<StatChangeEntry>();
            planerkaObserverReaction.choices = new[]
            {
                MakeChoice("Сделать выводы по команде", Chapter3PlanerkaNodes.ObserverFollowupNodeId)
            };

            var planerkaObserverFollowup = StoryMvpSetup.LoadOrCreateNode(NodeChapter3PlanerkaObserverFollowupPath);
            planerkaObserverFollowup.id = Chapter3PlanerkaNodes.ObserverFollowupNodeId;
            planerkaObserverFollowup.locationId = "planerka";
            planerkaObserverFollowup.timeDisplay = "08:33";
            planerkaObserverFollowup.dayBlock = DayBlock.AfterMeeting;
            planerkaObserverFollowup.chapterLabel = "ГЛАВА 3 · ПЛАНЕРКА";
            planerkaObserverFollowup.speaker = "Руководитель";
            planerkaObserverFollowup.bodyText =
                "— Спасибо за спокойный взгляд со стороны, это пригодится вечером, когда будем разбирать детали. " +
                "Вы запоминаете интонации и микрореакции коллег.";
            planerkaObserverFollowup.portraitCharacterId = string.Empty;
            planerkaObserverFollowup.onEnterEffects = new List<StatChangeEntry>();
            planerkaObserverFollowup.choices = new[]
            {
                MakeChoice("Перейти к КРРБ/УК", Chapter4KrrbNodes.IntroNodeId)
            };

            var krrbIntro = StoryMvpSetup.LoadOrCreateNode(NodeChapter4KrrbIntroPath);
            krrbIntro.id = Chapter4KrrbNodes.IntroNodeId;
            krrbIntro.locationId = "krrb";
            krrbIntro.timeDisplay = "08:36";
            krrbIntro.dayBlock = DayBlock.KrrbUk;
            krrbIntro.chapterLabel = "ГЛАВА 4 · КРРБ / УК";
            krrbIntro.speaker = string.Empty;
            krrbIntro.bodyText =
                "Блок КРРБ/УК начинается с неловкой паузы: свободных мест мало, а тон разговора ещё не задан. " +
                "Нужно выбрать, где сесть, и от этого зависит, как пойдёт оставшаяся часть дня.";
            krrbIntro.portraitCharacterId = string.Empty;
            krrbIntro.onEnterEffects = new List<StatChangeEntry>();
            krrbIntro.choices = new[]
            {
                MakeChoice(
                    "Сесть рядом с Алевтиной",
                    Chapter4KrrbNodes.SeatAlevtinaReactionNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Respect, delta = 1 } },
                    new[] { Chapter4KrrbFlags.SatNearAlevtina }),
                MakeChoice(
                    "Сесть рядом с Козлихиным",
                    Chapter4KrrbNodes.SeatKozlikhinReactionNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Chaos, delta = 1 } },
                    new[] { Chapter4KrrbFlags.SatNearKozlikhin }),
                MakeChoice(
                    "Выбрать место в конце зала",
                    Chapter4KrrbNodes.SeatBackReactionNodeId,
                    new[] { new StatChangeEntry { stat = StatType.Calm, delta = 2 } },
                    new[] { Chapter4KrrbFlags.SatBackRow })
            };

            var krrbSeatAlevtinaReaction = StoryMvpSetup.LoadOrCreateNode(NodeChapter4KrrbSeatAlevtinaReactionPath);
            krrbSeatAlevtinaReaction.id = Chapter4KrrbNodes.SeatAlevtinaReactionNodeId;
            krrbSeatAlevtinaReaction.locationId = "krrb";
            krrbSeatAlevtinaReaction.timeDisplay = "08:38";
            krrbSeatAlevtinaReaction.dayBlock = DayBlock.KrrbUk;
            krrbSeatAlevtinaReaction.chapterLabel = "ГЛАВА 4 · КРРБ / УК";
            krrbSeatAlevtinaReaction.speaker = "Алевтина";
            krrbSeatAlevtinaReaction.bodyText =
                "— Хорошо, что сел рядом. Сейчас пойдут длинные статусы, и важно не потерять детали. " +
                "В первом ряду каждое слово руководства звучит как рабочее поручение.";
            krrbSeatAlevtinaReaction.portraitCharacterId = CharacterIds.Alevtina;
            krrbSeatAlevtinaReaction.onEnterEffects = new List<StatChangeEntry>();
            krrbSeatAlevtinaReaction.choices = new[]
            {
                MakeChoice(
                    "Собрать тезисы в чёткий список и подхватить деловой тон",
                    Chapter4KrrbNodes.SeatAlevtinaFollowupNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 3 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = -2 }
                    },
                    new[] { Chapter4KrrbFlags.HighSocialPresence }),
                new StoryChoice
                {
                    label = "Сослаться на маршрут с Ноздриковым и закрыть вопрос по рискам",
                    targetNodeId = Chapter4KrrbNodes.SeatAlevtinaFollowupNodeId,
                    requiredFlags = new[] { MorningBranchFlags.MetNozdrikovRoute },
                    statChanges = new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 2 },
                        new StatChangeEntry { stat = StatType.Calm, delta = 1 }
                    },
                    flagsToSet = new[] { Chapter4KrrbFlags.HighSocialPresence },
                    unavailableReason = "Эта реплика опирается на утренний разговор в лифте."
                },
                new StoryChoice
                {
                    label = "Поддержать коллегу, как в мини-сюжете с сотрудником",
                    targetNodeId = Chapter4KrrbNodes.SeatAlevtinaFollowupNodeId,
                    requiredFlags = new[] { Chapter2MiniStoryFlags.HelpedEmployee },
                    statChanges = new[]
                    {
                        new StatChangeEntry { stat = StatType.Calm, delta = 2 },
                        new StatChangeEntry { stat = StatType.Respect, delta = 1 }
                    },
                    flagsToSet = new[] { Chapter4KrrbFlags.HighSocialPresence },
                    unavailableReason = "Сначала нужно пройти мини-сюжет помощи сотруднику."
                }
            };

            var krrbSeatAlevtinaFollowup = StoryMvpSetup.LoadOrCreateNode(NodeChapter4KrrbSeatAlevtinaFollowupPath);
            krrbSeatAlevtinaFollowup.id = Chapter4KrrbNodes.SeatAlevtinaFollowupNodeId;
            krrbSeatAlevtinaFollowup.locationId = "krrb";
            krrbSeatAlevtinaFollowup.timeDisplay = "08:43";
            krrbSeatAlevtinaFollowup.dayBlock = DayBlock.KrrbUk;
            krrbSeatAlevtinaFollowup.chapterLabel = "ГЛАВА 4 · КРРБ / УК";
            krrbSeatAlevtinaFollowup.speaker = "Руководитель";
            krrbSeatAlevtinaFollowup.bodyText =
                "Доклад тянется дольше обычного, но рядом с Алевтиной получается держать общий ритм. " +
                "Ваши уточнения слышат, и разговор постепенно становится структурнее.";
            krrbSeatAlevtinaFollowup.portraitCharacterId = string.Empty;
            krrbSeatAlevtinaFollowup.onEnterEffects = new List<StatChangeEntry>();
            krrbSeatAlevtinaFollowup.choices = new[]
            {
                MakeChoice("Закрыть блок и перейти к вечеру", Chapter4KrrbNodes.EveningIntroNodeId)
            };

            var krrbSeatKozlikhinReaction = StoryMvpSetup.LoadOrCreateNode(NodeChapter4KrrbSeatKozlikhinReactionPath);
            krrbSeatKozlikhinReaction.id = Chapter4KrrbNodes.SeatKozlikhinReactionNodeId;
            krrbSeatKozlikhinReaction.locationId = "krrb";
            krrbSeatKozlikhinReaction.timeDisplay = "08:38";
            krrbSeatKozlikhinReaction.dayBlock = DayBlock.KrrbUk;
            krrbSeatKozlikhinReaction.chapterLabel = "ГЛАВА 4 · КРРБ / УК";
            krrbSeatKozlikhinReaction.speaker = "Козлихин";
            krrbSeatKozlikhinReaction.bodyText =
                "— Садись ближе, сейчас будет много спорных мест по УК. " +
                "Козлихин шепчет комментарии между длинными репликами докладчика, и темп заметно ускоряется.";
            krrbSeatKozlikhinReaction.portraitCharacterId = CharacterIds.Kozlikhin;
            krrbSeatKozlikhinReaction.onEnterEffects = new List<StatChangeEntry>();
            krrbSeatKozlikhinReaction.choices = new[]
            {
                MakeChoice(
                    "Поддержать быстрый формат обсуждения",
                    Chapter4KrrbNodes.SeatKozlikhinFollowupNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Chaos, delta = 2 },
                        new StatChangeEntry { stat = StatType.Respect, delta = 1 }
                    },
                    new[] { Chapter4KrrbFlags.HighSocialPresence }),
                new StoryChoice
                {
                    label = "Сослаться на утренний диалог с Козлихиным и выровнять тон",
                    targetNodeId = Chapter4KrrbNodes.SeatKozlikhinFollowupNodeId,
                    requiredFlags = new[] { MorningBranchFlags.MetKozlikhin },
                    statChanges = new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 3 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = -1 }
                    },
                    flagsToSet = new[] { Chapter4KrrbFlags.HighSocialPresence },
                    unavailableReason = "Эта ветка открывается после утренней встречи с Козлихиным."
                },
                new StoryChoice
                {
                    label = "Сохранить наблюдательный стиль с планёрки и гасить лишние споры",
                    targetNodeId = Chapter4KrrbNodes.SeatKozlikhinFollowupNodeId,
                    requiredFlags = new[] { Chapter3PlanerkaFlags.ObserverStyle },
                    statChanges = new[]
                    {
                        new StatChangeEntry { stat = StatType.Calm, delta = 2 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = -2 }
                    }
                }
            };

            var krrbSeatKozlikhinFollowup = StoryMvpSetup.LoadOrCreateNode(NodeChapter4KrrbSeatKozlikhinFollowupPath);
            krrbSeatKozlikhinFollowup.id = Chapter4KrrbNodes.SeatKozlikhinFollowupNodeId;
            krrbSeatKozlikhinFollowup.locationId = "krrb";
            krrbSeatKozlikhinFollowup.timeDisplay = "08:44";
            krrbSeatKozlikhinFollowup.dayBlock = DayBlock.KrrbUk;
            krrbSeatKozlikhinFollowup.chapterLabel = "ГЛАВА 4 · КРРБ / УК";
            krrbSeatKozlikhinFollowup.speaker = "Марат";
            krrbSeatKozlikhinFollowup.bodyText =
                "Темп остаётся высоким, но рядом с Козлихиным проще ловить логику обсуждения. " +
                "К финалу блока вы уже понимаете, какие пункты уйдут в вечерний разбор.";
            krrbSeatKozlikhinFollowup.portraitCharacterId = string.Empty;
            krrbSeatKozlikhinFollowup.onEnterEffects = new List<StatChangeEntry>();
            krrbSeatKozlikhinFollowup.choices = new[]
            {
                MakeChoice("Собрать заметки и перейти к вечеру", Chapter4KrrbNodes.EveningIntroNodeId)
            };

            var krrbSeatBackReaction = StoryMvpSetup.LoadOrCreateNode(NodeChapter4KrrbSeatBackReactionPath);
            krrbSeatBackReaction.id = Chapter4KrrbNodes.SeatBackReactionNodeId;
            krrbSeatBackReaction.locationId = "krrb";
            krrbSeatBackReaction.timeDisplay = "08:38";
            krrbSeatBackReaction.dayBlock = DayBlock.KrrbUk;
            krrbSeatBackReaction.chapterLabel = "ГЛАВА 4 · КРРБ / УК";
            krrbSeatBackReaction.speaker = "Марат";
            krrbSeatBackReaction.bodyText =
                "С заднего ряда видно всех сразу: кто кивает формально, кто реально включён, кто устал. " +
                "Длинные реплики звучат тише, зато проще замечать общую динамику зала.";
            krrbSeatBackReaction.portraitCharacterId = string.Empty;
            krrbSeatBackReaction.onEnterEffects = new List<StatChangeEntry>();
            krrbSeatBackReaction.choices = new[]
            {
                MakeChoice(
                    "Сфокусироваться на общей картине и не влезать в споры",
                    Chapter4KrrbNodes.SeatBackFollowupNodeId,
                    new[]
                    {
                        new StatChangeEntry { stat = StatType.Calm, delta = 3 },
                        new StatChangeEntry { stat = StatType.Chaos, delta = -1 }
                    }),
                new StoryChoice
                {
                    label = "Подтянуть факты из утренней переписки в лифте",
                    targetNodeId = Chapter4KrrbNodes.SeatBackFollowupNodeId,
                    requiredFlags = new[] { MorningBranchFlags.ElevatorMessagesRead },
                    statChanges = new[]
                    {
                        new StatChangeEntry { stat = StatType.Respect, delta = 2 },
                        new StatChangeEntry { stat = StatType.Calm, delta = 1 }
                    }
                },
                new StoryChoice
                {
                    label = "Деликатно снять напряжение после помощи коллеге",
                    targetNodeId = Chapter4KrrbNodes.SeatBackFollowupNodeId,
                    requiredFlags = new[] { Chapter2MiniStoryFlags.HandledDelicately },
                    statChanges = new[]
                    {
                        new StatChangeEntry { stat = StatType.Calm, delta = 2 },
                        new StatChangeEntry { stat = StatType.Respect, delta = 1 }
                    },
                    flagsToSet = new[] { Chapter4KrrbFlags.HighSocialPresence }
                }
            };

            var krrbSeatBackFollowup = StoryMvpSetup.LoadOrCreateNode(NodeChapter4KrrbSeatBackFollowupPath);
            krrbSeatBackFollowup.id = Chapter4KrrbNodes.SeatBackFollowupNodeId;
            krrbSeatBackFollowup.locationId = "krrb";
            krrbSeatBackFollowup.timeDisplay = "08:44";
            krrbSeatBackFollowup.dayBlock = DayBlock.KrrbUk;
            krrbSeatBackFollowup.chapterLabel = "ГЛАВА 4 · КРРБ / УК";
            krrbSeatBackFollowup.speaker = "Руководитель";
            krrbSeatBackFollowup.bodyText =
                "Вы почти не перебиваете докладчиков, но к концу обсуждения у вас самый цельный конспект. " +
                "Команда постепенно выдыхает: острые темы перенесены в вечерний блок.";
            krrbSeatBackFollowup.portraitCharacterId = string.Empty;
            krrbSeatBackFollowup.onEnterEffects = new List<StatChangeEntry>();
            krrbSeatBackFollowup.choices = new[]
            {
                MakeChoice("Завершить КРРБ/УК и перейти в вечер", Chapter4KrrbNodes.EveningIntroNodeId)
            };

            var eveningIntro = StoryMvpSetup.LoadOrCreateNode(NodeChapter4EveningIntroPath);
            eveningIntro.id = Chapter4KrrbNodes.EveningIntroNodeId;
            eveningIntro.locationId = "hall";
            eveningIntro.timeDisplay = "18:10";
            eveningIntro.dayBlock = DayBlock.Evening;
            eveningIntro.chapterLabel = "ГЛАВА 4 · ВЕЧЕР";
            eveningIntro.speaker = "Марат";
            eveningIntro.bodyText =
                "КРРБ/УК позади. Вечер начинается с короткой паузы в пустеющем холле: " +
                "можно наконец выдохнуть и подвести итоги насыщенного дня.";
            eveningIntro.portraitCharacterId = string.Empty;
            eveningIntro.onEnterEffects = new List<StatChangeEntry>();
            eveningIntro.choices = new[]
            {
                MakeChoice("А что дальше?", Chapter4KrrbNodes.EveningBankEmptyNodeId)
            };

            var eveningBankEmpty = StoryMvpSetup.LoadOrCreateNode(NodeChapter4EveningBankEmptyPath);
            eveningBankEmpty.id = Chapter4KrrbNodes.EveningBankEmptyNodeId;
            eveningBankEmpty.locationId = "hall";
            eveningBankEmpty.timeDisplay = "18:27";
            eveningBankEmpty.dayBlock = DayBlock.Evening;
            eveningBankEmpty.chapterLabel = "ГЛАВА 4 · ВЕЧЕР";
            eveningBankEmpty.speaker = "Марат";
            eveningBankEmpty.bodyText =
                "В коридорах почти тихо: кто-то уже уехал, кто-то собирает чашки и ноутбуки. " +
                "Остаётся решить, как завершить этот день рождения в Банке.";
            eveningBankEmpty.portraitCharacterId = string.Empty;
            eveningBankEmpty.onEnterEffects = new List<StatChangeEntry>();
            eveningBankEmpty.choices = new[]
            {
                new StoryChoice
                {
                    label = "Зайти в open space — там вроде готовят что-то особенное",
                    targetNodeId = Chapter4KrrbNodes.BigCongratulationNodeId,
                    flagsToSet = new[] { Chapter4KrrbFlags.BigCongratulationUnlocked },
                    requiredFlags = new[] { Chapter4KrrbFlags.HighSocialPresence },
                    requiredStats = new[]
                    {
                        new StatRequirement { stat = StatType.Respect, useMin = true, minValue = 55 },
                        new StatRequirement { stat = StatType.Calm, useMin = true, minValue = 55 }
                    },
                    unavailableReason = "Большое поздравление откроется, если в течение дня вы много общались."
                },
                MakeChoice("Тихо закрыть ноутбук и завершить день", Chapter4KrrbNodes.EveningGoodEndingNodeId)
            };

            var bigCongratulation = StoryMvpSetup.LoadOrCreateNode(NodeChapter4BigCongratulationPath);
            bigCongratulation.id = Chapter4KrrbNodes.BigCongratulationNodeId;
            bigCongratulation.locationId = "hall";
            bigCongratulation.timeDisplay = "18:40";
            bigCongratulation.dayBlock = DayBlock.Final;
            bigCongratulation.chapterLabel = "ФИНАЛ · БОЛЬШОЕ ПОЗДРАВЛЕНИЕ";
            bigCongratulation.speaker = string.Empty;
            bigCongratulation.bodyText =
                "Свет в open space приглушают, и коллеги почти синхронно поворачиваются к вам: " +
                "«С днём рождения, Марат!» Торт, аплодисменты, короткие тёплые тосты — " +
                "день заканчивается на высокой, но очень человеческой ноте.\n\n" +
                "«Наверное, ради этого всё и работает.»";
            bigCongratulation.portraitCharacterId = string.Empty;
            bigCongratulation.onEnterEffects = new List<StatChangeEntry>();
            bigCongratulation.choices = System.Array.Empty<StoryChoice>();
            bigCongratulation.requiredFlags = System.Array.Empty<string>();
            bigCongratulation.requiredStats = System.Array.Empty<StatRequirement>();

            var eveningGoodEnding = StoryMvpSetup.LoadOrCreateNode(NodeChapter4EveningGoodEndingPath);
            eveningGoodEnding.id = Chapter4KrrbNodes.EveningGoodEndingNodeId;
            eveningGoodEnding.locationId = "hall";
            eveningGoodEnding.timeDisplay = "18:35";
            eveningGoodEnding.dayBlock = DayBlock.Final;
            eveningGoodEnding.chapterLabel = "ФИНАЛ · ТЁПЛЫЙ ВЕЧЕР";
            eveningGoodEnding.speaker = "Марат";
            eveningGoodEnding.bodyText =
                "Вы неспешно собираете вещи, киваете коллегам и ловите спокойную мысль: " +
                "день вышел насыщенным и правильным по-своему. " +
                "Даже без большого финального сюрприза в этом вечере хватает тепла.";
            eveningGoodEnding.portraitCharacterId = string.Empty;
            eveningGoodEnding.onEnterEffects = new List<StatChangeEntry>();
            eveningGoodEnding.choices = System.Array.Empty<StoryChoice>();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            RemoveNodeById(database, LegacyEndingScreenNodeId);

            if (!ContainsNode(database, birthday))
                database.nodes.Add(birthday);
            if (!ContainsNode(database, bridge))
                database.nodes.Add(bridge);
            if (!ContainsNode(database, chapter2Entry))
                database.nodes.Add(chapter2Entry);
            if (!ContainsNode(database, chapter2CabinetEntry))
                database.nodes.Add(chapter2CabinetEntry);
            if (!ContainsNode(database, chapter2Mail))
                database.nodes.Add(chapter2Mail);
            if (!ContainsNode(database, chapter2Alevtina))
                database.nodes.Add(chapter2Alevtina);
            if (!ContainsNode(database, toiletIntro))
                database.nodes.Add(toiletIntro);
            if (!ContainsNode(database, toiletWashFace))
                database.nodes.Add(toiletWashFace);
            if (!ContainsNode(database, toiletPhoneMessages))
                database.nodes.Add(toiletPhoneMessages);
            if (!ContainsNode(database, toiletSilenceMonologue))
                database.nodes.Add(toiletSilenceMonologue);
            if (!ContainsNode(database, toiletMiddleStall))
                database.nodes.Add(toiletMiddleStall);
            if (!ContainsNode(database, meeting3544))
                database.nodes.Add(meeting3544);
            if (!ContainsNode(database, planerkaIntro))
                database.nodes.Add(planerkaIntro);
            if (!ContainsNode(database, planerkaBusinessReaction))
                database.nodes.Add(planerkaBusinessReaction);
            if (!ContainsNode(database, planerkaBusinessFollowup))
                database.nodes.Add(planerkaBusinessFollowup);
            if (!ContainsNode(database, planerkaHumorReaction))
                database.nodes.Add(planerkaHumorReaction);
            if (!ContainsNode(database, planerkaHumorFollowup))
                database.nodes.Add(planerkaHumorFollowup);
            if (!ContainsNode(database, planerkaObserverReaction))
                database.nodes.Add(planerkaObserverReaction);
            if (!ContainsNode(database, planerkaObserverFollowup))
                database.nodes.Add(planerkaObserverFollowup);
            if (!ContainsNode(database, krrbIntro))
                database.nodes.Add(krrbIntro);
            if (!ContainsNode(database, krrbSeatAlevtinaReaction))
                database.nodes.Add(krrbSeatAlevtinaReaction);
            if (!ContainsNode(database, krrbSeatAlevtinaFollowup))
                database.nodes.Add(krrbSeatAlevtinaFollowup);
            if (!ContainsNode(database, krrbSeatKozlikhinReaction))
                database.nodes.Add(krrbSeatKozlikhinReaction);
            if (!ContainsNode(database, krrbSeatKozlikhinFollowup))
                database.nodes.Add(krrbSeatKozlikhinFollowup);
            if (!ContainsNode(database, krrbSeatBackReaction))
                database.nodes.Add(krrbSeatBackReaction);
            if (!ContainsNode(database, krrbSeatBackFollowup))
                database.nodes.Add(krrbSeatBackFollowup);
            if (!ContainsNode(database, eveningIntro))
                database.nodes.Add(eveningIntro);
            if (!ContainsNode(database, eveningBankEmpty))
                database.nodes.Add(eveningBankEmpty);
            if (!ContainsNode(database, bigCongratulation))
                database.nodes.Add(bigCongratulation);
            if (!ContainsNode(database, eveningGoodEnding))
                database.nodes.Add(eveningGoodEnding);
            StoryMiniStoryFrameworkSetup.EnsureMiniStoryNodes(database, out var miniEntry, out var miniContent, out var miniExit);

            EditorUtility.SetDirty(birthday);
            EditorUtility.SetDirty(bridge);
            EditorUtility.SetDirty(chapter2Entry);
            EditorUtility.SetDirty(chapter2CabinetEntry);
            EditorUtility.SetDirty(chapter2Mail);
            EditorUtility.SetDirty(chapter2Alevtina);
            EditorUtility.SetDirty(toiletIntro);
            EditorUtility.SetDirty(toiletWashFace);
            EditorUtility.SetDirty(toiletPhoneMessages);
            EditorUtility.SetDirty(toiletSilenceMonologue);
            EditorUtility.SetDirty(toiletMiddleStall);
            EditorUtility.SetDirty(meeting3544);
            EditorUtility.SetDirty(planerkaIntro);
            EditorUtility.SetDirty(planerkaBusinessReaction);
            EditorUtility.SetDirty(planerkaBusinessFollowup);
            EditorUtility.SetDirty(planerkaHumorReaction);
            EditorUtility.SetDirty(planerkaHumorFollowup);
            EditorUtility.SetDirty(planerkaObserverReaction);
            EditorUtility.SetDirty(planerkaObserverFollowup);
            EditorUtility.SetDirty(krrbIntro);
            EditorUtility.SetDirty(krrbSeatAlevtinaReaction);
            EditorUtility.SetDirty(krrbSeatAlevtinaFollowup);
            EditorUtility.SetDirty(krrbSeatKozlikhinReaction);
            EditorUtility.SetDirty(krrbSeatKozlikhinFollowup);
            EditorUtility.SetDirty(krrbSeatBackReaction);
            EditorUtility.SetDirty(krrbSeatBackFollowup);
            EditorUtility.SetDirty(eveningIntro);
            EditorUtility.SetDirty(eveningBankEmpty);
            EditorUtility.SetDirty(bigCongratulation);
            EditorUtility.SetDirty(eveningGoodEnding);
            EditorUtility.SetDirty(miniEntry);
            EditorUtility.SetDirty(miniContent);
            EditorUtility.SetDirty(miniExit);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaratGame] Full day: hub ● → " + BirthdayEndNodes.BirthdaySceneNodeId +
                      " → " + BirthdayEndNodes.PrePlanerkaBridgeNodeId +
                      " → " + BirthdayEndNodes.Chapter2CabinetIntroNodeId +
                      " → chapter2 cabinet (mail / alevtina / toilet[4 actions] / meeting 35.44 / planerka)" +
                      " → chapter3 planerka (business / humor / observer) → " + Chapter4KrrbNodes.IntroNodeId +
                      " → chapter4 krrb seats (alevtina / kozlikhin / back row) → " + Chapter4KrrbNodes.EveningIntroNodeId +
                      " → " + Chapter4KrrbNodes.EveningBankEmptyNodeId +
                      " → (" + Chapter4KrrbNodes.BigCongratulationNodeId + " | " + Chapter4KrrbNodes.EveningGoodEndingNodeId + ")");
        }

        static void RemoveNodeById(StoryDatabase database, string nodeId)
        {
            if (database == null || string.IsNullOrEmpty(nodeId))
                return;

            for (var i = database.nodes.Count - 1; i >= 0; i--)
            {
                var entry = database.nodes[i];
                if (entry != null && entry.id == nodeId)
                    database.nodes.RemoveAt(i);
            }
        }

        static bool ContainsNode(StoryDatabase database, StoryNodeData node)
        {
            foreach (var entry in database.nodes)
            {
                if (entry != null && entry.id == node.id)
                    return true;
            }

            return false;
        }

        static StoryChoice MakeChoice(
            string label,
            string targetNodeId,
            StatChangeEntry[] statChanges = null,
            string[] flagsToSet = null)
        {
            return new StoryChoice
            {
                label = label,
                targetNodeId = targetNodeId,
                statChanges = statChanges ?? System.Array.Empty<StatChangeEntry>(),
                flagsToSet = flagsToSet ?? System.Array.Empty<string>()
            };
        }

        [MenuItem("MaratGame/Story/Run Birthday End Smoke Test")]
        public static void RunBirthdayEndSmokeTest()
        {
            CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            var state = GameState.Instance;
            state.Reset();

            var engine = new StoryEngine(state, database);
            engine.LoadNode(NavigationBar.HubNodeId);

            for (var i = 0; i < BirthdayEndNodes.MinDecisionsForBirthday; i++)
                state.RecordDecision();

            state.Flags.SetFlag(MorningBranchFlags.MetNozdrikovRoute);
            state.Flags.SetFlag(MorningBranchFlags.MetKozlikhinBreakfast);

            if (!MorningBranchProgress.IsReadyForHubBirthdayInspect(state))
                Debug.LogError("[MaratGame] Expected morning progress ready for hub inspect.");

            engine.LoadNode(BirthdayEndNodes.BirthdaySceneNodeId);
            if (engine.CurrentNode.id != BirthdayEndNodes.BirthdaySceneNodeId)
                Debug.LogError("[MaratGame] Expected birthday_scene.");

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != BirthdayEndNodes.PrePlanerkaBridgeNodeId)
                Debug.LogError("[MaratGame] Expected pre_planerka_bridge after «Далее».");

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != BirthdayEndNodes.Chapter2CabinetIntroNodeId)
                Debug.LogError("[MaratGame] Expected chapter2_cabinet_intro after bridge.");

            Debug.Log("[MaratGame] Birthday end smoke test passed. Respect=" + state.Stats.Respect +
                      "% Calm=" + state.Stats.Calm + "% Decisions=" + state.DecisionsCount);
        }

        [MenuItem("MaratGame/Story/Run Chapter 2 Cabinet Smoke Test")]
        public static void RunChapter2CabinetSmokeTest()
        {
            CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            RunCabinetPathSmoke(
                database,
                choiceIndex: 0,
                expectedTargetNodeId: Chapter2CabinetNodes.MailNodeId,
                expectedFlag: Chapter2CabinetFlags.MailChecked,
                expectedRespectDelta: 5,
                expectedCalmDelta: 0,
                expectedChaosDelta: -3);

            RunCabinetPathSmoke(
                database,
                choiceIndex: 1,
                expectedTargetNodeId: Chapter2CabinetNodes.AlevtinaNodeId,
                expectedFlag: Chapter2CabinetFlags.KrrbClosedMeetingHint,
                expectedRespectDelta: 2,
                expectedCalmDelta: 0,
                expectedChaosDelta: 0);

            RunCabinetPathSmoke(
                database,
                choiceIndex: 2,
                expectedTargetNodeId: Chapter2CabinetNodes.ToiletIntroNodeId,
                expectedFlag: Chapter2CabinetFlags.ToiletBreakTaken,
                expectedRespectDelta: 0,
                expectedCalmDelta: 0,
                expectedChaosDelta: 0);

            RunCabinetPathSmoke(
                database,
                choiceIndex: 3,
                expectedTargetNodeId: Chapter2CabinetNodes.Meeting3544NodeId,
                expectedFlag: Chapter2CabinetFlags.Meeting3544Visited,
                expectedRespectDelta: 0,
                expectedCalmDelta: -1,
                expectedChaosDelta: 0);

            Debug.Log("[MaratGame] Chapter 2 cabinet smoke test passed (4 in-cabinet loops + planerka transition choice).");
        }

        [MenuItem("MaratGame/Story/Run Chapter 3 Planerka Smoke Test")]
        public static void RunChapter3PlanerkaSmokeTest()
        {
            CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            RunPlanerkaStylePathSmoke(
                database,
                styleChoiceIndex: 0,
                expectedStyleFlag: Chapter3PlanerkaFlags.BusinessStyle,
                expectedRespectDelta: 3,
                expectedCalmDelta: 0,
                expectedChaosDelta: 0);

            RunPlanerkaStylePathSmoke(
                database,
                styleChoiceIndex: 1,
                expectedStyleFlag: Chapter3PlanerkaFlags.HumorStyle,
                expectedRespectDelta: 1,
                expectedCalmDelta: 3,
                expectedChaosDelta: 0);

            RunPlanerkaStylePathSmoke(
                database,
                styleChoiceIndex: 2,
                expectedStyleFlag: Chapter3PlanerkaFlags.ObserverStyle,
                expectedRespectDelta: 0,
                expectedCalmDelta: 1,
                expectedChaosDelta: -1);

            Debug.Log("[MaratGame] Chapter 3 planerka smoke test passed (3 styles -> krrb_intro).");
        }

        [MenuItem("MaratGame/Story/Run Chapter 4 KRRB Smoke Test")]
        public static void RunChapter4KrrbSmokeTest()
        {
            CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            RunKrrbSeatPathSmoke(
                database,
                seatChoiceIndex: 0,
                expectedSeatFlag: Chapter4KrrbFlags.SatNearAlevtina,
                contextualFlags: new[]
                {
                    MorningBranchFlags.MetNozdrikovRoute,
                    Chapter2MiniStoryFlags.HelpedEmployee
                });
            RunKrrbSeatPathSmoke(
                database,
                seatChoiceIndex: 1,
                expectedSeatFlag: Chapter4KrrbFlags.SatNearKozlikhin,
                contextualFlags: new[]
                {
                    MorningBranchFlags.MetKozlikhin,
                    Chapter3PlanerkaFlags.ObserverStyle
                });
            RunKrrbSeatPathSmoke(
                database,
                seatChoiceIndex: 2,
                expectedSeatFlag: Chapter4KrrbFlags.SatBackRow,
                contextualFlags: new[]
                {
                    MorningBranchFlags.ElevatorMessagesRead,
                    Chapter2MiniStoryFlags.HandledDelicately
                });

            Debug.Log("[MaratGame] Chapter 4 KRRB smoke test passed (3 seat branches -> evening_intro).");
        }

        [MenuItem("MaratGame/Story/Run Evening Big Congratulation Smoke Test")]
        public static void RunEveningBigCongratulationSmokeTest()
        {
            CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            RunEveningEndingPathSmoke(
                database,
                grantBigCongratulation: true,
                expectedNodeId: Chapter4KrrbNodes.BigCongratulationNodeId);
            RunEveningEndingPathSmoke(
                database,
                grantBigCongratulation: false,
                expectedNodeId: Chapter4KrrbNodes.EveningGoodEndingNodeId);

            Debug.Log("[MaratGame] Evening smoke test passed (big congratulation path + positive fallback).");
        }

        [MenuItem("MaratGame/Story/Run Chapter 2 Toilet Smoke Test")]
        public static void RunChapter2ToiletSmokeTest()
        {
            CreateBirthdayEndContent();

            var database = AssetDatabase.LoadAssetAtPath<StoryDatabase>(DatabasePath);
            if (database == null)
            {
                Debug.LogError("[MaratGame] StoryDatabase_Test missing.");
                return;
            }

            RunToiletPathSmoke(
                database,
                toiletChoiceIndex: 0,
                expectedNodeId: Chapter2CabinetNodes.ToiletWashFaceNodeId,
                expectedFlag: Chapter2ToiletFlags.WashedFace,
                expectedRespectDelta: 0,
                expectedCalmDelta: 3,
                expectedChaosDelta: 0);

            RunToiletPathSmoke(
                database,
                toiletChoiceIndex: 1,
                expectedNodeId: Chapter2CabinetNodes.ToiletPhoneMessagesNodeId,
                expectedFlag: Chapter2ToiletFlags.PhoneMessagesChecked,
                expectedRespectDelta: 1,
                expectedCalmDelta: 0,
                expectedChaosDelta: 0);

            RunToiletPathSmoke(
                database,
                toiletChoiceIndex: 2,
                expectedNodeId: Chapter2CabinetNodes.ToiletSilenceMonologueNodeId,
                expectedFlag: Chapter2ToiletFlags.SilenceMonologueHeard,
                expectedRespectDelta: 0,
                expectedCalmDelta: 2,
                expectedChaosDelta: 0);

            RunToiletPathSmoke(
                database,
                toiletChoiceIndex: 3,
                expectedNodeId: Chapter2CabinetNodes.ToiletMiddleStallNodeId,
                expectedFlag: Chapter2ToiletFlags.MiddleStallEntered,
                expectedRespectDelta: 2,
                expectedCalmDelta: 1,
                expectedChaosDelta: 0);

            Debug.Log("[MaratGame] Chapter 2 toilet smoke test passed (4 actions + return).");
        }

        static void RunCabinetPathSmoke(
            StoryDatabase database,
            int choiceIndex,
            string expectedTargetNodeId,
            string expectedFlag,
            int expectedRespectDelta,
            int expectedCalmDelta,
            int expectedChaosDelta)
        {
            var state = GameState.Instance;
            state.Reset();
            var engine = new StoryEngine(state, database);

            engine.LoadNode(BirthdayEndNodes.Chapter2CabinetIntroNodeId);
            if (engine.CurrentNode.id != BirthdayEndNodes.Chapter2CabinetIntroNodeId)
                Debug.LogError("[MaratGame] Expected chapter2_cabinet_intro on chapter 2 start.");

            if (state.CurrentDayBlock != DayBlock.BeforeMeeting)
                Debug.LogError("[MaratGame] Chapter 2 intro should set day block BeforeMeeting.");

            var startRespect = state.Stats.Respect;
            var startCalm = state.Stats.Calm;
            var startChaos = state.Stats.Chaos;

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != Chapter2CabinetNodes.EntryNodeId)
                Debug.LogError("[MaratGame] Intro should lead to chapter2_cabinet_entry.");

            var choices = engine.GetChoiceAvailability();
            if (choices.Length < 5)
                Debug.LogError("[MaratGame] Cabinet entry should have at least 5 choices including planerka transition.");

            engine.SelectChoice(choiceIndex);
            if (engine.CurrentNode.id != expectedTargetNodeId)
                Debug.LogError($"[MaratGame] Cabinet choice {choiceIndex} expected {expectedTargetNodeId}, got {engine.CurrentNode.id}.");

            if (!string.IsNullOrWhiteSpace(expectedFlag) && !state.Flags.HasFlag(expectedFlag))
                Debug.LogError($"[MaratGame] Cabinet choice {choiceIndex} should set flag {expectedFlag}.");

            if (engine.CurrentNode.choices == null || engine.CurrentNode.choices.Length == 0)
                Debug.LogError($"[MaratGame] Cabinet branch {expectedTargetNodeId} must have return choice.");
            else if (expectedTargetNodeId == Chapter2CabinetNodes.ToiletIntroNodeId)
                engine.SelectChoice(engine.CurrentNode.choices.Length - 1);
            else
                engine.SelectChoice(0);

            if (engine.CurrentNode.id != Chapter2CabinetNodes.EntryNodeId)
                Debug.LogError($"[MaratGame] Cabinet branch {expectedTargetNodeId} should return to chapter2_cabinet_entry.");

            var entryChoices = engine.GetChoiceAvailability();
            if (choiceIndex < entryChoices.Length && entryChoices[choiceIndex].IsAvailable)
                Debug.LogError($"[MaratGame] Cabinet choice {choiceIndex} should be unavailable after return to entry.");

            var respectDelta = state.Stats.Respect - startRespect;
            var calmDelta = state.Stats.Calm - startCalm;
            var chaosDelta = state.Stats.Chaos - startChaos;

            if (respectDelta != expectedRespectDelta || calmDelta != expectedCalmDelta || chaosDelta != expectedChaosDelta)
                Debug.LogError("[MaratGame] Cabinet stats mismatch for choice " + choiceIndex +
                               $": expected ({expectedRespectDelta},{expectedCalmDelta},{expectedChaosDelta}), " +
                               $"got ({respectDelta},{calmDelta},{chaosDelta}).");
        }

        static void RunPlanerkaStylePathSmoke(
            StoryDatabase database,
            int styleChoiceIndex,
            string expectedStyleFlag,
            int expectedRespectDelta,
            int expectedCalmDelta,
            int expectedChaosDelta)
        {
            var state = GameState.Instance;
            state.Reset();
            var engine = new StoryEngine(state, database);

            engine.LoadNode(BirthdayEndNodes.Chapter2CabinetIntroNodeId);
            engine.SelectChoice(0);
            engine.SelectChoice(4);

            if (engine.CurrentNode.id != Chapter3PlanerkaNodes.IntroNodeId)
                Debug.LogError("[MaratGame] Cabinet should transition to planerka_intro via choice #4.");

            var styles = engine.GetChoiceAvailability();
            if (styles.Length != 3)
                Debug.LogError("[MaratGame] Planerka intro must expose exactly 3 behavior styles.");

            var startRespect = state.Stats.Respect;
            var startCalm = state.Stats.Calm;
            var startChaos = state.Stats.Chaos;

            engine.SelectChoice(styleChoiceIndex);
            if (engine.CurrentNode.id == Chapter3PlanerkaNodes.IntroNodeId)
                Debug.LogError("[MaratGame] Planerka style choice should leave intro node.");

            engine.SelectChoice(0);
            engine.SelectChoice(0);

            if (engine.CurrentNode.id != Chapter4KrrbNodes.IntroNodeId)
                Debug.LogError("[MaratGame] Planerka style path should converge to krrb_intro.");

            if (!state.Flags.HasFlag(expectedStyleFlag))
                Debug.LogError($"[MaratGame] Planerka style choice {styleChoiceIndex} should set {expectedStyleFlag}.");

            if (state.CurrentDayBlock != DayBlock.KrrbUk)
                Debug.LogError("[MaratGame] krrb_intro should switch day block to KrrbUk.");

            var respectDelta = state.Stats.Respect - startRespect;
            var calmDelta = state.Stats.Calm - startCalm;
            var chaosDelta = state.Stats.Chaos - startChaos;
            if (respectDelta != expectedRespectDelta || calmDelta != expectedCalmDelta || chaosDelta != expectedChaosDelta)
            {
                Debug.LogError("[MaratGame] Planerka stats mismatch for style " + styleChoiceIndex +
                               $": expected ({expectedRespectDelta},{expectedCalmDelta},{expectedChaosDelta}), " +
                               $"got ({respectDelta},{calmDelta},{chaosDelta}).");
            }
        }

        static void RunToiletPathSmoke(
            StoryDatabase database,
            int toiletChoiceIndex,
            string expectedNodeId,
            string expectedFlag,
            int expectedRespectDelta,
            int expectedCalmDelta,
            int expectedChaosDelta)
        {
            var state = GameState.Instance;
            state.Reset();
            var engine = new StoryEngine(state, database);

            engine.LoadNode(BirthdayEndNodes.Chapter2CabinetIntroNodeId);
            engine.SelectChoice(0);
            engine.SelectChoice(2);

            if (engine.CurrentNode.id != Chapter2CabinetNodes.ToiletIntroNodeId)
                Debug.LogError("[MaratGame] Expected toilet_intro after cabinet choice.");

            var introChoices = engine.GetChoiceAvailability();
            if (introChoices.Length != 5)
                Debug.LogError("[MaratGame] Toilet intro should have 4 actions + return.");

            var startRespect = state.Stats.Respect;
            var startCalm = state.Stats.Calm;
            var startChaos = state.Stats.Chaos;

            engine.SelectChoice(toiletChoiceIndex);
            if (engine.CurrentNode.id != expectedNodeId)
                Debug.LogError($"[MaratGame] Toilet choice {toiletChoiceIndex} expected {expectedNodeId}, got {engine.CurrentNode.id}.");

            if (engine.CurrentNode.choices == null || engine.CurrentNode.choices.Length == 0)
                Debug.LogError($"[MaratGame] Toilet node {expectedNodeId} must return to cabinet.");
            else if (expectedNodeId == Chapter2CabinetNodes.ToiletMiddleStallNodeId)
            {
                engine.SelectChoice(0);
                if (engine.CurrentNode.id != Chapter2CabinetNodes.ToiletHelpEmployeeEntryNodeId)
                    Debug.LogError("[MaratGame] Middle stall should enter mini-story entry node.");

                engine.SelectChoice(0);
                if (engine.CurrentNode.id != Chapter2CabinetNodes.ToiletHelpEmployeeContentNodeId)
                    Debug.LogError("[MaratGame] Mini-story entry should lead to content node.");

                engine.SelectChoice(0);
                if (engine.CurrentNode.id != Chapter2CabinetNodes.ToiletHelpEmployeeExitNodeId)
                    Debug.LogError("[MaratGame] Mini-story content should lead to exit node.");

                engine.SelectChoice(0);
                if (engine.CurrentNode.id != Chapter2CabinetNodes.ToiletIntroNodeId)
                    Debug.LogError("[MaratGame] Mini-story exit should return to toilet_intro.");

                engine.SelectChoice(engine.CurrentNode.choices.Length - 1);
            }
            else
                engine.SelectChoice(0);

            if (!state.Flags.HasFlag(expectedFlag))
                Debug.LogError($"[MaratGame] Toilet choice {toiletChoiceIndex} should set flag {expectedFlag}.");

            if (engine.CurrentNode.id != Chapter2CabinetNodes.EntryNodeId)
                Debug.LogError($"[MaratGame] Toilet node {expectedNodeId} should return to chapter2_cabinet_entry.");

            var respectDelta = state.Stats.Respect - startRespect;
            var calmDelta = state.Stats.Calm - startCalm;
            var chaosDelta = state.Stats.Chaos - startChaos;

            if (respectDelta != expectedRespectDelta || calmDelta != expectedCalmDelta || chaosDelta != expectedChaosDelta)
                Debug.LogError("[MaratGame] Toilet stats mismatch for choice " + toiletChoiceIndex +
                               $": expected ({expectedRespectDelta},{expectedCalmDelta},{expectedChaosDelta}), " +
                               $"got ({respectDelta},{calmDelta},{chaosDelta}).");
        }

        static void RunKrrbSeatPathSmoke(
            StoryDatabase database,
            int seatChoiceIndex,
            string expectedSeatFlag,
            string[] contextualFlags)
        {
            var state = GameState.Instance;
            state.Reset();

            if (contextualFlags != null)
            {
                foreach (var flag in contextualFlags)
                    state.Flags.SetFlag(flag);
            }

            var engine = new StoryEngine(state, database);
            engine.LoadNode(Chapter4KrrbNodes.IntroNodeId);
            if (engine.CurrentNode.id != Chapter4KrrbNodes.IntroNodeId)
                Debug.LogError("[MaratGame] Expected krrb_intro before seat selection.");

            var seatChoices = engine.GetChoiceAvailability();
            if (seatChoices.Length != 3)
                Debug.LogError("[MaratGame] krrb_intro should expose exactly 3 seat choices.");

            engine.SelectChoice(seatChoiceIndex);
            if (!state.Flags.HasFlag(expectedSeatFlag))
                Debug.LogError($"[MaratGame] Seat choice {seatChoiceIndex} should set flag {expectedSeatFlag}.");

            if (engine.CurrentNode == null || engine.CurrentNode.choices == null || engine.CurrentNode.choices.Length == 0)
            {
                Debug.LogError("[MaratGame] Seat reaction node must expose follow-up choices.");
                return;
            }

            var selectedIndex = SelectPreferredAvailableChoice(engine);
            if (selectedIndex < 0)
            {
                Debug.LogError("[MaratGame] No available contextual choice found in seat branch.");
                return;
            }

            engine.SelectChoice(selectedIndex);
            if (engine.CurrentNode == null || engine.CurrentNode.choices == null || engine.CurrentNode.choices.Length == 0)
            {
                Debug.LogError("[MaratGame] Seat follow-up node should lead to evening intro.");
                return;
            }

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != Chapter4KrrbNodes.EveningIntroNodeId)
                Debug.LogError($"[MaratGame] Seat branch {seatChoiceIndex} should converge to evening_intro.");

            if (state.CurrentDayBlock != DayBlock.Evening)
                Debug.LogError("[MaratGame] evening_intro should set day block to Evening.");
        }

        static int SelectPreferredAvailableChoice(StoryEngine engine)
        {
            var availability = engine.GetChoiceAvailability();
            if (availability == null || availability.Length == 0)
                return -1;

            // Prefer contextual options first (usually non-zero indices), then fallback to default.
            for (var i = 1; i < availability.Length; i++)
            {
                if (availability[i].IsAvailable)
                    return availability[i].Index;
            }

            for (var i = 0; i < availability.Length; i++)
            {
                if (availability[i].IsAvailable)
                    return availability[i].Index;
            }

            return -1;
        }

        static void RunEveningEndingPathSmoke(
            StoryDatabase database,
            bool grantBigCongratulation,
            string expectedNodeId)
        {
            var state = GameState.Instance;
            state.Reset();

            if (grantBigCongratulation)
            {
                state.Flags.SetFlag(Chapter4KrrbFlags.HighSocialPresence);
                state.Stats.Reset(60, 60, state.Stats.Chaos);
            }

            var engine = new StoryEngine(state, database);
            engine.LoadNode(Chapter4KrrbNodes.EveningIntroNodeId);
            if (engine.CurrentNode.id != Chapter4KrrbNodes.EveningIntroNodeId)
            {
                Debug.LogError("[MaratGame] Evening smoke: expected evening_intro.");
                return;
            }

            engine.SelectChoice(0);
            if (engine.CurrentNode.id != Chapter4KrrbNodes.EveningBankEmptyNodeId)
            {
                Debug.LogError("[MaratGame] Evening smoke: evening_intro should lead to evening_bank_empty.");
                return;
            }

            var choices = engine.GetChoiceAvailability();
            if (choices.Length != 2)
            {
                Debug.LogError("[MaratGame] evening_bank_empty should have two ending options.");
                return;
            }

            var firstAvailable = choices[0].IsAvailable;
            if (grantBigCongratulation && !firstAvailable)
                Debug.LogError("[MaratGame] Expected big congratulation option to be available.");
            if (!grantBigCongratulation && firstAvailable)
                Debug.LogError("[MaratGame] Big congratulation option should stay locked without social condition.");

            var choiceIndex = firstAvailable ? 0 : 1;
            engine.SelectChoice(choiceIndex);
            if (engine.CurrentNode.id != expectedNodeId)
                Debug.LogError($"[MaratGame] Evening smoke expected {expectedNodeId}, got {engine.CurrentNode.id}.");

            if (state.CurrentDayBlock != DayBlock.Final)
                Debug.LogError("[MaratGame] Evening ending nodes should set day block Final.");
        }
    }
}
