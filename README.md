# AccountService

**AccountService** — это REST API-сервис для управления банковскими счетами и транзакциями. 

Сервис реализован на **ASP.NET Core 9**.

---

## Информация об API

- Создание, обновление, удаление счетов
- Получение списка счетов пользователя
- Проверка принадлежности счета пользователю
- Получение выписки по транзакциям за период
- Добавление транзакций (списание, пополнение)
- Перевод между счетами
- Получение JWT токена
- Начисление процентов по вкладам через хранимую процедуру accrue_interest
- Ежедневный Cron-Job HangFire для автоматического начисления процентов
- Закрытие вклада с последующим начислением процентов
- Оптимистичная блокировка через concurrency-token (xmin)
- Покрытие модульными и интеграционными тестами
- Использование составного индекса
---

## Технологии

- ASP.NET Core 9
- CQRS + MediatR
- FluentValidation
- AutoMapper
- Swagger (OpenAPI)
- Docker, Docker Compose
- Keycloak
- HangFire
- xUnit, Testcontainers, Moq

---

## Запуск

### Запуск из Visual Studio

В Visual Studio можно выбрать проект Docker Compose в качестве стартового (startup project) и запустить его через стандартный запуск Debug/Run.
Так вы запустите все контейнеры (Keycloak + AccountService) вместе.

### Запуск из консоли

1. Перейдите в папку  `docker`:

```bash
cd docker
```

2. Запустите:

```bash
docker compose up -d --build
```

