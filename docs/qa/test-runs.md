# Test runs

Журнал прогонов (ручных и CI). Добавляйте строку при значимой проверке фичи/hotfix.

| Дата | Feature / Hotfix | Среда | Ветка / коммит | Результат | Примечание |
|------|------------------|-------|----------------|-----------|------------|
| 2026-05-18 | bootstrap | docs-only | main | N/A | existing-repo-bootstrap: автозаполнение docs по evidence; прогоны с первой фичи |
| 2026-08-23 | api-v1-5-upgrade | локально, .NET 9 (dotnet test/build) | feature/api-v1-5-upgrade @ 8d11c10 | **pass** | QA-валидация миграции v1.4→v1.5: 160/160 тестов зелёные; grep — `/v1.4` в коде нет; пути `/v1.5/movie/search`, `/v1.5/movie/{id}` покрыты assertion'ами; фильтр года по ADR-0001 покрыт юнит-тестами; маппинг не изменён (дифф); warnings сборки = baseline (61× CS1591). Playwright неприменим (backend/.NET, без UI). Ограничение: smoke с реальным API-ключом не выполнялся (ключа нет) — риск strict-валидации `&year=` открыт, план изоляции в ADR-0001. Детали: [qa.md](../backlog/features/api-v1-5-upgrade/qa.md) |
