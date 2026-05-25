# MVP — шаги для отдельных чатов

**Цель MVP:** играбельное **утро, Глава 1** как [веб-MVP](https://makhmetsafin.github.io/marat-game/) — меню → холл 07:58 → телефон → навигация → короткие ветки → финальная статистика.

**Не в MVP:** главы 2–4, видео, полный текст всех веток ТЗ, финальный арт.

Для полного ТЗ после MVP используйте отдельную дорожную карту: [docs/post-mvp/README.md](../post-mvp/README.md).

---

## Как пользоваться

1. Открой **новый чат** в Cursor.
2. Скопируй **промпт** из таблицы.
3. После шага отметь `[x]` в статусе внизу.
4. Следующий чат — только когда предыдущий **Done**.

Общие правила: [AGENTS.md](../../AGENTS.md) · ТЗ: [SPEC.md](../SPEC.md) · Фото: [ASSETS.md](../ASSETS.md) · Анимации: [DOTWEEN.md](../DOTWEEN.md) · UI-ассеты: [UI_KIT.md](../UI_KIT.md)

**Текст UI:** только **TextMeshPro** (шаг 05). **Панели/кнопки:** Procedural Image + MPUIKit (шаг 05b).

**Unity MCP:** Editor открыт → `Tools → Unity MCP → Start Bridge`.

---

## Порядок шагов

| # | Файл | Суть | Зависит от |
|---|------|------|------------|
| 01 | [step-01-core.md](step-01-core.md) | `GameState`, статы, флаги | — |
| 02 | [step-02-story-runner.md](step-02-story-runner.md) | Узлы + `StoryRunner` | 01 |
| 03 | [step-03-scenes-flow.md](step-03-scenes-flow.md) | Boot → Menu → Game | 02 |
| 04 | [step-04-ui-shell.md](step-04-ui-shell.md) | HUD, диалог, выборы + DOTween | 03 |
| **05** | **[step-05-textmeshpro.md](step-05-textmeshpro.md)** | **Весь текст → TMP** | **04** |
| **05b** | **[step-05b-ui-kit.md](step-05b-ui-kit.md)** | **MPUIKit + Procedural UI** | **05** |
| 06 | [step-06-main-menu.md](step-06-main-menu.md) | «НАЧАТЬ ИГРУ» | 05b |
| 07 | [step-07-hall-morning.md](step-07-hall-morning.md) | Холл 07:58 + фон | 06 |
| 08 | [step-08-phone.md](step-08-phone.md) | 5 сообщений | 07 |
| 09 | [step-09-navigation-hub.md](step-09-navigation-hub.md) | ← ↑ → навигация | 08 |
| 10 | [step-10-branches-morning.md](step-10-branches-morning.md) | 3 ветки утра | 09 |
| 11 | [step-11-birthday-and-end.md](step-11-birthday-and-end.md) | Финал + Win build | 10 |
| 12 | [step-12-web-deploy.md](step-12-web-deploy.md) | **WebGL → GitHub Pages** | 11 |

**Оценка:** ~1 чат на шаг. Шаг **05** в Unity: меню `MaratGame/UI/Setup Step 05 (TextMeshPro)`.

**Играть в браузере:** [WEB_DEPLOY.md](../WEB_DEPLOY.md) → `https://f1nnland.github.io/MaratGame/`

---

## Промпты для чатов

### 01–04
```
Проект MaratGame. Выполни только docs/mvp/step-01-core.md (или 02/03/04). AGENTS.md.
```

### 05 — TextMeshPro
```
MaratGame: docs/mvp/step-05-textmeshpro.md. TMP + Setup Step 05 в Unity. Кириллица обязательна.
```

### 05b — UI Kit
```
MaratGame: docs/mvp/step-05b-ui-kit.md и docs/UI_KIT.md. Procedural панели + MPImage кнопки. Setup Step 05 + Apply UI Kit to MainMenu.
```

### 06 — Меню
```
MaratGame: docs/mvp/step-06-main-menu.md. Экран «НАЧАТЬ ИГРУ» (TMP уже есть).
```

### 07–11
```
MaratGame: docs/mvp/step-07-hall-morning.md
MaratGame: docs/mvp/step-08-phone.md
MaratGame: docs/mvp/step-09-navigation-hub.md
MaratGame: docs/mvp/step-10-branches-morning.md
MaratGame: docs/mvp/step-11-birthday-and-end.md
```

### 12 — Web
```
MaratGame: docs/WEB_DEPLOY.md. WebGL + GitHub Pages для публичной ссылки.
```

---

## Статус

- [x] 01 Core
- [x] 02 StoryRunner
- [x] 03 Scenes
- [x] 04 UI shell
- [ ] 05 TextMeshPro
- [ ] **05b UI Kit (MPUIKit + Procedural)** ← после 05
- [x] 06 Main menu
- [x] 07 Hall
- [x] 08 Phone
- [x] 09 Navigation
- [x] 10 Branches
- [x] 11 End + build

**MVP готов** — шаги 01–11 и **05b** отмечены.  
**Публичный веб** — шаг **12** + [WEB_DEPLOY.md](../WEB_DEPLOY.md).
