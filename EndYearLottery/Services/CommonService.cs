using System;
using System.Data;
using CSICommon;
using System.IO;
using ClosedXML.Excel;
using EndYearLottery.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace EndYearLottery.Services
{
    public class CommonService 
    {
        private IDBHelper _db;
        public CommonService(IDBHelper dBHelper)
        {
            this._db = dBHelper;
        }     
        /// <summary>
        /// 讀取excel樣板，將撈出的DB DataTable寫入excel 儲存在記憶體 傳出
        /// Author : Heng
        /// Create Time : 2021/1/10/17:00 
        /// </summary>
        /// <param name="excelItem"></param> excel資訊
        /// <returns></returns>
        public byte[] DownloadExcelContent(DataTable sqlResult)
        {
            try{
                byte[] content;
                using (var workbook = new XLWorkbook())
                {
                    workbook.Worksheets.Add("中獎清單");
                    var worksheet = workbook.Worksheet("中獎清單"); //選excel 第一張 table               
                    worksheet.Cell("A1").InsertTable(sqlResult); //將DB的table寫入
                    using (var stream = new MemoryStream()){ //存入記憶體 寫入content傳出
                        workbook.SaveAs(stream);
                        content = stream.ToArray();
                        return content;
                    }
                }
            }catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                throw e;
            }
        }
        public DataTable ReadExcelToDataTable(IFormFile formFile){
            try{
                var result      = new DataTable();
                var checkHeader = false;
                using (MemoryStream ms = new MemoryStream()){
                    formFile.CopyTo(ms);
                    using (var workbook = new XLWorkbook(ms)){
                        var rowItem     = new Dictionary<string,object>();
                        var worksheet   = workbook.Worksheet(1); //選excel 第一張 table
                        var rows        = worksheet.RowsUsed();
                        var firstCellIndex  = 0;
                        var lastCellIndex   = 0;
                        foreach(var row in rows){
                            if(!checkHeader){
                                firstCellIndex  = row.FirstCellUsed().Address.ColumnNumber;
                                lastCellIndex   = row.LastCellUsed().Address.ColumnNumber;
                                foreach(var column in row.Cells(firstCellIndex,lastCellIndex)){
                                    result.Columns.Add(column.Value.ToString());
                                }
                                checkHeader = true;
                                continue;
                            }
                            var rowData = new List<string>();
                            foreach(var column in row.Cells(firstCellIndex,lastCellIndex)){
                                rowData.Add(column.Value.ToString());
                            }
                            result.Rows.Add(rowData.ToArray());
                        }
                    }
                }
                return result;
            }catch(Exception e){
                Console.WriteLine($"error:{e.ToString()}");
                throw e;
            }
        }
        public bool CheckEnvColumnConfig(JObject configData, DataColumnCollection excelColumns){
            try{
                var result = true;
                foreach (var col in configData){
                    if(!excelColumns.Contains(col.Key)){
                        result = false;
                        break;
                    }
                }
                return result;
            }catch(Exception e){
                Console.WriteLine($"OMGOMG:{e.ToString()}");
                throw e;
            }
        }
    }
}

