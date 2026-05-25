# Шаг 07 — Телефон и сообщения

## Цель
Вибрация утра: панель **📱 Входящие**, 5 сообщений из ТЗ.

## Зависимости
**Шаг 06** — холл показывается.

## Тексты (из ТЗ / MVP)
1. «С днем рождения!»
2. «Вы на КРРБ будете?»
3. «Алевтина просила зайти»
4. «Торт привезли»
5. «Срочно нужен комментарий»

## Сделать

### UI
```
PhoneButton (иконка 📱, badge «5») — MPImage при желании
PhoneOverlay — ProceduralImage (панель) + CanvasGroup
  └── список MessageItem (TMP)
```
См. [UI_KIT.md](../UI_KIT.md).
`Assets/Scripts/Presentation/PhoneUI.cs`

### DOTween
- Открытие overlay: `UiTweens.ShowOverlay(phoneCanvasGroup)`
- Закрытие: `HideOverlay`
- Badge «5»: при входе в `hall_morning` — `UiTweens.PunchScale(phoneButton.transform)`; опционально loop `DOScale` 1↔1.05 с `SetLoops(-1, LoopType.Yoyo)` пока есть непрочитанные (kill при открытии)

### Сценарий
- Узел `hall_morning`: при входе показать badge / пульсацию
- Кнопка «Закрыть» → overlay off, выбор «Идти дальше» или автопереход к `hall_hub` (hub с навигацией)

### Узел `hall_hub`
- Тот же фон/время
- Текст: «Телефон стихает. Куда дальше?»
- Выборы — в шаге 08 (можно оставить одну заглушку)

## Не делать
- Ответы на SMS, ветвление по сообщениям

## Критерии готовности
- [ ] Badge 5, по клику — все 5 текстов
- [ ] После закрытия — можно продолжить сцену

## Handoff → шаг 09
`hall_hub` — точка для трёх направлений.

## Промпт
[README.md](README.md) — «Шаг 07».
