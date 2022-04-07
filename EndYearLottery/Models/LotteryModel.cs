using System;
using System.ComponentModel.DataAnnotations;
using CSICommon;

namespace EndYearLottery.Models
{
    public class LotteryModel{
        [Display(Description = "員工編號", Name = "emp_number")]
        [ColumnMappingFeature("empNumber","emp_number",false )]
        public String empNumber{get;set;}
        [Display(Description = "員工名稱", Name = "emp_name")]
        [ColumnMappingFeature("empName","emp_name",false )]
        public String empName{get;set;}
        [Display(Description = "獎品", Name = "present")]
        [ColumnMappingFeature("present","present",false )]
        public String present{get;set;}
        [Display(Description = "1為加碼", Name = "is_bonus")]
        [ColumnMappingFeature("is_bonus","is_bonus",false )]
        public Int32 is_bonus{get;set;}
        [Display(Description = "贊助廠商", Name = "sponsor")]
        [ColumnMappingFeature("sponsor","sponsor",false )]
        public String sponsor{get;set;}
        [Display(Description = "部門", Name = "department")]
        [ColumnMappingFeature("department","department",false )]
        public String department{get;set;}
        [Display(Description = "建立時間", Name = "create_time")]
        [ColumnMappingFeature("createTime","create_time",false )]
        public DateTime createTime{get;set;}
    }
}