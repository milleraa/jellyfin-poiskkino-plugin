# Decisions log

Краткий журнал со ссылками на ADR. Подробности только в ADR.

| Дата | ADR | Резюме |
|------|-----|--------|
| 2026-05-18 | — | existing-repo-bootstrap: docs заполнены по evidence из README, csproj, git remote |
| 2026-05-18 | — | Gaps закрыты: единственный maintainer/Стейхолдер; нет protected branches; dev только локально; CI — план GitHub Actions (DLL); фичи в backlog не заводим |
| 2026-08-23 | — | Миграция шаблона агентов Cursor → OpenCode (`.opencode/`); восстановлены случайно затёртые README.md и заполненные docs по evidence |
| 2026-08-23 | — | Docs актуализированы под репо: unit-тесты (xUnit+Moq), CI/Release workflows, переименование `Api-docs` → `api-docs`, solution `.slnx` |
| 2026-08-23 | — | Gaps закрыты: репозиторий публичный; приёмка подтверждена (единственный maintainer `milleraa`, PR → `main`, без protected branches); политика версий Jellyfin — последняя стабильная (10.11.x); провайдер персон отложен («пока не делаем»); покрытие тестами — по мере изменения кода |

## Как добавлять

1. Создайте `docs/architecture/decisions/NNNN-title.md` по шаблону.
2. Добавьте строку сюда.
3. Укажите ссылку в `architecture.md` фичи при необходимости.
