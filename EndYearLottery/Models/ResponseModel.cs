using Newtonsoft.Json.Linq;

namespace EndYearLottery.Models
{
    /// <summary>
	/// Response Model
	/// <summary>
	public class ResponseModel
	{
        public string Status { get; set; }
        public string Message { get; set; }
        public JObject ResponseData { get; set; }
    }
}