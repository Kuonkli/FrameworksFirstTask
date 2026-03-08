# TodoList

**TodoList** – пример веб-службы на ASP.NET Core с конвейером middleware для управления списком задач.

---

## Требования
- .NET 8 SDK

## Сборка и запуск

```bash
git clone <url-репозитория>
cd TodoList
dotnet run
```

Сервер запустится на `http://localhost:5041` (порт может отличаться).

---

## Примеры запросов (PowerShell)

### Создать задачу
```powershell
$body = @{
    title = "Сдать лабораторную работу"
    description = "Реализовать Todo-лист на C#"
    priority = 2
    dueDate = "2026-03-15T18:00:00Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5041/api/todos" -Method Post -Body $body -ContentType "application/json"
```

### Получить все задачи
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos" -Method Get
```

### Получить задачу по ID
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos/ваш-guid" -Method Get
```

### Обновить задачу
```powershell
$body = @{
    title = "Обновлённая задача"
    description = "Новое описание"
    priority = 3
    isCompleted = $true
    dueDate = "2026-03-20T12:00:00Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5041/api/todos/ваш-guid" -Method Put -Body $body -ContentType "application/json"
```

### Переключить статус
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos/ваш-guid/toggle" -Method Patch
```

### Удалить задачу
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos/ваш-guid" -Method Delete
```

### Фильтрация
```powershell
# Только невыполненные
Invoke-RestMethod -Uri "http://localhost:5041/api/todos?completed=false" -Method Get

# Поиск по тексту
Invoke-RestMethod -Uri "http://localhost:5041/api/todos?search=лабораторную" -Method Get
```

