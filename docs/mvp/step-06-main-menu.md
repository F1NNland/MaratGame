# Шаг 05 — Главное меню

## Цель
Экран как веб-MVP: название + **«НАЧАТЬ ИГРУ»** → `Game.unity`.

## Зависимости
**Шаг 05 + 05b** — TMP и UI Kit ([step-05-textmeshpro.md](step-05-textmeshpro.md), [step-05b-ui-kit.md](step-05b-ui-kit.md)).

## Сделать

### MainMenu.unity (MCP)
- Заголовок: «Обычный рабочий день» (две строки OK)
- Подзаголовок: «Офисная бродилка · Банк»
- Кнопка: **НАЧАТЬ ИГРУ**

### Скрипт
`Assets/Scripts/Presentation/MainMenuController.cs`
- OnClick → `SceneManager.LoadScene(SceneNames.Game)`
- На Game: `GameBootstrap` вызывает `GameState.Reset()` + `StoryRunner` с `startNodeId`

### UI Kit
- Кнопка «НАЧАТЬ ИГРУ» уже на **MPImage** (если был Setup 05) — только стиль/размер
- Опционально: полупрозрачная панель за заголовком через `ProceduralImage`

### DOTween (лёгко)
- При `Start`: `CanvasGroup` на корне меню — `UiTweens.Fade` 0→1
- Заголовок: `SlideAnchored` снизу (опционально)
- Кнопка «НАЧАТЬ ИГРУ»: появление с задержкой ~0.15s через `StaggerChoices` на одном `RectTransform` или scale `OutBack`

См. [DOTWEEN.md](../DOTWEEN.md).

### Boot
- Убрать Debug-кнопки с шага 03, если были

## Не делать
- Настройки, сохранения, локализация
- Контент холла

## Критерии готовности
- [ ] Boot → Menu → Start → Game с чистым state
- [ ] Повторный Play с Menu снова сбрасывает статы

## Handoff → шаг 07
Menu готов; на Game `BackgroundImage` и `LocationHeader` ждут контент холла.

## Промпт
[README.md](README.md) — «Шаг 05».
