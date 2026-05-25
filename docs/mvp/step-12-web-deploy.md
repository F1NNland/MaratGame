# Шаг 12 — WebGL и GitHub Pages

## Цель
Публичная ссылка на игру в браузере, как [веб-MVP](https://makhmetsafin.github.io/marat-game/).

## Зависимости
**MVP играбелен** в Editor (желательно шаг 11 — Win/Web build проверен локально).

## Прочитать
- [docs/WEB_DEPLOY.md](../WEB_DEPLOY.md) — полная инструкция

## Сделать

### GitHub (один раз)
- [ ] Settings → Pages → Source: **GitHub Actions**
- [ ] Secrets: `UNITY_LICENSE` (или `UNITY_EMAIL` + `UNITY_PASSWORD`)
- [ ] Push в `main` → workflow **WebGL → GitHub Pages** зелёный
- [ ] Открывается `https://<user>.github.io/MaratGame/`

### Локально (проверка)
- [ ] Build Settings → WebGL
- [ ] `MaratGame/Build/WebGL (GitHub Pages)`
- [ ] `npx serve build/WebGL` — игра стартует

## Не делать
- Коммитить `build/` и `Library/` в git
- Публиковать секреты Unity в репозиторий

## Критерии готовности
- [ ] Ссылка GitHub Pages открывается с другого устройства
- [ ] Меню → игра → утро проходят без ошибок в консоли браузера (F12)

## Промпт
```
MaratGame: docs/WEB_DEPLOY.md и step-12-web-deploy.md. Настроить/проверить WebGL GitHub Pages для F1NNland/MaratGame.
```
