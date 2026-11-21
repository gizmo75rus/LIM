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
    public class Miura_Worker : AbstractAstmWorker
    {
        public Miura_Worker(IHost host, ILogger logger, Analyzer analyzer) : base(host, logger, analyzer)
        {
        }

        internal override void ProcessQueryMessage(Message message)
        {
            var barcodes = new List<string>();
            var packetBuffer = new List<string>();
            int[] taskAnalysisIds = null;
            bool queryAll = false;

            //Q|1|ALL              |       |ALL            ||||||||O
            //Q|1|Barcode paziente |       |^barcode_test^^||||||||F
            //0|1|       2         |   3   |
            foreach (var record in message.Records.Where(x => x.StartsWith("Q")).ToList())
            {
                var fields = record.Split('|');
                var barcode_field = fields[2];
                if(barcode_field == "ALL")
                {
                    queryAll = true;

                }
            }
            //                H|¥^&|||Host^PC1|||||Prestige24i^System1        ||P|1|20000530192631<CR>
            //               0 | 1 |||   4    |||||        10                 || | | 14
            string header = @"H|\^&|||HOST^P_1|||||" + _analyzer.astm_name + "||P|LIS2-A2|" + DateTime.Now.ToString("yyyyMMddHHmmss");

            packetBuffer.Add(header);

            int patientNumber = 1;

            if (queryAll)
            {
                _logger.Trace("Запрос заданий из БД:");
                var requestedOrders = Dal.GetMiuraOrders();
                if (requestedOrders.Any())
                {
                    
                    foreach (var orders in requestedOrders.GroupBy(x=> new { x.BarCode }))
                    {
                        int orderNumber = 1;

                        packetBuffer.Add($"P|{patientNumber}|");

                        foreach (var order in orders)
                        {
                            //O|1                             |452236         |452236         |^^^2e                 |R||||||N||||S                   |||F                  |||||||F||||||
                            packetBuffer.Add($"O|{orderNumber}|{order.BarCode}|{order.BarCode}|^^^{order.AnaysisCode}|R||||||N||||{order.GetBioType()}|||{order.GetGender()}|||||||O|||||");
                            
                            orderNumber++;
                        }

                        patientNumber++;
                    }
                    taskAnalysisIds = requestedOrders.Select(x => x.Id).ToArray();

                }

            }

            packetBuffer.Add("L|1|N");

            _logger.Trace("Собран пакет для отправки в анализатор");

            if (TrySendToAnalyzer(packetBuffer)) 
            {
                if(taskAnalysisIds != null)
                {
                    _logger.Trace("Изменение статуса в TASK_ANALYSIS с 1 (новый) на 2(в работе)");
                    //Dal.SetTaskAnaysisStatus(taskAnalysisIds);
                }
            };
        }
    }
}

