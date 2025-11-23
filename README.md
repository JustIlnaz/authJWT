# AuthJWT - API для интернет-магазина

RESTful API для управления интернет-магазином с JWT аутентификацией и авторизацией на основе ролей.

## Описание

Проект представляет собой веб-API на ASP.NET Core 8.0, реализующее функционал интернет-магазина с системой аутентификации через JWT токены. API поддерживает управление товарами, категориями, корзиной, заказами, пользователями и способами доставки.

## Основные возможности

- Аутентификация и авторизация через JWT токены
- Управление пользователями с ролевой моделью (Администратор, Менеджер, Покупатель)
- Управление товарами и категориями
- Корзина покупок
- Система заказов со статусами
- Управление способами доставки
- Управление платёжными методами
- Профиль пользователя

## Технологии

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- SQL Server
- JWT (JSON Web Tokens)
- Swagger/OpenAPI

## Требования

- .NET 8.0 SDK или выше
- SQL Server (локальный или удалённый)
- Visual Studio 2022 или Visual Studio Code (или любая другая IDE с поддержкой .NET)

## Установка и настройка

1. Клонируйте репозиторий:
```bash
git clone <repository-url>
cd authJWT
```

2. Настройте строку подключения к базе данных в файле `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "dbJWT": "Server=YOUR_SERVER;Database=authJWT;Trusted_Connection=True;"
  }
}
```

3. Настройте параметры JWT в `appsettings.json`:
```json
{
  "JWT": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123456789",
    "Issuer": "authJWT",
    "Audience": "authJWT",
    "ExpirationInMinutes": 60
  }
}
```

4. Примените миграции базы данных:
```bash
dotnet ef database update --project authJWT
```

5. Запустите приложение:
```bash
dotnet run --project authJWT
```

Приложение будет доступно по адресу `https://localhost:5001` или `http://localhost:5000` (проверьте `launchSettings.json` для точных портов).

## Использование API

### Swagger UI

После запуска приложения откройте Swagger UI по адресу:
- `https://localhost:5001/swagger` (HTTPS)
- `http://localhost:5000/swagger` (HTTP)

### Аутентификация

1. Зарегистрируйте нового пользователя:
```
POST /api/Auth/register
```

2. Войдите в систему:
```
POST /api/Auth/login
```

В ответе вы получите JWT токен, который необходимо использовать в заголовке `Authorization` для всех защищённых эндпоинтов:
```
Authorization: Bearer <your-token>
```

## Структура проекта

```
authJWT/
├── Controllers/          # API контроллеры
│   ├── AuthController.cs
│   ├── CartController.cs
│   ├── CategoriesController.cs
│   ├── ItemsController.cs
│   ├── OrdersController.cs
│   ├── ProfileController.cs
│   ├── ShippingMethodsController.cs
│   └── UsersController.cs
├── Service/              # Бизнес-логика и сервисы
│   ├── CartService.cs
│   ├── CategoryService.cs
│   ├── ItemService.cs
│   ├── JwtService.cs
│   ├── JwtMiddleware.cs
│   ├── OrderService.cs
│   ├── ProfileService.cs
│   └── UserService.cs
├── Models/               # Модели данных
│   ├── User.cs
│   ├── Item.cs
│   ├── Category.cs
│   ├── Cart.cs
│   ├── Order.cs
│   └── ...
├── Connection/           # Контекст базы данных
│   └── ContextDb.cs
├── Interfaces/           # Интерфейсы сервисов
├── Requests/             # DTO для запросов
└── Migrations/            # Миграции Entity Framework
```

## Роли пользователей

- **Администратор** - полный доступ ко всем функциям системы
- **Менеджер** - управление товарами, заказами, просмотр покупателей
- **Покупатель** - просмотр товаров, управление корзиной, создание заказов, управление профилем

## Основные эндпоинты

### Аутентификация
- `POST /api/Auth/register` - Регистрация нового пользователя
- `POST /api/Auth/login` - Вход в систему

### Товары
- `GET /api/Items` - Получить список товаров (с фильтрацией)
- `POST /api/Items` - Создать товар (Администратор, Менеджер)
- `PUT /api/Items` - Обновить товар (Администратор, Менеджер)
- `DELETE /api/Items` - Удалить товар (Администратор, Менеджер)

### Категории
- `GET /api/Categories` - Получить список категорий
- `POST /api/Categories` - Создать категорию (Администратор)
- `PUT /api/Categories` - Обновить категорию (Администратор)
- `DELETE /api/Categories` - Удалить категорию (Администратор)

### Корзина
- `GET /api/Cart` - Получить корзину (Покупатель)
- `POST /api/Cart` - Добавить товар в корзину (Покупатель)
- `PUT /api/Cart` - Обновить количество товара (Покупатель)
- `DELETE /api/Cart` - Удалить товар из корзины (Покупатель)

### Заказы
- `GET /api/Orders` - Получить список заказов
- `POST /api/Orders` - Создать заказ (Покупатель)
- `PUT /api/Orders/status` - Обновить статус заказа (Администратор, Менеджер)
- `DELETE /api/Orders` - Отменить заказ

### Пользователи
- `GET /api/Users` - Получить список пользователей (Администратор, Менеджер)
- `POST /api/Users/employees` - Создать сотрудника (Администратор)
- `PUT /api/Users` - Обновить пользователя (Администратор, Менеджер)
- `PUT /api/Users/role` - Изменить роль пользователя (Администратор)
- `DELETE /api/Users` - Удалить пользователя (Администратор)

### Профиль
- `GET /api/Profile` - Получить профиль
- `PUT /api/Profile` - Обновить профиль
- `POST /api/Profile/payment-methods` - Добавить платёжный метод (Покупатель)
- `DELETE /api/Profile/payment-methods` - Удалить платёжный метод (Покупатель)

### Способы доставки
- `GET /api/ShippingMethods` - Получить список способов доставки
- `POST /api/ShippingMethods` - Создать способ доставки (Администратор)
- `PUT /api/ShippingMethods` - Обновить способ доставки (Администратор)
- `DELETE /api/ShippingMethods` - Удалить способ доставки (Администратор)

## Формат ответов

Все успешные операции возвращают соответствующие сообщения:

- Создание: `{ "message": "Успешно создано", "data": {...} }`
- Обновление: `{ "message": "Успешно обновлено" }`
- Удаление: `{ "message": "Успешно удалено" }`

## Безопасность

- Пароли хешируются с использованием `PasswordHasher<User>`
- JWT токены содержат информацию о пользователе и роли
- Middleware проверяет токены для всех защищённых эндпоинтов
- Авторизация на основе ролей через атрибут `[AuthorizeRole]`

## Разработка

### Создание новой миграции

```bash
dotnet ef migrations add MigrationName --project authJWT
```

### Применение миграций

```bash
dotnet ef database update --project authJWT
```

### Откат миграции

```bash
dotnet ef database update PreviousMigrationName --project authJWT
```

## Лицензия

Этот проект создан в образовательных целях.

## Автор

Проект разработан для изучения ASP.NET Core, JWT аутентификации и создания RESTful API.

