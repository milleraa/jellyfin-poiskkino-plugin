# Existing repo bootstrap

## Когда применять

Используй для репозитория, где уже есть код, конфиги, CI, тесты или существующая документация. Если репозиторий пустой или только что скопирован из шаблона без кода, используй `.agents/skills/sdlc/references/project-bootstrap.md`.

## Главный принцип

Сначала **инвентаризация и автозаполнение по evidence**, потом вопросы Стейхолдеру. Не спрашивай то, что можно достоверно вывести из репозитория.

## Evidence-first workflow

1. Составь карту репозитория:
   - solution и проекты (`*.slnx`, `*.csproj`): SDK, target framework, NuGet и команды сборки/тестов;
   - GitHub Actions (`.github/workflows/`), упаковка релиза и инструкции установки;
   - тестовые проекты, сценарии ручной проверки, внешние интеграции и настройки.
2. Изучи существующие docs (`README*`, `docs/**`, ADR, OpenAPI, runbooks), не перетирай полезный контент.
3. Заполни docs-манифест из `.agents/skills/sdlc/references/project-bootstrap.md` тем, что подтверждено evidence.
4. Для каждого автозаполненного раздела добавляй краткую ссылку на источник: файл, config, script, CI job или README.
5. Составь список gaps: что нельзя вывести из репозитория (владельцы, доступы, protected branches, secret storage, SLA, продуктовые приоритеты, кто принимает PR/MR).
6. Только после автозаполнения задай Стейхолдеру вопросы по gaps, сгруппировав их пакетами.
7. После ответов доведи манифест до DoBootstrap: факт, evidence или явное `N/A`; без «немых» TODO / `_TBD_`.

Если менялись файлы, сделай локальный commit без push.

## Что заполнить автоматически

Заполняй те же файлы, что и `project-bootstrap`, но порядок источников другой:

- `docs/product/vision.md`, `stakeholders.md`, `requirements.md`: из README, package metadata, API docs, docs, названий сервисов; вопросы только по бизнес-целям и owner.
- `docs/environments/*`: из Git remotes, GitHub Actions и инструкций установки; вопросы только по доступам, protected branches, secret storage и реальным URL, если их нет в репо.
- `docs/engineering/stack-notes.md` и `docs/engineering/stacks/*.md`: из файлов проекта, lockfiles, tool configs, test scripts.
- `docs/engineering/devops.md`: из CI, релизного workflow и инструкций установки; если артефактов нет, фиксируй `N/A` с evidence `не найдено`.
- `docs/architecture/overview.md`: из структуры плагина, API и интеграций.
- `docs/backlog/epics.md`: не выдумывай roadmap; если есть issue docs / roadmap / TODO docs — перенеси как черновик, иначе отметь `Epics не восстановлены из кода`.
- `docs/qa/test-strategy.md`, `jellyfin-smoke.md`, `test-runs.md`: из test folders, scripts, CI jobs.

## Формат вопросов по gaps

После автозаполнения задай короткий пакет:

```markdown
Я заполнил docs по evidence из репозитория. Остались gaps:
1. Product owner / приёмка PR/MR: ...
2. Git hosting / protected branches: ...
3. Secrets / access policy: ...
4. Environments URLs: ...
5. Неподтверждённые NFR / SLA: ...
```

Не задавай больше 8–10 вопросов за один пакет. Если gaps много, начни с блокеров для `docs/environments/` и delivery.

## Запреты

- Не коммить секреты и не копируй значения из `.env`, private keys, connection strings с паролями.
- Не удаляй существующие docs; если шаблон конфликтует с реальными docs, сохрани факты и добавь ссылки.
- Не начинай рефакторинг кода во время bootstrap; допустимы только docs и, по явному согласию, безвредные metadata-файлы.
- Не создавай новую feature/hotfix до завершения инвентаризации, если Стейхолдер явно не попросил начать работу.
