using System;
using System.Data;
using CSICommon;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Data.Common;
using EndYearLottery.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;

namespace EndYearLottery.Services
{
    public class EmployeeService 
    {
        private IDBHelper _db;
        private LotteryModel _lotteryModel;
        private ResponseModel _responseModel;
        private CommonService _commonService;
        private JObject _responseMsg;
        public EmployeeService(IDBHelper dBHelper,LotteryModel lotteryModel,ResponseModel responseModel
                ,CommonService commonService)
        {
            this._db            = dBHelper;
            this._lotteryModel  = lotteryModel;
            this._responseModel = responseModel;
            this._commonService = commonService;
            var myJsonString    = File.ReadAllText(@"json\config.json"); // 讀取回傳訊息json檔
            this._responseMsg   = JObject.Parse(myJsonString);
        }     
        /// <summary>
        /// 人員清單 
        /// Author : Heng
        /// Create Time : 2021/12/24/17:00 
        /// </summary>
        public JArray EmployeeList(){
            try{      
                string cmd  = @"SELECT  [emp_number],[emp_name],[job_title],[department]
                                FROM    [EndYearLottery].[dbo].[EmployeeInfo]";
                DataTable employeeDT     = _db.GetDataTable(cmd);
                JArray result          = new JArray();
                foreach(DataRow row in employeeDT.Rows){
                    JObject tempJObject = new JObject(){
                        {"empNumber"    , row["emp_number"].ToString()},
                        {"empName"      , row["emp_name"].ToString()},
                        {"jobTitle"     , row["job_title"].ToString()},
                        {"department"   , row["department"].ToString()},
                        {"operateBtn"   ,   "<button class=\"btn btn-danger btn-remove employee-delete\""+
                                            "value=\""+row["emp_number"].ToString()+"\">刪除</button>"}
                        // "<button class=\"btn btn-info btn-edit employee-edit\"" +
                        // "value=\""+row["emp_number"].ToString()+"\">編輯</button>"+
                    };
                    result.Add(tempJObject);
                }  
                return result;
            }
            catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                throw e;
            }
        }
        /// <summary>
        /// 新增人員 
        /// Author : Heng
        /// Create Time : 2021/1/5/17:00 
        /// </summary>
        public ResponseModel AddEmployee(string empNumber,string empName,string jobTitle,string department){
            ResponseModel result    =   this._responseModel;
            try{
                string cmd              = @"INSERT INTO dbo.EmployeeInfo 
                                            ([emp_number],[emp_name],[job_title],[department],[create_time])
                                            VALUES(@empNumber,@empName,@jobTitle,@department,GETDATE())";
                DataTable employeeDT    =   _db.GetDataTable(cmd,new DbParameter[] {
                                            this._db.GetParameter("@empNumber", empNumber),
                                            this._db.GetParameter("@empName", empName),
                                            this._db.GetParameter("@jobTitle", jobTitle),
                                            this._db.GetParameter("@department", department)});
                result.Status           =   this._responseMsg["status"]["success"].ToString();
                result.Message          =   this._responseMsg["message"]["insertSuccess"].ToString();
                return result;
            }
            catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                result.Status           =   this._responseMsg["status"]["fail"].ToString();
                result.Message          =   this._responseMsg["message"]["exceptionMessage"].ToString();
                return result;
            }      
        }
        /// <summary>
        /// 刪除人員 
        /// Author : Heng
        /// Create Time : 2021/1/5/17:00 
        /// </summary>
        public ResponseModel DeleteEmployee(string empNumber){
            ResponseModel result    =   this._responseModel;
            try{
                string cmd              = @"DELETE FROM dbo.EmployeeInfo
                                            WHERE emp_number = @empNumber";
                DataTable employeeDT    =   _db.GetDataTable(cmd,new DbParameter[] {
                                            this._db.GetParameter("@empNumber", empNumber)});
                result.Status           =   this._responseMsg["status"]["success"].ToString();
                result.Message          =   this._responseMsg["message"]["deleteSuccess"].ToString();
                return result;
            }
            catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                result.Status           =   this._responseMsg["status"]["fail"].ToString();
                result.Message          =   this._responseMsg["message"]["exceptionMessage"].ToString();
                return result;
            }      
        }
        /// <summary>
        /// 匯入員工excel
        /// Author : Heng
        /// Create Time : 2021/01/12/17:00 
        /// </summary>
        /// <param name="employeeFile">員工excel</param> 
        public ResponseModel ImportEmployeeExcel(IFormFile employeeFile){
            ResponseModel result = this._responseModel;
            try{
                var fileExtension   = Path.GetExtension(employeeFile.FileName);
                var excelType       = new List<string>(){ ".xlsx", ".xls" };

                if (employeeFile.Length == 0){
                    result.Status   = this._responseMsg["status"]["fail"].ToString();
                    result.Message  = this._responseMsg["importMessage"]["fileEmpty"].ToString();
                    return result;
                }
                if (!excelType.Contains(fileExtension)){
                    result.Status   = this._responseMsg["status"]["fail"].ToString();
                    result.Message  = this._responseMsg["importMessage"]["fileTypeError"].ToString();
                    return result;
                }
                var excelTb         = this._commonService.ReadExcelToDataTable(employeeFile);
                var configData      = JObject.Parse(this._responseMsg["employeeExcelColumn"].ToString());
                var checkColumns    = this._commonService.CheckEnvColumnConfig(configData,excelTb.Columns);
                if (!checkColumns){
                    result.Status   = this._responseMsg["status"]["fail"].ToString();
                    result.Message  = this._responseMsg["importMessage"]["columnFail"].ToString();
                    return result;
                }
                var employeeTB      = TransExcelToEmployeeInfoFile(configData,excelTb);   
                    
                var cmd = @"import_info.EmployeeExcel";
                this._db.ExecuteSqlProcedure<TBTypeEmployeeExcelModel>(cmd,
                    new SqlParameter[]{
                        this._db.GetSqlParameter("@employeeTB", employeeTB,"excel.EmployeeExcel",SqlDbType.Structured)
                });
                result.Status   = this._responseMsg["status"]["success"].ToString();
                result.Message  = this._responseMsg["importMessage"]["importSuccess"].ToString();
                return result;
            }catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                result.Status           =   this._responseMsg["status"]["fail"].ToString();
                result.Message          =   this._responseMsg["message"]["exceptionMessage"].ToString();
                return result;
            }
        }
        /// <summary>
        /// 員工excel欄位轉換
        /// Author : Heng
        /// Create Time : 2021/01/12/17:00 
        /// </summary>
        /// <param name="configData">excel名稱對照</param> 
        /// <param name="excelTb">excel內容</param> 
        public DataTable TransExcelToEmployeeInfoFile(JObject configData,DataTable excelTb){
            try{
                var employeeList    = new List<TBTypeEmployeeExcelModel>();
                foreach (var col in configData){
                    if(excelTb.Columns.Contains(col.Key)){
                        excelTb.Columns[col.Key].ColumnName = col.Value.ToString();
                    }
                }
                foreach(DataRow row in excelTb.Rows){
                    var empNumber   = row["empNumber"].ToString().Trim();
                    var empName     = row["empName"].ToString().Trim();
                    var department  = row["department"].ToString().Trim();
                    employeeList.Add(new TBTypeEmployeeExcelModel() { 
                        emp_number  = empNumber,
                        emp_name    = empName,
                        department  = department
                    });
                }
                return employeeList.ToDataTable<TBTypeEmployeeExcelModel>();
            }
            catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                throw e;
            }
        }
    }
}

