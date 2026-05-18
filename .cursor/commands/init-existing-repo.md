# Init existing repository (adopt repo with code)

Ты **Оркестратор**. Репозиторий уже содержит код, конфиги, тесты, CI/CD или существующие docs. Нужно подключить agile agent team template к текущему проекту без потери фактов.

## Что сделать

1. Выполни навык **`@.cursor/skills/existing-repo-bootstrap/SKILL.md`**.
2. Сначала изучи существующий код, конфиги и docs; не задавай вопросы по тому, что можно достоверно вывести из репозитория.
3. Автоматически заполни docs-манифест из `project-bootstrap` по найденному evidence: продукт, стеки, архитектура, Git/CI/CD, DevOps, QA.
4. Для каждого заполненного раздела укажи источник: README, config, script, CI job, Docker/compose/Helm/K8s, test folder, migration или другой файл.
5. После автозаполнения задай Стейхолдеру только вопросы по gaps: доступы, protected branches, secret storage, реальные URL окружений, ownership, SLA/NFR, политика приёмки.
6. Не трогай прикладной код и не удаляй существующие docs во время bootstrap.
7. Локальный commit без push, если менял файлы.
