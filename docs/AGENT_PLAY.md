# Автопрохождение игры для AI-агента

Агент может запускать Play Mode и «кликать» по UI так же, как игрок — через Unity MCP.

## Подготовка (один раз)

1. Unity Editor открыт на **MaratGame**, мост: **Tools → Unity MCP → Start Bridge**.
2. После обновления `C:/unitymcp`: `npm run build`, перезапуск MCP-сервера **user-unitymcp** в Cursor (чтобы появились `game_get_play_state`, `game_perform_action`).
3. Дождаться компиляции скриптов в Unity (фокус на Editor, если тип `AgentPlayBridge` не находится). Проверка: **Tools → Marat Game → Agent - Get Play State** (только в Play Mode).

## Типовой цикл

```
ping_unity → get_compile_status (isCompiling: false)
→ open_scene MainMenu (или Game)
→ enter_play_mode
→ game_get_play_state
→ game_perform_action { actionId: "menu:start" }   // или choice:0, nav:left, …
→ (пауза ~0.5–1 с при DOTween)
→ game_get_play_state …
→ exit_play_mode
```

## Инструменты MCP

| Инструмент | Назначение |
|------------|------------|
| `game_get_play_state` | Узел сценария, статы, массив `actions` |
| `game_perform_action` | Выполнить `actionId` из списка |
| `list_interactable_ui` | Все кликабельные `Button` в Play Mode |
| `invoke_ui_button` | Клик по иерархии `Canvas/…/Button` |
| `invoke_static` | Прямой вызов `MaratGame.Agent.AgentPlayBridge` |
| `capture_screenshot` | Визуальная проверка экрана |

Пока новые tools не подхватились Cursor, используйте `execute_batch`:

```json
{
  "commands": [
    { "command": "invoke_static", "typeName": "MaratGame.Agent.AgentPlayBridge", "methodName": "GetPlayState" },
    { "command": "invoke_static", "typeName": "MaratGame.Agent.AgentPlayBridge", "methodName": "PerformAction", "arg0": "choice:0" }
  ]
}
```

## Id действий

| Id | Когда |
|----|--------|
| `menu:start` | Сцена MainMenu |
| `choice:N` | Кнопки выбора в диалоге |
| `story:<nodeId>:choice:<N>` | Выбор с привязкой к конкретному узлу (устойчиво для автотестов) |
| `story:continue` | Выбрать первый доступный выбор на текущем узле |
| `phone:open` / `phone:close` | Утро в холле, телефон |
| `nav:left` / `nav:forward` / `nav:right` | Hub навигация |
| `hub:inspect` | «Осмотреться» → день рождения |
| `morning:canteen_*` / `morning:elevator_*` | Явные утренние ветки в столовой/лифтах |
| `chapter2:*`, `toilet:*`, `planerka:*`, `krrb:*`, `evening:*` | Явные post-MVP выборы полного дня |
| `ending:replay` | Финальный экран вечера |
| `ui:Canvas/…` | Любая активная UI-кнопка (дублирует реальный клик) |

Код: `Assets/Scripts/Agent/AgentPlayBridge.cs`.

## Full-day smoke (post-MVP step 13)

Минимум 2 маршрута для регрессии полного дня:

1. **Big congratulation path**
   - `menu:start` → телефон (`phone:open`/`phone:close`) → утренние ветки (`morning:*`) до `hub:inspect`
   - После `hub:inspect` — `story:continue` до главы 2 → `chapter2:planerka` → `planerka:business` (или другой стиль)
   - `krrb:seat_alevtina` (или другой сценарный seat) → `evening:what_next` → `evening:big_congratulation`
   - Проверить `nodeId == big_congratulation`, `dayBlock == Final`.
2. **Positive fallback path**
   - Та же канва, но к вечеру не выполнять условия большого поздравления
   - В `evening_bank_empty` выбрать `evening:good_ending`
   - Проверить `nodeId == evening_good_ending`, `dayBlock == Final`.

Полезные проверки после каждого шага:
- `game_get_play_state.flags` / `flagCount` для регрессии флагов;
- `respect`, `calm`, `chaos`, `decisions`;
- `actions[]` не пустой (нет soft-lock на промежуточных узлах).

Локальный regression helper: `MaratGame/Agent/Run Full Day Smoke (2 routes)`.
