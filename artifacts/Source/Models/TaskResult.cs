using BaseLibraries;
using System;

namespace AstmDriver.Models
{
    public class TaskResult
    {
        [ColumnMap(nameof(idm_tasks_analysis))]
        public int idm_tasks_analysis { get; set; }

        [ColumnMap(nameof(patient_ext_id))]
        public string patient_ext_id { get; set; }

        [ColumnMap(nameof(barcode))]
        public string barcode { get; set; }

        [ColumnMap(nameof(order_num))]
        public string order_num { get; set; }
        
        [ColumnMap(nameof(analysis_code))]
        public string analysis_code { get; set; }
        
        [ColumnMap(nameof(result))]
        public string result { get; set; }

        [ColumnMap(nameof(unit))]
        public string unit { get; set; }

        [ColumnMap(nameof(date))]
        public DateTime date { get; set; }

        public string GetDate()
        {
            return date.ToString("yyyyMMddHHmmss");
        }

        internal string GetCode()
        {
            return analysis_code;
        }
    }
}
