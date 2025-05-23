# OrderAccountingSystem

**OrderAccountingSystem** — демонстрационный проект, реализующий микросервисную архитектуру с использованием RabbitMQ для асинхронной коммуникации и PostgreSQL для хранения данных.

Проект реализован на платформе **.NET 8** и показывает, как можно строить независимые, легко масштабируемые сервисы, обменивающиеся событиями.

---

## 🔧 Микросервисы

В системе реализованы следующие сервисы:

| Микросервис             | Назначение                                                                          |
|-------------------------|-------------------------------------------------------------------------------------|
| **ApiGateway**          | [**Будет реализовано позже**] Единая точка входа в систему, маршрутизация запросов. |
| **AuthMicroservice**    | Аутентификация и авторизация пользователей (JWT, роли).                             |
| **CartMicroservice**    | Управление корзиной пользователя, хранение и обновление её состояния.               |
| **CatalogMicroservice** | Каталог товаров с поддержкой CRUD, фильтрацией и публикацией событий.               |
| **OrderMicroservice**   | Оформление и отслеживание заказов. Обработка событий корзины и платежей.            |
| **PaymentsMicroservice**| Обработка транзакций, интеграция с внешними платёжными провайдерами.                |
| **UserMicroservice**    | Профиль пользователя, контактные данные, история активности.                        |
| **DeliveryMicroservice**| Обработка логистики, статуса доставки, трекинг.                                     |
| **NotificationMicroservice** | [**Будет реализовано позже**] Обработка и доставка уведомлений пользователям.  |

---

## 🧱 Архитектура микросервисов

```mermaid
graph TD

%% ───── Gateway ─────
   subgraph Gateway
      AG[ApiGateway]
   end

%% ───── Core Services ─────
   subgraph Core Services
      AU[Auth Microservice]
      CA[Catalog Microservice]
      CR[Cart Microservice]
      OR[Orders Microservice]
      PA[Payments Microservice]
      DE[Delivery Microservice]
   end

%% ───── External: User Service ─────
   subgraph User Service
      US[User Microservice]
   end

%% ───── External: Notification ─────
   subgraph Notification Service
      NO[Notification Microservice]
   end

%% ───── Message Broker ─────
   subgraph Message Broker
      RMQ[(RabbitMQ)]
   end

%% Gateway → Core
   AG --> AU
   AG --> CA
   AG --> CR
   AG --> OR
   AG --> PA

%% Auth → User registration
   AU --> US

%% Core publishes events
   CR -- publishes --> RMQ
   OR -- publishes --> RMQ
   PA -- publishes --> RMQ
   DE -- publishes --> RMQ

%% Core services consume events
   RMQ -- Order Created --> PA
   RMQ -- Payment Successful --> OR
   RMQ -- Payment Failed --> OR
   RMQ -- Refund Issued --> OR
   RMQ -- Order Dispatched --> DE
   RMQ -- Order Picked Up --> OR
   RMQ -- Order Delivered --> OR

%% Notification subscribes to all order-related events
   RMQ -- Order Created --> NO
   RMQ -- Payment Successful --> NO
   RMQ -- Payment Failed --> NO
   RMQ -- Refund Issued --> NO
   RMQ -- Order Dispatched --> NO
   RMQ -- Order Picked Up --> NO
   RMQ -- Order Delivered --> NO
```

**Примечания:**

- ApiGateway — будет реализован позже.

- NotificationMicroservice — находится в разработке.

### 📌 Преимущества архитектуры
- **Масштабируемость**: каждый сервис можно масштабировать независимо в зависимости от нагрузки.

- **Гибкость**: возможность замены или обновления отдельных сервисов без влияния на всю систему.

- **Устойчивость**: сбой одного сервиса не приводит к остановке всей системы.

- **Простота сопровождения**: четкое разделение ответственности между сервисами облегчает поддержку и развитие проекта.
---

- Асинхронная коммуникация осуществляется через RabbitMQ (exchange’ы и очереди для каждого типа событий).

- Каждый сервис имеет собственную базу данных PostgreSQL (принцип Database per Service).

- API Gateway (опционально) может предоставлять единый вход для REST- или gRPC-запросов и маршрутизировать их к нужным сервисам.

