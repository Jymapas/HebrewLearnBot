# AGENTS.md — конвенции репозитория

Описывает то, что есть в коде. Не описывай здесь несуществующее: если фича не реализована — её здесь быть не должно.

## Что это

Телеграм-бот для заучивания слов и алфавита иврита: квиз с четырьмя вариантами ответа на инлайн-кнопках, подсветка правильного/неправильного варианта, импорт словаря из CSV. Второй, независимый квиз — по алфавиту (буква → название), с тем же движком подбора вопросов и собственным прогрессом. Пользовательские сценарии — в [README.md](README.md).

## Стек

.NET 10 (`net10.0`), Telegram.Bot 22.10.2.1, PostgreSQL + EF Core 9 (Npgsql), Serilog, NUnit 4. Long polling, не webhook. Версии пакетов — только в `Directory.Packages.props` (central package management), в `.csproj` версии не указываются.

**Telegram.Bot не ниже 22.10.2.1**: подсветка кнопок использует `InlineKeyboardButton.Style` (`KeyboardButtonStyle.Success` / `Danger`), появившийся в Bot API 9.4. В более старых версиях этого свойства нет.

## Архитектура

Четыре слоя, зависимости строго в одну сторону:

```
Domain  ←  Application  ←  Infrastructure  ←  Worker
   ↑___________________________________________|
```

- **`Domain`** — сущности, enum'ы, `DomainException`, работа с ивритским текстом. Зависимостей нет вообще.
- **`Application`** — порты (`Abstractions/`), DTO, чистые алгоритмы квиза (`Quiz/`), форматирование сообщений (`Formatting/`), парсер CSV (`Import/`), use case'ы. Не знает ни про EF, ни про Telegram.
- **`Infrastructure`** — EF Core, конфигурации, репозитории, миграции, реализации портов.
- **`Worker`** (`Microsoft.NET.Sdk.Web`, `RootNamespace=HebrewLearnBot`) — точка входа, DI, всё, что касается Telegram API.

Типы Telegram (`Message`, `Update`, `InlineKeyboardMarkup`) не должны просачиваться за пределы `Worker`.

## Стиль кода

- file-scoped namespaces; `sealed` практически на каждом классе; `public static class` для констант, хелперов и клавиатур;
- **primary constructors не используются** — классические конструкторы с полями `_camelCase`;
- аллманов стиль скобок, отступ 4 пробела, фигурные скобки даже для однострочных `if`;
- XML-документации нет; комментарии `//` по-русски и только там, где логика неочевидна;
- `CancellationToken cancellationToken` — последний параметр, **без `= default`** (исключение — extension-обёртки над Telegram API);
- возвращаемые коллекции — `IReadOnlyList<T>`;
- асинхронные методы всегда `*Async`.

Правила отступов, именования и запрет primary constructors закреплены в `.editorconfig`.

## Домен

Сущности — rich model: свойства `{ get; private set; }`, приватный parameterless-конструктор для EF, валидация в конструкторе через `DomainException`, мутации только явными методами.

**Сообщения `DomainException` — по-английски** («Hebrew spelling is required.»). Тексты для пользователя — по-русски. Логи `ILogger<T>` — структурированные, шаблоны по-английски.

**Время в домен передаётся параметром** (`DateTime createdAtUtc` / `updatedAtUtc`). `DateTime.UtcNow` внутри `Domain` и `Application` запрещён — только через порт `IClock`. Случайность — только через `IRandomSource`. Оба порта подменяются в тестах.

## База данных

Шесть таблиц: `topics`, `words`, `word_progress`, `user_settings`, `conversation_states`, `letter_progress`.

Алфавит иврита не хранится в БД — это статический список `HebrewAlphabet.All` (`Domain/Alphabet`) со стабильными `Id`. `letter_progress` ссылается на эти `Id` без внешнего ключа (буквы — не таблица).

Только Fluent API, один `IEntityTypeConfiguration<T>` на сущность, имена колонок задаются вручную через `.HasColumnName(...)` в snake_case, таблицы во множественном числе. Атрибуты маппинга не используются.

