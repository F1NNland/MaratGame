# Шаг 02 — StoryRunner и данные узла

## Цель
Движок «узел → эффекты → выборы → следующий узел» на данных, без красивого UI.

## Зависимости
**Шаг 01** — `GameState`, `StatChange`, флаги.

## Сделать

### Папки
```
Assets/Scripts/Narrative/
Assets/Scripts/Data/
Assets/Data/Story/          # ScriptableObjects
```

### Типы данных
| Тип | Поля (минимум) |
|-----|----------------|
| `StoryChoice` | `label`, `targetNodeId`, `List<StatChange>`, `flagsToSet[]` |
| `StoryNodeData` : ScriptableObject | `id`, `locationId`, `timeDisplay`, `chapterLabel`, `bodyText`, `speaker`, `onEnterEffects`, `choices[]` |
| `StoryDatabase` : ScriptableObject | `List<StoryNodeData>` или ссылка на стартовый `startNodeId` |

### StoryRunner
- `MonoBehaviour` или сервис: держит `GameState`, ссылку на `StoryDatabase`
- `LoadNode(string id)` — применить `onEnter`, обновить location/time в state, событие `OnNodeChanged(StoryNodeData)`
- `SelectChoice(int index)` — эффекты выбора, `DecisionsCount++`, загрузка `targetNodeId`
- Если узел без выборов — опционально `OnNodeComplete` (для шага 10)

### Тестовый контент
Создать SO `Node_TestHall` с id `hall_intro`, текст-заглушка, 1 выбор «Далее» → тот же или `hall_intro_2`.

## Не делать
- Полноценный Canvas (достаточно `Debug.Log` в `OnNodeChanged` или временный `OnGUI`)
- Все узлы главы 1

## Критерии готовности
- [ ] В Play Mode на тестовой сцене `SampleScene` + объект с `StoryRunner` — в Console видны смена узла и статов при выборе
- [ ] `StoryNodeData` создаётся через Create Asset Menu

## Handoff → шаг 03
`StoryRunner` готов к подписке UI; `startNodeId = hall_intro` (или переименовать в шаге 06).

## Промпт
[README.md](README.md) — «Шаг 02».
