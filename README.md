# AccountService

**AccountService** — это REST API-сервис для управления банковскими счетами и транзакциями. 

Сервис реализован на **ASP.NET Core 9**.

---

## Возможности API

- Создание, обновление, удаление счетов
- Получение списка счетов пользователя
- Проверка принадлежности счета пользователю
- Получение выписки по транзакциям за период
- Добавление транзакций (списание, пополнение)
- Перевод между счетами

---

## Технологии

- ASP.NET Core 9
- CQRS + MediatR
- FluentValidation
- AutoMapper
- Swagger (OpenAPI)

---

## Запуск

1. Клонируйте репозиторий, запустите проект:

```bash
git clone https://github.com/holo2k/AccountService.git
cd AccountService
````
Swagger будет доступен по адресу:
[http://localhost:5000/index.html](http://localhost:5000/index.html)

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

Тестовые пользователи

| ID                                     | Примечание      |
| -------------------------------------- | --------------- |
| `1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656` | Пользователь #1 |
| `43007588-4211-492f-ace0-f5b10aefe26b` | Пользователь #2 |
| `4650ec28-5afc-4bb2-8f47-90550012646e` | Пользователь #3 |


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
│   AccountService.csproj
│   Program.cs
│   appsettings.json
│
├───AutoMapper
│       MappingProfile.cs
│
├───CurrencyService          // заглушка сервиса валют
│   ├───Abstractions
│   │       ICurrencyService.cs
│   └───Implementations
│           CurrencyService.cs
│
├───Features
│   ├───Account
│   │   ├───AddAccount
│   │   ├───DeleteAccount
│   │   ├───GetAccount
│   │   ├───GetAccountBalance
│   │   ├───GetAccountsByOwnerId
│   │   ├───GetAccountStatement
│   │   └───UpdateAccount
│   │       ...
│   └───Transaction
│       ├───AddTransaction
│       └───TransferBetweenAccounts
│           ...
│
├───Infrastructure           // заглушка хранения счетов и транзакций
│   └───Repository
│       ├───Abstractions
│       └───Implementations
│
├───PipelineBehaviors
│       MbResult.cs
│       ValidationBehavior.cs
│
├───Startup
│       Startup.cs
│       ServiceCollectionExtensions.cs
│
├───UserService              // заглушка сервиса верификации клиентов
│   ├───Abstractions
│   └───Implementations
│       ...

```
## Контакты

* TG: [holo21k](https://t.me/holo21k)
* Email: [nneketaa@yandex.ru](mailto:nneketaa@yandex.ru)