---

## 🧰 Технологический стек
- .NET 8 — платформа микросервисов

- RabbitMQ — брокер сообщений

- PostgreSQL — хранилище данных

- Entity Framework Core — ORM для работы с БД

- Docker & Docker Compose — для контейнеризации и локального запуска

- JWT — для аутентификации

- Swagger / OpenAPI — документация REST API каждого сервиса

---
## 📁 Структура репозитория

```
src/
  ├── ApiGateway/                 # Единая точка входа (будет реализовано позже)
  ├── AuthMicroservice/           # JWT-аутентификация, авторизация
  ├── CartMicroservice/           # Корзина товаров для заказов
  ├── CatalogMicroservice/        # Каталог товаров (CRUD + Events)
  ├── OrderMicroservice/          # Управление заказами
  ├── PaymentsMicroservice/       # Обработка платежей
  ├── UserMicroservice/           # Данные о пользователях
  ├── DeliveryMicroservice/       # Доставка
  ├── NotificationMicroservice/   # (будет реализовано позже)
  └── Shared.Contracts/           # DTO и события

```

---

## ⚙️ Установка и запуск

**1. Клонирование проекта**

``` bash
git clone https://github.com/NVolnukhin/OrderAccountingSystem.git
cd OrderAccountingSystem
```

**2. Настройка переменных окружения**

Заполните .env:
```aiignore
# PostgreSQL
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password

# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
```
**3. Запуск через Docker Compose (Будет реализовано позже)**
```bash
docker-compose up --build
```

**4. Запуск из IDE**
- Откройте OrderAccountingSystem.sln в Visual Studio или Rider.

- Запустите нужные проекты (по одному или все сразу)

## 🧪 Примеры взаимодействия

1. Получение списка товаров из каталога 
    ```
   curl -X 'GET' \
    'http://localhost:5080/api/Products' \
    -H 'accept: text/plain'
   ```
    Пример ответа:
    ```
   Code: 200
   
   [
      {
        "id": 0,
        "name": "string",
        "description": "string",
        "price": 0,
        "stockQuantity": 0,
        "categoryId": 0,
        "imageUrl": "string",
        "attributes": [
          {
            "key": "string",
            "value": "string"
          }
        ]
      }
    ]
   ```
   
2. Добавление товара в корзину
   ```
    curl -X 'POST' \
    'http://localhost:5081/api/Cart/items' \
    -H 'accept: text/plain' \
    -H 'Content-Type: application/json' \
    -d '{
      "productId": 15,
      "quantity": 2
    }'
    ```
   Пример ответа:
    ```
   Code: 200
   
   {
      "id": "0196ab5a-059a-7c3e-83b8-ed49a2f90206",
      "userId": "67ed5e1d-1f1a-4f71-baa0-6b3d6add9e67",
      "sessionToken": null,
      "createdAt": "2025-05-07T15:25:25.769808Z",
      "updatedAt": null,
      "items": [
        {
          "id": "0196c452-6699-7575-9367-0e55900d6281",
          "productId": 15,
          "productName": "Bleu de Chanel",
          "productImageUrl": "https://example.com/bleu.jpg",
          "quantity": 2,
          "price": 150
        }
      ],
      "totalPrice": 300
    }
   ```
3. Создание заказа с содержимым корзины:
    ```
   curl -X 'POST' \
    'http://localhost:5081/api/Cart/checkout' \
    -H 'accept: */*' \
    -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjY3ZWQ1ZTFkLTFmMWEtNGY3MS1iYWEwLTZiM2Q2YWRkOWU2NyIsImV4cCI6MTc0NzA1NDA0NSwiaXNzIjoiYXV0aC1zZXJ2aWNlIiwiYXVkIjoib3JkZXItYWNjb3VudGluZy1zeXN0ZW0ifQ.6p2Z9f_k-qQl3mpWTKdaf5Y57azTnAQJB0VcoOouCuw' \
    -H 'Content-Type: application/json' \
    -d '{
      "deliveryAddress": "string"
    }'
    ```
    Пример ответа:
    ```
    Code: 200
   
    {
       "message": "Checkout successful"
    }
    ```
