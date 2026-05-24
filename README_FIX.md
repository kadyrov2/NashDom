# Исправление ошибки: "An exception has been raised that is likely due to a transient failure"

## Проблема
Ошибка возникает при попытке подключения к базе данных PostgreSQL. Это временная ошибка подключения (transient failure), которая обычно вызвана следующими причинами:

1. Сервер PostgreSQL не запущен
2. Неправильные параметры подключения
3. Сетевые проблемы
4. База данных ещё не готова принимать подключения

## Решение

### 1. Добавлена политика повторных попыток (Retry Policy)
В файле `Data/ApplicationDbContext.cs` добавлена конфигурация автоматических повторных попыток подключения:

```csharp
optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
{
    // Добавляем политику повторных попыток для временных ошибок
    npgsqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorCodesToAdd: null);
});
```

Это позволяет автоматически повторять подключение до 5 раз с задержкой до 30 секунд при временных ошибках.

### 2. Улучшена обработка ошибок в MainViewModel
В файле `ViewModels/MainViewModel.cs` улучшен блок обработки ошибок:

- Проверяется наличие внутренней ошибки (InnerException)
- Определяются временные ошибки подключения по ключевым словам
- Пользователю показывается подробное сообщение с возможными причинами и инструкциями

### 3. Запуск PostgreSQL

Для запуска базы данных выполните команду в терминале:

```bash
docker-compose up -d
```

Проверить статус контейнера:

```bash
docker-compose ps
```

Просмотреть логи PostgreSQL:

```bash
docker-compose logs postgres
```

## Параметры подключения

По умолчанию используются следующие параметры (указаны в `ApplicationDbContext.cs`):

- Host: localhost
- Port: 5432
- Database: NashDomDB
- Username: postgres
- Password: postgres123

Если параметры отличаются, измените строку подключения в файле `Data/ApplicationDbContext.cs`.

## Проверка работы

После внесения изменений:

1. Убедитесь, что Docker запущен
2. Выполните `docker-compose up -d`
3. Дождитесь готовности PostgreSQL (10-15 секунд)
4. Запустите приложение

