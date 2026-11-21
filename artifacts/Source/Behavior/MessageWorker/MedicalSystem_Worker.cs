using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using AstmDriver.Models;

namespace AstmDriver.Behavior.MessageWorker
{
    /// <summary>
    /// Воркер для анализаторов MedicalSystem: MS-380 / MS-680 / MS-2080 / MS-L8080.
    /// Протокол: HL7 v2.3.1 поверх MLLP.
    ///
    /// Особенности вендора (см. MedicalSystem Instruments LIS Online Manual):
    /// - MSH-15 не используется, но ACK на ORU обязателен;
    /// - MSH-16 = тип результата: 0 — образец, 1 — калибровка (не поддерж.), 2 — QC;
    /// - при MSH-16=2 поля OBR-11..23 перегружены под параметры QC;
    /// - OBX-3 — порядковый номер, OBX-4 — имя проекта ^ число повторов;
    /// - QRY^Q02 → ответ DSR^Q03 (MSH + MSA + 3× DSP).
    /// </summary>
    public class MedicalSystem_Worker : Hl7WorkerBase
    {
        public MedicalSystem_Worker(IHost host, ILogger logger,Analyzer analyzer)
            : base(host, logger, analyzer) { }

        #region ShouldSendAck
        /// <summary>
        /// MedicalSystem всегда ждёт ACK на ORU, независимо от MSH-15.
        /// На QRY отвечает DSR (не ACK).
        /// </summary>
        protected override bool ShouldSendAck(string type, string msh15, string msh16)
            => type == "ORU";
        #endregion

        #region ORU
        protected override void ProcessOru(string[] segments, char fieldSep,
                                           string raw, string controlId, string msh16)
        {
            if (msh16 == "2")
            {
                ProcessQualityControl(segments, fieldSep, raw);
                return;
            }

            ProcessPatientResult(segments, fieldSep, raw);
        }

        /// <summary>
        /// Обычный результат образца (MSH-16 = 0).
        /// Структура: MSH, PID, OBR, OBX (может быть несколько OBX).
        /// </summary>
        private void ProcessPatientResult(string[] segments, char fieldSep, string raw)
        {
            string patientId = null;
            string sampleBarcode = null;
            string sampleNumber = null;
            string rackPos = null;
            string examDoctor = null;

            foreach (var seg in segments)
            {
                if (seg.StartsWith("PID"))
                {
                    var f = seg.Split(fieldSep);
                    // PID-3 — Patient Identifier List (medical record number).
                    patientId = FirstComponent(GetField(f, 2));
                }
                else if (seg.StartsWith("OBR"))
                {
                    var f = seg.Split(fieldSep);
                    sampleBarcode = FirstComponent(GetField(f, 1));  // OBR-2
                    sampleNumber  = FirstComponent(GetField(f, 2));  // OBR-3
                    rackPos       = GetField(f, 9);                  // OBR-10 (rack^pos)
                    examDoctor    = GetField(f, 19);                 // OBR-20
                }
                else if (seg.StartsWith("OBX"))
                {
                    var fields = seg.Split(fieldSep);

                    var valueType = GetField(fields, 1);   // OBX-2: NM / ST
                    var seqNum    = GetField(fields, 2);   // OBX-3: порядковый номер
                    var testRaw   = GetField(fields, 3);   // OBX-4: NAME^repeat
                    var testName  = FirstComponent(testRaw);
                    var repeat    = Component(testRaw, 1);
                    var value     = GetField(fields, 4);   // OBX-5
                    var units     = GetField(fields, 5);   // OBX-6
                    var refRange  = GetField(fields, 6);   // OBX-7
                    var abnormal  = GetField(fields, 7);   // OBX-8: L / H / N
                    var nature    = GetField(fields, 9);   // OBX-10: F (test) / C (review)
                    var rawResult = GetField(fields, 12);  // OBX-13
                    var obsTime   = GetField(fields, 13);  // OBX-14
                    var observer  = GetField(fields, 15);  // OBX-16

                    _logger.Info(
                        $"OBX patient={patientId} barcode={sampleBarcode} " +
                        $"sample={sampleNumber} rack={rackPos} " +
                        $"test={testName} repeat={repeat} value={value} {units} " +
                        $"ref={refRange} flag={abnormal} type={valueType} " +
                        $"nature={nature} raw={rawResult} time={obsTime} " +
                        $"doctor={examDoctor ?? observer}");

                    // TODO: сохранить результат в БД (Dal.WriteResult).
                }
            }

            // Сырой лог всего пакета.
            long id = Dal.WriteLog(_host.GetClientIP(), raw, _analyzer.Port);
        }

