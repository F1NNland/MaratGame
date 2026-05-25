# DOTween в MaratGame

## Установка (уже в проекте)

| Путь | Назначение |
|------|------------|
| `Assets/Plugins/Demigiant/DOTween/` | Плагин |
| `Assets/Resources/DOTweenSettings.asset` | Глобальные настройки |

**Один раз в Unity:** `Tools → Demigiant → DOTween Utility Panel` → **Setup DOTween** (если после клона репо твины не работают). Модули **UI** и **TextMeshPro** (если используете TMP) должны быть включены.

## Код

Общие твины UI — **`Assets/Scripts/Presentation/UiTweens.cs`** (`MaratGame.Presentation`).

Правила:
- Перед новым твином на объекте: `UiTweens.Kill(target)`
- Overlay/панели: компонент **`CanvasGroup`** (alpha), не только `Image.color`
- Не вызывать `Time.timeScale = 0` без `SetUpdate(true)` на твинах
- В `OnDestroy` — `Kill` на тех же `Transform`/`CanvasGroup`

## Где какие анимации (MVP)

| Шаг | Что анимировать | Метод / паттерн |
|-----|-----------------|-----------------|
| **04** UI shell | Смена узла: фон, диалог, кнопки; цифры HUD | `TransitionNode`, `RevealDialogue`, `StaggerChoices`, `Counter` |
| **05** TMP | — | Только смена компонентов текста |
| **06** Меню | Появление заголовка и кнопки | `Fade` + `SlideAnchored` |
| **07** Холл | Смена фона локации | `CrossFadeImage` (два Image) или `Fade` на `CanvasGroup` фона |
| **08** Телефон | Overlay, badge | `ShowOverlay` / `HideOverlay`, `PunchScale` |
| **11** Финал | Экран итогов | `ShowOverlay` на full-screen `CanvasGroup` |

## Длительности (дефолты в `UiTweens`)

- **Fast** 0.2s — hover/мелочи  
- **Normal** 0.35s — панели, выборы  
- **Slow** 0.5s — фон, крупные переходы  

Ease: `OutQuad` / `OutCubic` для появления, `InQuad` для скрытия.

## Не использовать DOTween для

- Логики сценария (`StoryEngine`, статы) — только отображение
- Долгих циклов без `SetLink(gameObject)` на UI, который уничтожается

## Ссылки

- [DOTween Documentation](http://dotween.demigiant.com/documentation.php)
- Шаг реализации UI: [mvp/step-04-ui-shell.md](mvp/step-04-ui-shell.md)
