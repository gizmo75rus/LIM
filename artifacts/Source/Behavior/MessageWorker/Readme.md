Ниже — обновлённый раздел `## MessageWorker` с учётом правок: HL7-ветка теперь тоже абстрактная база `Hl7WorkerBase` + вендорский `MedicalSystem_Worker`, реализованы ACK, QC-ветка и QRY→DSR.

---

## MessageWorker

### Назначение

Пространство `AstmDriver.Behavior.MessageWorker` — это **слой обработки сообщений**, который стоит между `IHost` (протокол + транспорт) и прикладной логикой (DAL, бизнес-правила, ответы анализатору).

Хост отвечает только за приём и передачу кадров. Всё, что связано с **смыслом** этих кадров — разбор пакета на записи/сегменты, определение типа сообщения, обработка (result / query / order), запись в БД, формирование ответа — делает `MessageWorker`.

Задачи слоя:

- принять данные от `IHost.OnReceiveString`;
- разобрать пакет (ASTM-записи или HL7-сегменты);
- определить тип сообщения;
- выполнить обработку (записать результат, собрать ответ на запрос, обработать ордер);
- при необходимости — отправить ответ анализатору через `IHost`.

---

### Архитектура

Слой `MessageWorker` построен **симметрично** для двух протоколов и имеет две независимые ветки:

- **ASTM-ветка** — общая база `AstmWorkerBase` + вендорские наследники (`Default_Worker`, `Saphir400_Worker`, ...). Работает поверх ASTM-хостов (`AstmHost`, `MiuraHost`, `MetascanHost`, `BeckmanCulterHost`).
- **HL7-ветка** — общая база `Hl7WorkerBase` + вендорские наследники (`MedicalSystem_Worker`, ...). Работает поверх `MllpHost`.

Обе базы реализуют `IMessageWorker` и предоставляют общий набор событий.

```
Анализатор
    ↓
ITransportChannel
    ↓
IHost (AstmHost / MllpHost / ...)
    ↓ OnReceiveString
                ┌────────────────────────────────────────┐
                │  MessageWorker                         │
                │                                        │
                │  AstmWorkerBase (abstract) ──► ASTM    │
                │    ├── Default_Worker                  │
                │    └── Saphir400_Worker                │
                │                                        │
                │  Hl7WorkerBase  (abstract) ──► HL7     │
                │    └── MedicalSystem_Worker            │
                └────────────────────────────────────────┘
    ↓
Process* / HandleHl7Message
    ↓
Dal / TrySendToAnalyzer / SendHl7 / SendAck
    ↓
IHost.EstablishSendMode / SendMessage / StopSendMode
    ↓
Анализатор
```

---

### Классы

| Класс | База | Протокол | Роль |
|---|---|---|---|
| `IMessageWorker` | — | — | Контракт воркера. |
| `AstmWorkerBase` | `IMessageWorker` | ASTM | Общая база: разбор ASTM-пакетов, очередь, обработка, отправка. |
| `Default_Worker` | `AstmWorkerBase` | ASTM | Дефолтный ASTM-воркер: результаты в лог, query/order игнорируются. |
| `Saphir400_Worker` | `AstmWorkerBase` | ASTM | Воркер для Saphir 400: обрабатывает запросы и отправляет задания. |
| `Hl7WorkerBase` | `IMessageWorker` | HL7 | Общая база: разбор MSH/сегментов, диспетчеризация, ACK, QRY/DSR. |
| `MedicalSystem_Worker` | `Hl7WorkerBase` | HL7 | Воркер для MedicalSystem (MS-380/680/2080/MS-L8080). |
| `Message` | — | ASTM | Модель ASTM-пакета: `Records`, `RawData`, флаги `IsResult` / `IsQuery` / `IsOrder`. |

---

## ASTM-ветка — `AstmWorkerBase`

Абстрактная база для всех ASTM-воркеров.

### Конструктор и зависимости

