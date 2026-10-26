# Tailly

## 📌 Проект

Серверная архитектура веб-приложения для услуг по уходу за питомцами с системой привлечения специалистов и продажи товаров.

Проект построен на основе микросервисной архитектуры и объединяет работу с пользователями, специалистами, услугами, бронированиями, товарами, профилями питомцев и контентом.

## 🚀 Запуск проекта

### Требования

- Git.
- Docker Desktop.

### Клонирование репозитория

```bash
git clone https://github.com/ob1tower/Tailly.git
cd Tailly
```

### Настройка

Перед запуском укажите собственные значения в файлах `appsettings.Development.json`:

- **AuthService** (`Tailly.AuthService`) — измените 4 поля:
  - `JwtConfig:SecretKey`
  - `Security:OtpPepper`
  - `EmailSettings:Email`
  - `EmailSettings:Password`

- **BookingService** (`Tailly.BookingService`) — `JwtConfig:SecretKey`
- **ClientProfileService** (`Tailly.ClientProfileService`) — `JwtConfig:SecretKey`
- **ShopService** (`Tailly.ShopService`) — `JwtConfig:SecretKey`
- **PostsService** (`Tailly.PostsService`) — `JwtConfig:SecretKey`
- **SpecialistService** (`Tailly.SpecialistService`) — `JwtConfig:SecretKey`

**Важно:** значение `JwtConfig:SecretKey` в остальных микросервисах должно совпадать с ключом в AuthService.

### Учётная запись главного администратора

При первом запуске AuthService автоматически создаётся учётная запись SuperAdmin.

- **Email:** `superadmin@tailly.com`
- **Пароль:** `12345678`

### Запуск

Запустите Docker Desktop и выполните команду из корневой папки проекта:

```bash
docker compose up --build -d
```

### Доступ к сервисам

- **API Gateway:** http://localhost:5000
- **AuthService (Swagger):** http://localhost:5001/swagger
- **ClientProfileService (Swagger):** http://localhost:5002/swagger
- **ShopService (Swagger):** http://localhost:5003/swagger
- **PostsService (Swagger):** http://localhost:5004/swagger
- **SpecialistService (Swagger):** http://localhost:5005/swagger
- **BookingService (Swagger):** http://localhost:5006/swagger
- **pgAdmin:** http://localhost:5050
  - Логин: `admin@admin.ru`
  - Пароль: `admin`
- **RabbitMQ Management:** http://localhost:15672
  - Логин: `guest`
  - Пароль: `guest`

### Управление контейнерами

Проверка состояния контейнеров:

```bash
docker compose ps
```

Просмотр логов:

```bash
docker compose logs -f
```

Остановка проекта:

```bash
docker compose down
```

## 🏗️ Архитектура проекта

<img width="624" height="400" alt="Рисунок1" src="https://github.com/user-attachments/assets/5ad9da86-fbd5-4eed-ab85-ad090c451bbc" />

## ✨ Достижения

- Реализовала **микросервисную архитектуру** из шести независимых сервисов с разделением ответственности по предметным областям.
- Разработала **API Gateway на базе Ocelot** для единой точки входа и маршрутизации запросов к микросервисам.
- Реализовала **асинхронное взаимодействие между сервисами** через MassTransit и RabbitMQ с использованием общей библиотеки контрактов.
- Реализовала **аутентификацию и авторизацию** на основе JWT с использованием Access и Refresh Token.
- Реализовала **регистрацию пользователей с подтверждением электронной почты**, восстановление и изменение пароля, изменение email и управление сессиями.
- Реализовала **ролевую модель доступа** с ролями Client, Specialist, Admin и SuperAdmin.
- Реализовала **систему привлечения специалистов**: подача и модерация заявок, создание профиля, управление услугами и календарём доступности.
- Реализовала **бронирование услуг** с управлением жизненным циклом заказа, статусами, медиафайлами и отзывами.
- Разработала **интернет-магазин** с каталогом товаров, категориями, корзиной, избранным, заказами, пунктами выдачи и отзывами.
- Реализовала **профили владельцев питомцев** и управление данными домашних животных.
- Разработала **сервис управления контентом** для работы с постами, тегами и баннерами.
- Организовала **раздельное хранение данных микросервисов** в PostgreSQL и взаимодействие между сервисами через API и асинхронные сообщения.
- Использовала **Redis для хранения временных данных** с ограниченным временем жизни.
- Реализовала **загрузку и хранение медиафайлов** для профилей, товаров, отзывов, специалистов и контента.
- Покрыла бизнес-логику микросервисов **модульными тестами** с использованием xUnit, Moq, FluentAssertions и AutoFixture.
- Организовала **контейнеризацию сервисов и инфраструктуры** с помощью Docker Compose.

## 🌐 Технологический стек

### Backend

- **Платформа**: .NET 8
- **Фреймворк**: ASP.NET Core Web API
- **Архитектура**: микросервисная
- **API Gateway**: Ocelot
- **ORM**: Entity Framework Core
- **Взаимодействие между сервисами**: REST, RabbitMQ, MassTransit
- **Контракты сообщений**: отдельная библиотека Contracts

### Хранение данных

- **СУБД**: PostgreSQL
- **Кэш и временные данные**: Redis
- **Подход к данным**: отдельная база данных для каждого микросервиса

### Аутентификация и авторизация

- **Аутентификация**: JWT
- **Токены**: Access Token, Refresh Token
- **Роли**: Client, Specialist, Admin, SuperAdmin

### Тестирование

- **Unit Testing**: xUnit
- **Mocking**: Moq
- **Assertions**: FluentAssertions
- **Test Data Generation**: AutoFixture

### Инфраструктура

- **Контейнеризация**: Docker
- **Оркестрация контейнеров**: Docker Compose
- **Брокер сообщений**: RabbitMQ