3. После запуска сервис будет доступен по адресу:
   [http://localhost:80/swagger](http://localhost:80/swagger)

---

## Аутентификация и работа с токеном

### Получение токена

API содержит эндпоинт для получения тестового токена:

```http
GET /auth/token
```

Используются следующие тестовые учётные данные:

* username: `testuser`
* password: `password`
* client\_id: `account-api`

В ответе возвращается JSON с `access_token` и другими параметрами.

### Регистрация токена в Swagger

Для тестирования авторизации через Swagger UI выполните следующие шаги:

1. Нажмите кнопку **Authorize** (справа сверху) в Swagger UI.

2. В открывшемся окне будет OAuth2 форма авторизации.

3. Нажмите кнопку **Authorize** — вы будете перенаправлены на страницу входа Keycloak.

4. Введите ваши тестовые учётные данные:

   * username: `testuser`
   * password: `password`

5. После успешного входа Keycloak перенаправит обратно в Swagger UI, и токен будет автоматически зарегистрирован.

6. Теперь можно выполнять защищённые запросы — Swagger UI будет автоматически добавлять полученный токен в заголовок `Authorization: Bearer <token>`.

---

**Примечание:**
Если вы хотите использовать токен вручную, получите его через эндпоинт `/auth/token` и введите в поле **Value** в окне **Authorize** в формате:

```
Bearer <ваш_access_token>
```
---

## Использование результата с MbResult

В проекте для стандартного результата используется универсальный класс `MbResult<T>`

* `MbResult<T>` содержит либо успешный результат (`Result`), либо ошибку (`Error`).
* Используется для унифицированной обработки ошибок и результатов в бизнес-логике.
* Валидация через FluentValidation интегрирована с `MbResult` — в случае ошибки создаётся объект `MbError` с детальной информацией.
* Добавлен метод расширения класса ControllerBase, возвращающий HTTP-код, соответствующий состоянию MbResult

---

## Hangfire

Для автоматического ежедневного начисления процентов по вкладам используется Hangfire.

* Проверить работу можно путём создания счёта с типом Deposit и выполнением job'ы по адресу:
[http://localhost/hangfire/recurring](http://localhost/hangfire/recurring)

---
## Примечание

* Валюта указывается в формате ISO 4217 (В качестве заглушки есть 3 валюты - `RUB`, `USD`, `EUR`)
* Поле `type` для счёта:

  * `0` — `Checking`
  * `1` — `Deposit`
  * `2` — `Credit`
    
* Поле `type` для транзакции:

  * `0` — `Credit`
  * `1` — `Debit`

Тестовые пользователи (в заглушке, не в Keycloak)

| ID                                     | Примечание      |
| -------------------------------------- | --------------- |
| `1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656` | Пользователь #1 |
| `43007588-4211-492f-ace0-f5b10aefe26b` | Пользователь #2 |
| `4650ec28-5afc-4bb2-8f47-90550012646e` | Пользователь #3 |

Подключение к базе данных API:
localhost:5433
db - account_db
user - postgres
password - postgres

Для проверки использования индексов можно использовать команду PostgreSQL:
```
EXPLAIN ANALYZE
SELECT *
FROM public."Transactions"
WHERE "AccountId" = '{account_id}'
  AND "Date" between '2025-08-01' AND '2025-08-31'
ORDER BY "Date";
```

---

## Примеры curl-запросов

### Получить счета пользователя

```bash
curl -X GET "http://localhost:5000/accounts/1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656"
```

---

### Создать новый счёт

```bash
curl -X POST "http://localhost:5000/accounts" \
  -H "Content-Type: application/json" \
  -d '{
    "account": {
      "ownerId": "1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656",
      "type": 1,
      "currency": "RUB",
      "balance": 1000.0,
      "percentageRate": 1.5
    }
  }'
```

---

### Обновить счёт по ID

```bash
curl -X PUT "http://localhost:5000/accounts/{accountId}" \
  -H "Content-Type: application/json" \
  -d '{
    "account": {
      "ownerId": "1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656",
      "type": 1,
      "currency": "RUB",
      "balance": 1200.0,
      "percentageRate": 2.0,
      "openDate": "2025-07-28T19:37:07.6422334Z"
    }
  }'
```

---

### Удалить счёт

```bash
curl -X DELETE "http://localhost:5000/accounts/{accountId}"
```

---

### Проверить принадлежность счета пользователю

```bash
curl -X GET "http://localhost:5000/accounts/{accountId}/owner/1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656/exists"
```

---

### Получить выписку по счёту

```bash
curl -X GET "http://localhost:5000/accounts/{accountId}/statement?from=2025-07-28&to=2025-07-29"
```

---

### Добавить транзакцию

```bash
curl -X POST "http://localhost:5000/transactions" \
  -H "Content-Type: application/json" \
  -d '{
    "transaction": {
      "accountId": "{accountId_1}",
      "amount": 500.0,
      "currency": "RUB",
      "type": 0,
      "description": "Пополнение",
      "date": "2025-07-28T10:00:00Z"
    }
  }'
```

---

### Перевод между счетами

```bash
curl -X POST "http://localhost:5000/transactions/transfer" \
  -H "Content-Type: application/json" \
  -d '{
    "payloadModel": {
      "fromAccountId": "{accountId_1}",
      "toAccountId": "{accountId_2}",
      "amount": 500,
      "currency": "RUB",
      "description": "Перевод на другой счёт"
    }
  }'
```

---

## Структура проекта

```
AccountService
├── AutoMapper
│   └── MappingProfile.cs
├── CurrencyService (заглушка сервиса валют)
│   ├── Abstractions
│   └── Implementations
├── Features
│   ├── Account
│   │   ├── AccrueInterest
│   │   ├── AddAccount
│   │   ├── CheckAccountOwnership
│   │   ├── CloseDeposit
│   │   ├── DeleteAccount
│   │   ├── GetAccount
│   │   ├── GetAccountBalance
│   │   ├── GetAccountsByOwnerId
│   │   ├── GetAccountStatement
│   │   └── UpdateAccount
│   └── Transaction
│       ├── AddTransaction
│       └── TransferBetweenAccounts
├── Filters
├── Infrastructure
│   ├── Helpers
│   └── Repository
│       ├── Abstractions
│       └── Implementations
├── Jobs
├── PipelineBehaviors
├── Startup
│   └── Auth
├── UserService (заглушка сервиса пользователей)
│   ├── Abstractions
│   ├── Implementations
│   ├── AuthController.cs
│   └── User.cs
├── Program.cs
└── AccountService.csproj

```
## Контакты

* TG: [holo21k](https://t.me/holo21k)
* Email: [nneketaa@yandex.ru](mailto:nneketaa@yandex.ru)
