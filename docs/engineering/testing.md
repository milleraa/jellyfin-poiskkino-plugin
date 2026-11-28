# Тестирование

- Автоматические тесты: `PoiskKinoMetadataPlugin.UnitTests` на xUnit и Moq. Из корня репозитория запускайте `dotnet test`. GitHub Actions собирает плагин в Release и запускает тесты.
- Изменения настроек и поведения провайдеров проверяйте по [ручному smoke-чеклисту Jellyfin](../qa/jellyfin-smoke.md).
- При написании и ревью тестов используйте [`test-audit`](../../.agents/skills/test-audit/SKILL.md). Результаты QA фиксируйте в [журнале прогонов](../qa/test-runs.md).
- Подробности: [стратегия тестирования](../qa/test-strategy.md), [стек .NET](stacks/dotnet.md), [документация .NET testing](https://learn.microsoft.com/dotnet/core/testing/).