```csharp
public AstmWorkerBase(IHost host, DriverConfig config)
```

Зависимости:

- `IHost host` — источник входящих данных и приёмник исходящих;
- `DriverConfig config` — конфигурация драйвера (`RemoteHost`, `Port`, ...);
- `ILogger` — логгер;
- `Dal` — доступ к БД;
- `Analyzer _analyzer` — сущность анализатора, полученная через `Dal.GetAnalyzer(config.RemoteHost, config.Port)`.

При создании база:

- подписывается на события хоста: `OnReceiveString`, `OnEOTReceived`, `OnConnectionClosed`;
- инициализирует очередь `ConcurrentQueue<Message> _messages`.

### Приём данных

`_host_OnReceiveString`:

- разбивает `e.Data` по `\r` на отдельные записи;
- для каждой непустой записи вызывает `OnDataReciveid`;
- каждую запись отправляет в `Task.Run(() => ParseReceivedRecord(...))`.

### Разбор записей

`ParseReceivedRecord` — ключевой метод разбора ASTM. По первой букве записи определяется её роль:

| Первая буква | Значение | Что делает база |
|---|---|---|
| `H` | Начало пакета | Создаёт новый `Message`. |
| `Q` | Query | Помечает `IsQuery = true`. |
| `R` | Result | Помечает `IsResult = true`. |
| `O` | Order | Помечает `IsOrder = true`. |
| `L` | Конец пакета | Кладёт клон в `_messages`, сбрасывает `_currentMessage`. |

### Обработка очереди

`ProceesData()` вызывается по `OnEOTReceived`:

- выполняется в `Task.Run`;
- если из очереди удалось достать `Message`:
    - `IsResult` → `ProcessResultMessage`;
    - `IsQuery` → `ProcessQueryMessage`;
    - `IsOrder` → `ProcessOrderMessage`;
- после обработки, если очередь не пуста — рекурсивный вызов;
- исключения логируются и передаются в `OnError`.

### Базовая обработка результатов

`ProcessResultMessage` пишет количество записей `R|` и передаёт каждую сырую строку в `Dal.WriteLog(_host.GetClientIP(), raw, _config.Port)`.

### Отправка ответа анализатору

`TrySendToAnalyzer(List<string> packetBuffer)`:

- `_host.EstablishSendMode()`;
- построчно `_host.SendMessage($"{line}{CR}")` с паузой 50 мс;
- `_host.StopSendMode()`;
- генерирует `OnDataTransivied` на каждую строку;
- ошибки логирует и отдаёт в `OnError`.

### События

| Событие | Когда |
|---|---|
| `OnDataReciveid` | Принята запись от анализатора. |
| `OnDataTransivied` | Отправлена запись анализатору. |
| `OnLastEOT` | Получен `EOT`. |
| `OnInfo` | Информационное сообщение. |
| `OnError` | Ошибка разбора / обработки / отправки. |

### Наследники

#### `Default_Worker`

```csharp
public class Default_Worker : AstmWorkerBase
{
    public Default_Worker(IHost host, DriverConfig config) : base(host, config) { }

    internal override void ProcessQueryMessage(Message message) { }
    internal override void ProcessOrderMessage(Message message) { }
}
```

Дефолтный ASTM-воркер. Результаты обрабатываются базовым методом (запись в лог), query/order игнорируются.

#### `Saphir400_Worker`

Воркер для анализатора **Saphir 400** (имя в конфиге — `Saphir400`). Переопределяет только `ProcessQueryMessage`.

**Логика:**

1. Из `message.Records` берёт записи `Q`, разбирает по `|`.
2. `fields[2]` — штрих-код пробирки (может быть `^barcode`).
3. Формирует заголовок `H|\^&|||HOST^P_1|||||<astm_name>||P|1|<yyyyMMddHHmmss>`.
4. Для каждого штрих-кода:
    - через `Dal.AnalyzerSEND_GetCoderByBarcode(barcode, ip, port)` получает коды анализов;
    - добавляет `P|<patientCount>`;
    - формирует `O|<orderCount>|<barcode>|0|^^^<assy_num>^<code>^0\...|R||||||N||||Serum||||||||||O`;
    - счётчики `patientCount` (1..7) и `orderCount` инкрементируются.
