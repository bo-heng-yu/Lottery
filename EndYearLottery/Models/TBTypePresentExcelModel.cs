using System;
using Newtonsoft.Json.Linq;

namespace EndYearLottery.Models
{
    /// <summary>
	/// Response Model
	/// <summary>
    public class TBTypePresentExcelModel
    {
        public String present_id { get; set; }
        public String present { get; set; }
        public String sponsor { get; set; }
        public String amount { get; set; }
    }
}