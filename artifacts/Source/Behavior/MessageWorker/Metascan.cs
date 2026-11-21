using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Commons.ASTM;
using AstmDriver.Core;
using AstmDriver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace AstmDriver.Behavior.MessageWorker
{
    public class Metascan : AbstractAstmWorker
    {
        internal const char ACK = '\u0006';
        System.Timers.Timer _backTimer;
        public Metascan(IHost host, ILogger logger, Analyzer analyzer) : base(host, logger, analyzer)
        {
            //_backTimer = new System.Timers.Timer(config.BackTimerInterval * 60 * 1000);
            _backTimer = new System.Timers.Timer(1 *60 * 1000);
            _backTimer.AutoReset = true;
            _backTimer.Elapsed += _backTimer_Elapsed;
            _backTimer.Start();
        }

        private void _backTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_host.TransportChannel.IsConnected)
                return;

            if (_host.Status == ChannelStatus.Idle)
            {
                try
                {
                    _backTimer.Enabled = false;

                    IEnumerable<TaskResult> data = Dal.GetDataForExport(_analyzer.id);
                    if (data.Any())
                    {
                        if (_host.EstablishSendMode())
                        {
                            var grp = data.GroupBy(x => new { x.patient_ext_id, x.order_num, x.barcode });

                            int p_num = 1;
                            foreach (var g in grp)
                            {
                                //                0|  1|||4
                                string header = @"H|\^&|||NgLAB^Beckman^Coulter|||||||P|1|" + DateTime.Now.ToString("yyyyMMddHHmmss");
                                string patient = $"P|{p_num}|{g.Key.patient_ext_id}||||||";

                                string doctor = "";
                                string order = $"O|1|{g.Key.barcode}|{g.Key.order_num}|||||||||||||{doctor}|||||||||F";
                                p_num++;

                                SendToAnalyzer(header);
                                SendToAnalyzer(patient);
                                SendToAnalyzer(order);

                                int r_num = 1;

                                foreach (var row in g)
                                {
                                    //                1|   2   |  3                   |   4        |5|6|7|8|9
                                    string result = $"R|{r_num}|^^^{row.GetCode()}|{row.result}|{row.unit}||||F||||{row.GetDate()}";
                                    SendToAnalyzer(result);
                                    r_num++;

                                    Dal.SetDateExport(row.idm_tasks_analysis);
                                }

                                SendToAnalyzer($"L|1|N");
                            }
                            _host.StopSendMode();
                        }
                    }
                    else
                    {
                        _host.TransportChannel.WriteData($"{ACK}");
                    }
                }
                catch (Exception ex)
                {
                    OnErrorInvoke("Ошибка при отправке результатов анализов в медиалог");
                    _logger.Error($"Ошибка при отправке результатов анализов в медиалог:{Environment.NewLine}{ex}");
                }
                finally
                {
                    _backTimer.Enabled = true;
                }
            }
            else
            {
                Thread.Sleep(1000);
                _backTimer_Elapsed(sender, e);
            }
        }

        internal override void ProcessQueryMessage(Message message)
        {
            //var barcodes = new List<string>();
            //var packetBuffer = new List<string>();

            ////Q|1|^232000|^232000|ALL||||||||O
            ////0|1|   2   |   3   |
            //foreach (var record in message.Records.Where(x => x.StartsWith("Q")).ToList())
            //{
            //    var fields = record.Split('|');
            //    var barcode_field = fields[2];
            //    if (barcode_field.Contains('^'))
            //    {
            //        var barcode = barcode_field.Split('^').LastOrDefault();
            //        if (!string.IsNullOrEmpty(barcode))
            //        {
            //            barcodes.Add(barcode);
            //        }
            //    }
            //}
            ////                H|¥^&|||Host^PC1|||||Prestige24i^System1        ||P|1|20000530192631<CR>
            ////               0 | 1 |||   4    |||||        10                 || | | 14
            //string header = @"H|\^&|||NgLAB^Bеckman^Coulter|||||||P|1|" + DateTime.Now.ToString("yyyyMMddHHmmss");

            //packetBuffer.Add(header);

            //int patientCount = 1;

            //foreach (string barcode in barcodes)
            //{
            //    _logger.Trace("Запрос анализов по пробирке из БД:" + barcode);
            //    var codes = Dal.AnalyzerSEND_GetCoderByBarcode(barcode, _analyzer.ip, _analyzer.port);

            //    packetBuffer.Add($"P|{patientCount}");

            //    int orderCount = 1;
            //    if (codes.Count > 0)
            //    {
            //        _logger.Trace("Получены коды анализов:" + string.Join(",", codes));
            //        //O|1|1234||^^^1^GOT^0¥^^^2^GPT^0¥^^^3^ALP^0|R||||||||N|||||Serum|||||||||||O<CR>
            //        //3O|1|706176||^^^DBILB\^^^TBILB\^^^PHOS\^^^MG\^^^I Ater\^^^CA\^^^GLUC\^^^AMY H\^^^UREA H\^^^UA H\^^^ALB V\^^^ALT H\^^^AST H\^^^TP\^^^IRON\^^^TBIL\^^^DBIL H\^^^TRIG H\^^^GGT H\^^^ALP H\^^^NDBIL_\^^^LDL-V\^^^CHOL H\^^^HDL-V|R||20230127||||N||||BLOOD||||||||||N

            //        var codeFrame = string.Join("\\", codes.Select(x => $"^^^{x.assy_num}^{x.code}^0").ToArray());
            //        //var codeFrame = "^^^1^GLU-VB^0";
            //        //                       1       |    2     |  3 |       4   |5||||||11||||15  ||||||||||25
            //        packetBuffer.Add($"O|{orderCount}|{barcode}|0|{codeFrame}|R||||||N||||Serum||||||||||O");

            //        orderCount++;
            //    }
            //    else
            //    {
            //        _logger.Error($"Для пробирк {barcode} остутсвуют анализы в задании");
            //        packetBuffer.Add($"O|{orderCount}|{barcode}|||R||||||N||||Serum||||||||||O");
            //    }
            //    patientCount = patientCount == 7 ? 0 : patientCount += 1;
            //}

            //packetBuffer.Add("L|1|N");

            //_logger.Trace("Собран пакет для отправки в анализатор");

            //SendToAnalyzer(packetBuffer);
        }

        internal override void ProcessOrderMessage(Message message)
        {
            foreach (var data in message.RawData)
            {
                Dal.WriteLog(_analyzer.ip, data, _analyzer.Port);
            }

            var patientRecord = message.Records.FirstOrDefault(x => x.StartsWith("P"));
            if(patientRecord != null)
            {
                Patient patient = null;

                if (TryParsePatient(patientRecord,out patient))
                {
                    var orderRecord = message.Records.FirstOrDefault(x => x.StartsWith("O"));
                    if (orderRecord != null)
                    {
                        Order order = null;
                        if (TryParseOrder(orderRecord, patient, out order)) 
                        {
                            if (Dal.LinkOrder(order)) {
                                OnInfoInvoke($"Для пробирки № {order.Barcode} создано задание № {order.TaskID} ");
                            }
                        }
                        else
                        {
                            OnInfoInvoke("Не удалось распарсить строку Order");
                            _logger.Debug("Не удалось распарсить строку Order: " + orderRecord);
                        }
                    }
                }
                else
                {
                    OnInfoInvoke("Не удалось распарсить строку Patient");
                    _logger.Debug("Не удалось распарсить строку Patient: " + patientRecord);
                }
            }
        }

        internal override void ProcessResultMessage(Message message)
        {
            _logger.Info("Всего записей:" + message.Records.Count(x => x.StartsWith("R|")));
            foreach (var raw in message.RawData)
            {
                Dal.WriteLog(_host.GetClientIP(), raw, _analyzer.Port);
            }
        }


        private bool TryParseOrder(string record,Patient patient, out Order order)
        {
            // 0|1|  2       ||       4         |5||   7    |8|9|10|11|12|13|14|  15 |16|17|18|19|20|21|22|23|24|25|26 
            // O|1|1002637113||^^^GLUC          |R||20230127| | |  |N |  |  |  |BLOOD|  |  |  |  |  |  |  |  |  | N
            // O|1|706176    ||^^^DBILB\^^^TBILB|R||20230127| | |  |N |  |  |  |
            order = new Order();
            
            order.Patient = patient;

            var fields = record.Split('|');

            string date = string.IsNullOrEmpty(fields[7]) ? DateTime.Now.ToString("yyyyMMdd") : fields[7];

            order.AnalyzerID = _analyzer.id;

            order.Date = new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2)));

            order.Number = $"{patient.ext_id}_{date}";

            // пробирка
            if (!string.IsNullOrEmpty(fields[2]))
            {
                order.Barcode = fields[2].Trim();
            }

            //заказ
            if (!string.IsNullOrEmpty(fields[3]))
            {
                order.Number = fields[3].Trim();
            }

            //анализы
            if (!string.IsNullOrEmpty(fields[4]))
            {
                var analysis = fields[4].Split('\\');
                var grp_check = analysis.GroupBy(x => x).Select(x => new { code = x.Key, count = x.Count() });
                if (grp_check.Any(x => x.count > 1)) 
                {
                    var doubles = string.Join(",", grp_check.Where(x => x.count > 1).Select(x => x.code).ToArray());
                    string message = $"В заказе имееются дубли кодов - пробирка: {order.Barcode}, коды: {doubles} !!!";
                    OnErrorInvoke(message);
                    _logger.Info(message);

                    order.Analysis = grp_check.Select(x => x.code).ToList();
                }
                else
                {
                    order.Analysis = analysis.ToList();
                }
            }
            else
            {
                return false;
            }

            //Биоматериал
            if (fields.Length >= 26)
            {
                order.Biomaterial = fields[15];
                order.Type = fields[25];
            }
            else
            {
                return false;
            }

            return true;
        }
        private bool TryParsePatient(string record,out Patient patient)
        {
            patient = new Patient();

            // 0|1|  2    |||    5         |||8
            // P|1|8979568|||TEST TEST^Test|||M
            // P|1|7065215|||MORGHEVILOVA^Tat_yana|||F

            var fields = record.Split('|');


            // ЭМК
            if (!string.IsNullOrEmpty(fields[2]))
            {
                patient.ext_id = fields[2];
            }
            else
            {
                return false;
            }

            // ФИО
            if (!string.IsNullOrEmpty(fields[5]))
            {
                var fio = fields[5];
                
                if (fio.Contains("^"))
                {
                    var fio_fields = fio.Split('^');

                    patient.last_name = fio_fields.FirstOrDefault();
                    patient.first_name = fio_fields.LastOrDefault();

                }
                else
                {
                    patient.last_name = fio;
                }
            }

            //Дата рождения
            if (!string.IsNullOrEmpty(fields[7]))
            {
                DateTime date_b;

                if (DateTime.TryParseExact(fields[7], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out date_b)){
                    patient.date_b = date_b;
                }
                
            }

            //ПОЛ

            if (!string.IsNullOrEmpty(fields[8]))
            {
                patient.gender = fields[8] == "M" ? 2 : 1;
            }

            return true;
        }

        private void SendToAnalyzer(string data)
        {
            var message = $"{data}{CR}";
            _host.SendMessage(message);
            OnDataTransiveInvoke(data);
            Thread.Sleep(500);
        }

    }
}
