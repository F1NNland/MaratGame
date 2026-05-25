# Шаг 01 — Ядро: GameState и статы

## Цель
Модель состояния игры без UI и сцен.

## Контекст (прочитать 2 мин)
- [AGENTS.md](../../AGENTS.md) — namespace `MaratGame`, без game over
- Веб-MVP: старт **Уважение 50%**, **Спокойствие 60%**, время **07:58**

## Зависимости
Нет (первый шаг).

## Сделать

### Папки
```
Assets/Scripts/Core/
Assets/Scripts/Core/Enums/
```

### Классы (минимум)
| Класс | Назначение |
|-------|------------|
| `GameStats` | `Respect`, `Calm` (0–100), методы `AddRespect`, `AddCalm`, clamp |
| `GameFlags` | `HashSet<string>` или обёртка `SetFlag` / `HasFlag` |
| `GameState` | Singleton или plain object: stats, flags, `DecisionsCount`, `CurrentTime` (string или `TimeSpan`), `CurrentLocationId` |
| `StatChange` | struct: `StatType`, `int Delta` (для будущих узлов) |

### Поведение
- Стартовые значения: Respect=50, Calm=60, Time=`07:58`, Location=`hall`
- `Reset()` для «сыграть ещё раз» (понадобится в шаге 10)
- Без `MonoBehaviour` в ядре (чистый C#), кроме опционального `GameStateBehaviour` на сцене позже

## Не делать в этом шаге
- Сцены, Canvas, StoryRunner, ScriptableObjects сценария
- Input, сохранения на диск

## Критерии готовности (Done)
- [ ] Проект компилируется
- [ ] Можно из Edit Mode/Test вызвать: `GameState` reset → add respect → flag set/get
- [ ] Нет ошибок в Console при импорте скриптов

## Handoff → шаг 02
Передать: пути классов, имена полей, что `GameState` будет инжектиться в `StoryRunner`.

## Промпт для чата
См. [README.md](README.md) — «Шаг 01».
