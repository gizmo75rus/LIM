using BaseLibraries;

namespace AstmDriver.Models
{
    public class Analysis
    {
        [ColumnMap(nameof(assy_num))]
        public string assy_num { get; set; }

        [ColumnMap(nameof(code))]
        public string code { get; set; }
    }

}
