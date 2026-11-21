using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Commons.ASTM;
using AstmDriver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using AstmDriver.Models;

namespace AstmDriver.Behavior.MessageWorker
{
    /// <summary>
    /// Обработчик сообщений для анализатора Saphir 400
    /// Имя в конфигурационном файле: Saphir400
    /// </summary>
    public class Saphir400_Worker : AbstractAstmWorker
    {
       
        public Saphir400_Worker(IHost host, ILogger logger, Analyzer analyzer) : base(host, logger, analyzer)
        {

        }


        internal override void  ProcessQueryMessage(Message message)
        {
            var barcodes = new List<string>();
            var packetBuffer = new List<string>();

            //Q|1|^232000|^232000|ALL||||||||O
            //0|1|   2   |   3   |
            foreach (var record in message.Records.Where(x => x.StartsWith("Q")).ToList())
            {
                var fields = record.Split('|');
                var barcode_field = fields[2];
                if (barcode_field.Contains('^'))
                {
                    var barcode = barcode_field.Split('^').LastOrDefault();
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        barcodes.Add(barcode);
                    }
                }
            }
            //                H|¥^&|||Host^PC1|||||Prestige24i^System1        ||P|1|20000530192631<CR>
            //               0 | 1 |||   4    |||||        10                 || | | 14
            string header = @"H|\^&|||HOST^P_1|||||" + _analyzer.astm_name + "||P|1|" + DateTime.Now.ToString("yyyyMMddHHmmss");

            packetBuffer.Add(header);

            int patientCount = 1;
            
            foreach (string barcode in barcodes)
            {
                _logger.Trace("Запрос анализов по пробирке из БД:" + barcode);
                var codes = Dal.AnalyzerSEND_GetCoderByBarcode(barcode, _analyzer.ip, _analyzer.port);

                packetBuffer.Add($"P|{patientCount}");

                int orderCount = 1;
                if (codes.Count > 0)
                {
                    _logger.Trace("Получены коды анализов:" + string.Join(",", codes));
                    //O|1|1234||^^^1^GOT^0¥^^^2^GPT^0¥^^^3^ALP^0|R||||||||N|||||Serum|||||||||||O<CR>

                    var codeFrame = string.Join("\\", codes.Select(x => $"^^^{x.assy_num}^{x.code}^0").ToArray());
                    //var codeFrame = "^^^1^GLU-VB^0";
                    //                       1       |    2     |  3 |       4   |5||||||11||||15  ||||||||||25
                    packetBuffer.Add($"O|{orderCount}|{barcode }|0|{codeFrame}|R||||||N||||Serum||||||||||O");

                    orderCount++;
                }
                else
                {
                    _logger.Error($"Для пробирк {barcode} остутсвуют анализы в задании");
                   // packetBuffer.Add($"O|{orderCount}|{barcode}|||R||||||N||||Serum||||||||||O");
                }
                patientCount = patientCount == 7 ? 0 : patientCount += 1;
            }

            packetBuffer.Add("L|1|N");

            _logger.Trace("Собран пакет для отправки в анализатор");

            TrySendToAnalyzer(packetBuffer);
        }

       


    }
}