        /// <summary>
        /// QC-результат (MSH-16 = 2). В сообщении только MSH + OBR,
        /// поля OBR-11..23 несут QC-параметры.
        /// </summary>
        private void ProcessQualityControl(string[] segments, char fieldSep, string raw)
        {
            foreach (var seg in segments.Where(s => s.StartsWith("OBR")))
            {
                var f = seg.Split(fieldSep);

                var barcode    = FirstComponent(GetField(f, 1));  // OBR-2
                var sampleNum  = FirstComponent(GetField(f, 2));  // OBR-3
                var qcTimes    = GetField(f, 10);                 // OBR-11
                var moduleRing = GetField(f, 11);                 // OBR-12
                var qcName     = GetField(f, 12);                 // OBR-13
                var sampleType = GetField(f, 14);                 // OBR-15
                var lotNumber  = GetField(f, 15);                 // OBR-16 (lot)
                var mean       = GetField(f, 16);                 // OBR-17
                var sd         = GetField(f, 17);                 // OBR-18
                var result     = GetField(f, 18);                 // OBR-19
                var units      = GetField(f, 19);                 // OBR-20
                var flag       = GetField(f, 20);                 // OBR-21: +/-1SD, +/-2SD, +/-3SD
                var qcRule     = GetField(f, 22);                 // OBR-23: empty / 1-2s / 1-3s

                _logger.Info(
                    $"QC barcode={barcode} sample={sampleNum} times={qcTimes} " +
                    $"moduleRing={moduleRing} name={qcName} type={sampleType} " +
                    $"lot={lotNumber} mean={mean} sd={sd} result={result} " +
                    $"{units} flag={flag} rule={qcRule}");

                // TODO: сохранить QC в БД (Dal.WriteQcResult).
            }

            long logId = Dal.WriteLog(_host.GetClientIP(), raw, _analyzer.Port);
        }
        #endregion

        #region QRY → DSR
        
        /// <summary>
        /// Обработка QRY^Q02 и формирование ответа DSR^Q03.
        /// Формат ответа: MSH + MSA(OK/NF) + DSP1(пациент) + DSP2(образец) + DSP3(проекты).
        /// </summary>
        protected override void ProcessQry(string[] segments, char fieldSep, string controlId)
        {
            string queryFormat = null;  // QRD-3: SN (sample number) или BC (barcode)
            string barcode     = null;  // QRD-9: sample number или barcode
            string rackPos     = null;  // QRD-11: rack^pos
            string dilution    = null;  // QRD-12: Y / N

            foreach (var seg in segments)
            {
                if (seg.StartsWith("QRD"))
                {
                    var f = seg.Split(fieldSep);
                    queryFormat = GetField(f, 2);   // QRD-3
                    barcode     = GetField(f, 8);   // QRD-9
                    rackPos     = GetField(f, 10);  // QRD-11
                    dilution    = GetField(f, 11);  // QRD-12
                }
            }

            _logger.Info($"QRY format={queryFormat} barcode={barcode} " +
                         $"rackPos={rackPos} dilution={dilution}");

            if (string.IsNullOrEmpty(barcode))
            {
                SendDsrNotFound(controlId, "Empty query subject");
                return;
            }
                             
            _logger.Trace("Запрос анализов по пробирке из БД:" + barcode);
            var codes = Dal.AnalyzerSEND_GetCoderByBarcode(barcode, _analyzer.ip, _analyzer.port);

            if (codes == null || codes.Count == 0)
            {
                SendDsrNotFound(controlId, "Unknown key identifier");
                return;
            }
            
            _logger.Trace("Получены коды анализов:" + string.Join(",", codes.Select(x=>x.code).ToArray()));

            var dsp1 = BuildDspPatient();
            var dsp2 = BuildDspSample(barcode, rackPos, dilution);
            var dsp3 = BuildDspItems(codes.Select(c => c.code).ToList());

            var dsr = BuildDsrEnvelope(controlId, "OK", "")
                    + dsp1 + "\r"
                    + dsp2 + "\r"
                    + dsp3 + "\r";

            SendHl7(dsr);
            _logger.Info($"HL7 OUT DSR^Q03 for {controlId}");
        }

