# Шаг 05 — TextMeshPro (весь UI-текст)

## Цель
Заменить `UnityEngine.UI.Text` на **TextMeshProUGUI** везде: Game, MainMenu, префаб кнопки, скрипты Presentation.

## Зависимости
**Шаг 04** — UI shell на legacy Text.

## Стандарт проекта (после шага)

| Было | Стало |
|------|--------|
| `using UnityEngine.UI;` + `Text` | `using TMPro;` + `TextMeshProUGUI` |
| `LegacyRuntime.ttf` | `LiberationSans SDF` (кириллица) |
| `GetComponentInChildren<Text>()` | `GetComponentInChildren<TextMeshProUGUI>()` |

Шрифт: `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset`  
Константа в коде: `TmpAssets.DefaultFont` / `TmpAssets.DefaultFontPath`

## Сделать

### 1. Код (`Assets/Scripts/Presentation/`)
- [ ] `GameHudView`, `DialogueView`, `GameUIController` — поля `TextMeshProUGUI`
- [ ] `ChoicesView` — лейбл кнопки через TMP
- [ ] `UiTweens` — без изменений (работает с `Component`)

### 2. Editor
- [ ] `GameUiShellSetup` / `TmpUiFactory` — создавать TMP, не `Text`
- [ ] `ScenesFlowSetup` — MainMenu title/button на TMP (для новых сцен)
- [ ] Меню: **`MaratGame/UI/Setup Step 05 (TextMeshPro)`** — пересобрать Game UI + обновить MainMenu

### 3. Сцены и префабы
- [ ] Запустить Setup Step 05 в Unity (пересоздаёт Canvas на Game)
- [ ] `Assets/Prefabs/UI/ChoiceButton.prefab` — TMP на лейбле
- [ ] Проверить кириллицу: «Уважение», «Холл банка», выборы

### 4. DOTween
- `Tools → Demigiant → DOTween Utility Panel` → включить модуль **TextMeshPro** (если ещё не включён)

## Не делать
- Кастомный Font Asset с нуля (позже, если Liberation не устроит)
- Typewriter-эффект (отдельная задача)
- Шаги 06+ контента (только текстовые компоненты)

## Критерии готовности
- [ ] В проекте **нет** `UnityEngine.UI.Text` в `Assets/Scripts/` и на сценах Game/MainMenu (кроме отладки)
- [ ] Play Game: HUD, диалог, кнопки — кириллица читаема
- [ ] Console без missing script / TMP warnings

## Handoff → шаг 05b (UI Kit)
Сразу после TMP: [step-05b-ui-kit.md](step-05b-ui-kit.md) — скруглённые панели и кнопки.

## Handoff → шаг 06 (меню)
После 05b: оформление «НАЧАТЬ ИГРУ», не миграция компонентов.

## Промпт
```
MaratGame: docs/mvp/step-05-textmeshpro.md. Перевести весь UI на TextMeshPro (скрипты + Editor Setup Step 05). Кириллица обязательна. Не трогать контент главы 1.
```
