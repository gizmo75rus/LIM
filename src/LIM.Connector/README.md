# LIM.Connector

## Описание

LIM.Connector - это библиотека, предназначенная для подключения и взаимодействия с системой LIM (Laboratory Information Management).

## Зависимости

### Требуемые зависимости
- .NET 6.0
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions

### Дополнительные библиотеки (при необходимости)
- System.Text.Json
- System.Net.Http
- Microsoft.Extensions.Configuration.Abstractions

## Сборка проекта

Для сборки проекта используйте команду:

```bash
dotnet build LIM.Connector.csproj
```

## Тестирование

Для запуска тестов выполните:

```bash
dotnet test LIM.Connector.Tests.csproj
```

## Использование

Для использования библиотеки в своем проекте добавьте ссылку на нее:

```xml
<PackageReference Include="LIM.Connector" Version="1.0.0" />
```

или

```bash
dotnet add reference LIM.Connector.csproj
```