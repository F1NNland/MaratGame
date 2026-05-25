# Шаг 04 — UI: HUD, диалог, выборы + DOTween

## Цель
Подписать UI на `StoryRunner.OnNodeChanged` и **анимировать** смену контента через DOTween.

## Зависимости
**Шаг 03** — сцена `Game.unity` с Canvas.

## Обязательно прочитать
- [docs/DOTWEEN.md](../DOTWEEN.md)
- [Assets/Scripts/Presentation/UiTweens.cs](../../Assets/Scripts/Presentation/UiTweens.cs) — **не дублировать** твины, вызывать отсюда

## Unity MCP
1. `ping_unity` — иначе стоп, попросить открыть Unity.
2. `get_compile_status` → не компилируется.
3. После правок: `save_scene`.

## Иерархия Canvas (рекомендация)
```
Canvas
├── HUD                          (+ CanvasGroup опционально)
│   ├── TimeText
│   ├── RespectText              // твин Counter при изменении
│   ├── CalmText
│   └── ChapterText
├── LocationHeader
├── Background
│   ├── BackgroundImageA         // для CrossFadeImage
│   └── BackgroundImageB
├── DialoguePanel                (+ CanvasGroup)
│   ├── SpeakerText
│   └── BodyText
└── ChoicesContainer             // VerticalLayoutGroup
    └── ChoiceButton (prefab)    (+ CanvasGroup на корне кнопки)
```

### Скрипты
```
Assets/Scripts/Presentation/GameHudView.cs
Assets/Scripts/Presentation/DialogueView.cs
Assets/Scripts/Presentation/ChoicesView.cs
Assets/Scripts/Presentation/GameUIController.cs
```

### Поведение (логика)
- `GameUIController` подписан на `StoryRunner.OnNodeChanged`
- При смене узла:
  1. `UiTweens.TransitionNode(...)` → внутри callback: текст, локация, фон (спрайт)
  2. `ChoicesView` пересоздаёт кнопки → `UiTweens.StaggerChoices(...)`
- При изменении Respect/Calm: `UiTweens.Counter` от предыдущего значения к новому (кэш в view)
- `OnDestroy` / смена сцены: `DOKill` на корневых `CanvasGroup` панелей

### Анимации (минимум для Done)

| Элемент | Твин |
|---------|------|
| Смена узла | `TransitionNode` или fade диалога + `RevealDialogue` |
| Кнопки выбора | `StaggerChoices` |
| Статы HUD | `Counter` при дельте от выбора |
| Первый показ сцены Game | `Fade` HUD `CanvasGroup` 0→1 |

### Поведение (визуал)
- Шрифт/цвета — нейтральные, как веб-MVP (тёмный фон, светлый текст)
- Длительности — константы из `UiTweens` (не магические числа в каждом view)

## Не делать
- TextMeshPro (**шаг 05**), MPUIKit/Procedural (**шаг 05b**) — в Step 04 setup уже встроены, если пересобираешь после 05b
- Телефон, главное меню (шаги 06, 08)
- DOTween на `StoryEngine` / `GameState`
- Отдельные `DOTweenAnimation` на префабах без причины — предпочитать `UiTweens` из кода

## Критерии готовности
- [ ] Play Game: тестовый узел — текст и кнопки **появляются с анимацией**, не мгновенно
- [ ] Второй узел подряд — переход без зависших твинов и дублирующихся кнопок
- [ ] HUD 07:58, 50%, 60%; при выборе со статом — цифры **перетекают**, не прыгают
- [ ] Нет ошибок DOTween / Console

## Handoff → шаг 05 (TMP)
Перед меню и контентом — перевести весь UI на TextMeshPro: [step-05-textmeshpro.md](step-05-textmeshpro.md).

## Handoff → шаг 06 (после TMP)
- Имена GameObject
- `GameUIController` API
- Для меню переиспользовать `UiTweens.Fade` / `SlideAnchored`

## Промпт
```
MaratGame: docs/mvp/step-04-ui-shell.md. UI + StoryRunner. DOTween: docs/DOTWEEN.md и UiTweens.cs — все твины через них. Unity MCP для Canvas.
```
