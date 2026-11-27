# Brief — `eu-api-endpoint`

Дата: 2026-11-27. Автор: Стейхолдер (постановка), зафиксировано агентом `build`.

## Цель

Сделать базовый URL API ПоискКино настраиваемым, чтобы пользователи Jellyfin-серверов за пределами РФ могли указать зеркало `https://eu-api.poiskkino.dev` через UI плагина.

## Фактура (проверено)

| Факт | Evidence |
|------|----------|
| Хост зашит константой | `PoiskKinoApiClient.cs:15` — `private const string ApiBaseUrl = "https://api.poiskkino.dev";`, применяется в конструкторе: `_httpClient.BaseAddress = new Uri(ApiBaseUrl);` |
| Зеркало объявлено в спеке | `api-docs/documentation.yaml` → `servers: [https://api.poiskkino.dev, https://eu-api.poiskkino.dev]` (снимок 2026-11-27) |
| Условие использования зеркала | [llms.txt](https://api.poiskkino.dev/llms.txt): «Если запросы отправляются с сервера за границей, используйте адрес `https://eu-api.poiskkino.dev` — токен и методы те же». **Уточнение Аналитика (2026-11-27):** на текущей версии `llms.txt` (v1.4.1) этой формулировки нет — воспроизвести не удалось; факт существования зеркала опирается на `servers` спеки и пробы 401 (см. [analysis.md](analysis.md#1-фактура-проверено), F9–F11, риск R6) |
| Зеркало живое и требует тот же токен | `GET https://eu-api.poiskkino.dev/v1.5/token` без ключа → `401` (аутентификация до роутинга) |
| Эндпоинты плагина не меняются | Используются относительные пути: `/v1.5/movie/search` (`:81`), `/v1.5/movie/{id}` (`:202`), `/v1.5/season` (`:320`) — работают от любого base URL |
| Конфигурация уже расширяемая | `PluginConfiguration : BasePluginConfiguration` (`ApiKey`, `IgnoreTmdbImages`), UI — `Configuration/configPage.html` |
| Вызовы API собраны в одном месте | Все `SendAsync` — только в `PoiskKinoApiClient.cs` (`:91`, `:207`, `:325`) |

## Scope (in)

- Новое поле конфигурации с базовым URL API (значение по умолчанию — `https://api.poiskkino.dev`).
- UI-элемент в `Configuration/configPage.html` с сохранением/загрузкой значения (по образцу существующих полей).
- Валидация и нормализация значения (пустое → дефолт; обязательный `https`; усечение завершающего `/`; отклонение недекодируемого URL — без падения плагина).
- Применение значения в `PoiskKinoApiClient` вместо `const string ApiBaseUrl`.
- Unit-тесты: дефолт при пустой конфигурации, кастомное значение, нормализация, обработка невалидного значения.
- Обновление документации: `README.md`, `docs/architecture/overview.md` (если описан хост API).

## Out of scope

- Автоматический выбор зеркала по геолокации/пингу или failover между `api.` и `eu-api.` при сетевой ошибке.
- Прокси, самописные зеркала и произвольные HTTP-заголовки.
- Per-provider (movie/series/season) настройки URL.
- Ретраи, circuit breaker, изменение таймаутов (`Timeout = 120s` остаётся).
- Новые эндпоинты API (`/v1.5/box-office`, `/v1.5/view-option`) — отдельная работа.

## Открытые вопросы (для Аналитика на `analysis`)

- OQ-1: UI — свободное поле ввода URL или выбор из двух предустановленных значений + «другое»? Свободное поле проще, но повышает риск опечаток.
- OQ-2: нужен ли fallback на `api.poiskkino.dev` при сетевой ошибке к кастомному URL (сейчас — out of scope, подтвердить).
- OQ-3: считать ли изменение hotfix (пользователи вне РФ не могут работать) или enhancement-фичей; влияет на скорость потока.
- OQ-4: миграция существующих конфигураций (у `BasePluginConfiguration` поле новое — дефолт применяется автоматически; проверить сериализацию Jellyfin).

## Стейкхолдеры

См. [stakeholders](../../../product/stakeholders.md).