5. Добавляет `L|1|N`.
6. Отправляет через `TrySendToAnalyzer`.

---

## HL7-ветка — `Hl7WorkerBase`

Абстрактная база для всех HL7-воркеров.

### Конструктор и зависимости

```csharp
protected Hl7WorkerBase(IHost host, DriverConfig config)
```

Зависимости — те же, что и у `AstmWorkerBase`:

- `IHost host`,
- `DriverConfig config`,
- `ILogger`,
- `Analyzer _analyzer` (`Dal.GetAnalyzer(_config.RemoteHost, _config.Port)`).

При создании база:

- подписывается на события хоста: `OnReceiveString`, `OnConnectionClosed`, `OnError`;
- инициализирует счётчик исходящих control id `_outgoingMessageId = 1`.

### Приём данных

`Host_OnReceiveString`:

- берёт `e.Data` как целое HL7-сообщение;
- вызывает `OnDataReciveid`;
- **синхронно** вызывает `HandleHl7Message(hl7, e.RawData)` — потому что порядок сегментов HL7 критичен;
- исключения логирует и передаёт в `OnError`.

> В отличие от ASTM-ветки, здесь нет `Task.Run` на каждое сообщение — HL7 обрабатывается целиком и сразу.

### Разбор HL7

`HandleHl7Message(string hl7, string raw)`:

1. Разбивает на сегменты по `\r` / `\n`, `RemoveEmptyEntries`.
2. Проверяет, что первый сегмент начинается с `MSH`.
3. Определяет разделитель полей (`MSH-1` = 4-й символ строки).
4. Извлекает из `MSH`:

   | Поле | Что содержит |
      |---|---|
   | `MSH-9` | Тип (`ORU^R01`, `ORM^O01`, `QRY^Q02`, `ACK^R01`, `DSR^Q03`, ...) |
   | `MSH-10` | `controlId` |
   | `MSH-12` | Версия |
   | `MSH-15` | `Accept Ack Type` (`AL` / `SU` / `ER` / `NE`) |
   | `MSH-16` | `Application Ack Type` / тип результата |

   > **Важно:** в массиве `fields[]` индексы сдвинуты на 1 относительно номеров MSH: `fields[N] = MSH-(N+1)`. То есть `msh15 = fields[14]`, `msh16 = fields[15]`.

5. Берёт базовый тип и диспетчеризует:

   | Тип | Хук |
      |---|---|
   | `ORU` | `ProcessOru` — **abstract**, обязателен к реализации |
   | `ORM` | `ProcessOrm` — virtual, по умолчанию лог |
   | `QRY` | `ProcessQry` — virtual, по умолчанию лог |
   | `ACK` | `ProcessIncomingAck` — virtual, базовая реализация разбирает `MSA` |
   | иное | Логирование «неподдерживаемый тип» |

6. Если `ShouldSendAck(type, msh15, msh16)` вернул `true` — вызывает `SendAck(controlId, messageTypeRaw, "AA", "Message accepted")`.
7. При исключении — если `ShouldSendAck` вернул `true`, отправляет `SendAck(..., "AE", "Processing error")` и пробрасывает исключение.

### Политика отправки ACK

```csharp
protected virtual bool ShouldSendAck(string type, string msh15, string msh16)
{
    // AL — always, SU — on success, ER — on error, NE — never
    bool acceptsAck = msh15 == "AL" || msh15 == "SU" || msh15 == "ER";
    return type == "ORU" && acceptsAck;
}
```

Базовая реализация — стандартная для HL7 (ориентируется на `MSH-15`). Вендоры, у которых `MSH-15` не заполняется, **переопределяют** метод (пример: `MedicalSystem_Worker` отвечает ACK на ORU всегда).

