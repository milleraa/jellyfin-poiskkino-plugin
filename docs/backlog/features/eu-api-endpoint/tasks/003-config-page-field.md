# Task 003 — Поле «Базовый URL API» в `Configuration/configPage.html` (AC-6)

## Метаданные

- **ID:** 003
- **Feature / Hotfix:** [docs/backlog/features/eu-api-endpoint/](../)
- **Назначено:** `developer-csharp`
- **Стек / infra-область:** HTML + JS страницы конфигурации плагина Jellyfin (embedded resource `Configuration/configPage.html`); C# не меняется
- **Статус:** todo
- **Ветка:** `feature/eu-api-endpoint` (worktree `/home/alex/src/my/jellyfin-metadata-plugin`); выполнять строго после задачи [002](002-configuration-client-providers.md), task-branch не создавать

## Описание

Добавить на страницу настроек поле базового URL API, которое загружает и сохраняет `PluginConfiguration.ApiBaseUrl` ([architecture.md](../architecture.md) §2, §7; решение Стейхолдера Q1 — свободное поле).

Файл — [`PoiskKinoMetadataPlugin/Configuration/configPage.html`](../../../../../PoiskKinoMetadataPlugin/Configuration/configPage.html) (87 строк, паттерны `apiKey`/`ignoreTmdbImages`).

1. **Разметка** — новый `div.inputContainer` между блоком `apiKey` и чекбоксом `ignoreTmdbImages`:
   - `<label class="inputLabel inputLabelUnfocused" for="apiBaseUrl">Базовый URL API</label>`;
   - `<input id="apiBaseUrl" name="apiBaseUrl" type="text" is="emby-input" />` (именно `type="text"`, не `type="url"` — браузерная валидация не должна блокировать ввод без схемы, нормализация серверная);
   - `<div class="fieldDescription">` с подсказкой на русском: по умолчанию `https://api.poiskkino.dev`; если сервер Jellyfin находится вне РФ или основной адрес недоступен из вашей сети — укажите `https://eu-api.poiskkino.dev` (токен и методы те же); схему можно не указывать, завершающий `/` не нужен, путь после хоста не поддерживается; **изменение применяется после перезапуска Jellyfin**;
   - опционально (не обязательно, не блокирует приёмку) — `<datalist id="apiBaseUrlOptions">` с двумя значениями и `list="apiBaseUrlOptions"` на input; список нативный, логику не ветвит и ввод не ограничивает.

2. **`loadConfiguration`** — после обработки `ApiKey`:
   ```js
   document.getElementById('apiBaseUrl').value =
       (config && config.ApiBaseUrl) ? config.ApiBaseUrl : 'https://api.poiskkino.dev';
   ```
   Значение с сервера показывается **как есть** (канонизация в UI не выполняется, D-ARCH-4); при отсутствии/пустом — подставляется дефолт (AC-6).

3. **`saveConfiguration`** — в существующем блоке сохранения:
   ```js
   config.ApiBaseUrl = document.getElementById('apiBaseUrl').value;
   ```
   Без `trim()`, без добавления схемы, без удаления `/` — сырое значение сохраняется, нормализация выполняется резолвером при создании клиента (D-ARCH-4). `ApiKey`/`IgnoreTmdbImages` и `pluginUniqueId` не трогать.

4. **Не делать:** JS-валидацию формата URL, whitelist хостов, предупреждения «неверный URL» (AC-4 закрывается серверным fallback + `Warning` в лог), вызовы `GET /v1.5/token` из UI (architecture.md §5 — вне scope).

## Acceptance criteria

1. **AC-6:** в `configPage.html` есть поле базового URL с `id`/`name` `apiBaseUrl`, связанный `<label>`, и подсказка на русском, содержащая оба хоста (`api.poiskkino.dev`, `eu-api.poiskkino.dev`) и пояснение про сервер вне РФ.
2. **AC-6 (загрузка):** при сохранённом значении поле показывает его; при отсутствии элемента (`undefined`) или пустой строке — подставляет `https://api.poiskkino.dev` (не пустое поле).
3. **AC-6 (сохранение):** значение уходит в `updatePluginConfiguration` вместе с `ApiKey` и `IgnoreTmdbImages`; остальные поля и `Dashboard.processPluginConfigurationUpdateResult` работают как раньше.
4. **AC-3 (не терять ввод):** JS не нормализует и не обрезает значение — в конфигурацию попадает ровно то, что ввёл администратор (D-ARCH-4); при пустом вводе сохраняется пустая строка, и клиент уходит на дефолт без `Warning` (правило 1 резолвера).
5. **AC-6 (подсказка про перезапуск):** текст подсказки содержит требование перезапуска Jellyfin после смены адреса (согласовано с AC-7′ и задачей 004).
6. Разметка соответствует текущей странице: `is="emby-input"`, `inputContainer`, `inputLabel inputLabelUnfocused`, `fieldDescription`; `<html lang="ru">` и `pluginUniqueId` (`A1B2C3D4-E5F6-7890-ABCD-EF1234567890`) не изменены.
7. Ручной сценарий проверки описан в handoff и передан QA: открыть Dashboard → Plugins → PoiskKino Metadata → сохранить значение `https://eu-api.poiskkino.dev` → перезагрузить страницу настроек → значение на месте; затем очистить поле → сохранить → перезагрузить → подставлен дефолт.
8. Локальный commit без push.

## Технические заметки

- Зависимость: задача 002 принята (имя свойства `ApiBaseUrl` в C# сериализуется Jellyfin как `ApiBaseUrl`; в JS — та же camel/Pascal-форма, как у `ApiKey`).
- Страница — embedded resource (`PoiskKinoMetadataPlugin.csproj`); для проверки в живом Jellyfin нужна пересборка DLL, локального стенда Jellyfin у разработчика может не быть → AC-6 подтверждается статическим ревью + сценарием для QA (Оркестратор запускает QA после приёмки).
- Дефолт в JS дублирует строку только для отображения при отсутствующем значении; нормализацией это не является и на серверное поведение не влияет.
- `type="text"` обязателен: `type="url"` заблокирует ввод без схемы на уровне браузера и сломает AC-3 (нормализация «добавить `https://`» на сервере).
- Запрещено: менять `pluginUniqueId`, добавлять новые JS-библиотеки/внешние CDN, тянуть нормализацию в JS.

## DevOps impact check

- Новые/изменённые переменные окружения или secrets: **нет**
- Новые внешние интеграции, порты, очереди, storage, scheduled jobs: **нет** (страница ничего не запрашивает по сети, кроме существующего `ApiClient` Jellyfin)
- Docker/compose/CI/CD/deploy/Helm/Kubernetes/observability: **нет**
- Итог: DevOps-задача **не нужна**.

## Проверка

```bash
dotnet build PoiskKinoMetadataPlugin.slnx      # страница — embedded resource, сборка должна остаться зелёной
dotnet test PoiskKinoMetadataPlugin.slnx       # регрессий нет
```

Статическая проверка: `<label for="apiBaseUrl">` совпадает с `id`; в `loadConfiguration`/`saveConfiguration` поле читается/пишется; в тексте подсказки есть оба хоста и фраза о перезапуске.

Ручной smoke (QA/администратор, живой Jellyfin): сценарий из AC-7, плюс проверка, что старая конфигурация без поля показывает дефолт.

## Handoff

- После выполнения: локальный commit без push (например, `feat(config): add API base URL field to plugin configuration page`), статус задачи → `in review`, сообщить Техлиду: фрагмент разметки, изменения в `loadConfiguration`/`saveConfiguration`, сценарий ручной проверки для QA.
