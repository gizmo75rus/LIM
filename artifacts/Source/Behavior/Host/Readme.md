# AstmDriver.Behavior.Host

## Назначение

Пространство `AstmDriver.Behavior.Host` содержит реализации роли **хоста** для обмена с анализаторами (лабораторным оборудованием). Каждая реализация инкапсулирует поведение конкретного протокола или конкретного прибора.

Все хосты реализуют общий контракт `IHost` и работают поверх абстракции `ITransportChannel`. Часть хостов наследуется от базовой ASTM-реализации (`AstmHost`) и переопределяет только специфичные детали — например, поведение при подключении.

| Класс | Базовый класс | Протокол / прибор | Назначение |
|---|---|---|---|
| `AstmHost` | — | ASTM E1394 | Базовая реализация ASTM-хоста. |
| `MiuraHost` | `AstmHost` | ASTM (специфика Miura) | ASTM-хост: сразу переходит в `Receiving` при подключении. |
| `MetascanHost` | `AstmHost` | ASTM (специфика Metascan) | ASTM-хост: выполняет `ENQ` + `EOT` при подключении, затем `Idle`. |
| `MllpHost` | — | HL7 / MLLP | Обмен с анализаторами, поддерживающими HL7 (обёртка MLLP). |
| `BeckmanCulterHost` | — | Собственный (BC AU480) | «Тонкий» хост: обмен «сырыми» строками без обёртки. |

---

## Общий контракт — `IHost`

`IHost` описывает поведение хоста независимо от конкретного протокола:

| Член | Назначение |
|---|---|
| `Status` | Текущее состояние канала (`ChannelStatus`). |
| `TransportChannel` | Транспортный канал. |
| `Connect()` | Открыть соединение. |
| `DisConnect()` | Закрыть соединение. |
| `EstablishSendMode()` | Установить режим передачи. |
| `SendMessage(string)` | Отправить сообщение. |
| `StartReceiveTimeoutTimer()` | Запустить таймер приёма. |
| `StopSendMode()` | Завершить режим передачи. |
| `GetClientIP()` | Получить IP подключённого устройства. |
| `OnReceiveString` | Событие: принят и разобран кадр данных. |
| `OnConnectionClosed` | Событие: соединение закрыто. |
| `OnReceiveTimeOut` | Событие: истекло время ожидания приёма. |
| `OnEOTReceived` | Событие: получен `EOT` (актуально для ASTM). |
| `OnError` | Событие: произошла ошибка. |
| `OnReconnect` | Событие: выполняется повторное подключение. |

`IHost` реализует `IDisposable`.

---

## Роль `ITransportChannel`

`ITransportChannel` — абстракция транспорта. Он:

- открывает и закрывает соединение;
- отправляет и принимает данные (`WriteData`, `OnReceiveData`);
- сообщает о подключении (`OnConnecting`);
- предоставляет метод `ClearBuffers()`.

Хосты не знают, TCP это, COM-порт или что-то ещё — они работают только с абстракцией.

---

## `AstmHost`

### Назначение

Базовая реализация ASTM-хоста. Управляет обменом по ASTM E1394: полудуплексный протокол с подтверждениями (`ACK` / `NAK`), контрольной суммой и разбиением длинных сообщений на кадры. Служит базой для наследников (`MiuraHost`, `MetascanHost`).

### Служебные символы

| Символ | Значение |
|---|---|
| `STX` | Начало кадра. |
| `ETX` | Конец кадра. |
| `ETB` | Конец промежуточного кадра. |
| `ENQ` | Запрос на передачу. |
| `ACK` | Подтверждение. |
| `NAK` | Отрицательное подтверждение. |
| `EOT` | Конец передачи. |
| `CR`, `LF` | Разделители кадра. |

### Состояния канала

- `Idle` — свободен;
- `Establishing` — установка режима передачи;
- `Sending` — передача;
- `Receiving` — приём.

### Приём

Обрабатывается в `ChannelDataReceived` посимвольно:

- в `Idle` при получении `ENQ` отправляется `ACK`, состояние переходит в `Receiving`;
- в `Receiving` символы накапливаются в буфере; по `LF` кадр проверяется (`CheckChecksum`), отправляется `ACK` или `NAK`;
- промежуточные кадры с `ETB` накапливаются в `_tempIntermediateFrameBuffer`;
- конечный кадр с `ETX` отдаётся в событие `OnReceiveString`;
- по `EOT` приём завершается, статус возвращается в `Idle`, вызывается `OnEOTReceived`;
- работает таймер `_receiveTimeOutTimer`; при истечении — сброс в `Idle` и `OnReceiveTimeOut`.

### Передача

