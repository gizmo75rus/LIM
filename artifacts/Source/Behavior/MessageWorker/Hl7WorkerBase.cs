using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Core;
using System;
using System.Linq;
using AstmDriver.Models;

namespace AstmDriver.Behavior.MessageWorker
{
    /// <summary>
    /// Базовая реализация HL7-воркера.
    ///
    /// Отвечает за:
    /// - подписку на события IHost (приём HL7, разрыв соединения, ошибки);
    /// - разбиение HL7-сообщения на сегменты;
    /// - разбор MSH (MSH-9 тип, MSH-10 control ID, MSH-12 версия, MSH-15/16);
    /// - диспетчеризацию по типу (ORU / ORM / QRY / ACK);
    /// - отправку HL7 (SendHl7) и каркас ACK (SendAck).
    ///
    /// Наследники реализуют:
    /// - ProcessOru (обязательно);
    /// - ShouldSendAck (политика подтверждений);
    /// - при необходимости — ProcessQry, ProcessOrm, ProcessIncomingAck, SendAck.
    /// </summary>
    public abstract class Hl7WorkerBase : IMessageWorker
    {
        #region Fields
        protected readonly IHost _host;
        protected readonly ILogger _logger;
        protected readonly Analyzer _analyzer;

        // Счётчик исходящих сообщений (для случая, когда воркер сам инициирует обмен).
        // Для ответов на входящие используется MSH-10 входящего сообщения.
        private int _outgoingMessageId = 1;
        #endregion

        #region Events
        public event EventHandler<string> OnError = delegate { };
        public event EventHandler<string> OnDataReciveid = delegate { };
        public event EventHandler<string> OnDataTransivied = delegate { };
        public event EventHandler<DateTime> OnLastEOT = delegate { };
        public event EventHandler<string> OnInfo = delegate { };
        #endregion

