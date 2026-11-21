using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Core;
using AstmDriver.Models;
using System;

namespace AstmDriver.Behavior.MessageWorker
{
    /// <summary>
    /// Слой 
    /// </summary>
    public class HL7_Worker : IMessageWorker
    {
        private readonly IHost _host;
        private readonly ILogger _logger;
        private readonly DriverConfig _config;
        private readonly Analyzer _analyzer;

        public event EventHandler<string> OnError = delegate { };
        public event EventHandler<string> OnDataReciveid = delegate { };
        public event EventHandler<string> OnDataTransivied = delegate { };
        public event EventHandler<DateTime> OnLastEOT = delegate { };
        public event EventHandler<string> OnInfo = delegate { };

        public HL7_Worker(IHost host, DriverConfig config, ILogger logger, Analyzer analyzer)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer)); 

            _host.OnReceiveString += Host_OnReceiveString;
            _host.OnConnectionClosed += Host_OnConnectionClosed;
            _host.OnError += Host_OnError;
        }

        // -------------------- Host events --------------------

        private void Host_OnReceiveString(object sender, ChannelDataEventArgs e)
        {
            string hl7 = e.Data;
            if (string.IsNullOrWhiteSpace(hl7)) return;

            // Каждое входящее сообщение логируем и уведомляем UI
            OnDataReciveid?.Invoke(this, hl7);

            // Обрабатываем СИНХРОННО, порядок сегментов критичен
            try
            {
                HandleHl7Message(hl7, e.RawData);
            }
            catch (Exception ex)
            {
                _logger.Error("HL7 handle error: " + ex);
                OnError?.Invoke(this, "Ошибка разбора HL7, см. лог");
            }
        }

        private void Host_OnConnectionClosed(object sender, EventArgs e)
        {
            _logger.Debug("HL7 host connection closed");
        }

        private void Host_OnError(object sender, string e)
        {
            OnError?.Invoke(this, e);
        }

        // -------------------- HL7 processing --------------------

        private void HandleHl7Message(string hl7, string raw)
        {
            // Разделитель сегментов HL7 v2 — CR. На практике бывают \n и \r\n.
            var segments = hl7.Split(new[] { '\r', '\n' },
                                     StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0 || !segments[0].StartsWith("MSH"))
            {
                _logger.Error("HL7: сообщение без MSH: " + hl7);
                return;
            }

            // MSH-1 = field separator (всегда 4-й символ строки)
            char fieldSep = segments[0].Length > 3 ? segments[0][3] : '|';

            var mshFields = segments[0].Split(fieldSep);

            // MSH-9 (индекс 8) — тип сообщения: ORU^R01, ORM^O01, QRY^Q02, ACK
            string messageTypeRaw = GetField(mshFields, 8);
            string controlId      = GetField(mshFields, 9);   // MSH-10
            string version        = GetField(mshFields, 11);  // MSH-12
            string acceptAck      = GetField(mshFields, 15);  // MSH-15
            string appAck         = GetField(mshFields, 16);  // MSH-16

            string type = messageTypeRaw.Split('^')[0];

            _logger.Info($"HL7 IN: type={messageTypeRaw}, id={controlId}, ver={version}, ack={acceptAck}/{appAck}");

            try
            {
                switch (type)
                {
                    case "ORU": ProcessOru(segments, fieldSep, raw); break;
                    case "ORM": ProcessOrm(segments, fieldSep); break;
                    case "QRY": ProcessQry(segments, fieldSep); break;
                    case "ACK": ProcessIncomingAck(segments, fieldSep); break;
                    default:
                        _logger.Info("HL7: неподдерживаемый тип " + messageTypeRaw);
                        break;
                }

                // Accept ACK по MSH-15
                if (NeedsAcceptAck(acceptAck))
                    SendAck(controlId, messageTypeRaw, "AA", "OK");
            }
            catch (Exception ex)
            {
                _logger.Error("HL7 processing error: " + ex);
                if (NeedsAcceptAck(acceptAck))
                    SendAck(controlId, messageTypeRaw, "AE", "Processing error");
                throw;
            }
        }

        private static bool NeedsAcceptAck(string msh15)
        {
            // AL — always, SU — on success, ER — on error, NE — never
            return msh15 == "AL" || msh15 == "SU" || msh15 == "ER";
        }

        // -------------------- Handlers by message type --------------------

        private void ProcessOru(string[] segments, char fieldSep, string raw)
        {
            string patientId = null;
            string sampleId  = null;

            foreach (var seg in segments)
            {
                if (seg.StartsWith("PID"))
                {
                    var f = seg.Split(fieldSep);
                    // PID-3 — идентификатор пациента
                    if (f.Length > 3) patientId = f[3].Split('^')[0];
                }
                else if (seg.StartsWith("OBR"))
                {
                    var f = seg.Split(fieldSep);
                    // OBR-3 — Filler Order Number (обычно штрих-код/ID пробирки)
                    if (f.Length > 3) sampleId = f[3].Split('^')[0];
                }
                else if (seg.StartsWith("OBX"))
                {
                    var f = seg.Split(fieldSep);
                    var testCode = f.Length > 3 ? f[3] : "";  // OBX-3
                    var value    = f.Length > 5 ? f[5] : "";  // OBX-5
                    var units    = f.Length > 6 ? f[6] : "";  // OBX-6

                    // TODO: ваша бизнес-логика — сопоставить с заказом и записать в БД
                    _logger.Info($"OBX patient={patientId} sample={sampleId} " +
                                 $"test={testCode} value={value} {units}");
                }
            }

            // Сырой лог — как в ProcessResultMessage из базы
            Dal.WriteLog(_host.GetClientIP(), raw, _config.Port);
        }

        private void ProcessOrm(string[] segments, char fieldSep)
        {
            // Заказ от LIS — TODO
        }

        private void ProcessQry(string[] segments, char fieldSep)
        {
            foreach (var segment in segments)
            {
                _logger.Info($"Qry: {segment}");
            }
            // Запрос от анализатора — TODO
        }

        private void ProcessIncomingAck(string[] segments, char fieldSep)
        {
            // Мы получили ACK на отправленное сообщение
            foreach (var seg in segments)
            {
                if (seg.StartsWith("MSA"))
                {
                    var f = seg.Split(fieldSep);
                    var code = f.Length > 1 ? f[1] : "";  // MSA-1: AA/AE/AR
                    var ctrl = f.Length > 2 ? f[2] : "";  // MSA-2
                    _logger.Info($"HL7 IN ACK: code={code} for {ctrl}");
                }
            }
        }

        // -------------------- Sending --------------------

        /// Отправка ОДНОГО HL7-сообщения. Hl7Host оборачивает в VT...FS CR.
        public void SendHl7(string hl7Message)
        {
            _host.SendMessage(hl7Message);
            OnDataTransivied?.Invoke(this, hl7Message);
        }

        private void SendAck(string controlId, string origMessageType, string code, string text)
        {
            var now   = DateTime.Now.ToString("yyyyMMddHHmmss");
            var ackId = Guid.NewGuid().ToString("N").Substring(0, 20);

            // Обратите внимание: MSH-9 в ACK — это ACK^<событие исходного>
            var ackType = "ACK" + (origMessageType.Contains("^")
                ? origMessageType.Substring(origMessageType.IndexOf('^'))
                : "");

            // Разделитель сегментов — строго \r
            var ack =
                $"MSH|^~\\&|LIS|LAB|ANALYZER|LAB|{now}||{ackType}|{ackId}|P|2.3.1\r" +
                $"MSA|{code}|{controlId}|{text}\r";

            SendHl7(ack);
            _logger.Info($"HL7 OUT ACK {code} for {controlId}");
        }

        // -------------------- helpers --------------------

        private static string GetField(string[] fields, int index)
            => index >= 0 && index < fields.Length ? fields[index].Trim() : string.Empty;
    }
}