- `EstablishSendMode()` отправляет `ENQ` и ждёт `ACK`, переводя канал в `Sending`.
- `SendMessage(string)` разбивает сообщение на кадры (`_maxFrameSize` = 63993): промежуточные — с `ETB`, последний — с `ETX`.
- `SendString` формирует кадр `STX + frame + checksum + CR + LF`, ждёт `ACK`, при отсутствии — повторяет отправку.
- `StopSendMode()` отправляет `EOT` и переводит канал в `Idle`.

### Контрольная сумма

`CalculateChecksum` считает сумму байтов по модулю 256. `CheckChecksum` валидирует структуру кадра.

> ⚠️ В текущей реализации фактическое сравнение контрольной суммы закомментировано — метод возвращает `true` после структурных проверок. Если нужна строгая валидация ASTM — логику сравнения необходимо включить.

### Подключение

`Connect()` обрабатывает `SocketException` (`10060`, `10061`): при недоступности выполняется несколько повторных попыток (`_tryConnectAttems`), после исчерпания — пауза 15 минут и повторный цикл.

### Точка расширения — `OnConnectiongHandler`

`AstmHost` при подключении канала (`TransportChannel.OnConnecting`) вызывает виртуальный метод `OnConnectiongHandler()`. Базовая реализация:

```csharp
internal virtual void OnConnectiongHandler()
{
    _transportChannel.WriteData($"{ACK}");
    Thread.Sleep(250);
}
```

Наследники могут переопределять это поведение, не переписывая всю логику ASTM.

---

## `MiuraHost`

### Назначение

Хост для анализатора **Miura**, работающего на базе ASTM. Наследуется от `AstmHost`, переопределяет только поведение при подключении.

```csharp
public class MiuraHost : AstmHost
{
    public MiuraHost(ITransportChannel channel) : base(channel) { }

    internal override void OnConnectiongHandler()
    {
        //TransportChannel.WriteData($"{KeepAlive}");
        //Thread.Sleep(250);
        Status = ChannelStatus.Receiving;
    }
}
```

### Что меняется по сравнению с `AstmHost`

| Аспект | `AstmHost` | `MiuraHost` |
|---|---|---|
| Поведение при подключении | `ACK` + пауза 250 мс | Ничего не отправляет, сразу `Receiving` |
| Приём/передача кадров | ASTM | ASTM (наследуется) |
| Контрольная сумма | Как в `AstmHost` | Как в `AstmHost` |
| Retry при `Connect()` | Как в `AstmHost` | Как в `AstmHost` |

### Почему так

Miura сама начинает передачу после установки соединения. Отправка `ACK` ей не нужна, а ожидание `ENQ` от хоста — тоже. Поэтому статус сразу переводится в `Receiving`, чтобы входящие данные корректно обрабатывались `ChannelDataReceived`.

---

## `MetascanHost`

### Назначение

Хост для анализатора **Metascan**, работающего на базе ASTM. Наследуется от `AstmHost`, переопределяет только поведение при подключении.

```csharp
public class MetascanHost : AstmHost
{
    public MetascanHost(ITransportChannel channel) : base(channel) { }

    internal override void OnConnectiongHandler()
    {
        TransportChannel.WriteData($"{ENQ}");
        Thread.Sleep(250);
        TransportChannel.WriteData($"{EOT}");
        Thread.Sleep(250);
        Status = ChannelStatus.Idle;
    }
}
```

### Что меняется по сравнению с `AstmHost`

| Аспект | `AstmHost` | `MetascanHost` |
|---|---|---|
| Поведение при подключении | `ACK` + пауза 250 мс | `ENQ`, пауза, `EOT`, пауза, `Idle` |
| Приём/передача кадров | ASTM | ASTM (наследуется) |
| Контрольная сумма | Как в `AstmHost` | Как в `AstmHost` |
| Retry при `Connect()` | Как в `AstmHost` | Как в `AstmHost` |

### Почему так

Metascan требует **короткой служебной процедуры** сразу после установки соединения: хост отправляет `ENQ` (запрос на связь), затем `EOT` (завершение), после чего канал переводится в `Idle`. Такая последовательность «прогревает» соединение и переводит прибор в рабочее состояние, при котором он готов принимать и отправлять данные по обычной ASTM-логике.

Важно, что `OnConnectiongHandler()` работает **вне** основной логики ASTM: `_handshakeProgress` в базовом классе выставляется в `true` на время вызова, поэтому входящие данные в этот момент игнорируются.

---

## `MllpHost`

### Назначение

Реализация хоста для HL7 поверх MLLP (Minimal Lower Layer Protocol).

### Формат кадра MLLP

```text
VT + <HL7 сообщение> + FS + CR
```

| Символ | Значение |
|---|---|
| `VT` (`0x0B`) | Начало MLLP-фрейма. |
| `FS` (`0x1C`) | Конец MLLP-фрейма. |
| `CR` (`0x0D`) | Завершающий символ. |

