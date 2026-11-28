# Feature — `eu-api-endpoint`

## Кратко

Настраиваемый базовый URL API ПоискКино: дать возможность переключить `https://api.poiskkino.dev` на зеркало `https://eu-api.poiskkino.dev` (и произвольный URL) через конфигурацию плагина, не пересобирая плагин.

## Зачем

Живая спецификация API (`servers` в [api-docs/documentation.yaml](../../../../api-docs/documentation.yaml)) и [llms.txt](https://api.poiskkino.dev/llms.txt) с 2026-11 объявляют второй базовый адрес `https://eu-api.poiskkino.dev` — «если запросы отправляются с сервера за границей, используйте адрес `https://eu-api.poiskkino.dev`, токен и методы те же». Сейчас хост зашит константой в коде, поэтому пользователи Jellyfin вне РФ (или с блокировкой маршрута) не могут воспользоваться зеркалом без форка.

## Статус

Живой статус стадии и owner — только в [status.md](status.md) (шаблонные указания стадии в этом README не дублируются).

## Файлы

| Файл | Назначение |
|------|------------|
| [brief.md](brief.md) | Цель и scope от Стейхолдера |
| [status.md](status.md) | **Стадия процесса и отчёты ролей** |
| [analysis.md](analysis.md) | Аналитика, истории, AC (Аналитик) |
| [architecture.md](architecture.md) | Дизайн (Архитектор) |
| [tech-plan.md](tech-plan.md) | План Техлида |
| [tasks/](tasks/README.md) | Задачи разработчиков |
| [qa.md](qa.md) | Результаты QA |
| [finalization.md](finalization.md) | PR/MR delivery, acceptance/archive |

## Ссылки

- Фактура по изменению API: [api-docs/AGENTS.md](../../../../api-docs/AGENTS.md)
- Код: [`PoiskKinoApiClient.cs`](../../../../PoiskKinoMetadataPlugin/PoiskKinoApiClient.cs), [`Configuration.cs`](../../../../PoiskKinoMetadataPlugin/Configuration.cs), [`Configuration/configPage.html`](../../../../PoiskKinoMetadataPlugin/Configuration/configPage.html)
