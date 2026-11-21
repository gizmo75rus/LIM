using BaseLibraries;

namespace AstmDriver.Models
{
    public class AnalysisExtraCode
    {
        [ColumnMap(nameof(idm_analysis))]
        public int idm_analysis { get; set; }

        [ColumnMap(nameof(code))]
        public string code { get; set; }

        [ColumnMap(nameof(assy_num))]
        public string assy_num { get; set; }

    }
}