### Приём

`ChannelDataReceived` разбирает поток посимвольно:

- по `VT` буфер сбрасывается;
- по `FS` содержимое буфера отдаётся в `OnReceiveString`;
- по `CR` — игнорируется;
- остальные символы накапливаются.

В отличие от ASTM, в MLLP нет подтверждений и handshake.

### Передача

- `EstablishSendMode()` — выставляет `Status = Sending` и возвращает `true`.
- `SendMessage(string)` оборачивает сообщение в MLLP-фрейм и отправляет.
  - Если сообщение уже начинается с `VT`, оно отправляется как есть.
- `StopSendMode()` — переводит статус в `Idle`.

### Особенности

- Нет контрольных сумм.
- Нет подтверждений.
- Нет разбиения на кадры.
- `OnEOTReceived` не используется (присутствует только для совместимости с `IHost`).
- `StartReceiveTimeoutTimer()` — no-op.

---

## `BeckmanCulterHost`

### Назначение

Реализация хоста, специфичная для анализатора **Beckman Coulter AU480**.

В отличие от `AstmHost` и `MllpHost`, этот хост **не реализует собственный протокол обмена**: он не добавляет и не разбирает обёртки (`STX`/`ETX`, `VT`/`FS` и т. п.). Обмен идёт «сырыми» строками через транспортный канал, а разбор/форматирование сообщений выполняются на уровне выше.

### Приём

`ChannelDataReceived` делает минимум:

```csharp
var clear = ControlCodesUtility.ClearControlCodes(buffer);
OnReceiveString?.Invoke(this, new ChannelDataEventArgs(clear, buffer));
```

- принимает данные как есть;
- прогоняет их через `ControlCodesUtility.ClearControlCodes`;
- отдаёт «чистую» и «сырую» строку в `OnReceiveString`.

### Передача

```csharp
public void SendMessage(string message)
{
    TransportChannel.ClearBuffers();
    TransportChannel.WriteData(message);
}
```

### Особенности

- Нет handshake и подтверждений.
- Нет контрольных сумм.
- Нет разбиения на кадры.
- Нет тайм-аутов приёма.
- `Connect()` содержит retry-логику, реагирует только на `SocketException` с кодом `10060`.

---

## Сравнение всех хостов

| Возможность | `AstmHost` | `MiuraHost` | `MetascanHost` | `MllpHost` | `BeckmanCulterHost` |
|---|---|---|---|---|---|
| Базовый класс | — | `AstmHost` | `AstmHost` | — | — |
| Протокол | ASTM | ASTM | ASTM | HL7 / MLLP | Собственный (BC AU480) |
| Оборачивание кадра | `STX … ETX/ETB CR LF` | ASTM | ASTM | `VT … FS CR` | Нет |
| Handshake `ENQ`/`ACK` | Да | — | — | Нет | Нет |
| Подтверждения приёма | `ACK` / `NAK` | ASTM | ASTM | Нет | Нет |
| Контрольная сумма | Да | Да | Да | Нет | Нет |
| Разбиение длинных сообщений | Да | Да | Да | Нет | Нет |
| Полудуплексный режим | Да | Да | Да | Нет | Нет |
| Тайм-аут приёма | Да | Да | Да | Нет | Нет |
| `OnEOTReceived` | Используется | Используется | Используется | Заглушка | Заглушка |
| Поведение при подключении | `ACK` + пауза | Сразу `Receiving` | `ENQ` + `EOT` → `Idle` | — | — |
| Retry при `Connect()` | Да | Да | Да | Нет | Да |

---

## Публичные методы

| Метод | `AstmHost` | `MiuraHost` | `MetascanHost` | `MllpHost` | `BeckmanCulterHost` |
|---|---|---|---|---|---|
| `Connect()` | С retry | С retry | С retry | Без retry | С retry |
| `DisConnect()` | Ждёт `EOT` | Ждёт `EOT` | Ждёт `EOT` | Закрывает | Закрывает |
| `EstablishSendMode()` | `ENQ` + `ACK` | `ENQ` + `ACK` | `ENQ` + `ACK` | Ставит `Sending` | Ставит `Sending` |
| `SendMessage(string)` | ASTM-кадры | ASTM-кадры | ASTM-кадры | MLLP-обёртка | Как есть |
| `StartReceiveTimeoutTimer()` | Запускает | Запускает | Запускает | No-op | No-op |
| `StopSendMode()` | `EOT` | `EOT` | `EOT` | `Idle` | `Idle` |
| `GetClientIP()` | IP клиента | IP клиента | IP клиента | IP клиента | IP клиента |
| `Dispose()` | Освобождает | Освобождает | Освобождает | Освобождает | Освобождает |

---

## Примеры использования