### Отправка

| Метод | Что делает |
|---|---|
| `SendHl7(string hl7Message)` | Публичный метод отправки одного HL7-сообщения. Вызывает `_host.SendMessage(...)` (MllpHost сам оборачивает в MLLP) и поднимает событие `OnDataTransivied`. |
| `SendAck(controlId, origType, code, text)` | Формирует `MSH + MSA`. **`MSH-10` в ACK = `MSH-10` исходного сообщения** (требование HL7 v2.3.1). Отправитель/получатель берутся из `_analyzer`. |
| `NextMessageControlId()` | Инкремент `_outgoingMessageId` — для сообщений, которые воркер инициирует сам (не ответ на входящее). |

### Хелперы

- `GetField(fields, index)` — безопасное извлечение поля;
- `FirstComponent(field)` — первый компонент (до `^`);
- `Component(field, index)` — компонент по индексу;
- `OnErrorInvoke(message)` / `OnInfoInvoke(message)` — обёртки с логированием.

### События

| Событие | Когда |
|---|---|
| `OnDataReciveid` | Принято HL7-сообщение. |
| `OnDataTransivied` | Отправлено HL7-сообщение или ACK. |
| `OnLastEOT` | Объявлено для совместимости; в HL7-ветке не используется. |
| `OnInfo` | Информационное сообщение. |
| `OnError` | Ошибка разбора или проброс ошибки хоста. |

### Наследник — `MedicalSystem_Worker`

Воркер для анализаторов **MedicalSystem MS-380 / MS-680 / MS-2080 / MS-L8080** (HL7 v2.3.1 поверх MLLP).

```csharp
public class MedicalSystem_Worker : Hl7WorkerBase
{
    public MedicalSystem_Worker(IHost host, DriverConfig config)
        : base(host, config) { }

    protected override bool ShouldSendAck(string type, string msh15, string msh16)
        => type == "ORU";

    protected override void ProcessOru(string[] segments, char fieldSep,
                                       string raw, string controlId, string msh16)
    { /* образец или QC */ }

    protected override void ProcessQry(string[] segments, char fieldSep,
                                       string controlId)
    { /* QRY^Q02 → DSR^Q03 */ }
}
```

#### Особенности вендора (из документации MedicalSystem)

- `MSH-15` **не используется**, ACK на ORU обязателен → `ShouldSendAck` переопределён.
- `MSH-16` = тип результата: `0` — образец, `1` — калибровка (не поддерж.), `2` — QC.
- При `MSH-16 = 2` поля `OBR-11..23` **перегружены** под QC-параметры (`times`, `module^ring`, `qcName`, `sampleType`, `lot`, `mean`, `SD`, `result`, `units`, `flag`, `qcRule`).
- `OBX-3` — порядковый номер, `OBX-4` — `NAME^repeat`.
- `QRY^Q02` → ответ `DSR^Q03` (`MSH + MSA + 3× DSP`).

#### `ProcessOru`

Развилка по `MSH-16`:

- `MSH-16 = 0` → `ProcessPatientResult`;
- `MSH-16 = 2` → `ProcessQualityControl`.

**`ProcessPatientResult`:**

- `PID-3` → `patientId` (medical record number);
- `OBR-2` → `sampleBarcode`, `OBR-3` → `sampleNumber`, `OBR-10` → `rackPos`, `OBR-20` → `examDoctor`;
- для каждого `OBX`:
    - `OBX-2` — тип (`NM` / `ST`),
    - `OBX-3` — порядковый номер,
    - `OBX-4` — `NAME^repeat`,
    - `OBX-5` — значение,
    - `OBX-6` — единицы,
    - `OBX-7` — референс,
    - `OBX-8` — флаг (`L` / `H` / `N`),
    - `OBX-10` — природа (`F` / `C`),
    - `OBX-13` — сырой результат,
    - `OBX-14` — время,
    - `OBX-16` — наблюдатель;
