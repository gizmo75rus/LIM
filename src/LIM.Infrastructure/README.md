# LIM.Infrastructure

## Описание

LIM.Infrastructure - это инфраструктурный слой проекта LIM (Laboratory Information Management). Этот проект содержит реализации инфраструктурных компонентов, необходимых для работы приложения, включая данные, конфигурации и сервисы.

## Назначение

Проект LIM.Infrastructure служит для:

1. **Реализации данных** - предоставление доступа к данным через Entity Framework Core
2. **Конфигурация приложения** - настройка и управление конфигурацией
3. **Сервисы инфраструктуры** - реализация сервисов, зависящих от внешних библиотек
4. **Подключение к базе данных** - реализация подключения к PostgreSQL с использованием Entity Framework Core

## Зависимости

### Внешние библиотеки
- **EFCore.NamingConventions** (v6.0.0) - правила именования для EF Core
- **Microsoft.AspNetCore.Authentication.JwtBearer** (v6.0.16) - аутентификация через JWT токены
- **Microsoft.EntityFrameworkCore** (v6.0.16) - ORM для работы с базой данных
- **Microsoft.EntityFrameworkCore.InMemory** (v6.0.0) - in-memory провайдер EF Core для тестирования
- **Npgsql.EntityFrameworkCore.PostgreSQL** (v6.0.8) - провайдер PostgreSQL для EF Core
- **Serilog.Extensions** (v2.2.2) - расширения для Serilog

### Внутренние зависимости
- **LIM.ApplicationCore** - ссылка на бизнес-логику приложения
- **LIM.SharedKernel** - ссылка на общие компоненты системы

## Архитектура

LIM.Infrastructure реализует следующую архитектурную структуру:
- Контекст базы данных (DbContext)
- Репозитории данных
- Сервисы инфраструктуры
- Конфигурации и настройки
- Миграции базы данных

## Использование

Проект используется в основном слое приложения (ApplicationCore) и веб-интерфейсе (WebApi) для:
- Подключения к базе данных
- Реализации репозиториев данных
- Настройки сервисов инфраструктуры
- Обеспечения доступа к данным

Для добавления ссылки в другой проект используйте:
```xml
<PackageReference Include="LIM.Infrastructure" Version="1.0.0" />
```

или

```bash
dotnet add reference LIM.Infrastructure.csproj
```