### ASTM

```csharp
var channel = new TcpTransportChannel("127.0.0.1", 5000);
using (var host = new AstmHost(channel, 30))
{
    host.OnReceiveString += (s, e) => Console.WriteLine($"ASTM: {e.Data}");
    host.OnError += (s, msg) => Console.WriteLine($"ERR: {msg}");

    host.Connect();
    if (host.EstablishSendMode())
    {
        host.SendMessage("H|\\^&|||Analyzer^1.0|||||||P|E1394-97|20250101120000");
        host.StopSendMode();
    }
}
```

### Miura

```csharp
var channel = new TcpTransportChannel("192.168.1.50", 5000);
using (var host = new MiuraHost(channel))
{
    host.OnReceiveString += (s, e) => Console.WriteLine($"Miura: {e.Data}");
    host.OnError += (s, msg) => Console.WriteLine($"ERR: {msg}");

    host.Connect(); // после подключения сразу Receiving
}
```

### Metascan

```csharp
var channel = new TcpTransportChannel("192.168.1.60", 5000);
using (var host = new MetascanHost(channel))
{
    host.OnReceiveString += (s, e) => Console.WriteLine($"Metascan: {e.Data}");
    host.OnError += (s, msg) => Console.WriteLine($"ERR: {msg}");

    host.Connect(); // при подключении ENQ → EOT → Idle
    // дальше — обычная ASTM-логика
}
```

### HL7 / MLLP

```csharp
var channel = new TcpTransportChannel("127.0.0.1", 5000);
using (var host = new MllpHost(channel, 30))
{
    host.OnReceiveString += (s, e) => Console.WriteLine($"HL7: {e.Data}");
    host.Connect();
    host.EstablishSendMode();

    string hl7 = @"MSH|^~\&|Analyzer|Lab|LIS|Lab|20250101120000||ORU^R01|MSG00001|P|2.3";
    host.SendMessage(hl7);

    host.StopSendMode();
    host.DisConnect();
}
```

### Beckman Coulter AU480

```csharp
var channel = new TcpTransportChannel("192.168.1.100", 5000);
using (var host = new BeckmanCulterHost(channel, 30))
{
    host.OnReceiveString += (s, e) =>
        Console.WriteLine($"BC raw: {e.RawData}\nBC clean: {e.Data}");
    host.OnError += (s, msg) => Console.WriteLine($"ERR: {msg}");

    host.Connect();
    host.EstablishSendMode();
    host.SendMessage("some-vendor-specific-message");
    host.StopSendMode();
    host.DisConnect();
}
```

---

## Когда использовать какой хост

| Ситуация | Хост |
|---|---|
| Анализатор работает по ASTM E1394 и требует стандартного handshake. | `AstmHost` |
| Анализатор Miura: ASTM, но передачу инициирует сам прибор после подключения. | `MiuraHost` |
| Анализатор Metascan: ASTM, но при подключении требует короткой процедуры `ENQ` → `EOT`. | `MetascanHost` |
| Анализатор работает по HL7 через MLLP. | `MllpHost` |
| Анализатор Beckman Coulter AU480 с собственным форматом. | `BeckmanCulterHost` |
| Нужен «тонкий» хост без обёртки — только передать/принять строку. | `BeckmanCulterHost` (как шаблон) |

Выбор хоста определяется протоколом анализатора. Транспорт задаётся реализацией `ITransportChannel`.

---

## Как расширять

- **Новый ASTM-совместимый прибор с нестандартным поведением при подключении** — наследуйтесь от `AstmHost` и переопределяйте `OnConnectiongHandler()`, как это сделано в `MiuraHost` и `MetascanHost`.
- **Новый протокол с собственной обёрткой** — реализуйте `IHost` с нуля по образцу `MllpHost`.
- **Новый прибор с «сырым» обменом** — реализуйте `IHost` по образцу `BeckmanCulterHost`.

---

## Краткое резюме

- `IHost` — общий контракт хоста.
- `AstmHost` — полноценная реализация ASTM и база для наследников: handshake, подтверждения, кадры, контрольные суммы, retry, тайм-ауты.
- `MiuraHost` — наследник `AstmHost`, переопределяет поведение при подключении: сразу `Receiving`, без `ACK`.
- `MetascanHost` — наследник `AstmHost`, переопределяет поведение при подключении: `ENQ` → `EOT` → `Idle`.
- `MllpHost` — минимальная обёртка HL7 в MLLP: `VT … FS CR`, без подтверждений и контрольных сумм.
- `BeckmanCulterHost` — «тонкий» хост для BC AU480: передаёт и принимает строки как есть, разбор делегирует вышестоящему слою.
- Все хосты работают через `ITransportChannel` и используют единый набор событий, что позволяет заменять их на уровне `IHost`.