- логирует и передаёт в `Dal.WriteLog`.

**`ProcessQualityControl`:**

- проходит по `OBR`;
- читает QC-параметры из `OBR-11..23`;
- логирует и передаёт в `Dal.WriteLog`.

#### `ProcessQry` → `DSR`

1. Из `QRD` разбирает:
    - `QRD-3` — `BC` / `SN`,
    - `QRD-9` — subject (barcode или sample number),
    - `QRD-11` — `rack^pos`,
    - `QRD-12` — dilution.
2. Через `Dal.GetPatientBySubject` и `Dal.GetAssignedItemsBySubject` получает пациента и назначения.
3. Если данных нет — `SendDsrNotFound` (`MSA|NF`) или `MSA|AR` с текстом.
4. Если есть — собирает `DSR^Q03`:
    - `MSH` + `MSA|OK`,
    - `DSP|1|...` — patient info (`DSP-N = PID-(N-1)`),
    - `DSP|2|...` — sample info (`DSP-N = OBR-(N-1)`),
    - `DSP|3|<count>|<items>` — проекты через `^`.
5. Отправляет через `SendHl7`.

#### `ProcessIncomingAck`

Не переопределяется — базовой реализации достаточно: она логирует `MSA-1` (`AA` / `AE` / `AR`) и `MSA-2` (control ID исходного сообщения). Используется для `ACK^Q03`, который анализатор шлёт в подтверждение `DSR`.

---

## Жизненные циклы

### ASTM (на примере `Saphir400_Worker`)

1. Анализатор отправляет `H`, `Q`, `L`.
2. `IHost` собирает кадры и по `EOT` генерирует `OnEOTReceived`.
3. `AstmWorkerBase._host_OnReceiveString` разбивает данные по `\r`.
4. `ParseReceivedRecord` создаёт `Message`, выставляет флаги, кладёт клон в очередь по `L`.
5. `_host_OnEOTReceived` → `ProceesData`.
6. `ProceesData` → `Saphir400_Worker.ProcessQueryMessage`.
7. `ProcessQueryMessage` собирает пакет `H / P / O / L` и вызывает `TrySendToAnalyzer`.
8. `TrySendToAnalyzer` → `IHost.EstablishSendMode / SendMessage / StopSendMode`.

### HL7 ORU (на примере `MedicalSystem_Worker`)

1. Анализатор передаёт `ORU^R01` в MLLP-обёртке.
2. `MllpHost` снимает `VT ... FS CR`, генерирует `OnReceiveString` с чистым HL7.
3. `Hl7WorkerBase.Host_OnReceiveString` → `HandleHl7Message`.
4. По `MSH-9` → `ProcessOru`.
5. `ProcessOru` разбирает `PID / OBR / OBX` (или QC-ветку при `MSH-16 = 2`), пишет в `Dal.WriteLog` (и в будущем — в БД).
6. `ShouldSendAck` → `true` (MedicalSystem отвечает ACK всегда).
7. `SendAck` формирует `MSH + MSA`, отправляет через `SendHl7` → `MllpHost.SendMessage`.

### HL7 QRY → DSR (на примере `MedicalSystem_Worker`)

1. Анализатор передаёт `QRY^Q02`.
2. `MllpHost` → `OnReceiveString` → `HandleHl7Message`.
3. `ProcessQry` разбирает `QRD`, обращается к `Dal`.
4. Формируется `DSR^Q03` (`MSH + MSA|OK + DSP1 + DSP2 + DSP3`).
5. `SendHl7` → `MllpHost.SendMessage`.
6. Анализатор подтверждает приём `ACK^Q03`, который обрабатывается `ProcessIncomingAck`.

---

## Связь с `IHost`

Воркеры **никогда не работают с транспортом напрямую**. Всё взаимодействие — только через `IHost`:

