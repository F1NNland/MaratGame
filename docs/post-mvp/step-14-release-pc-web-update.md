# Шаг 14 — Release PC + Web Update

## Цель
Закрыть релиз полного дня: стабильная Windows сборка и обновленный WebGL билд.

## Зависимости
- [step-13-full-day-agent-smoke.md](step-13-full-day-agent-smoke.md)

## Сделать

### Windows release
- Проверить Build Settings (Boot -> MainMenu -> Game).
- Собрать Windows x64:
  - путь: `Builds/Windows/MaratGame.exe`;
  - smoke запуск вне Unity Editor.
- Пройти полный день в exe без soft-lock и критических ошибок.

### WebGL update
- Обновить web build по [docs/WEB_DEPLOY.md](../WEB_DEPLOY.md).
- Проверить, что на GitHub Pages выкатывается текущий full-day контент.
- Проверить в браузере минимум один полный маршрут.

### Quality gate
- Сверить DoD с [docs/PROJECT_PLAN.md](../PROJECT_PLAN.md):
  - полный день играбелен;
  - нет dead-end;
  - docs синхронизированы с реализацией.
- Обновить пользовательские документы: README и roadmap ссылки.

## Не делать
- Не добавлять новые механики в релизный шаг.
- Не начинать крупные рефакторы в ветке стабилизации.

## Критерии готовности
- [ ] Windows x64 билд стабильно запускается и проходится до финала.
- [ ] WebGL версия опубликована и воспроизводит full-day поток.
- [ ] Все критические smoke/checklists зеленые.
- [ ] Документация обновлена и соответствует текущему состоянию игры.

## Текущий статус выполнения
- [x] Build Settings подтверждены: `Boot -> MainMenu -> Game`.
- [x] Full-day smoke (`MaratGame.Editor.FullDayAgentSmokeTest.Run`) проходит в Editor.
- [x] Windows x64 release build собран (`Builds/Windows/MaratGame.exe`) и smoke запуска вне Editor подтвержден.
- [x] WebGL build собран локально (`build/WebGL`), локальный HTTP smoke (`http://localhost:4173`) отдает `200 OK`.
- [ ] Публикация WebGL в GitHub Pages и проверка полного маршрута в браузере после деплоя.
- [x] Release-документация синхронизирована (`README`, `WEB_DEPLOY`, `PROJECT_PLAN`).

## Unity
- Play Mode sanity check после release build.
- Проверка console logs на пустой критический список.

## Промпт
```text
MaratGame: выполни только docs/post-mvp/step-14-release-pc-web-update.md.
Закрой релиз полного дня: Windows build + WebGL update + финальная проверка DoD.
```
