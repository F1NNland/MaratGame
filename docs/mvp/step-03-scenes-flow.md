# Шаг 03 — Сцены и переходы Boot → Menu → Game

## Цель
Три сцены в Build Settings и загрузка без геймплея.

## Зависимости
**Шаг 02** — `StoryRunner` существует.

## Unity MCP
1. `ping_unity` — иначе стоп, попросить открыть Unity.
2. `get_compile_status` → не компилируется.
3. После правок: `save_scene`.

## Сделать

### Сцены (`Assets/Scenes/`)
| Сцена | Содержимое |
|-------|------------|
| `Boot.unity` | Пустой + скрипт `BootLoader`: через 0.5с `SceneManager.LoadScene("MainMenu")` |
| `MainMenu.unity` | Пустой Canvas-заглушка (текст «Меню») |
| `Game.unity` | `GameStateBehaviour` (если есть), `StoryRunner`, пустой Canvas |

### Скрипты
```
Assets/Scripts/Core/SceneNames.cs   // константы имён сцен
Assets/Scripts/Core/BootLoader.cs
Assets/Scripts/Core/GameBootstrap.cs  // на Game: Init GameState, StoryRunner.StartStory()
```

### Build Settings
- Порядок: Boot (0), MainMenu (1), Game (2)
- Обновить `EditorBuildSettings` через MCP или вручную один раз

## Не делать
- Полный HUD и меню (шаги 04–05)
- Контент узлов кроме стартового id

## Критерии готовности
- [ ] Play с Boot → попадаем в MainMenu → можно вручную Load Game scene (временная кнопка Debug OK)
- [ ] На Game сцене `StoryRunner` стартует без NullReference
- [ ] `save_scene` на всех трёх

## Handoff → шаг 04
Сцена `Game.unity` с Canvas root; имена объектов зафиксировать в комментарии для UI-шага.

## Промпт
[README.md](README.md) — «Шаг 03».
