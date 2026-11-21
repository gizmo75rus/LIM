using BaseLibraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstmDriver.Models
{
    public class Patient
    {
        [ColumnMap(nameof(id))]
        public int id { get; set; }

        [ColumnMap(nameof(ext_id))]
        public string ext_id { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        [ColumnMap(nameof(first_name))]
        public string first_name { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string last_name { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        [ColumnMap(nameof(date_b))]
        public DateTime? date_b { get; set; }

        /// <summary>
        /// Пол
        /// </summary>
        [ColumnMap(nameof(gender))]
        public int? gender { get; set; }
   
    }
}
