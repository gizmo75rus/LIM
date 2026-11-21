using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AstmDriver.Models;

namespace AstmDriver.Behavior.MessageWorker
{
    public class AU480_Worker : AbstractAstmWorker
    {
        private const char STX = (char)DataLinkControlCodes.STX;
        private const char ETX = (char)DataLinkControlCodes.ETX;

        public AU480_Worker(IHost host, ILogger logger, Analyzer analyzer) : base(host, logger, analyzer)
        {
        }

        internal override void ParseReceivedRecord(string record, string rawData)
        {
            if (record == "RB")
            {
                _logger.Info("Начало пакета (Запрос анализов)");
            }

            if(record == "DB")
            {
                _logger.Info("Начало пакета (Получение результатов)");
            }


            if (record == "RE" || record == "DE" || record == "SE")
            {
                _logger.Info("Конец пакета");
            }


            if(record[0] == 'R' && record?.Length > 2)
            {

                // Запрос задания от анализатора
                //  положение        номер пробирки         
                //  в реке
                //
                // R 000301 0001              705963
                // R 000201U0001                8800
                // RH000201 0002            10000233

                var rec_type = record.Substring(1, 1);
                var rack_no = record.Substring(2, 4);
                var pos = record.Substring(6, 2);
                var sample_type = record.Substring(8, 1);

                var sample = record.Substring(9, 4);
                var barcode = record.Substring(13, 20);

                ProcessQuery(rec_type, rack_no, pos,sample_type, sample, barcode);
            }

            if(record[0] == 'D' && record?.Length > 2)
            {
                ProcessResult(rawData);
            }

            OnLastEOTInvoke();
        }

      
        private void ProcessQuery(string rec_type, string rack_no,string pos,string sample_type, string sample,string barcode)
        {
            //Ответ от ЛИС в анализатор 
            //S 000000 0003                        E030039



            string request = $"S{rec_type}";

            request = string.Format(@"{0}{1}{2}{3}{4}{5}    E", request, rack_no, pos,sample_type,sample, barcode.PadLeft(20));
            
            if (!string.IsNullOrEmpty(barcode.Trim(' ')))
            {
                var data = Dal.AnalyzerSEND_GetCoderByBarcode(barcode, _analyzer.ip, _analyzer.port);
                if (data.Any())
                {
                    foreach(var row in data)
                    {
                        request = string.Format(@"{0}{1}", request, row.assy_num);
                    }
                }
            }
            SendToAnalyzer(request);
            Thread.Sleep(500);
            SendToAnalyzer("SE");
        }

        private void ProcessResult(string data) 
        {
            Task.Run(() => {
                Dal.WriteLog(_analyzer.ip, data, _analyzer.Port);
            });
        }

        internal override void _host_OnEOTReceived(object sender, EventArgs e)
        {

        }

        private void SendToAnalyzer(string data)
        {
            var message = $"{STX}{data}{ETX}";
            _host.SendMessage(message);
            OnDataTransiveInvoke(data);
        }
    }
}