Миграции применяются одним `await dbContext.Database.MigrateAsync()` при старте. Миграции пишутся только генерацией:

```bash
dotnet ef migrations add <Name> \
  --project src/HebrewLearnBot.Infrastructure \
  --startup-project src/HebrewLearnBot.Infrastructure \
  --output-dir Migrations
```

Startup-проект здесь — сама `Infrastructure`, а не `Worker`: `dotnet ef` требует пакет `Microsoft.EntityFrameworkCore.Design` в startup-проекте, а он подключён только к `Infrastructure`. Подключение к БД для дизайн-тайма даёт `HebrewDbContextFactory`, поэтому `Worker` для генерации не нужен.

## Состояние диалога

FSM хранится в `conversation_states`, PK — `user_id` (одна активная беседа на пользователя). Имена бесед и шаги — строковые константы в `ConversationNames` / `QuizSteps` / `ImportSteps`, не enum'ы. Данные шага сериализуются в `state_json` (camelCase).

Настройки пользователя (режим, тема) живут в `user_settings`, **вне** FSM: поход в настройки не должен ломать активную тренировку.

## Telegram-слой

- Навигация — `ReplyKeyboardMarkup` (клавиатура вместо обычной), внутри квиза — инлайн-кнопки.
- **`ParseMode` не используется.** Весь текст plain, оформление — эмодзи и переводы строк. Превью ссылок отключается через `BotClientExtensions.SendNoPreview`.
- `AnswerCallbackQuery` вызывается **на любой ветке** обработки callback — иначе у пользователя крутится спиннер до таймаута.
- Правки сообщений (`EditMessageText`) — в `try/catch (ApiRequestException)` с `LogWarning`: сообщение могли удалить, а окно редактирования у Telegram 48 часов.
- `callback_data` ограничен **64 байтами**, поэтому темы адресуются числовым id, а не названием. Формат префиксов — в `CallbackData`.
- Кнопку нельзя обезвредить, убрав `callback_data`: Bot API требует ровно одно поле-«тип» помимо `Text`, `IconCustomEmojiId` и `Style`. Вместо этого подставляется инертный токен `CallbackData.Noop`.

### RTL — обязательно

Telegram определяет направление абзаца по первому сильному символу, из-за чего строка с ивритом разворачивается, скобки зеркалятся, а нейтральные символы прыгают. Поэтому:

- каждая строка сообщения, которая может содержать иврит, проходит через `BidiText.Line(...)`;
- каждый ивритский фрагмент внутри русской строки — через `BidiText.Isolate(...)`;
- все подписи инлайн-кнопок — через `BidiText.ForButton(...)`, даже чисто русские.

Используется только LRM (`U+200E`), не изоляты FSI/PDI: последние на старых клиентах рисуются видимыми квадратиками.

## Тесты

NUnit 4, **без мок-фреймворков** — ручные `FakeXxxRepository`, `TestClock`, `SequenceRandomSource` в `tests/HebrewLearnBot.Tests/Fakes/`. Логгеры — `NullLogger<T>.Instance`.

Имена тестов — `Method_Condition_ExpectedResult`, констрейнт-модель `Assert.That(x, Is.EqualTo(y))`, AAA без комментариев-разделителей. Статистические тесты используют фиксированный seed и не должны быть флаки.

## Конфигурация

Только переменные окружения из `secrets.env` (в `.gitignore`), шаблон — `secrets.env.example`. Читается через `DotNetEnv` в `Program.cs`, разбирается в `AppOptions.FromConfiguration` с `throw new InvalidOperationException("X is required.")`.

Токен бота — `BOT_TOKEN`, главный админ — `MAIN_ADMIN_ID` (одно число). `IOptions<T>` не используется — `AppOptions` регистрируется синглтоном напрямую.

DI — плоским списком в `Program.cs`, без `AddApplication()`/`AddInfrastructure()`-экстеншенов.

## Чего здесь сознательно нет

MediatR, AutoMapper, FluentValidation, Result-типов, Unit of Work, Quartz, webhook, режима «Микс», таблиц истории сессий и аудита импорта. Не добавляй их без явного запроса.
