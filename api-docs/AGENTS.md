Read https://api.poiskkino.dev/llms.txt first

## Снапшот спецификации

- `documentation.yaml` — снимок живой спеки с https://api.poiskkino.dev/documentation-yaml (<https://api.poiskkino.dev/documentation-json> — то же в JSON).
- Обновлён: 2026-11-27 (предыдущий снимок — 2026-08-23).
- Что изменилось при обновлении: удалены **все** пути `/v1.4/*` и deprecated `/v1.5/review`, `/v1/movie/possible-values-by-field`; добавлены `/v1.5/box-office` и `/v1.5/view-option`; в `servers` появился `https://eu-api.poiskkino.dev` (тот же токен и методы — для серверов за пределами РФ).
- Пути, которые использует плагин (`/v1.5/movie/search`, `/v1.5/movie/{id}`, `/v1.5/season`), и их схемы ответов при обновлении не изменились.
- Локальный файл — для оффлайн-анализа кода и тестов; при расхождении источником истины считается живая спека.
