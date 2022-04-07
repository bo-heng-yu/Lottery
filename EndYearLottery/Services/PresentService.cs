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
    public class PresentService 
    {
        private IDBHelper _db;
        private LotteryModel _lotteryModel;
        private ResponseModel _responseModel;
        private CommonService _commonService;
        private JObject _responseMsg;
        public PresentService(IDBHelper dBHelper,LotteryModel lotteryModel,ResponseModel responseModel
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
        /// 獎項清單 
        /// Author : Heng
        /// Create Time : 2021/12/24/17:00 
        /// </summary>
        public JArray PresentList(){
            try{
                string cmd  = @"SELECT  [present_id],[present],[amount],[sponsor]
                                FROM    [EndYearLottery].[dbo].[presentInfo]";
                DataTable presentDT     = _db.GetDataTable(cmd);
                JArray result           = new JArray();
                foreach(DataRow row in presentDT.Rows){
                    JObject tempJObject = new JObject(){
                        {"presentId"    , row["present_id"].ToInt32()},
                        {"present"      , row["present"].ToString()},
                        {"amount"       , row["amount"].ToInt32()},
                        {"sponsor"      , row["sponsor"].ToString()},
                        {"operateBtn"   ,   "<button class=\"btn btn-info btn-edit present-edit\"" +
                                            "value=\""+row["present_id"].ToInt32()+"\">編輯</button>"+  
                                            "<button class=\"btn btn-danger btn-remove present-delete\""+
                                            "value=\""+row["present_id"].ToInt32()+"\">刪除</button>"}
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
        /// 新增獎項
        /// Author : Heng
        /// Create Time : 2021/1/7/17:00 
        /// </summary>
        public ResponseModel AddPresent(string present,string sponsor,int amount){
            ResponseModel result    =   this._responseModel;
            try{
                string cmd              = @"INSERT INTO dbo.PresentInfo
                                            ([present],[sponsor],[amount],[create_time])
                                            VALUES(@present,@sponsor,@amount,GETDATE())";
                DataTable presentDT     =   _db.GetDataTable(cmd,new DbParameter[] {
                                            this._db.GetParameter("@present", present),
                                            this._db.GetParameter("@sponsor", sponsor),
                                            this._db.GetParameter("@amount", amount)});
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
        /// 編輯獎項
        /// Author : Heng
        /// Create Time : 2021/1/7/17:00 
        /// </summary>
        public ResponseModel EditPresent(string present,string sponsor,int amount,int presentId){
            ResponseModel result    =   this._responseModel;
            try{
                string cmd              = @"UPDATE [dbo].[PresentInfo]
                                            SET   present = @present, sponsor = @sponsor, amount = @amount
                                            WHERE present_id = @presentId;";
                DataTable presentDT    =   _db.GetDataTable(cmd,new DbParameter[] {
                                            this._db.GetParameter("@present", present),
                                            this._db.GetParameter("@sponsor", sponsor),
                                            this._db.GetParameter("@amount", amount),
                                            this._db.GetParameter("@presentId", presentId)});
                result.Status           =   this._responseMsg["status"]["success"].ToString();
                result.Message          =   this._responseMsg["message"]["updateSuccess"].ToString();
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
        /// 刪除獎項
        /// Author : Heng
        /// Create Time : 2021/1/7/17:00 
        /// </summary>
        public ResponseModel DeletePresent(int presentId){
            ResponseModel result    =   this._responseModel;
            try{
                string cmd              = @"DELETE FROM dbo.PresentInfo
                                            WHERE present_id = @presentId";
                DataTable employeeDT    =   _db.GetDataTable(cmd,new DbParameter[] {
                                            this._db.GetParameter("@presentId", presentId)});
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
        /// 匯入獎項excel
        /// Author : Heng
        /// Create Time : 2021/01/12/17:00 
        /// </summary>
        /// <param name="presentFile">獎項excel</param> 
        public ResponseModel ImportPresentExcel(IFormFile presentFile){
	        var result = new ResponseModel();
            try{
                var fileExtension   = Path.GetExtension(presentFile.FileName);
                var excelType       = new List<string>(){ ".xlsx", ".xls" };

                if (presentFile.Length == 0){
                    result.Status   = this._responseMsg["status"]["fail"].ToString();
                    result.Message  = this._responseMsg["importMessage"]["fileEmpty"].ToString();
                    return result;
                }
                if (!excelType.Contains(fileExtension)){
                    result.Status   = this._responseMsg["status"]["fail"].ToString();
                    result.Message  = this._responseMsg["importMessage"]["fileTypeError"].ToString();
                    return result;
                }
                var excelTb         = this._commonService.ReadExcelToDataTable(presentFile);
                var configData      = JObject.Parse(this._responseMsg["presentExcelColumn"].ToString());
                var checkColumns    = this._commonService.CheckEnvColumnConfig(configData,excelTb.Columns);
                if (!checkColumns){
                    result.Status   = this._responseMsg["status"]["fail"].ToString();
                    result.Message  = this._responseMsg["importMessage"]["columnFail"].ToString();
                    return result;
                }
                var presentTB       = TransExcelToPresentInfoFile(configData,excelTb);   
                    
                var cmd = @"import_info.PresentExcel";
                this._db.ExecuteSqlProcedure<TBTypePresentExcelModel>(cmd,
                    new SqlParameter[]{
                        this._db.GetSqlParameter("@presentTB", presentTB,"excel.PresentExcel",SqlDbType.Structured)
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
        /// 獎項excel欄位轉換
        /// Author : Heng
        /// Create Time : 2021/01/12/17:00 
        /// </summary>
        /// <param name="configData">excel名稱對照</param> 
        /// <param name="excelTb">excel內容</param> 
        public DataTable TransExcelToPresentInfoFile(JObject configData,DataTable excelTb){
            try{
                var presentList    = new List<TBTypePresentExcelModel>();
                foreach (var col in configData){
                    if(excelTb.Columns.Contains(col.Key)){
                        excelTb.Columns[col.Key].ColumnName = col.Value.ToString();
                    }
                }
                foreach(DataRow row in excelTb.Rows){
                    var presendId   = row["presendId"].ToString().Trim();
                    var present     = row["present"].ToString().Trim();
                    var amount      = row["amount"].ToString().Trim();
                    var sponsor     = row["sponsor"].ToString().Trim();

                    presentList.Add(new TBTypePresentExcelModel() { 
                        present_id  = presendId,
                        present     = present,
                        amount      = amount,
                        sponsor     = sponsor
                    });
                }
                return presentList.ToDataTable<TBTypePresentExcelModel>();
            }
            catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                throw e;
            }
        }
    }
}

