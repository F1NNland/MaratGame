# План разработки — MaratGame (Unity PC)

## Текущее состояние репозитория

| Есть | Нет |
|------|-----|
| Unity **6000.3.7f1**, URP 2D, Input System | Скрипты игры (`Assets/Scripts`) |
| Пакет **Unity MCP** (`com.local.unitymcp`) | Сцены кроме `SampleScene` |
| `.vscode` для отладки | Контент (диалоги, спрайты, видео) |

**Референс поведения:** [веб-MVP](https://makhmetsafin.github.io/marat-game/) — холл, HUD (время, уважение, спокойствие), телефон, глава «Утро», диалоги, экран итогов.

---

## Целевая архитектура

```
Assets/
  Scripts/
    Core/           GameState, GameClock, StatSystem, Save (опционально)
    Narrative/      StoryRunner, SceneNode, Choice, Condition, Flag
    Presentation/   DialogueUI, PhoneUI, LocationHubUI, TransitionFX
    Data/           ScriptableObject definitions + импорт из JSON/Yarn (позже)
  Scenes/
    Boot.unity
    MainMenu.unity
    Game.unity          # единая сцена с под-состояниями или additive locations
  Prefabs/UI/
  Art/                  # 2D/спрайты, позже псевдо-3D фон
  Audio/
  StreamingAssets/      # видео тяжёлые (КРРБ, вечер, мини-сюжеты)
docs/
  SPEC.md
  PROJECT_PLAN.md
```

### Принципы

1. **Данные отделяются от кода** — сцены и выборы в ScriptableObjects / JSON, не хардкод в MonoBehaviour.
2. **Флаги и статы** — `HasFlag("met_kozlikhin_breakfast")`, `Respect >= 50` для ветвления вечера.
3. **Без проигрыша** — ветки меняют статы/флаги/доступные сцены, не `GameOver`.
4. **PC-first** — мышь + клавиатура; геймпад опционально через Input System.
5. **Визуальная новелла + хаб** — MVP: текст + кнопки навигации; позже иллюстрации/видео на слотах.

---

## Фазы (roadmap)

> **MVP (Глава 1, утро):** [docs/mvp/README.md](mvp/README.md) — 11 шагов + **05b** (TMP, MPUIKit/Procedural UI). См. [UI_KIT.md](UI_KIT.md).
>
> **После MVP (полный ТЗ):** [docs/post-mvp/README.md](post-mvp/README.md) — пошаговый roadmap 01-14 для отдельных чатов.

### Фаза 0 — Основа (1–2 сессии агента)

- [ ] Структура папок `Assets/Scripts`, namespace `MaratGame`
- [ ] `GameState`: время, Respect, Calm, flags, decisions count
- [ ] `StoryRunner`: загрузка узла → показ UI → применение эффектов → переход
- [ ] Сцена `Game.unity`: Canvas (HUD + dialogue panel + choice buttons)
- [ ] Boot → MainMenu → Game
- [ ] Сборка **Windows Standalone** в Build Settings

**Критерий готовности:** Play Mode, клик по выбору меняет статы и локацию на экране.

### Фаза 1 — Паритет с веб-MVP (Глава 1, Утро)

- [ ] Главное меню «НАЧАТЬ ИГРУ»
- [ ] Холл 07:58 + вибрация телефона + 5 сообщений
- [ ] Навигация: столовая / лифты / кабинет (хотя бы по 1 выбору на ветку)
- [ ] Диалог «С днём рождения, Марат!»
- [ ] HUD: время, % уважения и спокойствия, глава/блок дня
- [ ] Экран итогов (уважение, спокойствие, решений) + «Сыграть ещё раз»

**Критерий:** прохождение утра без Unity-ошибок; ощущение как у MVP.

### Фаза 2 — Глава 2 (Кабинет + Туалет)

- [ ] Узлы кабинета (4 выбора)
- [ ] Локация «Туалет» + 4 варианта
- [ ] Мини-сюжет «Помочь сотруднику» (заглушка текста, слот под гиф/видео)
- [ ] Мини-сюжет «Не то совещание» (VideoPlayer + placeholder)

### Фаза 3 — Главы 3–4 (Планерка, КРРБ)

- [ ] Планерка: 3 стиля → разные флаги / реплики вечера
- [ ] КРРБ: выбор места, длинные реплики NPC
- [ ] Переход «Вечер»

### Фаза 4 — Финал и полировка

- [ ] Ветка «Большое поздравление» + условие «много общался»
- [ ] Внутренние монологи (отдельный стиль UI)
- [ ] Звук UI, музыка офиса (опционально)
- [ ] Локализация: все строки в одном месте (RU сейчас)
- [ ] Иконка, название окна, разрешение 1920×1080 / 16:9

### Фаза 5 — Визуал (после геймплея)

- [ ] Фоны локаций (2D или псевдо-3D силуэт Марата сзади)
- [ ] Портреты/баблы для Алевтины, Козлихина, Ноздрикова
- [ ] Подключение реальных гиф/видео из ТЗ

---

## Модель данных (черновик)

```csharp
// Узел сценария (ScriptableObject или JSON)
StoryNode {
  string id;
  string locationId;      // "hall", "canteen", ...
  string timeDisplay;     // "07:58"
  string chapter;         // "ГЛАВА 1", "УТРО"
  string bodyText;
  string speaker;
  MediaSlot media;        // none | image | gif | video
  Choice[] choices;
  StatEffect[] onEnter;
  string[] requiredFlags;
}

Choice {
  string label;
  string targetNodeId;
  StatEffect[] effects;
  string[] setFlags;
}
```

---

## Риски и решения

| Риск | Решение |
|------|---------|
| ТЗ растёт (хаос, скрытые сцены) | Расширяемый `StatId` enum + флаги, не переписывать UI |
| Много видео | `StreamingAssets` + Addressables при росте |
| Агент ломает .unity YAML | Сцены/UI через **Unity MCP**; логику только в `.cs` |
| Нет исходников MVP | Копировать **поток экранов**, не код с GitHub Pages |

---

## Definition of Done (релиз PC/Web)

- [ ] Сборка Windows x64 запускается без Unity Editor
- [ ] Полный день: утро → … → финал с тортом (хотя бы 1 путь)
- [ ] Нет soft-lock (всегда есть выход / продолжение)
- [ ] WebGL-сборка опубликована и воспроизводит минимум 1 полный маршрут
- [ ] `docs/SPEC.md` и контент синхронизированы
