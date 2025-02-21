using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CCSHealthFamilyWelfareDept.DAL;
using CCSHealthFamilyWelfareDept.Filters;
using CCSHealthFamilyWelfareDept.Models;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace CCSHealthFamilyWelfareDept.Controllers
{
    public class NiveshReconciliationController : Controller
    {
        //
        // GET: /NiveshReconciliation/
        Common_DB objComDB = new Common_DB();
        NiveshMitraAPI napi = new NiveshMitraAPI();
        public ActionResult Index()
        {
            returnServiceStatusRequest model = new returnServiceStatusRequest();
            returnServiceStatusResponse modelRes = new returnServiceStatusResponse();
            int res = 0;
            modelRes.returnServiceStatusResponseList = objComDB.GetListForReconciliation();

            if (modelRes.returnServiceStatusResponseList != null && modelRes.returnServiceStatusResponseList.Count > 0)
            {
                foreach (var item in modelRes.returnServiceStatusResponseList)
                {
                    model.ApplicationId = item.ApplicationId;
                    model.ControlId = item.ControlId;
                    model.UnitId = item.UnitId;
                    model.DeptId = item.DeptId;
                    model.ServiceId = item.ServiceId;
                    model.RequestId = item.RequestId;
                    model.ProcessIndustryId = item.ProcessIndustryId;
                    model.StatusCode = item.StatusCode;
                    model.ApplicationURL = "";
                    model.Remarks = item.Remarks;
                    model.PendecyLevel =  item.PendecyLevel;
                    model.Pending_with_Officer = item.Pending_with_Officer;
                    model.D1 = item.D1;
                    model.D2 = item.D2;
                    model.D3 = item.D3;
                    model.D4 = item.D4;
                    model.D5 = item.D5;
                    model.D6 = item.D6;
                    model.D7 = item.D7;
                    model.D8 = item.D8;
                    model.D19 = item.D9;
                    model.D10 = item.D10;
                    model.D11 = item.D11;
                    model.D12 = item.D12;
                    model.D13 = item.D13;
                    model.D14 = item.D14;
                    model.D15 = item.D15;
                    model.D16 = item.D16;
                    model.D17 = item.D17;
                    model.D18 = item.D18;
                    model.D19 = item.D19;
                    model.D20 = item.D20;
                    model.feeamount = 0;
                    model.feestatus = "";
                    model.NOC_Certificate_Number = item.NOC_Certificate_Number;
                    model.NOC_Url = item.NOC_Url;
                    model.IsNocUrlActiveYesNo = item.IsNocUrlActiveYesNo;
                    model.Objection_Rejection_Code = item.Objection_Rejection_Code;
                    model.Objection_Rejection_Url = item.Objection_Rejection_Url;
                    model.Is_Certificate_Valid_Life_Time = item.Is_Certificate_Valid_Life_Time;
                    model.Certificate_EXP_Date_DDMMYYYY = item.Certificate_EXP_Date_DDMMYYYY;
                    string GetAaplication = napi.returnServiceStatus(model);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);
                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        res = objComDB.UpdateReconcilation( item.ApplicationId);
                    }
                    else
                    {

                        res = 0; 
                    }

                }
            }
            return View();
        }
    }
}
