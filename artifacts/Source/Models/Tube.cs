using BaseLibraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstmDriver.Models
{
    public class Tube
    {
        [ColumnMap(nameof(id))]
        public int id { get; set; }

        [ColumnMap(nameof(barcode))]
        public string barcode { get; set; }

        [ColumnMap(nameof(idm_biomaterial))]
        public int? idm_biomaterial { get; set; }
    }
}
