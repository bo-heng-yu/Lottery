using System;
using Newtonsoft.Json.Linq;

namespace EndYearLottery.Models
{
    /// <summary>
	/// Response Model
	/// <summary>
    public class TBTypeEmployeeExcelModel
    {
        public String emp_number { get; set; }
        public String emp_name { get; set; }
        public String department { get; set; }
    }
}