### Статистика
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos/stats" -Method Get
```

### Ошибочные запросы
```powershell
# Пустой заголовок (400 Bad Request)
$body = @{ title = ""; description = "Описание"; priority = 1 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5041/api/todos" -Method Post -Body $body -ContentType "application/json"

# Несуществующий ID (404 Not Found)
Invoke-RestMethod -Uri "http://localhost:5041/api/todos/11111111-1111-1111-1111-111111111111" -Method Get
```

### Передача своего RequestId
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos" -Method Get -Headers @{ "X-Request-Id" = "my-test-123" }
```

---

## Ожидаемые результаты

- **Успешные запросы**: 200 OK, 201 Created (с Location), 204 No Content
- **Ошибочные запросы**: JSON с полями `Code`, `Message`, `RequestId`, `Timestamp` и статусами 400/404/500
- **В консоли** – логи с requestId, методом, путём, статусом и временем

---

## 1. Пояснение архитектуры

**Компоненты:**

- **Middleware**:
    - `RequestIdMiddleware` – обеспечивает сквозной идентификатор запроса (из заголовка или генерирует)
    - `ErrorHandlingMiddleware` – перехватывает исключения, возвращает единый JSON-ответ
    - `TimingAndLogMiddleware` – измеряет время выполнения, пишет логи

- **Слой сервисов**: репозиторий `InMemoryTodoRepository` (с защитой от конфликтов через `lock`), `ValidationService`

- **Слой моделей**: `TodoItem`, `CreateTodoRequest`, `UpdateTodoRequest`, `ErrorResponse`

- **Слой ошибок**: иерархия `DomainException` → `NotFoundException`, `ValidationException`

**Преимущества подхода:**
- Переиспользуемость middleware
- Тестируемость компонентов
- Единый формат ошибок
- Сквозная трассировка через requestId

---

## 2. Сценарии проверки

### Успешное создание задачи
```powershell
$body = @{ title = "Купить продукты"; description = "Молоко, хлеб"; priority = 1 } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:5041/api/todos" -Method Post -Body $body -ContentType "application/json"
$response.id # сохранить для следующих тестов
```
201 Created, задача с id, `isCompleted = false`

### Получение по ID
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos/$id" -Method Get
```
200 OK, данные задачи

### Несуществующий ID
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos/11111111-1111-1111-1111-111111111111" -Method Get
```
404 Not Found, JSON с `code: "not_found"` и `requestId`

### Ошибка валидации (пустой заголовок)
```powershell
$body = @{ title = ""; description = "Описание"; priority = 1 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5041/api/todos" -Method Post -Body $body -ContentType "application/json"
```
400 Bad Request, `code: "validation_error"`, понятное сообщение

### Переключение статуса
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos/$id/toggle" -Method Patch
```
200 OK, `isCompleted` меняется на противоположный, `completedAt` заполняется

### Фильтрация
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos?completed=false&priority=2&search=тест" -Method Get
```
200 OK, отфильтрованный массив

### Проверка requestId
```powershell
Invoke-RestMethod -Uri "http://localhost:5041/api/todos" -Method Get -Headers @{ "X-Request-Id" = "test-123" } -ResponseHeadersVariable headers
$headers['X-Request-Id']
```
В ответе тот же `X-Request-Id`, в логах консоли запись с этим id

---

## 3. Экспериментальная часть

### 3.1 Проверка обработки необработанного исключения
Временно добавлен `throw new Exception()` в эндпоинт.

**Результат:**
- Клиент: 500 Internal Server Error, `{ "code": "internal_error", "message": "Внутренняя ошибка сервера", ... }`
- Лог: `error` с деталями исключения

Middleware скрывает детали реализации, но логирует их для разработчика.

### 3.2 Проверка защиты от конфликтов
10 параллельных запросов на создание задачи с одинаковым названием "Дубликат".

**Результат:** 1 успех (201), 9 ошибок валидации (400) – блокировка `lock` работает.

### 3.3 Измерение времени
100 запросов GET /api/todos: минимум 2 мс, максимум 18 мс, среднее 5.3 мс.

Стабильная производительность in-memory хранилища.

### 3.4 Эксперимент с порядком middleware
При неправильном порядке (ErrorHandlingMiddleware после TimingAndLogMiddleware) в логах ошибочно фиксировался статус 200 вместо 400.

**Вывод:** порядок критичен – ErrorHandlingMiddleware должен быть перед TimingAndLogMiddleware.

---

## 4. Выводы

- Архитектура с тремя middleware обеспечивает **сквозную функциональность** без вмешательства в бизнес-логику
- **Единый формат ошибок** упрощает разработку клиентов и отладку
- **RequestId** позволяет трассировать запросы через логи
- **Защита от конфликтов** (`lock`) гарантирует целостность данных
- **Фильтрация и статистика** демонстрируют гибкость API

**Ограничения:**
- Хранение в памяти – данные теряются при перезапуске
- Отсутствие пагинации
- Простая блокировка может быть узким местом при высокой нагрузке

---

## 5. Ответы на контрольные вопросы

**1. Независимые и зависимые переменные**
- **Независимые**: HTTP-метод, путь, параметры запроса, тело, заголовок X-Request-Id
- **Зависимые**: статус ответа, тело, requestId, время выполнения, логи

**2. Угрозы валидности и их уменьшение**
- Нестабильность времени ОС → множественные замеры
- Состояния гонки → блокировки и параллельные тесты
- Порядок middleware → эксперименты и фиксация правильной последовательности

**3. Ядро каркаса vs приложение**
- **Ядро**: middleware, базовые исключения, ErrorResponse, интерфейс репозитория
- **Приложение**: модели, конкретные исключения, реализация репозитория, валидация, эндпоинты

**4. Обнаруженные антипаттерны**
- Неправильный порядок middleware
- Отсутствие try-finally в TimingAndLogMiddleware
- Состояния гонки → добавлен lock
- "Проглатывание" исключений → централизованная обработка
- Магические строки → коды ошибок в исключениях

**5. Масштабирование ×10**
- In-memory → БД (PostgreSQL)
- Блокировка → оптимистическая блокировка или шардирование
- Логи в консоли → централизованное логирование (ELK)
- RequestId → распределённая трассировка
- Middleware останутся, но добавятся метрики и балансировка
