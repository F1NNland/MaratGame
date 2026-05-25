# Шаг 10 — День рождения, итоги, сборка

## Цель
Замкнуть MVP: сцена поздравления + экран статистики + рестарт + **Windows x64 build**.

## Зависимости
**Шаг 09** — ветки утра играбельны.

## Сделать

### Узел `birthday_scene`
- Триггер: на `hall_hub` кнопка **● Действие** «Осмотреться» вручную, когда `MorningBranchProgress` (≥2 зоны из столовая/лифты/кабинет и ≥3 решений). Авто-триггер выключен. По полному ТЗ финальная цитата — вечер (`big_congratulation`), не утро.
- Текст MVP: «С днём рождения, Марат!»
- Утро: короткое поздравление без финальной цитаты (по полному ТЗ цитата — вечер, `big_congratulation`)
- Портрет не обязателен

### DOTween
- Появление `birthday_scene`: `RevealDialogue` или `ShowOverlay`
- Экран итогов: `ShowOverlay` на full-screen `CanvasGroup`; цифры — `Counter` для Respect/Calm/решений
- «Сыграть ещё раз»: перед загрузкой меню — `HideOverlay` или короткий `Fade` всего Canvas

### Экран итогов `ending_screen`
- Overlay или отдельный UI state:
  - Уважение — итог %
  - Спокойствие — итог %
  - Решений принято — `DecisionsCount`
- Кнопка **↺ СЫГРАТЬ ЕЩЁ РАЗ** → `GameState.Reset()` → `MainMenu` или `hall_morning`

### Скрипты
`EndingUI.cs`, доработка `GameBootstrap.ResetGame()`

### Build
- File → Build Settings → Windows Standalone x64
- Путь: `Builds/Windows/MaratGame.exe` (в .gitignore)
- Проверка: exe запускается, меню → утро → концовка

## Не делать
- Несколько финалов, ветка «Большое поздравление» из полного ТЗ
- Steam, инсталлятор

## Критерии готовности (MVP DONE)
- [x] Полный цикл 5–10 мин без ошибок Console
- [x] Итоги совпадают с накопленными статами
- [x] Рестарт сбрасывает игру
- [x] Windows build запускается

## Unity
Меню: **MaratGame → Story → Setup Step 11 (Birthday & End)** — контент, UI, Build Settings.
Сборка: **MaratGame → Build → Windows x64 MVP** → `Builds/Windows/MaratGame.exe`.
Smoke: **MaratGame → Story → Run Birthday End Smoke Test**.

## После шага
Отметить все чекбоксы в [README.md](README.md). Следующий эпик — `docs/epics/epic-chapter-2.md` (создать при необходимости).

## Промпт
[README.md](README.md) — «Шаг 10».