        /// <summary>
        /// Ответ DSR^Q03, когда данные не найдены или запрос некорректен.
        /// </summary>
        private void SendDsrNotFound(string controlId, string reason)
        {
            var dsr = BuildDsrEnvelope(controlId, "NF", reason);
            SendHl7(dsr);
            _logger.Info($"HL7 OUT DSR^Q03 NF for {controlId} ({reason})");
        }

        /// <summary>
        /// Формирует MSH + MSA для DSR^Q03.
        /// </summary>
        private string BuildDsrEnvelope(string controlId, string code, string text)
        {
            var now = DateTime.Now.ToString("yyyyMMddHHmmss");

            var sendingApp   = _analyzer != null ? (_analyzer.astm_system_name ?? "LIS")      : "LIS";
            var sendingFac   = _analyzer != null ? (_analyzer.astm_name ?? "LAB")      : "LAB";
            var receivingApp = _analyzer != null ? (_analyzer.astm_name ?? "ANALYZER") : "ANALYZER";
            var receivingFac = _analyzer != null ? (_analyzer.astm_name ?? "LAB")      : "LAB";

            var msh =
                $"MSH|^~\\&|{sendingApp}|{sendingFac}|{receivingApp}|{receivingFac}|" +
                $"{now}||DSR^Q03|{controlId}|P|2.3.1||||0||ANSI|\r";

            var msa = $"MSA|{code}|{controlId}|{text}|||0\r";

            return msh + msa;
        }

        /// <summary>
        /// DSP 1: patient info. Соответствует PID-2..PID-31,
        /// то есть DSP-N = PID-(N-1).
        /// </summary>
        private string BuildDspPatient()
        {
            var fields = new List<string> { "DSP", "1" };

            fields.Add(Guid.NewGuid().ToString().Split(new char[] {'-'}).First()    ?? "");  // DSP-3  = PID-2  (admission number)
            fields.Add(""); // DSP-4  = PID-3  (medical record)
            fields.Add("");  // DSP-5  = PID-4  (bed)
            fields.Add("");  // DSP-6  = PID-5  (name)
            fields.Add("");  // DSP-7  = PID-6  (wards)
            fields.Add("");  // DSP-8  = PID-7  (DOB)
            fields.Add("");  // DSP-9  = PID-8  (sex)
            // Остальные PID-9..PID-31 — пустые, но не пропускаем, если хотим сохранить позиции.
            // Наследник может дополнить.

            return string.Join("|", fields);
        }

        /// <summary>
        /// DSP 2: sample info. Соответствует OBR-2..OBR-47,
        /// то есть DSP-N = OBR-(N-1).
        /// </summary>
        private string BuildDspSample(string barcode,
                                      string rackPos, string dilution)
        {
            var fields = new List<string> { "DSP", "2" };

            fields.Add(barcode                  ?? "");                     // DSP-3  = OBR-2  (barcode)
            fields.Add("");                                                 // DSP-4  = OBR-3  (sample number)
            fields.Add("");                                                  // DSP-5  = OBR-4
            fields.Add("N");                                                 // DSP-6  = OBR-5  (priority)
            fields.Add("");                                                  // DSP-7  = OBR-6
            fields.Add(DateTime.Now.ToString("yyyyMMddHHmmss"));             // DSP-8  = OBR-7  (observation date/time)
            fields.Add("");                                                  // DSP-9  = OBR-8
            fields.Add("1");                                                 // DSP-10 = OBR-9  (repeat count)
            fields.Add(rackPos                  ?? "");                     // DSP-11 = OBR-10 (rack^pos)
            fields.Add("");                                                  // DSP-12 = OBR-11
            fields.Add(string.IsNullOrEmpty(dilution) ? "N" : dilution);     // DSP-13 = OBR-12 (dilution)

            return string.Join("|", fields);
        }

        /// <summary>
        /// DSP 3: project info. Set ID = 3, Item Count, Items (имена через ^).
        /// </summary>
        private string BuildDspItems(List<string> items)
        {
            var names = string.Join("^", items.Where(x => !string.IsNullOrEmpty(x)));
            return $"DSP|3|{items.Count}|{names}";
        }
        #endregion

        // ProcessIncomingAck не переопределяем — базовой реализации достаточно:
        // она логирует MSA-1 (AA/AE/AR) и MSA-2 (control ID).
    }
}