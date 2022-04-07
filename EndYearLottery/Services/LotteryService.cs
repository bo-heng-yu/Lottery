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
    public class LotteryService 
    {
        private IDBHelper _db;
        private LotteryModel _lotteryModel;
        private ResponseModel _responseModel;
        private CommonService _commonService;
        private JObject _responseMsg;
        public LotteryService(IDBHelper dBHelper,LotteryModel lotteryModel,ResponseModel responseModel
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
        /// 匯出中獎者 
        /// Author : Heng
        /// Create Time : 2021/1/7/17:00 
        /// </summary>
        public DataTable ExportWinnerList(){
            try{
                string cmd  = @"    SELECT   [emp_number]   as '員工編號'
                                            ,[emp_name]		as '員工名稱'
                                            ,[present]		as '中獎獎品'
                                            ,[sponsor]		as '贊助廠商'
                                            ,[department]   as '部門'
                                            ,[is_bonus]		as '一般獎項 = 0 加碼獎品 = 1'
                                            ,[create_time]	as '建立時間'
                                    FROM     [dbo].[WinnerInfo]";
                DataTable winnerDT     = _db.GetDataTable(cmd);
                return winnerDT;
            }
            catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                throw e;
            }
        }
        public JArray WinnerList(){
            try{
                string cmd  = @"SELECT  [emp_number],[emp_name],[department],[present]
                                FROM    [EndYearLottery].[dbo].[WinnerInfo]";
                DataTable winnerDT     = _db.GetDataTable(cmd);
                JArray result           = new JArray();
                foreach(DataRow row in winnerDT.Rows){
                    JObject tempJObject = new JObject(){
                        {"empNumber"    , row["emp_number"].ToString()},
                        {"empName"      , row["emp_name"].ToString()},
                        {"present"      , row["present"].ToString()},
                        {"department"   , row["department"].ToString()}
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
        /// 清除中獎清單table
        /// Author : Heng
        /// Create Time : 2021/1/4/17:00 
        /// </summary>
        public ResponseModel ClearWinnerList(){  
            ResponseModel result    = this._responseModel;
            try{
                string cmd              = @"DELETE [dbo].[WinnerInfo]";
                _db.GetDataTable(cmd);
                result.Status   = this._responseMsg["status"]["success"].ToString();
                result.Message  = this._responseMsg["message"]["clearWinnerListSuccess"].ToString();
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
        /// 抽出獎項
        /// Author : Heng
        /// Create Time : 2021/12/24/17:00 
        /// </summary>
        /// <param name="present">獎品</param>
        /// <param name="sponsor">贊助廠商</param>
        /// <param name="amount">獎項數量</param>
        /// <param name="isBonus">是否為加碼</param>
        public ResponseModel WinnerDraw(string present , string sponsor , int amount  , int isBonus = 0){
            ResponseModel result    = this._responseModel;
            try{
                //亂數選出得獎者
                string pickWinnerCmd    = @"SELECT top (@amount) ei.[emp_number],ei.[emp_name],ei.[department]
                                            FROM        dbo.EmployeeInfo  as ei
                                            LEFT JOIN   dbo.winnerInfo    as wi
                                            ON          ei.emp_number = wi.emp_number 
                                            and         wi.is_bonus = 0
                                            WHERE       wi.emp_number is NULL                                       
                                            ORDER BY NEWID()";
                DataTable winnerDT      =   _db.GetDataTable(pickWinnerCmd,new DbParameter[] {
                                            this._db.GetParameter("@amount", amount)});
                //準備寫入得獎者
                string insertWinnerCmd  = @"INSERT INTO [dbo].[WinnerInfo] 
                                            (emp_number, emp_name, present, sponsor, department, is_bonus, create_time)
                                            VALUES"; 
                //將得獎者迴圈整理成json格式並將每個得獎者加入到writeWinnerCmd          
                JArray winnerResult    = new JArray();
                if(winnerDT.Rows.Count < 1){
                    result.Status   = this._responseMsg["status"]["fail"].ToString();
                    result.Message  = this._responseMsg["message"]["noEmpCanDraw"].ToString();
                    return result;
                }
                string winnerNumberList = string.Empty;   
                foreach(DataRow row in winnerDT.Rows){
                    string empNumber    = row["emp_number"].ToString();
                    string empName      = row["emp_name"].ToString();
                    string department   = row["department"].ToString();               
                    JObject tempJObject = new JObject(){
                        {"empNumber"    , empNumber},
                        {"empName"      , empName},
                        {"department"   , department}
                    };
                    winnerResult.Add(tempJObject);
                    insertWinnerCmd    += @$"  ('{empNumber}'   ,'{empName}'    ,'{present}',
                                                '{sponsor}'     ,'{department}' ,'{isBonus}'  ,GETDATE()),";
                }
                insertWinnerCmd         = insertWinnerCmd.Remove(insertWinnerCmd.Length - 1);  //消除最後逗點
                _db.GetDataTable(insertWinnerCmd); //寫入得獎者資訊與獎品
                result.Status       = this._responseMsg["status"]["success"].ToString();
                result.Message      = this._responseMsg["message"]["winningMessage"].ToString();
                result.ResponseData = new JObject(){{"winner",winnerResult}};
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
        /// 抽出加碼獎項
        /// Author : Heng
        /// Create Time : 2021/12/24/17:00 
        /// </summary>
        /// <param name="amount">獎項數量</param>
        /// <param name="present">獎品</param>
        /// <param name="sponsor">贊助廠商</param>
        /// <param name="isBonus">是否為加碼</param>
        public ResponseModel BonusDraw(string present , string sponsor , int amount  , int isBonus = 1){
            ResponseModel result = this._responseModel;
            try{
                //亂數選出得獎者
                string pickWinnerCmd    = @"SELECT top (@amount) ei.[emp_number],ei.[emp_name],ei.[department]
                                            FROM        dbo.EmployeeInfo  as ei
                                            LEFT JOIN   dbo.winnerInfo    as wi
                                            ON          ei.emp_number = wi.emp_number 
                                            and         wi.is_bonus = 1
                                            WHERE       wi.emp_number is NULL                                      
                                            ORDER BY NEWID()";
                DataTable winnerDT      =   _db.GetDataTable(pickWinnerCmd,new DbParameter[] {
                                            this._db.GetParameter("@amount", amount)});
                //準備寫入得獎者
                string insertWinnerCmd  = @"INSERT INTO [dbo].[WinnerInfo] 
                                            (emp_number, emp_name, present, sponsor, department, is_bonus, create_time)
                                            VALUES"; 
                //將得獎者迴圈整理成json格式並將每個得獎者加入到writeWinnerCmd          
                JArray winnerResult     = new JArray();
                if(winnerDT.Rows.Count < 1){
                    result.Status   = this._responseMsg["status"]["fail"].ToString();
                    result.Message  = this._responseMsg["message"]["noEmpCanDraw"].ToString();
                    return result;
                }
                string winnerNumberList = string.Empty;
                foreach(DataRow row in winnerDT.Rows){
                    string empNumber    = row["emp_number"].ToString();
                    string empName      = row["emp_name"].ToString();
                    string department   = row["department"].ToString();               
                    JObject tempJObject = new JObject(){
                        {"empNumber"    , empNumber},
                        {"empName"      , empName},
                        {"department"   , department}
                    };
                    winnerResult.Add(tempJObject);
                    insertWinnerCmd    += @$"  ('{empNumber}'   ,'{empName}'    ,'{present}',
                                                '{sponsor}'     ,'{department}' ,'{isBonus}'  ,GETDATE()),";
                }
                insertWinnerCmd         = insertWinnerCmd.Remove(insertWinnerCmd.Length - 1);  //消除最後逗點
                _db.GetDataTable(insertWinnerCmd); //寫入得獎者資訊與獎品
                result.Status       = this._responseMsg["status"]["success"].ToString();
                result.Message      = this._responseMsg["message"]["winningMessage"].ToString();
                result.ResponseData = new JObject(){{"winner",winnerResult}};
                return result;
            }
            catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                result.Status           =   this._responseMsg["status"]["fail"].ToString();
                result.Message          =   this._responseMsg["message"]["exceptionMessage"].ToString();
                return result;
            }  
        }    
    }
}

