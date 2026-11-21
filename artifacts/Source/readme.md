# AstmDriver

Драйвер для обмена с лабораторными анализаторами по протоколам **ASTM E1394** и **HL7 v2.3.1** (MLLP).

---

## Архитектура

```
Program → ChannelFactory → HostFactory → MessageWorkerFactory
             ↓                 ↓                  ↓
      ITransportChannel      IHost         IMessageWorker
                                                  ↓
                                                Dal
```

- **`ITransportChannel`** — транспорт (TCP: клиент/сервер v1/v2/v3).
- **`IHost`** — протокол (ASTM / HL7 / MLLP / raw). Не знает смысла сообщений.
- **`IMessageWorker`** — разбор и обработка сообщений, вызовы `Dal`, ответы анализатору.
- **`Dal`** — БД (LIS, логи).

---

## `Program`

Читает `DriverConfig`, создаёт цепочку через три фабрики, подписывается на события, вызывает `host.Connect()`, крутится в `while(true)`.

Обработка исключений: `SqlException` / `ArgumentConfigException` / `NotImplementedException` → лог и ожидание действия пользователя; прочие → автоперезапуск.

---

## Фабрики

| Фабрика | Ключ в конфиге | Реализации |
|---|---|---|
| `ChannelFactory` | `IsServerMode`, `ServerVersion` | `TcpChannelServer` (v1/v2/v3), `TcpChannelClient` |
| `HostFactory` | `HostType` | `AstmHost`, `MllpHost`, `MiuraHost`, `MetascanHost`, `BeckmanCoulterHost` |
| `MessageWorkerFactory` | `MessageWorker` | `Default`, `Saphir400`, `AU480`, `Metascan`, `Miura`, `Aspect` |

---

## Хосты (`IHost`)

| Хост | Протокол | Особенность |
|---|---|---|
| `AstmHost` | ASTM | База: `ENQ`/`ACK`, кадры, `EOT`, retry. Точка расширения — `OnConnectiongHandler`. |
| `MiuraHost` | ASTM | Наследник `AstmHost`. При подключении сразу `Receiving`. |
| `MetascanHost` | ASTM | Наследник `AstmHost`. При подключении `ENQ` → `EOT` → `Idle`. |
| `MllpHost` | HL7/MLLP | Обёртка `VT + HL7 + FS + CR`, без handshake. |
| `BeckmanCoulterHost` | raw | «Сырые» строки, без обёртки. |

Общий контракт: `Connect` / `DisConnect` / `EstablishSendMode` / `SendMessage` / `StopSendMode` / `GetClientIP` + события `OnReceiveString`, `OnEOTReceived`, `OnConnectionClosed`, `OnError`, `OnReconnect`.

---

## Воркеры (`IMessageWorker`)

Две симметричные ветки.

### ASTM — `AstmWorkerBase` (abstract)

Собирает `Message` из записей `H/Q/R/O/L`, обрабатывает по `OnEOTReceived` через `ProcessResult` / `ProcessQuery` / `ProcessOrder`, отправляет через `TrySendToAnalyzer`.

Наследники: `Default_Worker`, `Saphir400_Worker`, `AU480_Worker`, `Metascan`, `Miura_Worker`.

### HL7 — `Hl7WorkerBase` (abstract)

Разбирает `MSH`, диспетчеризует по типу:

| Тип | Хук |
|---|---|
| `ORU` | `ProcessOru` (abstract) |
| `ORM` | `ProcessOrm` (virtual) |
| `QRY` | `ProcessQry` (virtual) |
| `ACK` | `ProcessIncomingAck` (virtual) |

Политика ACK — `ShouldSendAck`. Отправка — `SendHl7` / `SendAck`.

Наследники: `HL7_Worker` (Aspect), `MedicalSystem_Worker` (MS-380/680/2080/L8080).

---

## Расширение

| Задача | Что делать |
|---|---|
| Новый транспорт | `ITransportChannel` + `ChannelFactory`. |
| ASTM со спец. handshake | Наследник `AstmHost`, переопределить `OnConnectiongHandler`. |
| ASTM-воркер | Наследник `AstmWorkerBase`, переопределить `Process*`. |
| HL7-вендор | Наследник `Hl7WorkerBase`, реализовать `ProcessOru`, при необходимости `ProcessQry`/`ShouldSendAck`. |
| Совсем новый воркер | Реализовать `IMessageWorker` + добавить в `MessageWorkerFactory`. |

После добавления класса — обновить соответствующую фабрику и ключ в конфиге.

---

## Ключи `DriverConfig`

| Поле | Влияет на |
|---|---|
| `RemoteHost`, `Port`, `Encoding` | Канал. |
| `IsServerMode`, `ServerVersion` | `ChannelFactory`. |
| `HostType` | `HostFactory`. |
| `MessageWorker` | `MessageWorkerFactory`. |
| `LogType`, `LogLevel` | Логирование. |

---

## Связь с `Dal`

`SetConnectionString` (в `Program`), `GetAnalyzer`, `WriteLog`, `AnalyzerSEND_GetCoderByBarcode`, `GetPatientBySubject` / `GetAssignedItemsBySubject` (HL7 QRY, к добавлению).