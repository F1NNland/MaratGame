# Шаг 05b — MPUIKit + Procedural UI Image

## Цель
Скруглённые панели и кнопки вместо плоских `Image` — визуально ближе к веб-MVP.

## Зависимости
**Шаг 05** (TMP) — текст уже на TextMeshPro.

## Прочитать
- [docs/UI_KIT.md](../UI_KIT.md)
- `UiStyle.cs`, `UiKitFactory.cs`

## Сделать

### Автоматически (рекомендуется)
1. Unity → **`MaratGame/UI/Setup Step 05 (TextMeshPro)`**  
   (пересобирает Game: Procedural панель диалога, MPImage на ChoiceButton)
2. **`MaratGame/UI/Apply UI Kit to MainMenu`** — кнопка на MainMenu
3. Play Game — панель диалога со скруглением, кнопки выбора с MPImage

### Проверить вручную
- [ ] Canvas Game: Additional Shader Channels включают TexCoord1–3
- [ ] `DialoguePanel` → `ProceduralImage` + `FreeModifier`
- [ ] `ChoiceButton` prefab → `MPImage` (Rectangle), не `Image`
- [ ] Console без ошибок shader / material

### Дальше по MVP (06+)
- Главное меню «НАЧАТЬ ИГРУ» — MPImage на CTA (шаг 06)
- Телефон overlay — Procedural panel (шаг 08)

## Не делать
- MPImage на фонах `Фото Марат`
- Переписывать `UiTweens` / `StoryRunner`

## Критерии готовности
- [ ] Диалог и кнопки визуально скруглены
- [ ] Клики и DOTween-анимации работают как до шага
- [ ] Кириллица на TMP не сломалась

## Промпт
```
MaratGame: docs/mvp/step-05b-ui-kit.md и docs/UI_KIT.md. Проверь UiKitFactory в GameUiShellSetup, запусти Setup Step 05 + Apply UI Kit to MainMenu.
```
