using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndYearLottery.Models;
using EndYearLottery.Services;
using Newtonsoft.Json.Linq;
using System.Data;
using Microsoft.AspNetCore.Http;

namespace EndYearLottery.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private LotteryService _lotteryService;
        private EmployeeService _employeeService;
        private PresentService _presentService;
        private CommonService _commonService;
        private ResponseModel _responseModel;
        public HomeController(LotteryService lotteryService , EmployeeService employeeService,PresentService presentService,
            CommonService commonService,ResponseModel responseModel)
        {
            this._lotteryService    =   lotteryService;
            this._employeeService   =   employeeService;
            this._presentService    =   presentService;
            this._commonService     =   commonService;
            this._responseModel     =   responseModel;
        }
        //導入頁面
        public IActionResult Index(){
            JArray presentList  = this._presentService.PresentList();
            return View(presentList);
        }
        public IActionResult Employee(){
            JArray employeeList = this._employeeService.EmployeeList();
            return View(employeeList);
        }
        public IActionResult Present(){
            JArray presentList  = this._presentService.PresentList();
            return View(presentList);
        }
        //主要抽獎頁面功能
        [HttpPost]
        public IActionResult WinnerDraw(string present , string sponsor , int amount){
            int isBonus = 0;
            this._responseModel     = this._lotteryService.WinnerDraw(present,sponsor,amount,isBonus);
            return Json(this._responseModel);
        }
        [HttpPost]
        public IActionResult WinnerList(string present , string sponsor , int amount){
            JArray winnerList     = this._lotteryService.WinnerList();
            return Json(winnerList);
        }
        [HttpPost]
        public IActionResult BonusDraw(string present , string sponsor , int amount){
            int isBonus = 1;
            this._responseModel     = this._lotteryService.BonusDraw(present,sponsor,amount,isBonus);
            return Json(this._responseModel);
        }
        [HttpPost]
        public IActionResult ExportWinnerList(IFormFile file = null){
            DataTable result    = this._lotteryService.ExportWinnerList();
            var content         = this._commonService.DownloadExcelContent(result);
            return File(content, "application/vnd.ms-excel", "中獎清單.xlsx");
        }
        [HttpPost]
        public IActionResult ClearWinnerList(){
            this._responseModel    = this._lotteryService.ClearWinnerList();
            return Json(this._responseModel);
        }   
        //員工頁面功能
        [HttpPost]
        public IActionResult AddEmployee(string empNumber,string empName,string jobTitle,string department){
            this._responseModel     = this._employeeService.AddEmployee(empNumber,empName,jobTitle,department);
            return Json(this._responseModel);
        }
        [HttpPost]
        public IActionResult DeleteEmployee(string empNumber){
            this._responseModel     = this._employeeService.DeleteEmployee(empNumber);
            return Json(this._responseModel);
        }
        public IActionResult ImportEmployeeExcel(IFormFile employeeExcel){
            this._responseModel = this._employeeService.ImportEmployeeExcel(employeeExcel);
            return Json(this._responseModel);
        }
        //獎項頁面功能
        [HttpPost]
        public IActionResult AddPresent(string present,string sponsor,int amount){
            this._responseModel     = this._presentService.AddPresent(present,sponsor,amount);
            return Json(this._responseModel);
        }
        [HttpPost]
        public IActionResult EditPresent(string present,string sponsor,int amount,int presentId){
            this._responseModel     = this._presentService.EditPresent(present,sponsor,amount,presentId);
            return Json(this._responseModel);
        }
        [HttpPost]
        public IActionResult DeletePresent(int presentId){
            this._responseModel     = this._presentService.DeletePresent(presentId);
            return Json(this._responseModel);
        }
        public IActionResult ImportPresentExcel(IFormFile presentExcel){
            this._responseModel = this._presentService.ImportPresentExcel(presentExcel);
            return Json(this._responseModel);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
