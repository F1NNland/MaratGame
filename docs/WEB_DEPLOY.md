# Публикация игры в браузере (GitHub Pages)

Игра Unity собирается в **WebGL** и открывается как статический сайт — по тому же принципу, что [веб-MVP](https://makhmetsafin.github.io/marat-game/).

**Репозиторий:** [github.com/F1NNland/MaratGame](https://github.com/F1NNland/MaratGame)  
**URL после настройки:** `https://f1nnland.github.io/MaratGame/`  
(имя пользователя и репозитория на GitHub в нижнем регистре)

---

## Два способа

| Способ | Когда |
|--------|--------|
| **A. GitHub Actions** (рекомендуется) | Пуш в `main` → автосборка и деплой |
| **B. Локальная сборка** | Нет лицензии Unity в CI / отладка WebGL |

---

## A. Автодеплой (GitHub Actions)

Файл: [`.github/workflows/webgl-github-pages.yml`](../.github/workflows/webgl-github-pages.yml)

### 1. Включить GitHub Pages

1. GitHub → репозиторий **MaratGame** → **Settings** → **Pages**
2. **Build and deployment** → Source: **Deploy from a branch**
3. **Branch:** `main` · папка **`/docs`**
4. Сохранить

Ветки `gh-pages` **не будет**, пока workflow её не создаст — это нормально. Билд кладётся в папку **`docs/`** на `main` (`index.html` игры рядом с `docs/mvp/`, `SPEC.md` и т.д.).

Не используй Source = «GitHub Actions», если деплой идёт через peaceiris в `docs/` — будет конфликт.

### 2. Секрет Unity (обязательно для CI)

Нужна **бесплатная Personal** лицензия Unity.

1. На своём ПК: [Unity Manual — Manual activation](https://docs.unity3d.com/Manual/ManualActivationGuide.html)  
   или через Unity Hub получить файл `.ulf` / activation request.
2. Для **game-ci** чаще всего используют один из вариантов:

**Вариант 1 — `UNITY_LICENSE` (файл лицензии в base64):**

```bash
# PowerShell (Windows)
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\to\Unity_v2022.x.ulf")) | Set-Clipboard
```

GitHub → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**  
Имя: `UNITY_LICENSE` — вставить base64.

**Вариант 2 — email + password:**

| Secret | Значение |
|--------|----------|
| `UNITY_EMAIL` | Email аккаунта Unity |
| `UNITY_PASSWORD` | Пароль |

### 3. Первый запуск

```bash
git add .
git commit -m "Add WebGL GitHub Pages workflow"
git push origin main
```

**Actions** → workflow **WebGL → GitHub Pages** → дождаться зелёной галочки (первая сборка 20–40 мин).

### 4. Проверка

Открыть: **https://f1nnland.github.io/MaratGame/**

> Если 404: подождать 2–5 мин; в Pages: **`main`** + папка **`/docs`**. В репозитории на `main` должен появиться `docs/index.html` после успешного Deploy.

### Что делает workflow

1. Собирает WebGL (Unity **6000.3.7f1**)
2. Копирует `build/WebGL/*` в **`docs/`** на ветке `main` (остальные файлы в `docs/` сохраняются)

**Unity MCP** не в `manifest.json` в git (ломает CI). Локально добавь строку из `Packages/manifest.local.json.example` в свой `manifest.json` и не коммить её.

---

## B. Локальная сборка + ручной деплой

### 1. Сборка в Unity

1. **File → Build Settings** → Platform **WebGL** → Switch Platform (один раз)
2. Сцены: Boot, MainMenu, Game (уже в списке)
3. Меню: **`MaratGame → Build → WebGL (GitHub Pages)`**  
   или Build Settings → Build  
4. Выход: папка `build/WebGL/` в корне проекта

### 2. Проверка локально

Простой сервер (корень = `build/WebGL`):

```bash
npx --yes serve build/WebGL
```

Открыть URL из консоли (часто `http://localhost:3000`).

### 3. Выложить на GitHub Pages

**Через git (ветка `gh-pages`):**

```bash
cd build/WebGL
git init
git add .
git commit -m "WebGL build"
git push -f git@github.com:F1NNland/MaratGame.git main:gh-pages
```

Затем в **Settings → Pages** → Source: **Deploy from branch** → branch `gh-pages` / root.

**Или** закоммитить артефакт в `docs/site/` (не рекомендуется — большие `.wasm` файлы раздувают репо; лучше Actions или `gh-pages`).

---

## Настройки WebGL в проекте

Уже в `ProjectSettings`:

- Разрешение по умолчанию в браузере: 960×600 (можно поднять в Player Settings → WebGL)
- Сжатие: проверь **Publishing Settings → Compression Format** (Gzip/Brotli — меньше размер, дольше загрузка)

Рекомендации для веба:

- **Managed Stripping Level** — Medium/High (меньше билд)
- Отключить **Unity Splash Screen** в Player Settings, если не нужен
- Тяжёлые фото в `Фото Марат` — при росте билда сжать JPEG

---

## Сравнение с HTML-MVP

| | HTML MVP | Unity WebGL |
|---|----------|-------------|
| Репозиторий | отдельный (Pages) | этот репо, ветка/Actions |
| Обновление | push статики | push кода → CI собирает |
| Размер | малый | десятки МБ (`.wasm` + данные) |
| Время загрузки | быстро | первая загрузка дольше |

Имеет смысл держать **старый MVP** как эталон UX, а **Unity WebGL** — как «полная» версия по мере готовности MVP.

---

## Частые проблемы

| Проблема | Решение |
|----------|---------|
| CI: `com.local.unitymcp` not found | Workflow уже удаляет пакет; не добавляй обратно в CI |
| CI: No UNITY_LICENSE | Добавить секреты (раздел A.2) |
| Белый экран в браузере | F12 → Console; часто нехватка памяти WebGL → увеличить **Initial Memory Size** |
| 404 на GitHub Pages | Pages: **main** + **/docs**; дождаться 2–5 минут после зелёного workflow; репо должен быть публичным |
| `touch .nojekyll: Permission denied` | Обнови workflow (`.nojekyll` в job Deploy) |
| `Multiple artifacts named "github-pages"` | Не жми **Re-run** только Deploy; push новый workflow (peaceiris → `gh-pages`) |
| Pages = GitHub Actions + ошибка deploy | Pages: **`main`** + **`/docs`**, не GitHub Actions |
| В списке веток только `main` | Ок — выбери **main** и папку **/docs** |
| Кириллица / TMP | LiberationSans SDF в билде; проверить **Include Font Data** у TMP |

---

## Release-checklist (step 14)

1. Прогнать full-day smoke в Unity: `MaratGame/Agent/Run Full Day Smoke (2 routes)`.
2. Собрать Windows: `MaratGame/Build/Windows x64 MVP` и запустить `Builds/Windows/MaratGame.exe` вне Editor.
3. Собрать WebGL: `MaratGame/Build/WebGL (GitHub Pages)` и проверить локально через `npx --yes serve build/WebGL`.
4. Запушить в `main` и дождаться workflow `WebGL → GitHub Pages`.
5. Проверить route в браузере: старт → утро → финальный узел без soft-lock.

---

## Связанные файлы

- Workflow: `.github/workflows/webgl-github-pages.yml`
- Локальная сборка: `Assets/Scripts/Editor/WebGLBuildMenu.cs`
- Сцены в билде: `ProjectSettings/EditorBuildSettings.asset`
