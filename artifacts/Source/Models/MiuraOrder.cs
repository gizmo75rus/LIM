using BaseLibraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstmDriver.Models
{
    public class MiuraOrder
    {
        [ColumnMap("id")]
        public int Id { get; set; }

		[ColumnMap("date_in")]
        public DateTime CreatedAt { get; set; }

        [ColumnMap("tube_barcode")]
        public string BarCode { get; set; }

		[ColumnMap("color_id")]
		public int? TubeColorId { get; set; }

		[ColumnMap("code")]
        public string AnaysisCode { get; set; }

		[ColumnMap("p_gender")]
        public int? PatGender { get; set; }

        public string GetDate()
		{
			return CreatedAt.ToString("yyyyMMddHHmmss");
		}

        public string GetGender() 
		{
			return PatGender.HasValue ? (PatGender.Value == 1 ? "F":"M") : "M";
		}

        public string GetBioType() 
		{
            if (TubeColorId.HasValue)
            {
				if (_biomaterialMap.TryGetValue(TubeColorId.Value, out string value))
					return value;
            }

			return "S";
		}

		Dictionary<int, string> _biomaterialMap = new Dictionary<int, string>
		{
			[1] = "S", //Красная
			[2] = "S", //Фиолетовая
			[3] = "U"  //Желтая
		};



        /*
         [id] [int] IDENTITY(1,1) NOT NULL,
	[date_in] [datetime] NULL,
	[idm_task_analysis] [int] NULL,
	[task_analysis_date_in] [datetime] NULL,
	[idm_analysis] [int] NULL,
	[code] [varchar](30) NULL,
	[order_number] [varchar](30) NULL,
	[tube_barcode] [varchar](30) NULL,
	[p_fam] [varchar](50) NULL,
	[p_name] [varchar](50) NULL,
	[p_otch] [varchar](50) NULL,
	[p_birthdate] [datetime] NULL,
	[is_send] [bit] NULL,
	[send_date] [datetime] NULL,
         
         */

    }
}
