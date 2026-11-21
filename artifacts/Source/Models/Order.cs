using System;
using System.Collections.Generic;

namespace AstmDriver.Models
{
    public class Order
    {
        public string TaskID { get; set; }
        public int AnalyzerID { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Barcode { get; set; }
        public string Biomaterial { get; set; }
        public string Type { get; set; }
        public Patient Patient { get; set; }
        public List<string> Analysis { get; set; } = new List<string>();
    }
}