        #region Ctors
        protected Hl7WorkerBase(IHost host, ILogger logger, Analyzer analyzer)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));

            _host.OnReceiveString += Host_OnReceiveString;
            _host.OnConnectionClosed += Host_OnConnectionClosed;
            _host.OnError += Host_OnError;
        }
        #endregion

        #region Host event handlers (не переопределяются)
        private void Host_OnReceiveString(object sender, ChannelDataEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;

            OnDataReciveid?.Invoke(this, e.Data);

            try
            {
                HandleHl7Message(e.Data, e.RawData);
            }
            catch (Exception ex)
            {
                _logger.Error("HL7 handle error: " + ex);
                OnError?.Invoke(this, "Ошибка разбора HL7, см. лог");
            }
        }

        private void Host_OnConnectionClosed(object sender, EventArgs e)
            => _logger.Debug("HL7 host connection closed");

        private void Host_OnError(object sender, string e)
            => OnError?.Invoke(this, e);
        #endregion

        #region Разбор и диспетчеризация
        /// <summary>
        /// Разбирает входящее HL7-сообщение и вызывает соответствующий Process-метод.
        /// </summary>
        protected virtual void HandleHl7Message(string hl7, string raw)
        {
            var segments = hl7.Split(new[] { '\r', '\n' },
                                     StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0 || !segments[0].StartsWith("MSH"))
            {
                _logger.Error("HL7: сообщение без MSH: " + hl7);
                return;
            }

            // MSH-1 = разделитель полей = 4-й символ (индекс 3).
            char fieldSep = segments[0].Length > 3 ? segments[0][3] : '|';

            var msh = segments[0].Split(fieldSep);

            // ВАЖНО: индексы в массиве полей сдвинуты на 1 относительно номеров MSH.
            // fields[0] = "MSH", fields[1] = MSH-2, fields[N] = MSH-(N+1).
            string messageTypeRaw = GetField(msh, 8);   // MSH-9
            string controlId      = GetField(msh, 9);   // MSH-10
            string version        = GetField(msh, 11);  // MSH-12
            string msh15          = GetField(msh, 14);  // MSH-15 Accept Ack Type
            string msh16          = GetField(msh, 15);  // MSH-16 Application Ack Type / Result Type

            string type = messageTypeRaw.Split('^')[0];

            _logger.Info($"HL7 IN: type={messageTypeRaw}, id={controlId}, " +
                         $"ver={version}, msh15={msh15}, msh16={msh16}");

            try
            {
                switch (type)
                {
                    case "ORU":
                        ProcessOru(segments, fieldSep, raw, controlId, msh16);
                        break;
                    case "ORM":
                        ProcessOrm(segments, fieldSep, controlId, msh16);
                        break;
                    case "QRY":
                        ProcessQry(segments, fieldSep, controlId);
                        break;
                    case "ACK":
                        ProcessIncomingAck(segments, fieldSep);
                        break;
                    default:
                        _logger.Info("HL7: неподдерживаемый тип " + messageTypeRaw);
                        break;
                }

                if (ShouldSendAck(type, msh15, msh16))
                    SendAck(controlId, messageTypeRaw, "AA", "Message accepted");
            }
            catch (Exception ex)
            {
                _logger.Error("HL7 processing error: " + ex);
                if (ShouldSendAck(type, msh15, msh16))
                    SendAck(controlId, messageTypeRaw, "AE", "Processing error");
                throw;
            }
        }

        /// <summary>
        /// Обработка ORU (результаты). Обязательна к реализации в наследнике.
        /// </summary>
        /// <param name="msh16">MSH-16: 0 — результат образца, 1 — калибровка (не поддерж.), 2 — QC.</param>
        protected abstract void ProcessOru(string[] segments, char fieldSep,
                                           string raw, string controlId, string msh16);

        /// <summary>
        /// Обработка ORM (заказ от LIS). По умолчанию — лог.
        /// </summary>
        protected virtual void ProcessOrm(string[] segments, char fieldSep,
                                          string controlId, string msh16)
            => _logger.Info("HL7 ORM не реализован в " + GetType().Name);

        /// <summary>
        /// Обработка QRY (запрос от анализатора). По умолчанию — лог.
        /// Наследник должен сформировать DSR^Q03 и отправить его.
        /// </summary>
        protected virtual void ProcessQry(string[] segments, char fieldSep,
                                          string controlId)
            => _logger.Info("HL7 QRY не реализован в " + GetType().Name);

        /// <summary>
        /// Обработка входящего ACK от анализатора (например, ACK^Q03 после DSR).
        /// </summary>
        protected virtual void ProcessIncomingAck(string[] segments, char fieldSep)
        {
            foreach (var seg in segments)
            {
                if (seg.StartsWith("MSA"))
                {
                    var f = seg.Split(fieldSep);
                    var code = GetField(f, 1);  // MSA-1: AA / AE / AR
                    var ctrl = GetField(f, 2);  // MSA-2: control ID исходного
                    _logger.Info($"HL7 IN ACK: code={code} for {ctrl}");
                }
            }
        }

        /// <summary>
        /// Нужно ли отвечать ACK на входящее сообщение.
        /// Базовая политика — стандартная для HL7: ориентироваться на MSH-15.
        /// Наследники переопределяют (например, MedicalSystem требует ACK всегда на ORU).
        /// </summary>
        protected virtual bool ShouldSendAck(string type, string msh15, string msh16)
        {
            // AL — always, SU — on success, ER — on error, NE — never
            bool acceptsAck = msh15 == "AL" || msh15 == "SU" || msh15 == "ER";
            return type == "ORU" && acceptsAck;
        }
        #endregion

        #region Отправка
        /// <summary>
        /// Отправка одного HL7-сообщения. MllpHost сам оборачивает в VT...FS CR.
        /// </summary>
        public void SendHl7(string hl7Message)
        {
            _host.SendMessage(hl7Message);
            OnDataTransivied?.Invoke(this, hl7Message);
        }

        /// <summary>
        /// Формирует и отправляет ACK на исходное сообщение.
        /// MSH-10 в ACK совпадает с MSH-10 исходного сообщения (HL7 v2.3.1).
        /// </summary>
        protected virtual void SendAck(string controlId, string origMessageType,
                                       string code, string text)
        {
            var now = DateTime.Now.ToString("yyyyMMddHHmmss");

            // ACK^<событие исходного>: ORU^R01 → ACK^R01.
            string ackType = "ACK" + (origMessageType.Contains("^")
                ? origMessageType.Substring(origMessageType.IndexOf('^'))
                : "");

            // MSH-3/4 — отправитель (LIS), MSH-5/6 — получатель (анализатор).
            var sendingApp   = _analyzer != null ? (_analyzer.astm_system_name   ?? "LIS")      : "LIS";
            var sendingFac   = _analyzer != null ? (_analyzer.astm_name ?? "LAB")      : "LAB";
            var receivingApp = _analyzer != null ? (_analyzer.astm_name ?? "ANALYZER") : "ANALYZER";
            var receivingFac = _analyzer != null ? (_analyzer.astm_name ?? "LAB")      : "LAB";

            var ack =
                $"MSH|^~\\&|{sendingApp}|{sendingFac}|{receivingApp}|{receivingFac}|" +
                $"{now}||{ackType}|{controlId}|P|2.3.1||||0||ANSI|\r" +
                $"MSA|{code}|{controlId}|{text}|||0\r";

            SendHl7(ack);
            _logger.Info($"HL7 OUT {ackType} {code} for {controlId}");
        }

        /// <summary>
        /// Следующий исходящий Message Control ID. Нужен только для сообщений,
        /// которые воркер инициирует сам (не ответ на входящее).
        /// </summary>
        protected string NextMessageControlId() => (_outgoingMessageId++).ToString();
        #endregion

        #region Хелперы
        protected static string GetField(string[] fields, int index)
            => index >= 0 && index < fields.Length ? fields[index].Trim() : string.Empty;

        /// <summary>Первый компонент поля (до символа '^').</summary>
        protected static string FirstComponent(string field)
            => string.IsNullOrEmpty(field) ? string.Empty : field.Split('^')[0];

        /// <summary>Компонент поля по индексу (разделитель '^').</summary>
        protected static string Component(string field, int index)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            var parts = field.Split('^');
            return index >= 0 && index < parts.Length ? parts[index] : string.Empty;
        }

        protected void OnErrorInvoke(string message)
        {
            _logger.Error(message);
            OnError?.Invoke(this, message);
        }

        protected void OnInfoInvoke(string message)
        {
            _logger.Info(message);
            OnInfo?.Invoke(this, message);
        }
        #endregion
    }
}