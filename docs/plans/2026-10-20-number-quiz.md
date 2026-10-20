# План реализации: изучение чисел

Дизайн: docs/designs/2026-10-20-number-quiz.md

## Фаза 1 — контракты и логика

- [x] Domain: каталог, режимы, прогресс.
- [x] Application: порт, вопросы, ввод, статистика и запись ответов.
- [x] NUnit: каталог и алгоритмы, ручной fake.

## Фаза 2 — адаптеры (параллельные задачи)

- [x] Worker: самостоятельный обработчик чисел, меню, callback, текстовый ввод, DI, статистика.
- [x] Infrastructure: number_progress, репозиторий, генерация EF-миграции.
- [x] README и AGENTS: только реализованное поведение.

Worker и Infrastructure выполняются разными агентами, каждый редактирует только свой слой; общий Program.cs принадлежит Worker-задаче. Миграция генерируется после готовности Domain.

## Фаза 3 — проверка

- [x] Release build/test и исправление найденных проблем.
- [x] Проверка callback и сценариев завершения/пропуска.

## Фаза 4 — Security checkpoint

- [x] security-review диффа и проверка secure-coding: актуальность токенов, принадлежность сообщений, границы ввода, отсутствие секретов в логах.

## Результаты проверки

- `dotnet build HebrewLearnBot.sln -c Release --no-restore`: 0 предупреждений, 0 ошибок.
- `dotnet test HebrewLearnBot.sln -c Release --no-restore`: 268 пройдено, 0 ошибок, 0 пропусков (включая 65 новых тестов).
- `dotnet ef migrations has-pending-model-changes --project src/HebrewLearnBot.Infrastructure --startup-project src/HebrewLearnBot.Infrastructure`: модель соответствует миграциям.
- `git diff --check`: ошибок нет.
- Повторное ревью: замечания по устаревшему вводу/повторному пропуску и неограниченному накоплению ошибок исправлены; новых замечаний нет.

Telegram API проверен через ручной fake HTTP-транспорт. Живой бот и применение миграции к PostgreSQL не запускались; локальный Docker daemon недоступен (`docker.sock` отсутствует).
