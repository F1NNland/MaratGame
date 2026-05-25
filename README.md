# MaratGame — «Обычный рабочий день»

Визуальная новелла / офисная бродилка про день рождения Марата в банке.  
Unity 6 (URP 2D), PC и **WebGL** (браузер).

## Играть в браузере

**WebGL (после успешного CI):** [https://f1nnland.github.io/MaratGame/docs/](https://f1nnland.github.io/MaratGame/docs/)

Если открывается этот README, а не игра — в **Settings → Pages** укажи ветку **`main`**, папку **`/docs`** (не `/`). Тогда игра будет на [https://f1nnland.github.io/MaratGame/](https://f1nnland.github.io/MaratGame/).

Подробнее: [docs/WEB_DEPLOY.md](docs/WEB_DEPLOY.md).

Эталон UX (HTML): [makhmetsafin.github.io/marat-game](https://makhmetsafin.github.io/marat-game/)

## Разработка

| Документ | Содержание |
|----------|------------|
| [AGENTS.md](AGENTS.md) | Правила для AI-агента |
| [docs/mvp/README.md](docs/mvp/README.md) | MVP по шагам |
| [docs/post-mvp/README.md](docs/post-mvp/README.md) | Full ТЗ roadmap после MVP |
| [docs/WEB_DEPLOY.md](docs/WEB_DEPLOY.md) | **WebGL + GitHub Pages** |
| [docs/SPEC.md](docs/SPEC.md) | ТЗ |
| [docs/UI_KIT.md](docs/UI_KIT.md) | TMP, MPUIKit, Procedural UI |

Unity **6000.3.7f1**. Локально может быть пакет `com.local.unitymcp` (Cursor) — для CI он убирается автоматически.

### Быстрый старт

1. Клонировать репозиторий, открыть в Unity Hub  
2. Сцены: Boot → MainMenu → Game  
3. Меню: `MaratGame/UI/Setup Step 05 (TextMeshPro)` при первой настройке UI  

### Финальная стабилизация (step 14)

- Шаг: [docs/post-mvp/step-14-release-pc-web-update.md](docs/post-mvp/step-14-release-pc-web-update.md)
- Release DoD: [docs/PROJECT_PLAN.md](docs/PROJECT_PLAN.md)
- Web deploy: [docs/WEB_DEPLOY.md](docs/WEB_DEPLOY.md)
- Full-day smoke: `MaratGame/Agent/Run Full Day Smoke (2 routes)`
- Windows build: `MaratGame/Build/Windows x64 MVP` → `Builds/Windows/MaratGame.exe`
- WebGL build: `MaratGame/Build/WebGL (GitHub Pages)` → `build/WebGL/`

### Дорожные карты

- MVP-поток (ограниченный объем): [docs/mvp/README.md](docs/mvp/README.md)
- Полная игра по ТЗ: [docs/post-mvp/README.md](docs/post-mvp/README.md)

## Лицензия

Уточни у автора проекта.