| Направление | `AstmWorkerBase` | `Hl7WorkerBase` |
|---|---|---|
| Из хоста в воркер | `OnReceiveString`, `OnEOTReceived`, `OnConnectionClosed` | `OnReceiveString`, `OnConnectionClosed`, `OnError` |
| Из воркера в хост | `EstablishSendMode`, `SendMessage`, `StopSendMode`, `GetClientIP` | `SendMessage`, `GetClientIP` |

Такое разделение позволяет использовать ASTM-воркеры с любым ASTM-хостом, а HL7-воркеры — с `MllpHost`, без изменений в логике протокола.

---

## Связь с `Dal`

| Метод `Dal` | Где используется |
|---|---|
| `Dal.GetAnalyzer(host, port)` | `AstmWorkerBase`, `Hl7WorkerBase` — загрузка `Analyzer`. |
| `Dal.WriteLog(ip, raw, port)` | `AstmWorkerBase.ProcessResultMessage`, `MedicalSystem_Worker.ProcessPatientResult` / `ProcessQualityControl` — сырой лог. |
| `Dal.AnalyzerSEND_GetCoderByBarcode(barcode, ip, port)` | `Saphir400_Worker.ProcessQueryMessage` — назначенные анализы по штрих-коду. |
| `Dal.GetPatientBySubject(...)` | `MedicalSystem_Worker.ProcessQry` — пациент по barcode/sample number *(к добавлению)*. |
| `Dal.GetAssignedItemsBySubject(...)` | `MedicalSystem_Worker.ProcessQry` — список назначенных проектов *(к добавлению)*. |

---

## Сводная таблица воркеров

| Воркер | База | Протокол | `ProcessResult` | `ProcessQuery` | `ProcessOrder` | Отправка |
|---|---|---|---|---|---|---|
| `Default_Worker` | `AstmWorkerBase` | ASTM | Базовый (лог) | Пусто | Пусто | `TrySendToAnalyzer` |
| `Saphir400_Worker` | `AstmWorkerBase` | ASTM | Базовый (лог) | Формирует `O`-записи | Базовый (пусто) | `TrySendToAnalyzer` |
| `MedicalSystem_Worker` | `Hl7WorkerBase` | HL7/MLLP | `ProcessOru` (образец + QC) | `ProcessQry` → DSR | Базовый (`ProcessOrm`) | `SendHl7` / `SendAck` |

---

## Как расширять

- **Новый ASTM-прибор с собственной логикой результатов** → наследуйся от `AstmWorkerBase`, переопредели `ProcessResultMessage`. По умолчанию он пишет в лог; обычно нужно парсить `Records` и писать в БД.
- **Поддержка запросов от анализатора по ASTM** → переопредели `ProcessQueryMessage`, формируй ответ и отправляй через `TrySendToAnalyzer` (пример — `Saphir400_Worker`).
- **Поддержка ордеров по ASTM** → переопредели `ProcessOrderMessage`.
- **Новый HL7-вендор** → наследуйся от `Hl7WorkerBase`:
    - **обязательно** реализуй `ProcessOru`;
    - переопредели `ShouldSendAck`, если вендор не следует стандартной политике HL7 по `MSH-15`;
    - при необходимости — `ProcessQry` (для двусторонки), `ProcessOrm`, `SendAck` (если вендор требует свой формат);
    - пример — `MedicalSystem_Worker`.
- **Двусторонний обмен по HL7 (QRY → DSR)** → реализуй `ProcessQry` в наследнике `Hl7WorkerBase`, формируй `DSR^Q03` и отправляй через `SendHl7`.
- **QC-результаты по HL7** → обрабатывай `MSH-16 = 2` в `ProcessOru` — там другой набор полей `OBR`.
- **Новый протокол, отличный от ASTM и HL7** → реализуй `IMessageWorker` с нуля: подпишись на `IHost.OnReceiveString`, разбери свой формат, обработай, при необходимости отправь ответ через `IHost.SendMessage`.
