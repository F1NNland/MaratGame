# Инструкции для AI-агента — MaratGame

## Проект в двух словах

**«Обычный рабочий день»** — визуальная новелла / офисная бродилка на **PC**, Unity 6 (URP 2D). Игрок ведёт Марата по Банку в день рождения; все выборы «правильные», ветвление без game over.

| Документ | Назначение |
|----------|------------|
| [docs/SPEC.md](docs/SPEC.md) | Полное ТЗ (из docx 12.05.2026) |
| [docs/PROJECT_PLAN.md](docs/PROJECT_PLAN.md) | Фазы, архитектура, DoD |
| [docs/ASSETS.md](docs/ASSETS.md) | Временные фото: `Assets/Фото Марат/` |
| [docs/mvp/README.md](docs/mvp/README.md) | **MVP по шагам** — один чат = один файл |
| [docs/DOTWEEN.md](docs/DOTWEEN.md) | UI-анимации, `UiTweens.cs` |
| [docs/mvp/step-05-textmeshpro.md](docs/mvp/step-05-textmeshpro.md) | Весь UI-текст — **TextMeshProUGUI** |
| [docs/UI_KIT.md](docs/UI_KIT.md) | **Procedural Image** (панели) + **MPUIKit** (кнопки) |
| [docs/WEB_DEPLOY.md](docs/WEB_DEPLOY.md) | **WebGL**, GitHub Pages, CI |
| [веб-MVP](https://makhmetsafin.github.io/marat-game/) | Эталон UX для Главы 1 |

## Unity MCP (обязательно использовать)

В проекте подключён пакет `com.local.unitymcp` и MCP-сервер **`user-unitymcp`**. Unity Editor должен быть **открыт** с этим проектом.

### Перед работой со сценой

1. Прочитать схему инструмента в `mcps/user-unitymcp/tools/*.json`.
2. `CallMcpTool` → `ping_unity` — если нет ответа, попросить пользователя открыть Unity.
3. `get_compile_status` — дождаться `false` (не компилируется) перед Play Mode.
4. После правок сцены: `save_scene`.

### Когда MCP, когда файлы

| Задача | Инструмент |
|--------|------------|
| C#, ScriptableObjects, архитектура | Правка `.cs` в репозитории |
| Canvas, кнопки, иерархия, компоненты | Unity MCP (`create_gameobject`, `set_component_field`, …) |
| Проверка в рантайме | `enter_play_mode` → тест → `exit_play_mode` |
| Ошибки | `get_editor_errors`, `get_console_logs` |
| Пакетные операции | `execute_batch` |

**Не редактировать вручную** YAML сцен/префабов без крайней необходимости — высокий риск merge-конфликтов и битых ссылок.

### Полезные MCP-команды

- `list_project_scenes`, `open_scene`, `get_active_scene`, `list_scene_objects`
- `find_objects`, `get_components`, `get_component_fields`, `set_component_field`
- `set_rect_transform`, `set_object_reference` — привязка UI к скриптам

### Автопрохождение игры (Play Mode)

1. `enter_play_mode` (или уже в Play Mode после `enter_play_mode`)
2. `game_get_play_state` — узел сценария, статы, список `actions` (`choice:0`, `nav:left`, `phone:open`, `ui:Canvas/…`, `menu:start`, …)
3. `game_perform_action` с `actionId` из списка — то же, что клик по кнопке / выбор в диалоге
4. При DOTween-переходах подождать ~0.5–1 с и снова `game_get_play_state` (или `capture_screenshot` для проверки UI)
5. Универсально: `list_interactable_ui` + `invoke_ui_button` по `path`
6. Логика в `Assets/Scripts/Agent/AgentPlayBridge.cs`; после обновления Unity MCP — `npm run build` в `C:/unitymcp` и перезапуск MCP в Cursor

## Соглашения по коду

- Папка: `Assets/Scripts/`, namespace **`MaratGame`**
- Один `StoryRunner` / `GameState` — не плодить синглтоны без нужды
- Строки сценария: пока RU в SO/JSON; не размазывать реплики по десятку классов
- Input: **Unity Input System** (уже в manifest), не legacy `Input`
- Рендер: **URP 2D**; не переключать на 3D без запроса
- Коммиты: **только по явной просьбе** пользователя

## Порядок работы агента (типовая сессия)

1. Уточнить фазу из [PROJECT_PLAN.md](docs/PROJECT_PLAN.md) (или спросить пользователя).
2. `ping_unity` + прочитать `get_editor_errors` если что-то ломалось.
3. Реализовать логику в `.cs` → дождаться компиляции.
4. Собрать/обновить UI через MCP.
5. Play Mode smoke-test.
6. Кратко отчитаться: что сделано, что следующий шаг.

## Релизный поток (полный день)

Сверять с [docs/SPEC.md](docs/SPEC.md):

1. Утро: холл, телефон, ветки → «Осмотреться» → `birthday_scene` → **сразу** глава 2 (без MVP-экрана итогов утра)
2. Кабинет, планёрка, КРРБ/УК, вечер
3. Финал: `big_congratulation` или `evening_good_ending` + экран итогов дня

Веб-MVP — только UX-референс HUD/навигации, не обязательный финал после утра.

## Чего не делать

- Не добавлять «game over» или наказание за выбор
- Не тянуть в проект тяжёлые VN-фреймворки без согласования (достаточно своего лёгкого `StoryRunner`)
- UI-анимации: **DOTween** через `MaratGame.Presentation.UiTweens` — не плодить сырые `DOTween.To` в каждом view
- UI-текст: только **TMP** (`TextMeshProUGUI`), шрифт `TmpAssets.DefaultFontPath` — не `UnityEngine.UI.Text`
- UI-хром: панели → `ProceduralImage` + `FreeModifier`; кнопки → `MPImage` (Rectangle); фото-фоны → обычный `Image`. См. `UiStyle`, `UiKitFactory`
- Не коммитить `Library/`, `Temp/`, `Logs/`, `UserSettings/`
- Unity MCP только локально: см. `Packages/manifest.local.json.example` (не коммитить `com.local.unitymcp` в manifest)

## Окружение

- **Unity:** 6000.3.7f1 (`ProjectSettings/ProjectVersion.txt`)
- **ОС разработки:** Windows
- **Целевая платформа:** Standalone Windows PC (добавить в Build Settings при первой сборке)

## Связь с пользователем

Если ТЗ и MVP расходятся — **MVP для UX**, **docs/SPEC.md для контента**. Спорные механики (например «хаос») — спросить перед реализацией.
