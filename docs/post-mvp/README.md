# Full ТЗ — шаги после MVP

Цель: перевести проект от MVP к полной версии по [docs/SPEC.md](../SPEC.md), не ломая текущую играбельную ветку.

## Как пользоваться

1. Открывай новый чат на каждый шаг.
2. Передавай только один файл шага из `docs/post-mvp/`.
3. После завершения шага отмечай чекбокс в статусе.
4. Если шаг помечен как параллельный, можно запускать в отдельном чате одновременно.

Общие документы:
- [AGENTS.md](../../AGENTS.md)
- [docs/PROJECT_PLAN.md](../PROJECT_PLAN.md)
- [docs/SPEC.md](../SPEC.md)
- [docs/ASSETS.md](../ASSETS.md)
- [docs/DOTWEEN.md](../DOTWEEN.md)
- [docs/WEB_DEPLOY.md](../WEB_DEPLOY.md)

## Лента шагов

| # | Файл | Суть | Зависит от | Параллельно |
|---|------|------|------------|-------------|
| 01 | [step-01-full-day-bridge.md](step-01-full-day-bridge.md) | Мост от MVP-утра к полноценному дню | MVP 01-12 | Нет |
| 02 | [step-02-engine-stats-media.md](step-02-engine-stats-media.md) | Расширение движка: Chaos, media, условия | 01 | Нет |
| 03 | [step-03-morning-full-branches.md](step-03-morning-full-branches.md) | Полные ветки утра по ТЗ | 02 | Да |
| 04 | [step-04-chapter2-cabinet.md](step-04-chapter2-cabinet.md) | Глава 2: кабинет | 02 | Да |
| 05 | [step-05-chapter2-toilet.md](step-05-chapter2-toilet.md) | Глава 2: туалет | 04 | Нет |
| 06 | [step-06-ministory-framework.md](step-06-ministory-framework.md) | Каркас мини-сюжетов | 05 | Нет |
| 07 | [step-07-ministory-help-employee.md](step-07-ministory-help-employee.md) | Мини-сюжет: помочь сотруднику | 06 | Да |
| 08 | [step-08-chapter3-planerka.md](step-08-chapter3-planerka.md) | Глава 3: планерка (3 стиля) | 05 | Да |
| 09 | [step-09-chapter4-krrb.md](step-09-chapter4-krrb.md) | Глава 4: КРРБ/УК | 07, 08 | Нет |
| 10 | [step-10-evening-big-congratulation.md](step-10-evening-big-congratulation.md) | Вечер и большое поздравление | 09 | Нет |
| 11 | [step-11-monologue-and-ui-states.md](step-11-monologue-and-ui-states.md) | Отдельные UI-режимы монологов | 02 | Да |
| 12 | [step-12-visual-and-media-pass.md](step-12-visual-and-media-pass.md) | Визуал и медиа по всем главам | 02 | Да |
| 13 | [step-13-full-day-agent-smoke.md](step-13-full-day-agent-smoke.md) | Автосмоук полного дня | 10 | Нет |
| 14 | [step-14-release-pc-web-update.md](step-14-release-pc-web-update.md) | Release PC + обновление WebGL | 13 | Нет |

## Критический путь

`01 -> 02 -> 04 -> 05 -> 06 -> 08 -> 09 -> 10 -> 13 -> 14`

## Промпты для отдельных чатов

### 01
```text
MaratGame: выполни только docs/post-mvp/step-01-full-day-bridge.md.
Контекст: после MVP игра не должна заканчиваться утром. Сохрани совместимость с текущими шагами mvp.
```

### 02
```text
MaratGame: выполни только docs/post-mvp/step-02-engine-stats-media.md.
Добавь скрытый Chaos, MediaSlot и условия на узлы/выборы, без ручного редактирования scene yaml.
```

### 03
```text
MaratGame: выполни только docs/post-mvp/step-03-morning-full-branches.md.
Нужны полные ветки утра из SPEC: столовая и лифты по 3 варианта, диалоги NPC.
```

### 04
```text
MaratGame: выполни только docs/post-mvp/step-04-chapter2-cabinet.md.
Реализуй кабинет с 4 выборами по SPEC, с влиянием на статы и флаги.
```

### 05
```text
MaratGame: выполни только docs/post-mvp/step-05-chapter2-toilet.md.
Реализуй локацию туалет и 4 варианта действий, включая телефонные сообщения.
```

### 06
```text
MaratGame: выполни только docs/post-mvp/step-06-ministory-framework.md.
Сделай общий каркас мини-сюжетов с безопасным возвратом в основной сценарий.
```

### 07
```text
MaratGame: выполни только docs/post-mvp/step-07-ministory-help-employee.md.
Подключи мини-сюжет "Помочь сотруднику" на каркасе из step-06.
```

### 08
```text
MaratGame: выполни только docs/post-mvp/step-08-chapter3-planerka.md.
Реализуй планерку и 3 стиля поведения с разными флагами на вечер.
```

### 09
```text
MaratGame: выполни только docs/post-mvp/step-09-chapter4-krrb.md.
Реализуй КРРБ/УК, выбор места, реплики и переход в вечер.
```

### 10
```text
MaratGame: выполни только docs/post-mvp/step-10-evening-big-congratulation.md.
Сделай вечер, кнопку "А что дальше?" и большое поздравление по условию общения.
```

### 11
```text
MaratGame: выполни только docs/post-mvp/step-11-monologue-and-ui-states.md.
Добавь отдельный UI-режим для монологов, не ломая обычный диалоговый режим.
```

### 12
```text
MaratGame: выполни только docs/post-mvp/step-12-visual-and-media-pass.md.
Подключи фоны, портреты, gif/video по MediaSlot и locationId.
```

### 13
```text
MaratGame: выполни только docs/post-mvp/step-13-full-day-agent-smoke.md.
Собери agent smoke полного дня через AgentPlayBridge и Unity MCP.
```

### 14
```text
MaratGame: выполни только docs/post-mvp/step-14-release-pc-web-update.md.
Финальный релиз: Windows x64 + обновление WebGL/GitHub Pages.
```

## Статус

- [ ] 01 Full Day Bridge
- [ ] 02 Engine Stats and Media
- [ ] 03 Morning Full Branches
- [ ] 04 Chapter 2 Cabinet
- [ ] 05 Chapter 2 Toilet
- [ ] 06 Mini-Story Framework
- [ ] 07 Mini-Story Help Employee
- [ ] 08 Chapter 3 Planerka
- [ ] 09 Chapter 4 KRRB
- [ ] 10 Evening Big Congratulation
- [ ] 11 Monologue and UI States
- [ ] 12 Visual and Media Pass
- [ ] 13 Full Day Agent Smoke
- [ ] 14 Release PC and Web Update
