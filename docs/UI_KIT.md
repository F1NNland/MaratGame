# UI: MPUIKit + Procedural UI Image

Два ассета для **векторного** UI без спрайтов-9slice. Фото-фоны (холл) остаются на обычном `Image`.

| Ассет | Путь | Когда использовать |
|-------|------|-------------------|
| **Procedural UI Image** | `Assets/ProceduralUIImage/` | Большие **панели**: диалог, оверлеи, карточки |
| **MPUIKit** | `Assets/MPUIKit/` | **Кнопки**, чипы, акцентные блоки с радиусом и градиентом |

Документация MPUIKit: [scrollbie.com/mpuikit](https://scrollbie.com/documentations/mpuikit-docs/)

## Правила проекта

1. **Текст** — только `TextMeshProUGUI` ([DOTWEEN.md](DOTWEEN.md) / шаг 05 MVP).
2. **Фоны локаций** — `Image` + sprite (`Фото Марат`), не Procedural.
3. **Панели** — `ProceduralImage` + `FreeModifier`, радиус из `UiStyle.PanelCornerRadius`.
4. **Кнопки выбора / CTA** — `MPImage` (`DrawShape.Rectangle`) + `Button`, радиус `UiStyle.ButtonCornerRadius`.
5. Создание в Editor — **`UiKitFactory`** (`Assets/Scripts/Editor/UiKitFactory.cs`), не вручную в Inspector.
6. Canvas для Procedural Image должен иметь **TexCoord1–3** → `UiKitFactory.EnsureCanvasSupportsProceduralImage`.

## Цвета и радиусы

`Assets/Scripts/Presentation/UiStyle.cs` — единый источник для setup-скриптов.

## Unity-меню

| Меню | Действие |
|------|----------|
| `MaratGame/UI/Setup Step 04 (Game Canvas)` | HUD + диалог (Procedural) + кнопки (MPImage) |
| `MaratGame/UI/Setup Step 05 (TextMeshPro)` | Step 04 + TMP на MainMenu |
| `MaratGame/UI/Apply UI Kit to MainMenu` | Кнопка меню → MPImage (без пересборки Game) |
| `MaratGame/UI/Apply Reference Layout 2001×981 (Game scene)` | CanvasScaler + якоря/размеры под макет Figma |
| `MaratGame/UI/Apply Reference Layout 2001×981 (MainMenu scene)` | То же для MainMenu |
| `MaratGame/UI/Import Figma UI from Downloads` | PNG из `Downloads/Projects (2)` → `Assets/Art/UI/FigmaExport` |
| `MaratGame/UI/Apply Figma Design (Game scene)` | Спрайты + layout 2001×981 на Game |

После установки ассетов или смены UI: **запусти Step 05** (или Step 04), чтобы сцены совпали с кодом.

## Ручное создание (редко)

- Procedural: `GameObject → UI → Procedural Image` → Modifier **Free**, радиус углов.
- MPUI: `GameObject → UI → MPUI → MPImage` → Shape **Rectangle**, Corner Radius.

## MVP-шаг

Отдельный бриф: [mvp/step-05b-ui-kit.md](mvp/step-05b-ui-kit.md) (после TMP, перед меню).

## Не смешивать

- Не вешать `Image` и `ProceduralImage` на один объект.
- Не использовать `MPImage` для полноэкранных JPEG-фонов.
