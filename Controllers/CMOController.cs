using CCSHealthFamilyWelfareDept.DAL;
using CCSHealthFamilyWelfareDept.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CCSHealthFamilyWelfareDept.Filters;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CCSHealthFamilyWelfareDept.ReportModel;
using System.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Data.Entity.Infrastructure;

namespace CCSHealthFamilyWelfareDept.Controllers
{
    [AuthorizeAdmin]
    public class CMOController : Controller
    {
        NUH_DB objNUHDB = new NUH_DB();
        FAP_DB objFAP_DB = new FAP_DB();
        AGC_DB objAGC_DB = new AGC_DB();
        MER_DB objMER_DB = new MER_DB();
        CMO_DB objCMODB = new CMO_DB();
        Common_DB comndb = new Common_DB();
        Common objCom = new Common();
        Account_DB objAccDb = new Account_DB();
        SessionManager objSM = new SessionManager();
        NiveshMitraAPI napi = new NiveshMitraAPI();
        #region Method Set Sweet Alert Message
        protected void setSweetAlertMsg(string msg, string msgStatus)
        {
            ViewBag.Msg = msg;
            ViewBag.MsgStatus = msgStatus;
        }
        #endregion

        #region Medical Establishment Azeez
        [HttpGet]
        [AuthorizeAdmin(1)]
        public ActionResult AppliedApplicationNUH()
        {
            ProcessType model = new ProcessType();
            model = objCMODB.getMethodApplicationCountNUH(objSM.UserID);
            model.DistrictList = objCMODB.GetDistrictList(7, 34);
            ViewBag.totalUploadedApp = objCMODB.getMethodApplicationCountNUHForImageUpload(objSM.UserID).totalUploadedApp;
            ViewBag.totalAppApproved = objCMODB.getMethodApplicationCountNUHForImageUpload(objSM.UserID).totalAppApproved;
            return View(model);
            //ProcessType model = new ProcessType();

            //var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            //var resultData = objCMODB.GetAllNUHList().Where(m => lstCMODistrict.Any(p => p.districtId == m.districtid));
            //model.totalAppReceived = resultData.Where(m => m.appStatus == -1 || m.appStatus == 1).Count();
            //model.totalAppApproved = resultData.Where(m => m.appStatus == 6).Count();
            //model.totalAppRejected = resultData.Where(m => m.appStatus == 0 || m.appStatus == 4).Count();
            //model.totalAppInProcess = resultData.Where(m => m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 5).Count();

            //return View(model);
        }

        #region
        [HttpPost]
        public ActionResult AppliedApplicationNUH(ProcessType model)
        {
            if (!String.IsNullOrEmpty(model.fromDate) && !String.IsNullOrEmpty(model.toDate))
            {
                TempData["fDate"] = model.fromDate;
                TempData["tDate"] = model.toDate;
            }
            else
            {
                TempData["fDate"] = "";
                TempData["tDate"] = "";
                model.fromDate = "";
                model.toDate = "";
            }
            if (model.District != 0)
            {
                TempData["District"] = model.District;
            }
            else
            {
                TempData["District"] = 0;
                model.District = 0;
            }

            //ViewBag.District = comndb.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            model = objCMODB.getApplicationCountNUH(objSM.UserID, model.fromDate, model.toDate, model.District);
            model.DistrictList = objCMODB.GetDistrictList(7, 34);
            return View(model);
        }
        #endregion


        [AuthorizeAdmin(1)]
        public ActionResult ReceivedApplicationNUH()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("NUH").Where(m => m.Value == "-1" || m.Value == "1").ToList();
            return View();
        }

        [AuthorizeAdmin(1)]
        public ActionResult ReceivedApplicationListNUH(string registrationNo = "", string appDate = "", string status = "")
        {
            int intStatus = 0;
            List<NUHDetailsModel> lstNUHDetails = new List<NUHDetailsModel>();

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (TempData["fDate"] == null && TempData["tDate"] == null)
            {
                TempData["fDate"] = "";
                TempData["tDate"] = "";

            }
            if (Convert.ToInt32(TempData["District"]) == 0)
            {
                TempData["District"] = 0;
            }
            if (objSM.RollID == 8)
            {
                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString(), Convert.ToInt32(TempData["District"])).Where(m => (m.appStatus == -1 || m.appStatus == 1)).ToList();
            }
            else
            {
                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString()).Where(m => (m.appStatus == -1 || m.appStatus == 1) && (lstCMODistrict.Any(p => p.districtId == m.districtid))).ToList();

            }

            intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;
            lstNUHDetails = lstNUHDetails.Where(m => (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "")).ToList();

            return PartialView("_ReceivedApplicationListNUH", lstNUHDetails);
        }

        #region Save Query Raised vk
        //ADD A VIEW TO SAVE QUERY
        //[AuthorizeAdmin(1)]

        public ActionResult SaveQueryDetailToApplicant(string RegisNUHID, string QueryRaised, string file)
        {

            string errormsg = "";
            string StatusResult = string.Empty;
            NUHmodel model = new NUHmodel();
            model.regisIdNUH = Convert.ToInt64(RegisNUHID);
            string ip = Common.GetIPAddress();
            model = objNUHDB.GetNUHListBYRegistrationNo(model.regisIdNUH);

            long regByUser = model.regByUser;

            if (file == "queryFilePath")
            {
                file = null;

            }
            model = objCMODB.InsertUpdateQueryRaisedDetail(QueryRaised, file, objSM.UserID.ToString(), ip, Convert.ToInt64(RegisNUHID), "1");
            if (model != null)
            {
                
                //UPHEALTHNIC.upswp_niveshmitraservices ObjSendAppSubmitStatus = new UPHEALTHNIC.upswp_niveshmitraservices();
                NiveshMitraSendStatusModel userDetails = new NiveshMitraSendStatusModel();
                returnServiceStatusRequest nModel = new returnServiceStatusRequest();
                userDetails = objCMODB.GetNiveshMitraUserDetailsByID(Convert.ToInt64(RegisNUHID)).FirstOrDefault();
                if (userDetails != null)
                {
                    nModel.ControlId = userDetails.Control_ID;
                    nModel.UnitId = userDetails.Unit_Id;
                    nModel.DeptId = userDetails.Dept_ID;
                    nModel.ServiceId = userDetails.ServiceID;
                    nModel.RequestId = userDetails.RequestId;
                    nModel.ApplicationId = Convert.ToString(userDetails.UserID); //retval.regisIdNUH.ToString();  //objSM.UserID.ToString(); // retval.regisIdNUH.ToString(); 
                    nModel.ProcessIndustryId = objSM.Username;
                    nModel.StatusCode = "08";
                    nModel.ApplicationURL = "";
                    nModel.Remarks = "Query Raised by CMO ";
                    nModel.PendecyLevel = "Entrepreneur level pendency";
                    nModel.Pending_with_Officer = userDetails.CMODistrictName + ' ' + "(CMO)";

                    nModel.D1 = "";
                    nModel.D2 = "";
                    nModel.D3 = "";
                    nModel.D4 = "";
                    nModel.D5 = "";
                    nModel.D6 = "";
                    nModel.D7 = "";
                    nModel.D8 = "";
                    nModel.D9 = "";
                    nModel.D10 = "";
                    nModel.D11 = "";
                    nModel.D12 = "";
                    nModel.D13 = "";
                    nModel.D14 = "";

                    nModel.D15 = "";
                    nModel.D16 = "";
                    nModel.D17 = "";
                    nModel.D18 = "";
                    nModel.D19 = "";
                    nModel.D20 = "";

                    nModel.feeamount = 0;
                    nModel.feestatus = "";
                    nModel.NOC_Certificate_Number = "";
                    nModel.NOC_Url = "";
                    nModel.IsNocUrlActiveYesNo = "";
                    nModel.Objection_Rejection_Code = "";
                    nModel.Objection_Rejection_Url = "";
                    nModel.Is_Certificate_Valid_Life_Time = "";
                    nModel.Certificate_EXP_Date_DDMMYYYY = "";

                    string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
                    GetValidateResponse ga = new GetValidateResponse();
                    string GetAaplication = napi.returnServiceStatus(nModel);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);

                    RanSchedule("API returnServiceStatus :" + RR.isSuccess);

                    //objStatusModel.ProcessIndustryID = objStatusModel.UserName;
                    //objStatusModel.ApplicationID = RegisNUHID.ToString(); //objStatusModel.UserID.ToString();

                    //objStatusModel.StatusCode = "08";
                    //objStatusModel.Remarks = QueryRaised + "  " + "|" + " " + " Query Raised by " + " " + objSM.DisignationName + "," + " " + objSM.DistrictName;
                    //objStatusModel.PendencyLevel = "Entrepreneur level pendency";

                    //objStatusModel.FeeAmount = "";
                    //objStatusModel.FeeStatus = "";
                    //objStatusModel.TransectionID = "";
                    //objStatusModel.TranSactionDate = "";
                    //objStatusModel.TransectionDateAndTime = "";
                    //objStatusModel.NocCertificateNumber = "";
                    //objStatusModel.NocUrl = "";
                    //objStatusModel.IsNocUrlActiveYesNo = "";
                    //objStatusModel.Passalt = ConfigurationManager.AppSettings["PassKey"].ToString();
                    //objStatusModel.ObjectRejectionCode = "";
                    //objStatusModel.IsCertificateValidLifeTime = "";
                    //objStatusModel.CertificateExpireDateDDMMYYYY = "";
                    //objStatusModel.D1 = "";
                    //objStatusModel.D2 = "";
                    //objStatusModel.D3 = "";
                    //objStatusModel.D4 = "";
                    //objStatusModel.D5 = "";
                    //objStatusModel.D6 = "";
                    //objStatusModel.D7 = "";

                    //StatusResult = ObjSendAppSubmitStatus.WReturn_CUSID_STATUS(objStatusModel.Control_ID, objStatusModel.Unit_Id, objStatusModel.ServiceID, objStatusModel.ProcessIndustryID, objStatusModel.ApplicationID, objStatusModel.StatusCode,
                    //       objStatusModel.Remarks, objStatusModel.PendencyLevel, objStatusModel.FeeAmount, objStatusModel.FeeStatus, objStatusModel.TransectionID, objStatusModel.TranSactionDate, objStatusModel.TransectionDateAndTime, objStatusModel.NocCertificateNumber, objStatusModel.NocUrl, objStatusModel.IsNocUrlActiveYesNo, objStatusModel.Passalt, objStatusModel.RequestId, objStatusModel.ObjectRejectionCode
                    //        , objStatusModel.IsCertificateValidLifeTime, objStatusModel.CertificateExpireDateDDMMYYYY, objStatusModel.D1, objStatusModel.D2, objStatusModel.D3, objStatusModel.D4, objStatusModel.D5, objStatusModel.D6, objStatusModel.D7);

                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 5;

                    }
                    else
                    {

                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 5;

                    }

                    try
                    {
                        userDetails = objCMODB.SaveCmoActionAndNiveshStatus(userDetails).FirstOrDefault();

                    }
                    catch (Exception ex)
                    {

                    }

                }
                errormsg = model.QMessage;
                setSweetAlertMsg(errormsg, "warning");
            }
            else
            {

                errormsg = "0";
            }

            return Json(errormsg, JsonRequestBehavior.AllowGet);
            // return View();

        }

        #endregion

        [AuthorizeAdmin(1)]
        public ActionResult ViewAppDetailsNUH(long regisId)
        {

            NUHmodel model = new NUHmodel();
            model = objNUHDB.GetNUHListBYRegistrationNo(regisId);
            ////change vk
            //if (model.QueryRaisedByCMO == null)
            //{
            //    model.QueryRaisedByCMO = "";
            //}
            //if (model.ReplyQueryByApplicant == null)
            //{
            //    model.ReplyQueryByApplicant = "";
            //}

            ////change End


            //ViewBag.ReasonList = comndb.GetDropDownList(65, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            model.NUHModelList = objNUHDB.getNUHChild(regisId);
            ViewBag.VBoutpateint = objNUHDB.GetoutPatient(Convert.ToInt64(regisId));
            ViewBag.VBolaboratory = objNUHDB.GetNUHlaboratory(Convert.ToInt64(regisId));
            ViewBag.VBimaging = objNUHDB.GetNUHimaging(Convert.ToInt64(regisId));

            model.NUHPartnerList = objNUHDB.getNUHPartner(regisId);
            model.NUHDOCList = objNUHDB.getNUHdoc(regisId);
            model.NUHModelList = objNUHDB.getNUHChild(regisId);

            Session["regisIdNUH"] = regisId;


            return PartialView("_ViewAppDetailsNUH", model);
        }

        [AuthorizeAdmin(1)]
        public ActionResult InProcessApplicationNUH()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("NUH").Where(m => m.Value == "2" || m.Value == "3" || m.Value == "5").ToList();
            return View();
        }

        [AuthorizeAdmin(1)]
        public ActionResult InProcessApplicationListNUH(string registrationNo = "", string appDate = "", string status = "")
        {
            int intStatus = 0;
            List<NUHDetailsModel> lstNUHDetails = new List<NUHDetailsModel>();

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (TempData["fDate"] == null && TempData["tDate"] == null)
            {
                TempData["fDate"] = "";
                TempData["tDate"] = "";
            }
            if (Convert.ToInt32(TempData["District"]) == 0)
            {
                TempData["District"] = 0;
            }
            if (objSM.RollID == 8)
            {
                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString(), Convert.ToInt32(TempData["District"])).Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 5)).ToList();
            }
            else
            {

                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString()).Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 5) && (lstCMODistrict.Any(p => p.districtId == m.districtid))).ToList();
            }
            //Before
            //  lstNUHDetails = objCMODB.GetAllNUHList(0).Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 5) && (lstCMODistrict.Any(p => p.districtId == m.districtid))).ToList();

            intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;
            lstNUHDetails = lstNUHDetails.Where(m => (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "")).ToList();

            return PartialView("_InProcessApplicationListNUH", lstNUHDetails);
        }

        [AuthorizeAdmin(1)]
        public ActionResult NUHAppDetails(long regisId)
        {
            NUHDetailsModel model = new NUHDetailsModel();
            model = objCMODB.GetAllNUHList(0).Where(m => m.regisIdNUH == regisId).FirstOrDefault();
            return PartialView("_NUHAppDetails", model);
        }

        [AuthorizeAdmin(1)]
        public ActionResult UpdateAppProcessNUH(string regisId, string status)
        {
            bool validRequest = false;
            NUHAppProcessModel model = new NUHAppProcessModel();
            model.regisIdNUH = Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId));
            model.appStatus = Convert.ToInt32(OTPL_Imp.CustomCryptography.Decrypt(status));
            ViewBag.DLLCommittee = comndb.GetCommitteeDetailsForDLL().ToList(); //comndb.GetDropDownList(33, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() }); 
            model = objCMODB.GetNUHList(model.regisIdNUH, model.appStatus);

            if (model != null && objSM.districtId != model.districtid)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            if (model.appStatus == 2)
            {
                ViewBag.designation = comndb.GetDropDownList(33, Convert.ToInt32(objSM.UserID)).ToList();//objCMODB.binddesignation().ToList();
                if (ViewBag.designation != null && ViewBag.designation.Count > 0)
                {
                    validRequest = true;
                }
                //if (model.committeeId != 0 && model.committeeId != 0)
                //{
                //    int comid = Convert.ToInt32(model.committeeId);
                //    ViewBag.designation = comndb.GetDropDownList(34, comid).ToList();//objCMODB.binddesignation().ToList();
                //}
            }

            if (model.appStatus == -1)
            {
                ViewBag.PageTitle = "Accept/Reject Application";
            }
            else if (model.appStatus == 1)
            {
                ViewBag.PageTitle = "Schedule Inspection Application";
            }
            else if (model.appStatus == 2)
            {
                ViewBag.PageTitle = "Upload Inspection Report";
            }
            else if (model.appStatus == 3)
            {
                ViewBag.PageTitle = "Accept/Reject Inspection Report";
            }
            else if (model.appStatus == 5)
            {
                ViewBag.PageTitle = "Generate Certificate";
            }
            ViewBag.validRequest = validRequest;
            ViewBag.ReasonList = comndb.GetDropDownList(65, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            return View(model);
        }

        [AuthorizeAdmin(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAppProcessNUH(NUHAppProcessModel model, string button, FormCollection form)
        {
            if (Session["PhotoPath"] != null)
            {
                string[] fileArray = Session["PhotoPath"] as string[];

                if (fileArray != null)
                {
                    model.inspReportFilePhotoPath = fileArray;
                }
            }


            ModelState["QueryRaisedByCMO"].Errors.Clear();
            ModelState["inspReportFilePhoto"].Errors.Clear();
            var result = objCMODB.GetNUHList(model.regisIdNUH, model.appStatus);
            AuditMethods objAud = new AuditMethods();
            string errormsg = "";
            bool valStatus = false;
            if (result == null || objSM.districtId != result.districtid)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            ResultSet resultData = new ResultSet();
            int currAppStatus = model.appStatus;
            int? appStatus = null;
            bool isRedirectNewAction = false;
            string message = "", actionName = "";

            if (!string.IsNullOrEmpty(model.inspReportFilePath))
            {
                valStatus = objAud.IsValidLink(model.inspReportFilePath, "Document File", out errormsg);
                if (!valStatus)
                {
                    setSweetAlertMsg(errormsg, "warning");
                    return View(model);
                }
            }

            if (!string.IsNullOrEmpty(model.certificateFilePath))
            {
                valStatus = objAud.IsValidLink(model.certificateFilePath, "Document File", out errormsg);
                if (!valStatus)
                {
                    setSweetAlertMsg(errormsg, "warning");
                    return View(model);
                }
            }


            if (!string.IsNullOrEmpty(button))
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();
                ModelState["inspReportFile"].Errors.Clear();
                ModelState["inspReportFilePhoto"].Errors.Clear();
                //ModelState["ReasonID"].Errors.Clear();

                if (button.ToLower() == "appaccept")
                {
                    ModelState["ReasonID"].Errors.Clear();
                    ModelState["inspReportFilePhoto"].Errors.Clear();
                    appStatus = 1;
                    message = "Application Accepted";
                    actionName = "UpdateAppProcessNUH";
                }
                else if (button.ToLower() == "insaccept")
                {
                    ModelState["ReasonID"].Errors.Clear();
                    ModelState["inspReportFilePhoto"].Errors.Clear();
                    appStatus = 5;
                    message = "Inspection Report Accepted";
                    actionName = "UpdateAppProcessNUH";

                }
                else if (button.ToLower() == "gencertificate")
                {
                    ModelState["inspReportFilePhoto"].Errors.Clear();
                    appStatus = 6;
                    message = "Certificate Generated";
                    actionName = "UpdateAppProcessNUH";
                    TempData["DatSetSec"] = model.regisIdNUH;

                    // isRedirectNewAction = true;
                    //NUHgeneratedCertificate(model.regisIdNUH);
                }
            }
            else
            {
                if (model.appStatus == -1)
                {
                    ModelState["inspReportFilePhoto"].Errors.Clear();
                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["inspReportFile"].Errors.Clear();

                    appStatus = 0;
                    message = "Application Rejected";
                    actionName = "RejectedApplicationNUH";
                    isRedirectNewAction = true;
                }
                else if (model.appStatus == 1)
                {
                    ModelState["inspReportFilePhoto"].Errors.Clear();
                    ModelState["rejectedRemarks"].Errors.Clear();
                    ModelState["inspReportFile"].Errors.Clear();
                    //ModelState["ReasonID"].Errors.Clear();
                    appStatus = 2;
                    //Session["COMMITTEEID"] = model.committeeId;
                    message = "Inspection Scheduled";
                    actionName = "InProcessApplicationNUH";
                    isRedirectNewAction = true;
                }
                else if (model.appStatus == 2)
                {
                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["rejectedRemarks"].Errors.Clear();
                    ModelState["inspReportFilePhoto"].Errors.Clear();

                    //ModelState["ReasonID"].Errors.Clear();
                    appStatus = 3;

                    #region Vinod Bulkinsertion of Inspection Photo

                    string XmlDataPIC = "";


                    //var committeeMembderId = form.GetValues("chkdesig");

                    int countPic = model.inspReportFilePhotoPath.Count();
                    XmlDataPIC = "<InspectionPICS>";
                    //  long regisByuser = objSM.UserID;
                    for (int i = 0; i < countPic; i++)
                    {
                        if (model.inspReportFilePhotoPath[i] == "")
                        {
                            //XmlData = string.Empty;
                        }
                        else
                        {

                            XmlDataPIC += "<InspectionPIC><UploadPICFilePath>" + model.inspReportFilePhotoPath[i] + "</UploadPICFilePath></InspectionPIC>";
                        }

                    }
                    XmlDataPIC += "</InspectionPICS>";
                    model.XmlDataPhoto = XmlDataPIC;

                    #endregion

                    #region Riya bulkinsertion

                    string XmlData1 = "";


                    var committeeMembderId = form.GetValues("chkdesig");

                    int count = committeeMembderId.Count();
                    XmlData1 = "<Members>";
                    long regisByuser = objSM.UserID;
                    for (int i = 0; i < count; i++)
                    {
                        if (committeeMembderId[i] == "")
                        {
                            //XmlData = string.Empty;
                        }
                        else
                        {

                            XmlData1 += "<Member><committeeMembderId>" + committeeMembderId[i] + "</committeeMembderId></Member>";
                        }

                    }
                    XmlData1 += "</Members>";
                    model.XmlData = XmlData1;
                    #endregion
                    message = "Inspection Report Uploaded";
                    actionName = "UpdateAppProcessNUH";
                }
                else if (model.appStatus == 3)
                {
                    ModelState["inspReportFilePhoto"].Errors.Clear();
                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["inspReportFile"].Errors.Clear();

                    appStatus = 4;

                    message = "Inspection Report Rejected";
                    actionName = "RejectedApplicationNUH";
                    isRedirectNewAction = true;
                }
            }

            if (ModelState.IsValid && appStatus != null)
            {
                model.appStatus = Convert.ToInt32(appStatus);
                model.userId = objSM.UserID;
                model.transIp = Common.GetIPAddress();

                try
                {
                    resultData = objCMODB.UpdateAppProcessNUH(model.regisIdNUH, model.appStatus, model.committeeId, model.inspectionDate, model.inspReportFilePath, model.certificateFilePath, model.rejectedRemarks, model.ReasonID, model.userId, model.transIp, model.XmlData, model.XmlDataPhoto);
                }
                catch (Exception ex)
                {
                    setSweetAlertMsg(ex.Message, "warning");
                }
            }
            else
            {
                setSweetAlertMsg("Enter valid data !", "warning");
            }

            if (resultData != null && resultData.Flag > 0)
            {

                //Send Status to Niveshmitra of acccept application of medical eastablishment through web services.

                SaveAndSendCMOLevalStatusToNivesh(model.regisIdNUH, appStatus, resultData.RegisterByuserID);


                SendSMSUpdateProcessNUH(resultData.RegistrationNo, resultData.inspectionDate, resultData.MobileNo, resultData.appStatus);
                TempData["SuccessMsg"] = message;
                if (isRedirectNewAction)
                {
                    return RedirectToAction(actionName);
                }
                else
                {
                    return RedirectToAction(actionName, new { regisId = OTPL_Imp.CustomCryptography.Encrypt(model.regisIdNUH.ToString()), status = OTPL_Imp.CustomCryptography.Encrypt(model.appStatus.ToString()) });
                }

            }
            else
            {
                model.appStatus = Convert.ToInt32(currAppStatus);
                ViewBag.DLLCommittee = comndb.GetCommitteeDetailsForDLL().ToList();
                ViewBag.ReasonList = comndb.GetDropDownList(65, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                return View(model);
            }
        }


        public void SaveAndSendCMOLevalStatusToNivesh(long regisIdNUH, int? appStatus, long regisByUser)
        {
            NiveshMitraSendStatusModel userDetails = new NiveshMitraSendStatusModel();
            returnServiceStatusRequest nModel = new returnServiceStatusRequest();
            userDetails = objCMODB.GetNiveshMitraUserDetailsByID(Convert.ToInt64(regisIdNUH)).FirstOrDefault();
            //NiveshMitraSendStatusModel objStatusModel = objCMODB.GetNiveshMitraUserDetailsByID(regisIdNUH).FirstOrDefault();

            string StatusResult = string.Empty;

            if (userDetails != null)
            {
                if (appStatus == 1)
                {
                    nModel.ControlId = userDetails.Control_ID;
                    nModel.UnitId = userDetails.Unit_Id;
                    nModel.DeptId = userDetails.Dept_ID;
                    nModel.ServiceId = userDetails.ServiceID;
                    nModel.RequestId = userDetails.RequestId;
                    nModel.ApplicationId = Convert.ToString(userDetails.UserID); //retval.regisIdNUH.ToString();  //objSM.UserID.ToString(); // retval.regisIdNUH.ToString(); 
                    nModel.ProcessIndustryId = objSM.Username;
                    nModel.StatusCode = "05";
                    nModel.ApplicationURL = "";
                    nModel.Remarks = "Application Accepted";
                    nModel.PendecyLevel = "Officer level pendency";
                    nModel.Pending_with_Officer = "Pending at" + " " + objSM.DisignationName + "," + " " + objSM.DistrictName+ ' ' + "(CMO)";

                    nModel.D1 = "";
                    nModel.D2 = "";
                    nModel.D3 = "";
                    nModel.D4 = "";
                    nModel.D5 = "";
                    nModel.D6 = "";
                    nModel.D7 = "";
                    nModel.D8 = "";
                    nModel.D9 = "";
                    nModel.D10 = "";
                    nModel.D11 = "";
                    nModel.D12 = "";
                    nModel.D13 = "";
                    nModel.D14 = "";

                    nModel.D15 = "";
                    nModel.D16 = "";
                    nModel.D17 = "";
                    nModel.D18 = "";
                    nModel.D19 = "";
                    nModel.D20 = "";

                    nModel.feeamount = 0;
                    nModel.feestatus = "";
                    nModel.NOC_Certificate_Number = "";
                    nModel.NOC_Url = "";
                    nModel.IsNocUrlActiveYesNo = "";
                    nModel.Objection_Rejection_Code = "";
                    nModel.Objection_Rejection_Url = "";
                    nModel.Is_Certificate_Valid_Life_Time = "";
                    nModel.Certificate_EXP_Date_DDMMYYYY = "";

                    string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
                    GetValidateResponse ga = new GetValidateResponse();
                    string GetAaplication = napi.returnServiceStatus(nModel);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);

                    RanSchedule("API returnServiceStatus :" + RR.isSuccess);

                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 5;

                    }
                    else
                    {

                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 5;

                    }
                    try
                    {
                        userDetails = objCMODB.SaveCmoActionAndNiveshStatus(userDetails).FirstOrDefault();

                    }
                    catch (Exception ex)
                    {

                    }

                }
                else if (appStatus == 0)
                {
                    nModel.ControlId = userDetails.Control_ID;
                    nModel.UnitId = userDetails.Unit_Id;
                    nModel.DeptId = userDetails.Dept_ID;
                    nModel.ServiceId = userDetails.ServiceID;
                    nModel.RequestId = userDetails.RequestId;
                    nModel.ApplicationId = Convert.ToString(userDetails.UserID); //retval.regisIdNUH.ToString();  //objSM.UserID.ToString(); // retval.regisIdNUH.ToString(); 
                    nModel.ProcessIndustryId = objSM.Username;
                    nModel.StatusCode = "07";
                    nModel.ApplicationURL = "";
                    nModel.Remarks =  userDetails.Remarks + "  " + "|" + "  " + "Application Rejected by" + " " + objSM.DisignationName + "," + "  " + objSM.DistrictName; ;
                    nModel.PendecyLevel = "";
                    nModel.Pending_with_Officer = "";

                    nModel.D1 = "";
                    nModel.D2 = "";
                    nModel.D3 = "";
                    nModel.D4 = "";
                    nModel.D5 = "";
                    nModel.D6 = "";
                    nModel.D7 = "";
                    nModel.D8 = "";
                    nModel.D9 = "";
                    nModel.D10 = "";
                    nModel.D11 = "";
                    nModel.D12 = "";
                    nModel.D13 = "";
                    nModel.D14 = "";

                    nModel.D15 = "";
                    nModel.D16 = "";
                    nModel.D17 = "";
                    nModel.D18 = "";
                    nModel.D19 = "";
                    nModel.D20 = "";

                    nModel.feeamount = 0;
                    nModel.feestatus = "";
                    nModel.NOC_Certificate_Number = "";
                    nModel.NOC_Url = "";
                    nModel.IsNocUrlActiveYesNo = "";
                    nModel.Objection_Rejection_Code = userDetails.ObjectRejectionCode;
                    nModel.Objection_Rejection_Url = "P/Rject";
                    nModel.Is_Certificate_Valid_Life_Time = "";
                    nModel.Certificate_EXP_Date_DDMMYYYY = "";

                    string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
                    GetValidateResponse ga = new GetValidateResponse();
                    string GetAaplication = napi.returnServiceStatus(nModel);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);

                    RanSchedule("API returnServiceStatus :" + RR.isSuccess);

                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 5;

                    }
                    else
                    {

                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 5;

                    }

                   
                    try
                    {
                        userDetails = objCMODB.SaveCmoActionAndNiveshStatus(userDetails).FirstOrDefault();

                    }
                    catch (Exception ex)
                    {

                    }

                }
                else if (appStatus == 2)
                {

                    nModel.ControlId = userDetails.Control_ID;
                    nModel.UnitId = userDetails.Unit_Id;
                    nModel.DeptId = userDetails.Dept_ID;
                    nModel.ServiceId = userDetails.ServiceID;
                    nModel.RequestId = userDetails.RequestId;
                    nModel.ApplicationId = Convert.ToString(userDetails.UserID); //retval.regisIdNUH.ToString();  //objSM.UserID.ToString(); // retval.regisIdNUH.ToString(); 
                    nModel.ProcessIndustryId = objSM.Username;
                    nModel.StatusCode = "05";
                    nModel.ApplicationURL = "";
                    nModel.Remarks = "Inspection Scheduled";
                    nModel.PendecyLevel = "";
                    nModel.Pending_with_Officer = "Pending at" + " " + objSM.DisignationName + "," + " " + objSM.DistrictName + ' ' + "(CMO)";

                    nModel.D1 = "";
                    nModel.D2 = "";
                    nModel.D3 = "";
                    nModel.D4 = "";
                    nModel.D5 = "";
                    nModel.D6 = "";
                    nModel.D7 = "";
                    nModel.D8 = "";
                    nModel.D9 = "";
                    nModel.D10 = "";
                    nModel.D11 = "";
                    nModel.D12 = "";
                    nModel.D13 = "";
                    nModel.D14 = "";

                    nModel.D15 = "";
                    nModel.D16 = "";
                    nModel.D17 = "";
                    nModel.D18 = "";
                    nModel.D19 = "";
                    nModel.D20 = "";

                    nModel.feeamount = 0;
                    nModel.feestatus = "";
                    nModel.NOC_Certificate_Number = "";
                    nModel.NOC_Url = "";
                    nModel.IsNocUrlActiveYesNo = "";
                    nModel.Objection_Rejection_Code = "";
                    nModel.Objection_Rejection_Url = "";
                    nModel.Is_Certificate_Valid_Life_Time = "";
                    nModel.Certificate_EXP_Date_DDMMYYYY = "";

                    string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
                    GetValidateResponse ga = new GetValidateResponse();
                    string GetAaplication = napi.returnServiceStatus(nModel);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);

                    RanSchedule("API returnServiceStatus :" + RR.isSuccess);

                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 6;

                    }
                    else
                    {

                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 6;

                    }

                    try
                    {
                        userDetails = objCMODB.SaveCmoActionAndNiveshStatus(userDetails).FirstOrDefault();

                    }
                    catch (Exception ex)
                    {

                    }


                }
                else if (appStatus == 3)
                {

                    nModel.ControlId = userDetails.Control_ID;
                    nModel.UnitId = userDetails.Unit_Id;
                    nModel.DeptId = userDetails.Dept_ID;
                    nModel.ServiceId = userDetails.ServiceID;
                    nModel.RequestId = userDetails.RequestId;
                    nModel.ApplicationId = Convert.ToString(userDetails.UserID); //retval.regisIdNUH.ToString();  //objSM.UserID.ToString(); // retval.regisIdNUH.ToString(); 
                    nModel.ProcessIndustryId = objSM.Username;
                    nModel.StatusCode = "05";
                    nModel.ApplicationURL = "";
                    nModel.Remarks = "Inspection Report Uploaded";
                    nModel.PendecyLevel = "";
                    nModel.Pending_with_Officer = "Pending at" + " " + objSM.DisignationName + "," + " " + objSM.DistrictName + ' ' + "(CMO)";

                    nModel.D1 = "";
                    nModel.D2 = "";
                    nModel.D3 = "";
                    nModel.D4 = "";
                    nModel.D5 = "";
                    nModel.D6 = "";
                    nModel.D7 = "";
                    nModel.D8 = "";
                    nModel.D9 = "";
                    nModel.D10 = "";
                    nModel.D11 = "";
                    nModel.D12 = "";
                    nModel.D13 = "";
                    nModel.D14 = "";

                    nModel.D15 = "";
                    nModel.D16 = "";
                    nModel.D17 = "";
                    nModel.D18 = "";
                    nModel.D19 = "";
                    nModel.D20 = "";

                    nModel.feeamount = 0;
                    nModel.feestatus = "";
                    nModel.NOC_Certificate_Number = "";
                    nModel.NOC_Url = "";
                    nModel.IsNocUrlActiveYesNo = "";
                    nModel.Objection_Rejection_Code = "";
                    nModel.Objection_Rejection_Url = "";
                    nModel.Is_Certificate_Valid_Life_Time = "";
                    nModel.Certificate_EXP_Date_DDMMYYYY = "";

                    string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
                    GetValidateResponse ga = new GetValidateResponse();
                    string GetAaplication = napi.returnServiceStatus(nModel);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);

                    RanSchedule("API returnServiceStatus :" + RR.isSuccess);

                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 7;

                    }
                    else
                    {

                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 7;

                    }




                    try
                    {
                        userDetails = objCMODB.SaveCmoActionAndNiveshStatus(userDetails).FirstOrDefault();

                    }
                    catch (Exception ex)
                    {

                    }
                }
                else if (appStatus == 4)
                {
                    nModel.ControlId = userDetails.Control_ID;
                    nModel.UnitId = userDetails.Unit_Id;
                    nModel.DeptId = userDetails.Dept_ID;
                    nModel.ServiceId = userDetails.ServiceID;
                    nModel.RequestId = userDetails.RequestId;
                    nModel.ApplicationId = Convert.ToString(userDetails.UserID); //retval.regisIdNUH.ToString();  //objSM.UserID.ToString(); // retval.regisIdNUH.ToString(); 
                    nModel.ProcessIndustryId = objSM.Username;
                    nModel.StatusCode = "07";
                    nModel.ApplicationURL = "";
                    nModel.Remarks = userDetails.Remarks + "  " + "|" + "  " + "Application Rejected by" + " " + objSM.DisignationName + "," + "  " + objSM.DistrictName; ;
                    nModel.PendecyLevel = "";
                    nModel.Pending_with_Officer = "";

                    nModel.D1 = "";
                    nModel.D2 = "";
                    nModel.D3 = "";
                    nModel.D4 = "";
                    nModel.D5 = "";
                    nModel.D6 = "";
                    nModel.D7 = "";
                    nModel.D8 = "";
                    nModel.D9 = "";
                    nModel.D10 = "";
                    nModel.D11 = "";
                    nModel.D12 = "";
                    nModel.D13 = "";
                    nModel.D14 = "";

                    nModel.D15 = "";
                    nModel.D16 = "";
                    nModel.D17 = "";
                    nModel.D18 = "";
                    nModel.D19 = "";
                    nModel.D20 = "";

                    nModel.feeamount = 0;
                    nModel.feestatus = "";
                    nModel.NOC_Certificate_Number = "";
                    nModel.NOC_Url = "";
                    nModel.IsNocUrlActiveYesNo = "";
                    nModel.Objection_Rejection_Code = userDetails.ObjectRejectionCode;
                    nModel.Objection_Rejection_Url = "P/Rject";
                    nModel.Is_Certificate_Valid_Life_Time = "";
                    nModel.Certificate_EXP_Date_DDMMYYYY = "";

                    string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
                    GetValidateResponse ga = new GetValidateResponse();
                    string GetAaplication = napi.returnServiceStatus(nModel);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);

                    RanSchedule("API returnServiceStatus :" + RR.isSuccess);

                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 9;

                    }
                    else
                    {

                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 9;

                    }

                    try
                    {
                        userDetails = objCMODB.SaveCmoActionAndNiveshStatus(userDetails).FirstOrDefault();

                    }
                    catch (Exception ex)
                    {

                    }

                }
                else if (appStatus == 5)
                {
                    nModel.ControlId = userDetails.Control_ID;
                    nModel.UnitId = userDetails.Unit_Id;
                    nModel.DeptId = userDetails.Dept_ID;
                    nModel.ServiceId = userDetails.ServiceID;
                    nModel.RequestId = userDetails.RequestId;
                    nModel.ApplicationId = Convert.ToString(userDetails.UserID); //retval.regisIdNUH.ToString();  //objSM.UserID.ToString(); // retval.regisIdNUH.ToString(); 
                    nModel.ProcessIndustryId = objSM.Username;
                    nModel.StatusCode = "06";
                    nModel.ApplicationURL = "";
                    nModel.Remarks = "Application approve on the behalf of Inspection Report";
                    nModel.PendecyLevel = "Officer level pendency";
                    nModel.Pending_with_Officer = "Pending at" + " " + objSM.DisignationName + "," + " " + objSM.DistrictName + ' ' + "(CMO)";

                    nModel.D1 = "";
                    nModel.D2 = "";
                    nModel.D3 = "";
                    nModel.D4 = "";
                    nModel.D5 = "";
                    nModel.D6 = "";
                    nModel.D7 = "";
                    nModel.D8 = "";
                    nModel.D9 = "";
                    nModel.D10 = "";
                    nModel.D11 = "";
                    nModel.D12 = "";
                    nModel.D13 = "";
                    nModel.D14 = "";

                    nModel.D15 = "";
                    nModel.D16 = "";
                    nModel.D17 = "";
                    nModel.D18 = "";
                    nModel.D19 = "";
                    nModel.D20 = "";

                    nModel.feeamount = 0;
                    nModel.feestatus = "";
                    nModel.NOC_Certificate_Number = "";
                    nModel.NOC_Url = "";
                    nModel.IsNocUrlActiveYesNo = "";
                    nModel.Objection_Rejection_Code = "";
                    nModel.Objection_Rejection_Url = "";
                    nModel.Is_Certificate_Valid_Life_Time = "";
                    nModel.Certificate_EXP_Date_DDMMYYYY = "";

                    string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
                    GetValidateResponse ga = new GetValidateResponse();
                    string GetAaplication = napi.returnServiceStatus(nModel);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);

                    RanSchedule("API returnServiceStatus :" + RR.isSuccess);

                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 8;

                    }
                    else
                    {

                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 8;

                    }
                    try
                    {
                        userDetails = objCMODB.SaveCmoActionAndNiveshStatus(userDetails).FirstOrDefault();

                    }

                    catch (Exception ex)
                    {

                    }

                }

                else if (appStatus == 6)
                {
                    nModel.ControlId = userDetails.Control_ID;
                    nModel.UnitId = userDetails.Unit_Id;
                    nModel.DeptId = userDetails.Dept_ID;
                    nModel.ServiceId = userDetails.ServiceID;
                    nModel.RequestId = userDetails.RequestId;
                    nModel.ApplicationId = Convert.ToString(userDetails.UserID); //retval.regisIdNUH.ToString();  //objSM.UserID.ToString(); // retval.regisIdNUH.ToString(); 
                    nModel.ProcessIndustryId = objSM.Username;
                    nModel.StatusCode = "15";
                    nModel.ApplicationURL = "";
                    nModel.Remarks = "Application approve on the behalf of Inspection Report";
                    nModel.PendecyLevel = "Officer level pendency";
                    nModel.Pending_with_Officer = "Pending at" + " " + objSM.DisignationName + "," + " " + objSM.DistrictName + ' ' + "(CMO)";

                    nModel.D1 = "";
                    nModel.D2 = "";
                    nModel.D3 = "";
                    nModel.D4 = "";
                    nModel.D5 = "";
                    nModel.D6 = "";
                    nModel.D7 = "";
                    nModel.D8 = "";
                    nModel.D9 = "";
                    nModel.D10 = "";
                    nModel.D11 = "";
                    nModel.D12 = "";
                    nModel.D13 = "";
                    nModel.D14 = "";

                    nModel.D15 = "";
                    nModel.D16 = "";
                    nModel.D17 = "";
                    nModel.D18 = "";
                    nModel.D19 = "";
                    nModel.D20 = "";

                    nModel.feeamount = 0;
                    nModel.feestatus = "";
                    nModel.NOC_Certificate_Number = userDetails.NocCertificateNumber;
                    nModel.NOC_Url = "Test/Url";
                    nModel.IsNocUrlActiveYesNo = "NO";
                    nModel.Objection_Rejection_Code = "";
                    nModel.Objection_Rejection_Url = "";
                    nModel.Is_Certificate_Valid_Life_Time = "NO";
                    nModel.Certificate_EXP_Date_DDMMYYYY = userDetails.CertificateExpireDateDDMMYYYY;

                    string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
                    GetValidateResponse ga = new GetValidateResponse();
                    string GetAaplication = napi.returnServiceStatus(nModel);

                    ResponseResult RR = new ResponseResult();
                    RR = JsonConvert.DeserializeObject<ResponseResult>(GetAaplication);

                    RanSchedule("API returnServiceStatus :" + RR.isSuccess);

                    if (RR.isSuccess.ToUpper() == "SUCCESS" || RR.isSuccess.ToUpper() == "TRUE")
                    {
                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 8;

                    }
                    else
                    {

                        userDetails.SendDate = System.DateTime.Now;
                        userDetails.ResStatus = "";
                        userDetails.ServiceStatus = StatusResult;
                        userDetails.StepId = 8;

                    }
                    try
                    {
                        userDetails = objCMODB.SaveCmoActionAndNiveshStatus(userDetails).FirstOrDefault();

                    }

                    catch (Exception ex)
                    {

                    }

                }



            }
        }

        [AuthorizeAdmin(1)]
        public ActionResult BindScheduleOfCommitteeNUH(string inspectiondate = "")
        {
            if (inspectiondate != "" && inspectiondate != null)
            {
                var lstScheduleOfCommittee = objCMODB.GetScheduleOfCommittee(inspectiondate, objSM.UserID).ToList(); //objCMODB.GetScheduleOfCommittee(committeeId).ToList();
                return PartialView("_BindScheduleOfCommittee", lstScheduleOfCommittee);
            }
            else
            {
                return Content("NF");
            }
        }

        #region Vinod File
        public JsonResult UploadFilePhoto(HttpPostedFileBase[] Files)
        {

            NUHAppProcessModel model = new NUHAppProcessModel();
            // model.inspReportFilePhoto = Files;
            string[] terms = new string[Files.Length];
            string Dirpath = "~/Content/writereaddata/NUH/InspectionReport/";
            string path = "";
            var filename = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            int count = 0;
            foreach (HttpPostedFileBase file in Files)
            {

                if (file != null)
                {

                    string ext = Path.GetExtension(file.FileName);

                    if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
                    {
                        filename = "InsepectionPhoto_" + objSM.UserID + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ext;
                        string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                        if (System.IO.File.Exists(completepath))
                        {
                            System.IO.File.Delete(completepath);
                        }

                        long size = file.ContentLength;
                        if (size > 2097152)
                        {
                            path = "SNV";
                        }
                        else
                        {
                            file.SaveAs(completepath);
                            path = Dirpath + filename;
                            terms[count] = path;
                            count = count + 1;
                        }
                    }
                    else
                    {
                        path = "TNV";
                    }
                }
            }

            Session["PhotoPath"] = terms;
            List<string> plist = new List<string> { filename, path };
            return Json(terms);
        }
        #endregion

        public JsonResult UploadFile(HttpPostedFileBase File)
        {
            string Dirpath = "~/Content/writereaddata/NUH/InspectionReport/";
            string path = "";
            string filename = "";

            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }

            string ext = Path.GetExtension(File.FileName);

            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {
                filename = Path.GetFileNameWithoutExtension(File.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";
            }

            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }

        [AuthorizeAdmin(1)]
        public ActionResult ApprovedApplicationNUH()
        {
            return View();
        }

        [AuthorizeAdmin(1)]
        public ActionResult ApprovedApplicationListNUH(string registrationNo = "", string appDate = "")
        {
            List<NUHDetailsModel> lstNUHDetails = new List<NUHDetailsModel>();

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (TempData["fDate"] == null && TempData["tDate"] == null)
            {
                TempData["fDate"] = "";
                TempData["tDate"] = "";
            }
            if (Convert.ToInt32(TempData["District"]) == 0)
            {
                TempData["District"] = 0;
            }
            if (objSM.RollID == 8)
            {
                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString(), Convert.ToInt32(TempData["District"])).Where(m => m.appStatus == 6 && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "")).ToList();
            }
            else
            {

                lstNUHDetails = objCMODB.GetAllNUHListForCMO(0, TempData["fDate"].ToString(), TempData["tDate"].ToString()).Where(m => m.appStatus == 6 && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "") && (lstCMODistrict.Any(p => p.districtId == m.districtid))).ToList();
            }
            return PartialView("_ApprovedApplicationListNUH", lstNUHDetails);
        }

        [AuthorizeAdmin(1)]
        public ActionResult RejectedApplicationNUH()
        {
            return View();
        }

        [AuthorizeAdmin(1)]
        public ActionResult RejectedApplicationListNUH(string registrationNo = "", string appDate = "")
        {
            List<NUHDetailsModel> lstNUHDetails = new List<NUHDetailsModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (TempData["fDate"] == null && TempData["tDate"] == null)
            {
                TempData["fDate"] = "";
                TempData["tDate"] = "";
            }
            if (Convert.ToInt32(TempData["District"]) == 0)
            {
                TempData["District"] = 0;
            }
            if (objSM.RollID == 8)
            {
                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString(), Convert.ToInt32(TempData["District"])).Where(m => (m.appStatus == 0 || m.appStatus == 4) && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "")).ToList();
            }
            else
            {

                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString()).Where(m => (m.appStatus == 0 || m.appStatus == 4) && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "") && (lstCMODistrict.Any(p => p.districtId == m.districtid))).ToList();
            }

            return PartialView("_RejectedApplicationListNUH", lstNUHDetails);
        }

        void SendSMSUpdateProcessNUH(string registrationNo, string inspectionDate, string mobileNo, int appstatus)
        {


            ForgotPasswordModel otpChCount = new ForgotPasswordModel();
            string txtmsg = "";
            if (appstatus == 0 || appstatus == 4)
            {
               // txtmsg = "Dear Citizen,\n\nYour Application Form No. " + registrationNo + " for Registration of Medical Establishment has been rejected. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details." + objSM.DisplayName + "\nMHFWD, UP";
                txtmsg = "Dear Citizen,Your Application Form No. " + registrationNo + " for Registration of Medical Establishment has been rejected. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details. " + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007872372964565791";
            }
            else if (appstatus == 2)
            {
                //txtmsg = "Dear Citizen,\n\n As per your application for Registration of Medical Establishment, a committee for Inspection has been scheduled. We request to kindly be present at your medical establishment on the inspection date " + inspectionDate + " and  coordinate accordingly.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
                txtmsg = "Dear Citizen, As per your application for Registration of Medical Establishment, a committee for Inspection has been scheduled. We request to kindly be present at your medical establishment on the inspection date " + inspectionDate + "  and  coordinate accordingly." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007983650803613497";
            
            }
            else if (appstatus == 6)
            {
                //txtmsg = "Dear Citizen,\n\n Your Application Form No. " + registrationNo + " for Registration of Medical Establishment has been approved. Please get in touch with the office of Chief Medical Officer to collect your certificate. You can also download the Certificate from your dashboard.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
                txtmsg = "Dear Citizen, Your Application Form No. " + registrationNo + " for Registration of Medical Establishment has been approved. Please get in touch with the office of Chief Medical Officer to collect your certificate. You can also download the Certificate from your dashboard." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007660854294308567";
            
            }

            if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(txtmsg))
            {
                string status = SMS.SMSSend(txtmsg, Convert.ToString(mobileNo));
                objAccDb.SMSLog(txtmsg, mobileNo, status, registrationNo);
            }
        }

        [AuthorizeAdmin]
        public ActionResult PrintApplicationFormNUH(string regisIdNUH)
        {
            regisIdNUH = OTPL_Imp.CustomCryptography.Decrypt(regisIdNUH);
            Session["regisIdNUH"] = regisIdNUH;
            NUH_DB objNUH_DB = new NUH_DB();
            NUHmodel model = new NUHmodel();
            model = objNUH_DB.GetNUHListBYRegistrationNo(Convert.ToInt64(regisIdNUH));

            if (objSM.RollID == 2 || objSM.RollID == 20)
            {
                if (model == null || objSM.districtId != model.districtid)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID == 18)
            {
                var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
                int count = lstCMODistrict.Where(e => e.districtId == model.districtid).ToList().Count();
                if (count == 0)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID != 8)
            {
                return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
            }

            ViewBag.VBoutpateint = objNUHDB.GetoutPatient(model.regisIdNUH);
            ViewBag.VBolaboratory = objNUHDB.GetNUHlaboratory(model.regisIdNUH);
            ViewBag.VBimaging = objNUHDB.GetNUHimaging(model.regisIdNUH);

            model.NUHPartnerList = objNUHDB.getNUHPartner(Convert.ToInt64(regisIdNUH));
            model.NUHDOCList = objNUHDB.getNUHdoc(Convert.ToInt64(regisIdNUH));
            model.NUHModelList = objNUHDB.getNUHChild(Convert.ToInt64(regisIdNUH));

            return View(model);
        }

        [AuthorizeAdmin(1)]
        public JsonResult UploadCertificateFileNUH(HttpPostedFileBase File)
        {
            long regisId = Convert.ToInt64(Session["registrationIdNUH"].ToString());

            var lstNUHDetails = objCMODB.GetNUHCertificate(regisId);

            string Dirpath = "~/Content/SignedCertificate/NUH/" + objSM.DistrictName + "/";
            string path = "";
            string filename = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            string ext = Path.GetExtension(File.FileName);
            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {

                filename = "Signed_" + lstNUHDetails.CertificateNo + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";//"warning_Maximum 2MB file size are allowed";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";//"warning_Invalid File Format only pdf and jpg files are allow!";
            }

            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }

        [AuthorizeAdmin(1)]
        [HttpGet]
        public ActionResult UploadCertificateNUH(string regisId)
        {
            long regId = Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId));

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            var resultData = objCMODB.GetAllNUHList(regId).Where(m => (lstCMODistrict.Any(p => p.districtId == m.districtid))).Count();

            if (resultData == 0)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            Session["registrationIdNUH"] = regId;

            return View();
        }

        [AuthorizeAdmin(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadCertificateNUH(FormCollection frm)
        {

            var CertificatePath = frm.GetValues("hdnfileUploadCertificate");
            string IPAddress = Common.GetIPAddress();
            if (CertificatePath[0] != null || CertificatePath[0] != "")
            {
                var res = objCMODB.UpdateNUHCertificate(Convert.ToInt64(Session["registrationIdNUH"]), CertificatePath[0], objSM.UserID, IPAddress);
                if (res.Flag == 1)
                {

                    var result = objCMODB.GetNiveshUserDetailByID(res.RegisterByuserID);
                    if (result != null)
                    {
                        UPHEALTHNIC.upswp_niveshmitraservices ObjSendAppSubmitStatus = new UPHEALTHNIC.upswp_niveshmitraservices();
                        //NiveshMitraSendStatusModel objStatusModel = objCMODB.GetNiveshMitraUserDetailsByID(result.RegisterByuserID).FirstOrDefault();
                        NiveshMitraSendStatusModel objStatusModel = objCMODB.GetNiveshMitraUserDetailsByID(Convert.ToInt64(Session["registrationIdNUH"])).FirstOrDefault();
                        if (objStatusModel != null)
                        {

                            string StatusResult = string.Empty;
                            string StatusResultBinaryFormat = string.Empty;
                            string UrlFile = ConfigurationManager.AppSettings["DownloadShinedCertificateUrl"].ToString() + Url.Action("DownloadFileByIDAndPath", "Account", new { FilePath = OTPL_Imp.CustomCryptography.Encrypt(objStatusModel.NocUrl) });

                            objStatusModel.ProcessIndustryID = objStatusModel.UserName;
                            objStatusModel.ApplicationID = Session["registrationIdNUH"].ToString();    //objStatusModel.UserID.ToString();

                            objStatusModel.StatusCode = "15";
                            objStatusModel.Remarks = "Certificate Generated";
                            //  objStatusModel.PendencyLevel = "Entrepreneur level pendency";
                            objStatusModel.PendencyLevel = "";

                            objStatusModel.FeeAmount = "";
                            objStatusModel.FeeStatus = "";
                            objStatusModel.TransectionID = "";
                            objStatusModel.TranSactionDate = "";
                            objStatusModel.TransectionDateAndTime = "";
                            //   objStatusModel.NocCertificateNumber = "";
                            objStatusModel.NocUrl = UrlFile;
                            objStatusModel.IsNocUrlActiveYesNo = "Yes";
                            objStatusModel.Passalt = ConfigurationManager.AppSettings["PassKey"].ToString();
                            objStatusModel.ObjectRejectionCode = "";
                            objStatusModel.IsCertificateValidLifeTime = "No";
                            // objStatusModel.CertificateExpireDateDDMMYYYY = objStatusModel.CertificateExpireDateDDMMYYYY;
                            objStatusModel.D1 = "";
                            objStatusModel.D2 = "";
                            objStatusModel.D3 = "";
                            objStatusModel.D4 = "";
                            objStatusModel.D5 = "";
                            objStatusModel.D6 = "";
                            objStatusModel.D7 = "";


                            StatusResult = ObjSendAppSubmitStatus.WReturn_CUSID_STATUS(objStatusModel.Control_ID, objStatusModel.Unit_Id, objStatusModel.ServiceID, objStatusModel.ProcessIndustryID, objStatusModel.ApplicationID, objStatusModel.StatusCode,
                                   objStatusModel.Remarks, objStatusModel.PendencyLevel, objStatusModel.FeeAmount, objStatusModel.FeeStatus, objStatusModel.TransectionID, objStatusModel.TranSactionDate, objStatusModel.TransectionDateAndTime, objStatusModel.NocCertificateNumber, objStatusModel.NocUrl, objStatusModel.IsNocUrlActiveYesNo, objStatusModel.Passalt, objStatusModel.RequestId, objStatusModel.ObjectRejectionCode
                                    , objStatusModel.IsCertificateValidLifeTime, objStatusModel.CertificateExpireDateDDMMYYYY, objStatusModel.D1, objStatusModel.D2, objStatusModel.D3, objStatusModel.D4, objStatusModel.D5, objStatusModel.D6, objStatusModel.D7);

                            if (StatusResult.ToUpper() == "SUCCESS")
                            {
                                objStatusModel.SendDate = System.DateTime.Now;
                                objStatusModel.ResStatus = "";
                                objStatusModel.ServiceStatus = StatusResult;
                                objStatusModel.StepId = 10;

                                string Attachment = CertificatePath[0];
                                if (!string.IsNullOrEmpty(Attachment))
                                {
                                    string base64String = "";
                                    var url = Server.MapPath(Convert.ToString(Attachment));
                                    using (WebClient wc = new WebClient())
                                    {
                                        var name = System.IO.Path.GetFileName(url);
                                        var bytes = wc.DownloadData(url);
                                        base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                                    }

                                    String ext = Path.GetExtension(Attachment);

                                    StatusResultBinaryFormat = ObjSendAppSubmitStatus.WReturn_CUSID_Entrepreneur_NOC_IN_BINARYFORMAT(objStatusModel.Control_ID, objStatusModel.Unit_Id, objStatusModel.ServiceID,
                                        objStatusModel.ProcessIndustryID, objStatusModel.NocCertificateNumber, base64String, objCom.GetMimeType(ext), objStatusModel.Passalt, objStatusModel.RequestId);

                                }

                            }
                            else
                            {

                                objStatusModel.SendDate = System.DateTime.Now;
                                objStatusModel.ResStatus = "";
                                objStatusModel.ServiceStatus = StatusResult;
                                objStatusModel.StepId = 10;

                            }

                            try
                            {
                                objStatusModel = objCMODB.SaveCmoActionAndNiveshStatus(objStatusModel).FirstOrDefault();

                            }
                            catch (Exception ex)
                            {

                            }

                        }
                    }
                    TempData["Message"] = res.Msg;
                }

                return RedirectToAction("ApprovedApplicationNUH", "CMO");
            }
            else
            {
                TempData["msg"] = "Please choose a file!";
                TempData["msgstatus"] = "warning";
                return RedirectToAction("UploadCertificateNUH");
            }
        }



        public ActionResult DownloadFileByIDAndPath(string filePath)
        {
            string decrypFilePath = OTPL_Imp.CustomCryptography.Decrypt(Server.UrlDecode(filePath));

            if (System.IO.File.Exists(Server.MapPath(decrypFilePath)))
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(decrypFilePath));
                Response.ClearContent();
                Response.ClearHeaders();
                Response.ContentType = "application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(decrypFilePath));
                Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                Response.WriteFile(decrypFilePath);
                Response.Flush();
                Response.Close();
            }

            return View("DownloadFile");
        }


        [AuthorizeAdmin(1)]
        public ActionResult NUHgeneratedCertificate(long regisIdNUH)
        {
            string setPdfName = "", setDigitalPdfName = "";
            string returnMsg = "T";
            bool isRedirect = false;
            var transIp = Common.GetIPAddress();

            var res = objCMODB.GetDetail(regisIdNUH);

            if (res != null && objSM.districtId != res[0].districtid)
            {
                returnMsg = "warning_Unauthorise Access";
                isRedirect = true;
                //return RedirectToAction("UnauthoriseAcess", "Home");
            }

            if (res != null && res.Count > 0 && returnMsg == "T")
            {
                var res2 = objCMODB.getNUHChildRpt(res[0].regisIdNUH);
                var resdoc = objCMODB.getNUHChildDOCRpt(res[0].regisIdNUH);
                var resowner = objCMODB.getNUHChildOwnerRpt(res[0].regisIdNUH);

                int staffCount = res2.Where(r => r.regisIdNUH > 0).Count();

                try
                {
                    ReportDocument rd = new ReportDocument();

                    rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rpt_NUHcertificate.rpt"));
                    rd.SetDataSource(res);

                    ReportDocument subShows = rd.Subreports["rpt_NUHcertificateChild.rpt"];
                    subShows.SetDataSource(res2);

                    ReportDocument subShowsDoc = rd.Subreports["rpt_NUHcertDOC.rpt"];
                    subShowsDoc.SetDataSource(resdoc);

                    ReportDocument subShowsOwner = rd.Subreports["rpt_NUHcertOwner.rpt"];
                    subShowsOwner.SetDataSource(resowner);

                    rd.SetParameterValue("districtName", objSM.DistrictName);

                    rd.SetParameterValue("staffCount", staffCount);

                    string NUHCertiValidityStatusURL = ConfigurationManager.AppSettings["NUHCertiValidityStatusURL"].ToString();
                    rd.SetParameterValue("NUHCertiValidityStatusURL", NUHCertiValidityStatusURL);

                    setPdfName = "UnSigned_" + res[0].certificateNo;

                    string folderpath = "~/Content/writereaddata/UnSignedCertificate/NUH/" + objSM.DistrictName + "/";

                    if (!System.IO.Directory.Exists(Server.MapPath(folderpath)))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(folderpath));
                    }

                    string flName = folderpath + setPdfName + ".pdf";

                    rd.ExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                    rd.ExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                    rd.ExportOptions.DestinationOptions = new DiskFileDestinationOptions();
                    ((DiskFileDestinationOptions)rd.ExportOptions.DestinationOptions).DiskFileName = Server.MapPath(flName);
                    rd.Export();
                    if (System.IO.File.Exists(Server.MapPath(flName)))
                    {
                        int result = objCMODB.InsertUnSignedCertiPath_NUH(res[0].regisIdNUH, flName);
                        if (result > 0)
                        {
                            var resultData = objCMODB.UpdateNUHCertificate(res[0].regisIdNUH, flName, objSM.UserID, transIp);
                            if (resultData.Flag == 1)
                            {
                                var resNM = objCMODB.SendStausToNiveshwithBinaryFormat(res[0].regisIdNUH, resultData.RegisterByuserID, flName);

                                returnMsg = resNM;
                            }
                        }
                    }
                    rd.Close();
                    rd.Dispose();


                    if (ConfigurationManager.AppSettings["IsDigitalSign"].ToString() != "Y")
                    {
                        FileInfo fileInfo = new FileInfo(Server.MapPath(flName));
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + setPdfName + ".pdf");
                        Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                        Response.WriteFile(flName);
                        Response.Flush();
                        Response.Close();
                    }
                    else
                    {
                        //digital sign

                        setDigitalPdfName = "MedicalEstablishmentCertificateDigitalSigned" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                        string digitalFlName = folderpath + setDigitalPdfName + ".pdf";

                        var sigDetails = comndb.GetDigitalSignatureDetails(objSM.UserID);

                        float llx = 580;
                        float lly = 300;
                        float urx = 440;
                        float ury = 200;
                        Comman_Classes.DigitalCeritificateManager dcm = new Comman_Classes.DigitalCeritificateManager();
                        Comman_Classes.MetaData md = new Comman_Classes.MetaData()
                        {
                            Author = sigDetails.Author,
                            Title = "Medical Establishment Certificate Authentication",
                            Subject = "Medical Establishment Certificate",
                            Creator = sigDetails.Creator,
                            Producer = sigDetails.Producer,
                            Keywords = sigDetails.Keywords
                        };

                        string Signaturepath = Server.MapPath(sigDetails.Signaturepath);
                        dcm.signPDF(Server.MapPath(flName), Server.MapPath(digitalFlName), Signaturepath,
                         sigDetails.signpwd, "Authenticate Medical Establishment Certificate", sigDetails.SigContact,
                         sigDetails.SigLocation, true, llx, lly, urx, ury, 1, md);

                        FileInfo fileInfo = new FileInfo(Server.MapPath(digitalFlName));
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + setDigitalPdfName + ".pdf");
                        Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                        Response.WriteFile(Server.MapPath(digitalFlName));
                        Response.Flush();
                        Response.Close();

                        if (System.IO.File.Exists(Server.MapPath(flName)))
                        {
                            System.IO.File.Delete(Server.MapPath(flName));
                        }

                        if (System.IO.File.Exists(Server.MapPath(digitalFlName)))
                        {
                            System.IO.File.Delete(Server.MapPath(digitalFlName));
                        }

                        //digital sign end 
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error Occour to Downloading. " + ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "Detail not found.";
            }

            if (returnMsg.ToLower() != "sucess")
            {
                var resultData = objCMODB.RevertCertificateGeneration(res[0].regisIdNUH, transIp, objSM.UserID);
            }

            if (isRedirect)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }
            return View("DownloadFile");
        }

        #region Generate NUH Certificate By Ajax
        public ActionResult NUHgeneratedCertificateByAjax(long regisIdNUH)
        {
            string setPdfName = "", setDigitalPdfName = "";
            string returnMsg = "T";
            var transIp = Common.GetIPAddress();

            var res = objCMODB.GetDetail(regisIdNUH);

            if (res != null && objSM.districtId != res[0].districtid)
            {
                returnMsg = "warning_Unauthorise Access";
            }

            if (res != null && res.Count > 0 && returnMsg == "T")
            {
                var res2 = objCMODB.getNUHChildRpt(res[0].regisIdNUH);
                var resdoc = objCMODB.getNUHChildDOCRpt(res[0].regisIdNUH);
                var resowner = objCMODB.getNUHChildOwnerRpt(res[0].regisIdNUH);

                int staffCount = res2.Where(r => r.regisIdNUH > 0).Count();

                try
                {
                    ReportDocument rd = new ReportDocument();

                    rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rpt_NUHcertificate.rpt"));
                    rd.SetDataSource(res);

                    ReportDocument subShows = rd.Subreports["rpt_NUHcertificateChild.rpt"];
                    subShows.SetDataSource(res2);

                    ReportDocument subShowsDoc = rd.Subreports["rpt_NUHcertDOC.rpt"];
                    subShowsDoc.SetDataSource(resdoc);

                    ReportDocument subShowsOwner = rd.Subreports["rpt_NUHcertOwner.rpt"];
                    subShowsOwner.SetDataSource(resowner);

                    rd.SetParameterValue("districtName", objSM.DistrictName);

                    rd.SetParameterValue("staffCount", staffCount);

                    string NUHCertiValidityStatusURL = ConfigurationManager.AppSettings["NUHCertiValidityStatusURL"].ToString();
                    rd.SetParameterValue("NUHCertiValidityStatusURL", NUHCertiValidityStatusURL);

                    setPdfName = "UnSigned_" + res[0].certificateNo;

                    string folderpath = "~/Content/writereaddata/UnSignedCertificate/NUH/" + objSM.DistrictName + "/";

                    if (!System.IO.Directory.Exists(Server.MapPath(folderpath)))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(folderpath));
                    }

                    string flName = folderpath + setPdfName + ".pdf";

                    rd.ExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                    rd.ExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                    rd.ExportOptions.DestinationOptions = new DiskFileDestinationOptions();
                    ((DiskFileDestinationOptions)rd.ExportOptions.DestinationOptions).DiskFileName = Server.MapPath(flName);
                    rd.Export();

                    if (System.IO.File.Exists(Server.MapPath(flName)))
                    {
                        int result = objCMODB.InsertUnSignedCertiPath_NUH(res[0].regisIdNUH, flName);
                        if (result > 0)
                        {
                            var resultData = objCMODB.UpdateNUHCertificate(res[0].regisIdNUH, flName, objSM.UserID, transIp);
                            if (resultData.Flag == 1)
                            {
                                var resNM = objCMODB.SendStausToNiveshwithBinaryFormat(res[0].regisIdNUH, resultData.RegisterByuserID, flName);

                                returnMsg = resNM;
                            }
                        }
                    }
                    rd.Close();
                    rd.Dispose();
                }
                catch (Exception ex)
                {
                    //ViewBag.Message = "Error Occour to Downloading. " + ex.Message;
                    returnMsg = "warning_Error Occour to Downloading. " + ex.Message;
                }
            }
            else
            {
                //ViewBag.Message = "Detail not found.";
                returnMsg = returnMsg != "T" ? returnMsg : "warning_Detail not found.";
            }

            if (returnMsg.ToLower() == "sucess")
            {
                returnMsg = "success_Certificate Generated Successfully";
            }
            else
            {
                var resultData = objCMODB.RevertCertificateGeneration(res[0].regisIdNUH, transIp, objSM.UserID);
                returnMsg = returnMsg != "T" ? returnMsg : "warning_Problem in Certificate Generating";
            }

            return Content(returnMsg);
        }
        #endregion

        [AuthorizeAdmin]
        public ActionResult ViewQueryReplyNUH(string regisIdNUH)
        {
            regisIdNUH = OTPL_Imp.CustomCryptography.Decrypt(regisIdNUH);
            Session["regisIdNUH"] = regisIdNUH;
            NUH_DB objNUH_DB = new NUH_DB();
            NUHmodel model = new NUHmodel();
            model = objNUH_DB.GetNUHListForQueryReply(Convert.ToInt64(regisIdNUH));

            //   if (objSM.RollID == 2 || objSM.RollID == 20)
            //   {
            //       if (model == null || objSM.districtId != model.districtid)
            //       {
            //           return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
            //       }
            //   }
            //   else if (objSM.RollID == 18)
            //   {
            //       var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            //       int count = lstCMODistrict.Where(e => e.districtId == model.districtid).ToList().Count();
            //       if (count == 0)
            //       {
            //           return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
            //       }
            //   }
            //   else if (objSM.RollID != 8)
            //   {
            //       return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
            //   }
            //


            return View(model);
        }
        #endregion

        #region Family Planing abhijeet

        [AuthorizeAdmin(7)]
        public ActionResult AppliedApplicationFAP()
        {
            ProcessType model = new ProcessType();

            model = objCMODB.getFAPprocessCount(1, objSM.UserID);

            return View(model);
            //ProcessType model = new ProcessType();

            //var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            //var resultData = objCMODB.GetAllRegistrationFAP().Where(m => lstCMODistrict.Any(p => p.districtId == m.healthunitDistrictId));
            //model.totalAppReceived = resultData.Where(m => m.appStatus == -1 || m.appStatus == 1 || m.appStatus == 2 || m.appStatus == 3).Count();
            //model.totalAppInProcess = resultData.Where(m => m.appStatus == 5 || m.appStatus == 6 || m.appStatus == 7).Count();
            //model.totalAppApproved = resultData.Where(m => m.appStatus == 10).Count();
            //model.totalAppRejected = resultData.Where(m => m.appStatus == 0 || m.appStatus == 4 || m.appStatus == 8).Count(); 

            //return View(model);
        }

        [AuthorizeAdmin(7)]
        public ActionResult ReceivedApplicationFAP()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("FAP").Where(m => m.Value == "-1" || m.Value == "1" || m.Value == "2" || m.Value == "3").ToList();

            return View();
        }

        [AuthorizeAdmin(7)]
        public ActionResult ReceivedApplicationListFAP(string registrationNo = "", string mobile = "", string requestDate = "", string status = "")
        {
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            List<FAPModel> lstFAPDetails = new List<FAPModel>();
            int intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;
            if (objSM.RollID == 8)
            {
                lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == -1 || m.appStatus == 1 || m.appStatus == 2 || m.appStatus == 3) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "")).ToList();

            }
            else
            {

                lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == -1 || m.appStatus == 1 || m.appStatus == 2 || m.appStatus == 3) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0) && (lstCMODistrict.Any(p => p.districtId == m.healthunitDistrictId))).ToList();
            }
            return PartialView("_TotalReceivedListFAP", lstFAPDetails);
        }

        [AuthorizeAdmin(7)]
        public ActionResult InProcessApplicationFAP()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("FAP").Where(m => m.Value == "5" || m.Value == "6" || m.Value == "7").ToList();
            return View();
        }

        [AuthorizeAdmin(7)]
        public ActionResult InProcessApplicationListFAP(string registrationNo = "", string mobile = "", string requestDate = "", string status = "")
        {

            List<FAPModel> lstFAPDetails = new List<FAPModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            int intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;

            if (objSM.RollID == 8)
            {
                lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 5 || m.appStatus == 6 || m.appStatus == 7) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0)).ToList();

            }
            else
            {

                lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 5 || m.appStatus == 6 || m.appStatus == 7) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0) && (lstCMODistrict.Any(p => p.districtId == m.healthunitDistrictId))).ToList();

            }
            //Before
            //  var lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 5 || m.appStatus == 6 || m.appStatus == 7) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0) && (lstCMODistrict.Any(p => p.districtId == m.healthunitDistrictId))).ToList();

            return PartialView("_InProcessApplicationListFAP", lstFAPDetails);
        }

        public ActionResult FAPDetailsById(long regisFAPId)
        {
            FAPModel model = new FAPModel();
            Session["regisIdFAP"] = regisFAPId;
            model = objCMODB.GetAllFAPList(2, regisFAPId, "", "", "", 0).Where(m => m.regisIdFAP == regisFAPId).FirstOrDefault();
            return PartialView("_FAPDetail", model);
        }

        public ActionResult BindChildListFAP()
        {
            FAPModel model = new FAPModel();

            model.FAPList = objCMODB.getFAPChild(Convert.ToInt64(Session["regisIdFAP"].ToString()));
            return PartialView("_ViewFAPChild", model.FAPList);
        }

        public ActionResult FAPAppDetails(long regisFAPId)
        {
            FAPModel model = new FAPModel();

            model = objCMODB.GetAllFAPList(2, regisFAPId, "", "", "", 0).Where(m => m.regisIdFAP == regisFAPId).FirstOrDefault();

            return PartialView("_FAPAppDetails", model);
        }

        public ActionResult FAPScheduledList(string inspectionDate)
        {
            FAPModel model = new FAPModel();
            model.FAPList = objCMODB.GetFAPScheduledCommitteeList(inspectionDate, objSM.UserID);
            return PartialView("_FAPScheduledlist", model.FAPList);
        }

        [AuthorizeAdmin(7)]
        [HttpGet]
        public ActionResult UpdateAppProcessFAP(string regisId)
        {
            bool validRequest = false;
            FAPAppProcessModel model = new FAPAppProcessModel();

            if (regisId == null || regisId == "")
            {
                regisId = OTPL_Imp.CustomCryptography.Decrypt(Session["regisId"].ToString());
                model = objCMODB.getFAPStatus(Convert.ToInt64(regisId));
            }
            else
            {
                Session["regisId"] = regisId;
                regisId = OTPL_Imp.CustomCryptography.Decrypt(regisId);
                model = objCMODB.getFAPStatus(Convert.ToInt64(regisId));
            }

            if (model != null && objSM.districtId != model.districtId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            ViewBag.forwardTypes = objCMODB.rblforwardType().Where(m => m.forwardtypeId == 3 || m.forwardtypeId == 5).ToList();
            ViewBag.DLLCommittee = comndb.GetDropDownList(30, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.designation = comndb.GetDropDownList(33, Convert.ToInt32(objSM.UserID)).ToList();//objCMODB.binddesignation().ToList();
            if (ViewBag.designation != null && ViewBag.designation.Count > 0)
            {
                validRequest = true;
            }
            ViewBag.forwardTo = Enumerable.Empty<SelectListItem>();
            ViewBag.validRequest = validRequest;
            return View(model);
        }


        [AuthorizeAdmin(7)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAppProcessFAP(FAPAppProcessModel model, FormCollection form)
        {

            if (Session["PhotoPath"] != null)
            {
                string[] fileArray = Session["PhotoPath"] as string[];

                if (fileArray != null)
                {
                    model.inspReportFilePhotoPath = fileArray;
                }
            }

            ModelState["inspReportFilePhoto"].Errors.Clear();
            var result = objCMODB.getFAPStatus(model.regisIdFAP);
            if (result != null && (objSM.districtId != result.districtId || model.appStatus != result.appStatus))
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }


            #region bulkinsertion

            string XmlData = "";
            if (model.appStatus == 5)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["sancationDate"].Errors.Clear();
                ModelState["stateReportFile"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();

                ModelState["inspReportFilePhoto"].Errors.Clear();

                #region Vinod Bulkinsertion of Inspection Photo

                string XmlDataPIC = "";
                int countPic = model.inspReportFilePhotoPath.Count();
                XmlDataPIC = "<InspectionPICS>";
                //  long regisByuser = objSM.UserID;
                for (int i = 0; i < countPic; i++)
                {
                    if (model.inspReportFilePhotoPath[i] == "")
                    {
                        //XmlData = string.Empty;
                    }
                    else
                    {

                        XmlDataPIC += "<InspectionPIC><UploadPICFilePath>" + model.inspReportFilePhotoPath[i] + "</UploadPICFilePath></InspectionPIC>";
                    }

                }
                XmlDataPIC += "</InspectionPICS>";
                model.XmlDataPhoto = XmlDataPIC;

                #endregion

                var committeeMembderId = form.GetValues("chkdesig");

                int count = committeeMembderId.Count();
                XmlData = "<Members>";
                long regisByuser = objSM.UserID;
                for (int i = 0; i < count; i++)
                {
                    if (committeeMembderId[i] == "")
                    {
                        //XmlData = string.Empty;
                    }
                    else
                    {

                        XmlData += "<Member><committeeMembderId>" + committeeMembderId[i] + "</committeeMembderId></Member>";
                    }

                }
                XmlData += "</Members>";
            }
            #endregion

            string IpAddress = Common.GetIPAddress();
            model.userId = objSM.UserID;
            model.transIp = IpAddress;

            if (model.rejectedRemarks == null)
            {
                model.status = true;
            }
            else
            {
                model.status = false;
            }
            if (model.appStatus == -1 || model.appStatus == 1)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["sancationDate"].Errors.Clear();
                ModelState["stateReportFile"].Errors.Clear();
                ModelState["districtReportFile"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();
            }
            if (model.appStatus == 3)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["sancationDate"].Errors.Clear();
                ModelState["stateReportFile"].Errors.Clear();
                ModelState["districtReportFile"].Errors.Clear();
            }
            if (model.appStatus == 6)
            {

                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["sancationDate"].Errors.Clear();
                ModelState["districtReportFile"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();

                ModelState["inspReportFilePhoto"].Errors.Clear();

                #region Vinod Bulkinsertion of Inspection Photo

                string XmlDataPIC = "";
                int countPic = model.inspReportFilePhotoPath.Count();
                XmlDataPIC = "<InspectionPICS>";
                //  long regisByuser = objSM.UserID;
                for (int i = 0; i < countPic; i++)
                {
                    if (model.inspReportFilePhotoPath[i] == "")
                    {
                        //XmlData = string.Empty;
                    }
                    else
                    {

                        XmlDataPIC += "<InspectionPIC><UploadPICFilePath>" + model.inspReportFilePhotoPath[i] + "</UploadPICFilePath></InspectionPIC>";
                    }

                }
                XmlDataPIC += "</InspectionPICS>";
                model.XmlDataPhoto = XmlDataPIC;

                #endregion


            }
            if (model.appStatus == 7 && model.rejectedRemarks == null)
            {
                ModelState["inspectionDate"].Errors.Clear();
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["stateReportFile"].Errors.Clear();
                ModelState["districtReportFile"].Errors.Clear();

            }
            if (model.appStatus == 7 && model.rejectedRemarks != null)
            {
                ModelState["sanctionAmount"].Errors.Clear();
                ModelState["sancationDate"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();
                ModelState["stateReportFile"].Errors.Clear();
                ModelState["districtReportFile"].Errors.Clear();

            }

            //forwardtoId

            if (ModelState.IsValid)
            {
                var CheckList = objCMODB.bindDropdownlistForAuditFilters(objSM.districtId).Where(e => e.forwardtoId == model.forwardtoId).ToList(); ;
                if (model.forwardtoId > 0)
                {
                    var data = CheckList.ToList();
                    if (data == null || data.Count == 0)
                    {
                        TempData["Msg"] = "Please Select a unit belong to Current CMO.";
                        TempData["MsgStatus"] = "error";
                        return RedirectToAction("UpdateAppProcessFAP");
                    }
                }

                AuditMethods objAud = new AuditMethods();
                string errormsg = "";
                bool valStatus = false;

                if (!string.IsNullOrEmpty(model.districtReportFilePath))
                {
                    valStatus = objAud.IsValidLink(model.districtReportFilePath, "Document File", out errormsg);
                    if (!valStatus)
                    {
                        setSweetAlertMsg(errormsg, "warning");
                        return View(model);
                    }
                }

                if (!string.IsNullOrEmpty(model.stateReportFilePath))
                {

                    valStatus = objAud.IsValidLink(model.stateReportFilePath, "Document File", out errormsg);
                    if (!valStatus)
                    {
                        setSweetAlertMsg(errormsg, "warning");
                        return View(model);
                    }
                }

                var res = objCMODB.updateFAPProcess(model, XmlData);
                if (res.Flag == 1)
                {


                    SendSMSUpdateProcessFAP(res.RegistrationNo, res.inspectionDate, res.MobileNo, res.appStatus);

                    TempData["Msg"] = res.Msg;
                    TempData["MsgStatus"] = "success";
                    if ((model.appStatus == -1 || model.appStatus == 0) && model.rejectedRemarks == null)
                    {
                        return RedirectToAction("UpdateAppProcessFAP");
                        //return Content(res.Msg + "_" + model.appStatus);
                    }
                    else if ((model.appStatus == -1 || model.appStatus == 0) && model.rejectedRemarks != null)
                    {

                        return RedirectToAction("RejectedApplicationFAP");
                    }

                    else if (model.appStatus == 2 || model.appStatus == 5 || model.appStatus == 6)
                    {

                        return RedirectToAction("UpdateAppProcessFAP");
                    }
                    else if (model.appStatus == 3 && model.rejectedRemarks == null)
                    {

                        return RedirectToAction("InProcessApplicationFAP");
                    }
                    else if (model.appStatus == 3 && model.rejectedRemarks != null)
                    {

                        return RedirectToAction("RejectedApplicationFAP");
                    }
                    else if (model.appStatus == 5 || model.appStatus == 6)
                    {

                        return RedirectToAction("UpdateAppProcessFAP");
                    }
                    else if (model.appStatus == 7 && model.rejectedRemarks == null)
                    {

                        return RedirectToAction("ApprovedApplicationFAP");
                    }
                    else if (model.appStatus == 7 && model.rejectedRemarks != null)
                    {

                        return RedirectToAction("RejectedApplicationFAP");
                    }
                    else
                    {
                        return RedirectToAction("UpdateAppProcessFAP");
                    }

                }
                else if (res.Flag == 0)
                {
                    TempData["Msg"] = res.Msg;
                    TempData["MsgStatus"] = "error";
                    return RedirectToAction("UpdateAppProcessFAP");
                }

                else
                {
                    TempData["Msg"] = "Some Error Occur!";
                    TempData["MsgStatus"] = "error";
                    return RedirectToAction("UpdateAppProcessFAP");
                }
            }
            else
            {
                TempData["Msg"] = "Some Error Occur!";
                TempData["MsgStatus"] = "error";
                return RedirectToAction("UpdateAppProcessFAP");
            }
        }

        [AuthorizeAdmin(7)]
        public JsonResult UploadFileFAP(HttpPostedFileBase File)
        {
            string Dirpath = "~/Content/writereaddata/FAP/";
            string path = "";
            string filename = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            string ext = Path.GetExtension(File.FileName);
            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {

                filename = Path.GetFileNameWithoutExtension(File.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";//"warning_Maximum 2MB file size are allowed";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";//"warning_Invalid File Format only pdf and jpg files are allow!";
            }
            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }

        [AuthorizeAdmin(7)]
        public ActionResult ApprovedApplicationFAP()
        {

            return View();
        }

        [AuthorizeAdmin(7)]
        public ActionResult ApprovedApplicationListFAP(string registrationNo = "", string mobile = "", string requestDate = "")
        {
            List<FAPModel> lstFAPDetails = new List<FAPModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (objSM.RollID == 8)
            {
                lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 10) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "")).ToList();
            }
            else
            {
                lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 10) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.healthunitDistrictId))).ToList();
            }
            //before
            // var lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 10) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.healthunitDistrictId))).ToList();

            return PartialView("_ApprovedApplicationListFAP", lstFAPDetails);
        }

        [AuthorizeAdmin(7)]
        public ActionResult RejectedApplicationFAP()
        {

            return View();
        }

        [AuthorizeAdmin(7)]
        public ActionResult RejectedApplicationListFAP(string registrationNo = "", string mobile = "", string requestDate = "")
        {
            List<FAPModel> lstFAPDetails = new List<FAPModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (objSM.RollID == 8)
            {
                lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 0 || m.appStatus == 4 || m.appStatus == 8) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "")).ToList();
            }
            else
            {

                lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 0 || m.appStatus == 4 || m.appStatus == 8) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.healthunitDistrictId))).ToList();
            }
            //  var lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.appStatus == 0 || m.appStatus == 4 || m.appStatus == 8) && (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.healthunitDistrictId))).ToList();

            return PartialView("_RejectedApplicationListFAP", lstFAPDetails);
        }

        void SendSMSUpdateProcessFAP(string registrationNo, string inspectionDate, string mobileNo, int appstatus)
        {

            if (mobileNo != "")
            {
                ForgotPasswordModel otpChCount = new ForgotPasswordModel();
                string txtmsg = "";
                if (appstatus == 0 || appstatus == 4 || appstatus == 8)
                {
                   // txtmsg = "Dear Citizen,\n\n Your Application Form Number " + registrationNo + " for Payment of Unsuccessful Family Planning has been rejected. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
                    txtmsg = "Dear Citizen,Your Application Form Number " + registrationNo + " for Payment of Unsuccessful Family Planning has been rejected. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007239257657407319";
                
                }
                else if (appstatus == 5)
                {
                    //txtmsg = "Dear Citizen,\n\n As per your application for Request of Payment of Unsuccessful Family Planning, a committee for Inspection has been scheduled. We request to kindly be present at Office of Chief Medical Officer on the inspection date " + inspectionDate + " and  coordinate accordingly.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
                    txtmsg = "Dear Citizen,As per your application for Request of Payment of Unsuccessful Family Planning, a committee for Inspection has been scheduled. We request to kindly be present at Office of Chief Medical Officer on the inspection date " + inspectionDate + " and  coordinate accordingly." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007988503720631762";
                }
                else if (appstatus == 10)
                {

                    //txtmsg = "Dear Citizen,\n\n Your Application Form Number " + registrationNo + " for Payment of Unsuccessful Family Planning has been approved. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
                    txtmsg = "Dear Citizen,Your Application Form Number " + registrationNo + " for Payment of Unsuccessful Family Planning has been approved. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007336488542696224";
                }

                string status = SMS.SMSSend(txtmsg, Convert.ToString(mobileNo));

                objAccDb.SMSLog(txtmsg, mobileNo, status, registrationNo);
            }

        }

        public JsonResult BindForwardDropdownlist(long rollId)
        {
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            var res = objCMODB.bindDropdownlist(rollId).Where(m => lstCMODistrict.Any(p => p.districtId == m.districtId)).Select(m => new SelectListItem { Text = m.forwardtoName, Value = m.forwardtoId.ToString() });

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeAdmin]
        public ActionResult PrintApplicationFormFAP(string regisIdFAP)
        {
            regisIdFAP = OTPL_Imp.CustomCryptography.Decrypt(regisIdFAP);
            Session["regisIdFAP"] = regisIdFAP;

            FAPModel model = new FAPModel();
            model = objFAP_DB.GetFAPListBYRegistrationNo(Convert.ToInt64(regisIdFAP));

            if (objSM.RollID == 20)
            {
                if (model == null || objSM.districtId != model.healthunitDistrictId)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID >= 2 && objSM.RollID <= 5)
            {
                if (model == null || objSM.districtId != model.healthunitDistrictId || !(objSM.RollID != 2 ? objSM.UserID == model.forwardTo : true))
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID == 18)
            {
                var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
                int count = lstCMODistrict.Where(e => e.districtId == model.healthunitDistrictId).ToList().Count();
                if (count == 0)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID != 8 && objSM.RollID != 13)
            {
                return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
            }

            return View(model);
        }

        #endregion

        #region Family Planing CHC/DH abhijeet

        public ActionResult ReceivedForwardApp()
        {
            ProcessType model = new ProcessType();
            model = objCMODB.getApplicationCountFAP(objSM.UserID);
            return View(model);
        }

        [HttpGet]
        public ActionResult ForwardedApplicationFAP()
        {
            return View();
        }

        public ActionResult ForwardedApplicationListFAP(string registrationNo = "", string mobile = "", string requestDate = "")
        {
            List<FAPModel> lstFAPDetails = new List<FAPModel>();

            lstFAPDetails = objCMODB.GetForwordedAppByUserId(objSM.UserID, registrationNo, mobile, "", requestDate).ToList();

            return PartialView("_ForwardedApplicationListFAP", lstFAPDetails);
        }

        [HttpGet]
        public ActionResult ForwardedAppApprovedFAP()
        {
            return View();
        }

        public ActionResult ForwardedAppApprovedListFAP(string registrationNo = "", string mobile = "", string requestDate = "")
        {
            List<FAPModel> lstFAPDetails = new List<FAPModel>();

            lstFAPDetails = objCMODB.GetForwordedAppByUserId(objSM.UserID, registrationNo, mobile, "1", requestDate).ToList();

            return PartialView("_ForwardedAppApprovedListFAP", lstFAPDetails);
        }

        [HttpGet]
        public ActionResult ForwardedAppRejectedFAP()
        {
            return View();
        }

        public ActionResult ForwardedAppRejectedListFAP(string registrationNo = "", string mobile = "", string requestDate = "")
        {
            List<FAPModel> lstFAPDetails = new List<FAPModel>();

            lstFAPDetails = objCMODB.GetForwordedAppByUserId(objSM.UserID, registrationNo, mobile, "0", requestDate).ToList();

            return PartialView("_ForwardedAppRejectedListFAP", lstFAPDetails);
        }

        [HttpGet]
        public ActionResult ConfirmAppProcessFAP(string regisId)
        {

            FAPAppProcessModel model = new FAPAppProcessModel();
            if (regisId == null || regisId == "")
            {
                regisId = OTPL_Imp.CustomCryptography.Decrypt(Session["regisId"].ToString());
                model = objCMODB.getFAPStatus(Convert.ToInt64(regisId));
            }
            else
            {
                Session["regisId"] = regisId;
                regisId = OTPL_Imp.CustomCryptography.Decrypt(regisId);
                model = objCMODB.getFAPStatus(Convert.ToInt64(regisId));
            }

            if (model != null && objSM.UserID != model.forwardtoId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmAppProcessFAP(FAPAppProcessModel model)
        {
            var result = objCMODB.getFAPStatus(model.regisIdFAP);
            if (result != null && objSM.UserID != result.forwardtoId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            string IpAddress = Common.GetIPAddress();
            model.userId = objSM.UserID;
            model.transIp = IpAddress;

            if (model.rejectedRemarks == null && model.appStatus == 2)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["sancationDate"].Errors.Clear();
                ModelState["stateReportFile"].Errors.Clear();
                ModelState["districtReportFile"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();

                ModelState["inspReportFilePhoto"].Errors.Clear();

                model.status = true;
            }
            else if (model.rejectedRemarks != null && model.appStatus == 2)
            {
                ModelState["sancationDate"].Errors.Clear();
                ModelState["stateReportFile"].Errors.Clear();
                ModelState["districtReportFile"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();

                ModelState["inspReportFilePhoto"].Errors.Clear();

                model.status = false;
            }



            if (ModelState.IsValid)
            {
                model.regisIdFAP = Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(Session["regisId"].ToString()));
                var res = objCMODB.updateFAPProcess(model, "");
                if (res.Flag == 1)
                {

                    TempData["Msg"] = res.Msg;
                    TempData["MsgStatus"] = "success";
                    return RedirectToAction("ForwardedApplicationFAP");

                }
                else
                {
                    TempData["Msg"] = "Some Error Occur!";
                    TempData["MsgStatus"] = "error";
                    return RedirectToAction("ConfirmAppProcessFAP");
                }

            }
            else
            {
                TempData["Msg"] = "Some Error Occur!";
                TempData["MsgStatus"] = "error";
                return RedirectToAction("ForwardedApplicationFAP");
            }

        }

        #endregion

        #region Disability Certificate Abhijeet

        [AuthorizeAdmin(4)]
        public ActionResult AppliedApplicationDIC()
        {

            ProcessType model = new ProcessType();

            model = objCMODB.getDICprocessCount(objSM.UserID);

            return View(model);
            //ProcessType model = new ProcessType();

            //var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            //var resultData = objCMODB.GetAllRegistrationDIC().Where(m => lstCMODistrict.Any(p => p.districtId == m.districtId));
            //model.totalAppReceived = resultData.Where(m => m.appStatus == -1 || m.appStatus == 1).Count();
            //model.totalAppApproved = resultData.Where(m => m.appStatus == 7).Count();
            //model.totalAppRejected = resultData.Where(m => m.appStatus == 0 || m.appStatus == 5).Count();
            //model.totalAppInProcess = resultData.Where(m => m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 4 || m.appStatus == 6).Count();

            //return View(model);
        }

        [AuthorizeAdmin(4)]
        public ActionResult ReceivedApplicationDIC()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("DIC").Where(m => m.Value == "-1" || m.Value == "1").ToList();

            return View();
        }

        [AuthorizeAdmin(4)]
        public ActionResult ReceivedApplicationListDIC(string registrationNo = "", string mobile = "", string requestDate = "", string status = "")
        {

            List<DICModel> lstDICDetails = new List<DICModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            int intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;
            if (objSM.RollID == 8)
            {
                lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == -1 || m.appStatus == 1) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0)).ToList();

            }
            else
            {

                lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == -1 || m.appStatus == 1) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0) && (lstCMODistrict.Any(p => p.districtId == m.districtId))).ToList();

            }
            //before
            // var lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == -1 || m.appStatus == 1) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0) && (lstCMODistrict.Any(p => p.districtId == m.districtId))).ToList();

            return PartialView("_TotalReceivedListDIC", lstDICDetails);
        }

        [AuthorizeAdmin(4)]
        public ActionResult ViewAppDetailsDIC(long regisId)
        {
            DICModel model = new DICModel();
            model = objCMODB.GetAllRegistrationDIC().Where(m => m.regisIdDIC == regisId).FirstOrDefault();
            return PartialView("_ViewAppDetailsDIC", model);
        }

        [AuthorizeAdmin(4)]
        public ActionResult InProcessApplicationDIC()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("NUH").Where(m => m.Value == "2" && m.Value == "3" && m.Value == "4" && m.Value == "6").ToList();
            return View();
        }

        [AuthorizeAdmin(4)]
        public ActionResult InProcessApplicationListDIC(string registrationNo = "", string mobile = "", string requestDate = "", string status = "")
        {
            List<DICModel> lstDICDetails = new List<DICModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            int intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;
            if (objSM.RollID == 8)
            {
                lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 4 || m.appStatus == 6) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0)).ToList();

            }
            else
            {
                lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 4 || m.appStatus == 6) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0) && (lstCMODistrict.Any(p => p.districtId == m.districtId))).ToList();

            }
            //Before
            // var lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 4 || m.appStatus == 6) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus == intStatus || intStatus == 0) && (lstCMODistrict.Any(p => p.districtId == m.districtId))).ToList();

            return PartialView("_InProcessApplicationListDIC", lstDICDetails);
        }

        [AuthorizeAdmin(4)]
        public ActionResult UpdateAppProcessDIC(string regisId, string status)
        {
            bool validRequest = false;
            DICAppProcessModel model = new DICAppProcessModel();
            model.regisIdDIC = Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId));
            model.appStatus = Convert.ToInt32(OTPL_Imp.CustomCryptography.Decrypt(status));

            var result = objCMODB.GetAllRegistrationDIC(model.regisIdDIC).FirstOrDefault();
            if (result != null && objSM.districtId != result.districtId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            ViewBag.designation = comndb.GetDropDownList(33, Convert.ToInt32(objSM.UserID)).ToList();//objCMODB.binddesignation().ToList();
            if (ViewBag.designation != null && ViewBag.designation.Count > 0)
            {
                validRequest = true;
            }
            ViewBag.Condition = comndb.GetDropDownList(60, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.Assessment = comndb.GetDropDownList(61, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.EncryptRegisId = regisId;
            FillViewBagDIC(model.regisIdDIC);
            if (model.appStatus == -1)
            {
                ViewBag.PageTitle = "Accept/Reject Application";
            }
            else if (model.appStatus == 1)
            {
                ViewBag.PageTitle = "Schedule Inspection Application";
            }
            else if (model.appStatus == 2)
            {
                ViewBag.PageTitle = "Upload Inspection Report";
            }
            else if (model.appStatus == 3)
            {
                ViewBag.PageTitle = "Upload Inspection Report";
            }
            else if (model.appStatus == 7)
            {
                ViewBag.PageTitle = "Generate Certificate";
            }
            ViewBag.validRequest = validRequest;
            return View(model);
        }

        [AuthorizeAdmin(4)]
        public ActionResult DICAppDetails(long regisId)
        {
            DICModel model = new DICModel();
            model = objCMODB.GetAllRegistrationDIC().Where(m => m.regisIdDIC == regisId).FirstOrDefault();
            return PartialView("_DICAppDetails", model);
        }

        protected void FillViewBagDIC(long regisId)
        {
            //ViewBag.DLLCommittee = comndb.GetCommitteeDetailsForDLL().ToList();
            ViewBag.forwardTypes = objCMODB.rblforwardType().Where(m => m.forwardtypeId == 5 || m.forwardtypeId == 7).ToList();
            ViewBag.forwardTo = Enumerable.Empty<SelectListItem>();
            //ViewBag.DLLTestType = comndb.GetTestTypeForDLL_DIC().ToList();
            ViewBag.CommitteeMember = comndb.GetCommitteeMember_DIC(regisId).ToList();
            var resultData = comndb.GetInspReportPersentageDIC(regisId);
            if (resultData != null)
            {
                ViewBag.IsLessFourtyPer = resultData.isLessFourtyPer;
            }
        }

        [AuthorizeAdmin(4)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAppProcessDIC(DICAppProcessModel model, string button, FormCollection form)
        {
            if (Session["PhotoPath"] != null)
            {
                string[] fileArray = Session["PhotoPath"] as string[];

                if (fileArray != null)
                {
                    model.inspReportFilePhotoPath = fileArray;
                }
            }

            ModelState["inspReportFilePhoto"].Errors.Clear();
            ResultSet resultData = new ResultSet();
            int currAppStatus = model.appStatus;
            int? appStatus = null;
            bool isRedirectNewAction = false;
            string message = "", actionName = "";

            var result = objCMODB.GetAllRegistrationDIC(model.regisIdDIC).FirstOrDefault();
            if (result != null && (objSM.districtId != result.districtId || model.appStatus != result.appStatus))
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            if (!string.IsNullOrEmpty(button))
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();
                ModelState["inspReportFile"].Errors.Clear();

                ModelState["disabilityPer"].Errors.Clear();
                ModelState["markOfIdentification"].Errors.Clear();
                ModelState["reassPeriodType"].Errors.Clear();

                //ModelState["conditionId"].Errors.Clear();
                //ModelState["reassId"].Errors.Clear();

                if (button.ToLower() == "appaccept")
                {
                    appStatus = 1;
                    message = "Application Accepted";
                    actionName = "UpdateAppProcessDIC";
                }
                else if (button.ToLower() == "appforward")
                {
                    //
                    ModelState["conditionId"].Errors.Clear();
                    ModelState["reassId"].Errors.Clear();
                    ModelState["reassPeriod"].Errors.Clear();
                    ModelState["reassPeriodType"].Errors.Clear();
                    appStatus = 3;
                    message = "Forwarded";
                    actionName = "UpdateAppProcessDIC";
                }
                else if (button.ToLower() == "insaccept")
                {
                    appStatus = 5;
                    message = "Inspection Report Accepted";
                    actionName = "UpdateAppProcessDIC";
                }
                else if (button.ToLower() == "gencertificate")
                {
                    appStatus = 7;
                    message = "Certificate Generated";
                    actionName = "UpdateAppProcessDIC";
                    //isRedirectNewAction = true;
                }
            }
            else
            {
                if (model.appStatus == -1)
                {

                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["inspReportFile"].Errors.Clear();
                    ModelState["disabilityPer"].Errors.Clear();
                    ModelState["markOfIdentification"].Errors.Clear();
                    ModelState["reassPeriodType"].Errors.Clear();

                    //ModelState["conditionId"].Errors.Clear();
                    //ModelState["reassId"].Errors.Clear();
                    //ModelState["reassPeriod"].Errors.Clear();


                    appStatus = 0;
                    message = "Application Rejected";
                    actionName = "RejectedApplicationDIC";
                    isRedirectNewAction = true;
                }
                else if (model.appStatus == 1)
                {
                    ModelState["rejectedRemarks"].Errors.Clear();
                    ModelState["inspReportFile"].Errors.Clear();
                    ModelState["disabilityPer"].Errors.Clear();
                    ModelState["markOfIdentification"].Errors.Clear();
                    ModelState["reassPeriodType"].Errors.Clear();

                    //ModelState["conditionId"].Errors.Clear();
                    //ModelState["reassId"].Errors.Clear();

                    appStatus = 2;
                    message = "Inspection Scheduled";
                    actionName = "InProcessApplicationDIC";
                    isRedirectNewAction = true;
                }
                else if (model.appStatus == 2 || model.appStatus == 3)
                {
                    if (model.isLessFourtyPer)
                    {
                        ModelState["disabilityPer"].Errors.Clear();
                        ModelState["markOfIdentification"].Errors.Clear();

                        ModelState["conditionId"].Errors.Clear();
                        ModelState["reassId"].Errors.Clear();
                        ModelState["reassPeriod"].Errors.Clear();
                        ModelState["reassPeriodType"].Errors.Clear();
                    }
                    if (model.reassId == 1)
                    {
                        ModelState["reassPeriod"].Errors.Clear();
                        ModelState["reassPeriodType"].Errors.Clear();
                    }

                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["rejectedRemarks"].Errors.Clear();

                    ModelState["inspReportFilePhoto"].Errors.Clear();
                    if (model.appStatus == 2)
                    {
                        ModelState["forwardedTo"].Errors.Clear();

                    }

                    appStatus = 4;
                    message = "Inspection Report Uploaded";
                    actionName = "UpdateAppProcessDIC";
                    string errormsg = "";
                    bool valStatus = false;
                    AuditMethods objAud = new AuditMethods();
                    valStatus = objAud.IsValidLink(model.inspReportFilePath, "Document File", out errormsg);
                    if (!valStatus)
                    {
                        setSweetAlertMsg(errormsg, "warning");
                        return RedirectToAction(actionName, new { regisId = OTPL_Imp.CustomCryptography.Encrypt(model.regisIdDIC.ToString()), status = OTPL_Imp.CustomCryptography.Encrypt(model.appStatus.ToString()) });

                    }

                    var committeeMembderId = form.GetValues("chkCommMember");

                    int count = committeeMembderId.Count();

                    if (count > 0)
                    {
                        string XmlData = "<Members>";

                        for (int i = 0; i < count; i++)
                        {
                            XmlData += "<Member><committeeMembderId>" + committeeMembderId[i] + "</committeeMembderId></Member>";
                        }
                        XmlData += "</Members>";

                        model.xmlCommiMembers = XmlData;
                    }

                    #region Vinod Bulkinsertion of Inspection Photo

                    string XmlDataPIC = "";
                    int countPic = model.inspReportFilePhotoPath.Count();
                    XmlDataPIC = "<InspectionPICS>";
                    //  long regisByuser = objSM.UserID;
                    for (int i = 0; i < countPic; i++)
                    {
                        if (model.inspReportFilePhotoPath[i] == "")
                        {
                            //XmlData = string.Empty;
                        }
                        else
                        {

                            XmlDataPIC += "<InspectionPIC><UploadPICFilePath>" + model.inspReportFilePhotoPath[i] + "</UploadPICFilePath></InspectionPIC>";
                        }

                    }
                    XmlDataPIC += "</InspectionPICS>";
                    model.XmlDataPhoto = XmlDataPIC;

                    #endregion

                }
                else if (model.appStatus == 4)
                {
                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["inspReportFile"].Errors.Clear();
                    ModelState["disabilityPer"].Errors.Clear();
                    ModelState["markOfIdentification"].Errors.Clear();
                    ModelState["reassPeriodType"].Errors.Clear();
                    //ModelState["reassPeriod"].Errors.Clear();
                    //ModelState["conditionId"].Errors.Clear();
                    //ModelState["reassId"].Errors.Clear();

                    appStatus = 5;
                    message = "Inspection Report Rejected";
                    actionName = "RejectedApplicationDIC";
                    isRedirectNewAction = true;
                }
            }

            if (ModelState.IsValid && appStatus != null)
            {
                model.appStatus = Convert.ToInt32(appStatus);
                model.userId = objSM.UserID;
                model.transIp = Common.GetIPAddress();

                try
                {
                    resultData = objCMODB.UpdateAppProcessDIC(model);
                }
                catch (Exception ex)
                {
                    setSweetAlertMsg(ex.Message, "warning");
                }
            }
            else
            {
                setSweetAlertMsg("Enter valid data !", "warning");
            }

            if (resultData != null && resultData.Flag > 0)
            {
                SendSMSUpdateProcessDIC(resultData.RegistrationNo, resultData.inspectionDate, resultData.MobileNo, resultData.appStatus);
                TempData["SuccessMsg"] = message;
                if (isRedirectNewAction)
                {
                    return RedirectToAction(actionName);
                }
                else
                {
                    return RedirectToAction(actionName, new { regisId = OTPL_Imp.CustomCryptography.Encrypt(model.regisIdDIC.ToString()), status = OTPL_Imp.CustomCryptography.Encrypt(model.appStatus.ToString()) });
                }

            }
            else
            {
                model.appStatus = Convert.ToInt32(currAppStatus);
                FillViewBagDIC(model.regisIdDIC);
                ViewBag.designation = comndb.GetDropDownList(33, Convert.ToInt32(objSM.UserID)).ToList();//objCMODB.binddesignation().ToList();
                ViewBag.Condition = comndb.GetDropDownList(60, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                ViewBag.Assessment = comndb.GetDropDownList(61, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                return View(model);
            }
        }

        [AuthorizeAdmin(4)]
        public ActionResult BindScheduleOfCommitteeDIC(string inspectionDate = "")
        {
            if (inspectionDate != "" && inspectionDate != null)
            {
                var lstScheduleOfCommitteeDIC = objCMODB.GetScheduleOfCommitteeDIC(inspectionDate, objSM.UserID).ToList();
                return PartialView("_BindScheduleOfCommitteeDIC", lstScheduleOfCommitteeDIC);
            }
            else
            {
                return Content("NF");
            }
        }

        [AuthorizeAdmin(4)]
        public JsonResult UploadFileDIC(HttpPostedFileBase File)
        {
            string Dirpath = "~/Content/writereaddata/DIC/InspectionReport/";
            string path = "";
            string filename = "";

            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }

            string ext = Path.GetExtension(File.FileName);

            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {
                filename = Path.GetFileNameWithoutExtension(File.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";
            }

            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }

        [AuthorizeAdmin(4)]
        public ActionResult ApprovedApplicationDIC()
        {
            return View();
        }

        [AuthorizeAdmin(4)]
        public ActionResult ApprovedApplicationListDIC(string registrationNo = "", string mobile = "", string requestDate = "")
        {
            List<DICModel> lstDICDetails = new List<DICModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (objSM.RollID == 8)
            {

                lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == 7) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "")).ToList();
            }
            else
            {

                lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == 7) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.districtId))).ToList();
            }
            return PartialView("_ApprovedApplicationListDIC", lstDICDetails);
        }

        [AuthorizeAdmin(4)]
        public ActionResult RejectedApplicationDIC()
        {

            return View();
        }

        [AuthorizeAdmin(4)]
        public ActionResult RejectedApplicationListDIC(string registrationNo = "", string mobile = "", string requestDate = "")
        {
            List<DICModel> lstDICDetails = new List<DICModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (objSM.RollID == 8)
            {

                lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == 0 || m.appStatus == 5) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "")).ToList();
            }
            else
            {

                lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == 0 || m.appStatus == 5) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.districtId))).ToList();
            }
            //  before
            // var lstDICDetails = objCMODB.GetAllRegistrationDIC().Where(m => (m.appStatus == 0 || m.appStatus == 5) && (m.registrationNo == registrationNo || registrationNo == "") && (m.mobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.districtId))).ToList();

            return PartialView("_RejectedApplicationListDIC", lstDICDetails);
        }

        void SendSMSUpdateProcessDIC(string registrationNo, string inspectionDate, string mobileNo, int appstatus)
        {
            ForgotPasswordModel otpChCount = new ForgotPasswordModel();
            string txtmsg = "";
            if (appstatus == 0 || appstatus == 5)
            {
               // txtmsg = "Dear Citizen,\n\nYour Application Form Number " + registrationNo + " for Issuance of Disability Certificate has been rejected. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
                txtmsg = "Dear Citizen,Your Application Form Number " + registrationNo + " for Issuance of Disability Certificate has been rejected. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007453334793005305";
            }
            else if (appstatus == 2)
            {
                txtmsg = "Dear Citizen,\n\n As per your application for Issuance of Disability Certificate, a committee for Inspection has been scheduled. We request to kindly be present at Office of Chief Medical Officer on the inspection date" + inspectionDate + "and coordinate accordingly.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
            }
            else if (appstatus == 3)
            {
                //txtmsg = "Dear Citizen,\n\nAs per your application for Issuance of Disability Certificate, a committee for Inspection has been re- scheduled on the inspection date " + inspectionDate + ".We request to kindly be present at Office of Chief Medical Officer and coordinate accordingly.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
                txtmsg = "Dear Citizen,As per your application for Issuance of Disability Certificate, a committee for Inspection has been re- scheduled on the inspection date " + inspectionDate + ". We request to kindly be present at Office of Chief Medical Officer and coordinate accordingly." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007215072676690242";
            }
            else if (appstatus == 7)
            {
                //txtmsg = "Dear Citizen,\n\n Your Application Form Number " + registrationNo + " for Issuance of Disability Certificate has been approved. Please get in touch with the office of Chief Medical Officer to collect your certificate. You can also download the Certificate from your dashboard.\n\n" + objSM.DisplayName + "\nMHFWD, UP";
                txtmsg = "Dear Citizen,Your Application Form Number " + registrationNo + " for Issuance of Disability Certificate has been approved. Please get in touch with the office of Chief Medical Officer to collect your certificate. You can also download the Certificate from your dashboard." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007712880702894169";
            }

            if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(txtmsg))
            {
                string status = SMS.SMSSend(txtmsg, Convert.ToString(mobileNo));

                objAccDb.SMSLog(txtmsg, mobileNo, status, registrationNo);
            }
        }

        [AuthorizeAdmin]
        public ActionResult PrintApplicationFormDIC(string registrationNo)
        {
            registrationNo = OTPL_Imp.CustomCryptography.Decrypt(registrationNo);
            DICModel model = new DICModel();
            DIC_DB objDB_DIC = new DIC_DB();
            model = objDB_DIC.GetDICListBYRegistrationNo(0, registrationNo);

            if (objSM.RollID == 2 || objSM.RollID == 20)
            {
                if (model == null || objSM.districtId != model.districtId)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID == 18)
            {
                var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
                int count = lstCMODistrict.Where(e => e.districtId == model.districtId).ToList().Count();
                if (count == 0)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID != 8)
            {
                return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
            }

            return View(model);
        }

        #region Method Generate Certificate DIC
        [AuthorizeAdmin(4)]
        public ActionResult GenerateCertificateDIC(string regisId)
        {
            string setPdfName = "", setDigitalPdfName = "";
            var data = new List<DICCertificateDetailModel>();
            var res = objCMODB.GetCertificateDetialDIC(Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId)));

            if (res != null && objSM.districtId != res[0].districtId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            var res1 = objCMODB.GetCommitteeMemberDIC(res[0].@inprocessDICId);
            if (res != null && res.Count > 0)
            {
                if (res[0].passportsizephoto == "" || res[0].passportsizephoto == null)
                {

                }
                else
                {
                    data = ConvertImageDIC(res.ToList());
                }
                try
                {
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rptCertificateDIC.rpt"));
                    if (res[0].passportsizephoto == "" || res[0].passportsizephoto == null)
                    {
                        rd.SetDataSource(res);
                    }
                    else
                    {
                        rd.SetDataSource(data);
                    }
                    ReportDocument subShows = rd.Subreports["rpt_DICcommitteeMember.rpt"];
                    subShows.SetDataSource(res1);
                    rd.SetParameterValue("districtName", objSM.DistrictName);

                    setPdfName = "UnSigned_" + res[0].certificateNo;

                    string folderpath = "~/Content/writereaddata/UnSignedCertificate/DIC/" + objSM.DistrictName + "/";

                    if (!System.IO.Directory.Exists(Server.MapPath(folderpath)))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(folderpath));
                    }

                    string flName = folderpath + setPdfName + ".pdf";

                    rd.ExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                    rd.ExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                    rd.ExportOptions.DestinationOptions = new DiskFileDestinationOptions();
                    ((DiskFileDestinationOptions)rd.ExportOptions.DestinationOptions).DiskFileName = Server.MapPath(flName);
                    rd.Export();
                    if (System.IO.File.Exists(Server.MapPath(flName)))
                    {
                        int result = objCMODB.InsertUnSignedCertiPath_DIC(res[0].regisIdDIC, flName);
                    }
                    rd.Close();
                    rd.Dispose();

                    if (ConfigurationManager.AppSettings["IsDigitalSign"].ToString() == "N")
                    {
                        FileInfo fileInfo = new FileInfo(Server.MapPath(flName));
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + setPdfName + ".pdf");
                        Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                        Response.WriteFile(flName);
                        Response.Flush();
                        Response.Close();
                    }
                    else
                    {
                        //digital sign

                        setDigitalPdfName = "DisabilityCertificateDigitalSigned" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                        string digitalFlName = folderpath + setDigitalPdfName + ".pdf";

                        var sigDetails = comndb.GetDigitalSignatureDetails(objSM.UserID);

                        float llx = 230;
                        float lly = 320;
                        float urx = 350;
                        float ury = 220;
                        Comman_Classes.DigitalCeritificateManager dcm = new Comman_Classes.DigitalCeritificateManager();
                        Comman_Classes.MetaData md = new Comman_Classes.MetaData()
                        {
                            Author = sigDetails.Author,
                            Title = "Disability Certificate Authentication",
                            Subject = "Disability Certificate",
                            Creator = sigDetails.Creator,
                            Producer = sigDetails.Producer,
                            Keywords = sigDetails.Keywords
                        };

                        string Signaturepath = Server.MapPath(sigDetails.Signaturepath);
                        dcm.signPDF(Server.MapPath(flName), Server.MapPath(digitalFlName), Signaturepath,
                         sigDetails.signpwd, "Authenticate Disability Certificate", sigDetails.SigContact,
                         sigDetails.SigLocation, true, llx, lly, urx, ury, 1, md);

                        FileInfo fileInfo = new FileInfo(Server.MapPath(digitalFlName));
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + setDigitalPdfName + ".pdf");
                        Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                        Response.WriteFile(Server.MapPath(digitalFlName));
                        Response.Flush();
                        Response.Close();

                        if (System.IO.File.Exists(Server.MapPath(flName)))
                        {
                            System.IO.File.Delete(Server.MapPath(flName));
                        }

                        if (System.IO.File.Exists(Server.MapPath(digitalFlName)))
                        {
                            System.IO.File.Delete(Server.MapPath(digitalFlName));
                        }

                        //digital sign end
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error Occour to Downloading.";
                }
            }

            return View("DownloadFile");
        }

        private List<DICCertificateDetailModel> ConvertImageDIC(List<DICCertificateDetailModel> list)
        {
            foreach (var item in list)
            {

                imageConverter imgCon = new imageConverter();

                FileStream signfs2;

                BinaryReader signbr2;

                String ImgPath = Server.MapPath(Convert.ToString(item.passportsizephoto));
                if (!System.IO.File.Exists(ImgPath))
                {
                    ImgPath = Server.MapPath("~/Images/document.png");
                }

                signfs2 = new FileStream(ImgPath, FileMode.Open);

                try
                {
                    signbr2 = new BinaryReader(signfs2);
                    byte[] imgbyte2 = new byte[signfs2.Length + 1];
                    imgbyte2 = signbr2.ReadBytes(Convert.ToInt32((signfs2.Length)));
                    item.Photo = imgCon.ResizeImageFile(imgbyte2, 210);


                    signfs2.Close();
                    signbr2.Close();
                }
                catch { signfs2.Close(); }
                finally { signfs2.Close(); }



            }
            return list;
        }
        #endregion

        [AuthorizeAdmin(4)]
        public JsonResult UploadCertificateFileDIC(HttpPostedFileBase File)
        {
            long regisId = Convert.ToInt64(Session["registrationIdDIC"].ToString());
            var lstDICDetails = objCMODB.GetDICCertificate(regisId);

            string Dirpath = "~/Content/SignedCertificate/DIC/" + objSM.DistrictName + "/";
            string path = "";
            string filename = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            string ext = Path.GetExtension(File.FileName);
            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {
                filename = "Signed_" + lstDICDetails.CertificateNo + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";//"warning_Maximum 2MB file size are allowed";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";//"warning_Invalid File Format only pdf and jpg files are allow!";
            }

            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }

        [AuthorizeAdmin(4)]
        [HttpGet]
        public ActionResult UploadCertificateDIC(string regisId)
        {
            long regId = Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId));

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            var resultData = objCMODB.GetAllRegistrationDIC(regId).Where(m => (lstCMODistrict.Any(p => p.districtId == m.districtId))).Count();

            if (resultData == 0)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            Session["registrationIdDIC"] = regId;

            return View();
        }

        [AuthorizeAdmin(4)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadCertificateDIC(FormCollection frm)
        {
            var CertificatePath = frm.GetValues("hdnfileUploadCertificate");
            string IPAddress = Common.GetIPAddress();
            if (CertificatePath[0] != null || CertificatePath[0] != "")
            {
                var res = objCMODB.UpdateDICCertificate(Convert.ToInt64(Session["registrationIdDIC"]), CertificatePath[0], objSM.UserID, IPAddress);
                if (res.Flag == 1)
                {
                    TempData["Message"] = res.Msg;
                }
                return RedirectToAction("ApprovedApplicationDIC", "CMO");
            }
            else
            {
                TempData["msg"] = "Please choose a file!";
                TempData["msgstatus"] = "warning";
                return RedirectToAction("UploadCertificateDIC");
            }
        }
        #endregion

        #region AGC Riya
        [AuthorizeAdmin(10)]
        public ActionResult AppliedApplicationAGC()
        {
            ProcessType model = new ProcessType();

            model = objCMODB.getApplicationCountAGC(objSM.UserID);

            return View(model);

        }

        [AuthorizeAdmin(10)]
        public ActionResult ReceivedApplicationAGC()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("DIC").Where(m => m.Value == "-1" || m.Value == "1").ToList();
            return View();
        }

        [AuthorizeAdmin(10)]
        public ActionResult ReceivedApplicationListAGC(string registrationNo = "", string status = "", string requestDate = "")
        {
            List<AGCModel> lstAGCDetails = new List<AGCModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            int intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;

            if (objSM.RollID == 8)
            {
                lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == -1 || m.appStatus == 1) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == requestDate || requestDate == "")).ToList();

            }
            else
            {

                lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == -1 || m.appStatus == 1) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();

            }

            //   var lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == -1 || m.appStatus == 1) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();

            return PartialView("_ReceivedApplicationListAGC", lstAGCDetails);
        }

        [AuthorizeAdmin(10)]
        [HttpGet]
        public ActionResult UpdateAppProcessAGC(string regisId, int status)
        {
            bool validRequest = false;
            AGCAppProcessModel model = new AGCAppProcessModel();
            if (regisId != "" && regisId != null)
            {
                regisId = OTPL_Imp.CustomCryptography.Decrypt(regisId);
                model = objCMODB.GetAGCList(regisId, status);
                ViewBag.DLLCommittee = comndb.GetDropDownList(30, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                ViewBag.forwardTypes = objCMODB.rblforwardType().Where(m => m.forwardtypeId == 3 || m.forwardtypeId == 5).ToList();
                ViewBag.forwardTo = Enumerable.Empty<SelectListItem>();
                //int comid = Convert.ToInt32(model.committeeId);
                ViewBag.designation = comndb.GetDropDownList(33, Convert.ToInt32(objSM.UserID)).ToList();//objCMODB.binddesignation().ToList();
                if (ViewBag.designation != null && ViewBag.designation.Count > 0)
                {
                    validRequest = true;
                }
                Session["regisIdAGC"] = regisId;
            }
            else
            {
                ViewBag.forwardTypes = objCMODB.rblforwardType().Where(m => m.forwardtypeId == 3 || m.forwardtypeId == 5).ToList();
                ViewBag.forwardTo = Enumerable.Empty<SelectListItem>();
                model = objCMODB.GetAGCList(Session["regisIdAGC"].ToString(), Convert.ToInt32(status));
                ViewBag.DLLCommittee = comndb.GetDropDownList(30, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                //if (ViewBag.DLLCommittee != null && ViewBag.DLLCommittee.count>0)
                //{
                //    validRequest = true;
                //}

                //int comid = Convert.ToInt32(model.committeeId);
                ViewBag.designation = comndb.GetDropDownList(33, Convert.ToInt32(objSM.UserID)).ToList();
            }

            if (model != null && objSM.districtId != model.susdistrictId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            ViewBag.validRequest = validRequest;
            return View(model);

        }

        [AuthorizeAdmin(10)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAppProcessAGC(AGCAppProcessModel model, FormCollection form)//Update Status For FIC
        {

            if (Session["PhotoPath"] != null)
            {
                string[] fileArray = Session["PhotoPath"] as string[];

                if (fileArray != null)
                {
                    model.inspReportFilePhotoPath = fileArray;
                }
            }

            ModelState["inspReportFilePhoto"].Errors.Clear();
            var result = objCMODB.GetAGCList(model.registrationNo, model.appStatus);
            if (result == null || objSM.districtId != result.susdistrictId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }
            model.userId = objSM.UserID;
            model.transIp = Common.GetIPAddress();
            string xmlData = "";
            if (model.rejectedRemarks == null)
            {
                model.status = true;
            }
            else
            {
                model.status = false;
            }

            if (model.appStatus == -1 && model.rejectedRemarks == null)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionRpt"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();

                ModelState["Age"].Errors.Clear();
                ModelState["xRayPlateNo"].Errors.Clear();
                ModelState["xRayDate"].Errors.Clear();
                ModelState["dentalPlateNo"].Errors.Clear();
                ModelState["markOfIdentification"].Errors.Clear();


            }
            if (model.appStatus == -1 && model.rejectedRemarks != null)
            {
                ModelState["inspectionRpt"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();

                ModelState["Age"].Errors.Clear();
                ModelState["xRayPlateNo"].Errors.Clear();
                ModelState["xRayDate"].Errors.Clear();
                ModelState["dentalPlateNo"].Errors.Clear();
                ModelState["markOfIdentification"].Errors.Clear();

            }
            if (model.appStatus == 1)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionRpt"].Errors.Clear();

                ModelState["Age"].Errors.Clear();
                ModelState["xRayPlateNo"].Errors.Clear();
                ModelState["xRayDate"].Errors.Clear();
                ModelState["dentalPlateNo"].Errors.Clear();
                ModelState["markOfIdentification"].Errors.Clear();

            }
            if (model.appStatus == 2 && model.rblDisbilityPer == 2)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["forwardedTo"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();



                model.status = true;
                var committeeMembderId = form.GetValues("chkdesig");

                int count = committeeMembderId.Count();
                xmlData = "<Members>";
                long regisByuser = objSM.UserID;
                for (int i = 0; i < count; i++)
                {
                    if (committeeMembderId[i] == "")
                    {
                        //XmlData = string.Empty;
                    }
                    else
                    {

                        xmlData += "<Member><committeeMembderId>" + committeeMembderId[i] + "</committeeMembderId></Member>";
                    }

                }
                xmlData += "</Members>";

            }
            if (model.appStatus == 2 && model.rblDisbilityPer == 1)
            {
                model.status = false;
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionRpt"].Errors.Clear();

                ModelState["Age"].Errors.Clear();
                ModelState["xRayPlateNo"].Errors.Clear();
                ModelState["xRayDate"].Errors.Clear();
                ModelState["dentalPlateNo"].Errors.Clear();
                ModelState["markOfIdentification"].Errors.Clear();

                ModelState["Age"].Errors.Clear();
            }
            if (model.appStatus == 3)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();

                ModelState["inspReportFilePhoto"].Errors.Clear();

                #region Vinod Bulkinsertion of Inspection Photo

                string XmlDataPIC = "";
                int countPic = model.inspReportFilePhotoPath.Count();
                XmlDataPIC = "<InspectionPICS>";
                //  long regisByuser = objSM.UserID;
                for (int i = 0; i < countPic; i++)
                {
                    if (model.inspReportFilePhotoPath[i] == "")
                    {
                        //XmlData = string.Empty;
                    }
                    else
                    {

                        XmlDataPIC += "<InspectionPIC><UploadPICFilePath>" + model.inspReportFilePhotoPath[i] + "</UploadPICFilePath></InspectionPIC>";
                    }

                }
                XmlDataPIC += "</InspectionPICS>";
                model.XmlDataPhoto = XmlDataPIC;

                #endregion

                var committeeMembderId = form.GetValues("chkdesig");

                int count = committeeMembderId.Count();
                xmlData = "<Members>";
                long regisByuser = objSM.UserID;
                for (int i = 0; i < count; i++)
                {
                    if (committeeMembderId[i] == "")
                    {
                        //XmlData = string.Empty;
                    }
                    else
                    {

                        xmlData += "<Member><committeeMembderId>" + committeeMembderId[i] + "</committeeMembderId></Member>";
                    }

                }
                xmlData += "</Members>";

            }
            if (model.appStatus == 4)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();
                ModelState["inspectionRpt"].Errors.Clear();

                ModelState["Age"].Errors.Clear();
                ModelState["xRayPlateNo"].Errors.Clear();
                ModelState["xRayDate"].Errors.Clear();
                ModelState["dentalPlateNo"].Errors.Clear();
                ModelState["markOfIdentification"].Errors.Clear();

            }
            if (model.appStatus == 6)
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();
                ModelState["inspectionRpt"].Errors.Clear();

                ModelState["Age"].Errors.Clear();
                ModelState["xRayPlateNo"].Errors.Clear();
                ModelState["xRayDate"].Errors.Clear();
                ModelState["dentalPlateNo"].Errors.Clear();
                ModelState["markOfIdentification"].Errors.Clear();

            }
            if (ModelState.IsValid)
            {

                AuditMethods objAud = new AuditMethods();
                string errormsg = "";
                bool valStatus = false;

                if (!string.IsNullOrEmpty(model.inspectionRptPath))
                {
                    valStatus = objAud.IsValidLink(model.inspectionRptPath, "Document File", out errormsg);
                    if (!valStatus)
                    {
                        setSweetAlertMsg(errormsg, "warning");
                        return View(model);
                    }
                }


                var res = objCMODB.AGCAppStatusInsertUpdate(model.regisIdAGC, model.appStatus, model.status, model.rejectedRemarks, model.xRayPlateNo, model.xRayDate, model.dentalPlateNo, model.markOfIdentification, model.Age, model.inspectionDate, model.inspectionRptPath, model.userId, model.transIp, model.forwardedType, model.forwardedTo, xmlData, model.XmlDataPhoto);

                if (res.Flag == 1)
                {
                    TempData["msg"] = res.Msg;
                    TempData["msgstatus"] = "success";

                    SendSMSUpdateProcessAGC(res.RegistrationNo, res.inspectionDate, res.MobileNo, res.appStatus);
                    if (model.appStatus == -1 && model.rejectedRemarks == null)
                    {
                        return RedirectToAction("UpdateAppProcessAGC", new { status = res.appStatus });
                    }
                    if (model.appStatus == 1)
                    {
                        return RedirectToAction("InProcessApplicationAGC");
                    }
                    if (model.appStatus == 2 || model.appStatus == 3)
                    {
                        return RedirectToAction("UpdateAppProcessAGC", new { status = res.appStatus });
                    }
                    if (model.appStatus == 4 && model.rejectedRemarks != null)
                    {
                        return RedirectToAction("RejectedApplicationAGC");
                    }
                    if (model.appStatus == 4 && model.rejectedRemarks == null)
                    {
                        return RedirectToAction("UpdateAppProcessAGC", new { status = res.appStatus });
                    }
                    if ((model.appStatus == -1 && model.rejectedRemarks != null) || (model.appStatus == 5 && model.rejectedRemarks != null))
                    {
                        return RedirectToAction("RejectedApplicationAGC");

                    }
                    if (model.appStatus == 6)
                    {
                        return RedirectToAction("UpdateAppProcessAGC", new { status = res.appStatus });
                    }
                    if (model.appStatus == 7)
                    {

                        return RedirectToAction("ApprovedApplicationAGC");
                    }
                }

            }
            return View();

        }

        [AuthorizeAdmin(10)]
        public ActionResult InProcessApplicationAGC()//All In process Application
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("DIC").Where(m => m.Value == "2" || m.Value == "3" || m.Value == "4" || m.Value == "6").ToList();
            return View();
        }

        [AuthorizeAdmin(10)]
        public ActionResult InProcessApplicationListAGC(string registrationNo = "", string status = "", string requestDate = "")//LIst Of all In Process Application
        {
            // int intStatus = 0;
            List<AGCModel> lstAGCDetails = new List<AGCModel>();

            //var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            //lstAGCetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 4 || m.appStatus == 6) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();

            //intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;
            //lstAGCetails = lstAGCetails.Where(m => (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == requestDate || requestDate == "") || (m.registrationNo == registrationNo || registrationNo == "")).ToList();

            //return PartialView("_InProcessApplicationListAGC", lstAGCetails);

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            int intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;

            if (objSM.RollID == 8)
            {
                lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 4 || m.appStatus == 6) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == requestDate || requestDate == "")).ToList();
            }
            else
            {

                lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 4 || m.appStatus == 6) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();
            }

            //Before
            //  var lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 2 || m.appStatus == 3 || m.appStatus == 4 || m.appStatus == 6) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appStatus == intStatus || intStatus == 0) && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();

            return PartialView("_InProcessApplicationListAGC", lstAGCDetails);
        }

        [AuthorizeAdmin(10)]
        public ActionResult ApprovedApplicationAGC()//All Approved Application
        {
            // ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("DIC").Where(m => m.Value == "5" || m.Value == "6").ToList();
            return View();
        }

        [AuthorizeAdmin(10)]
        public ActionResult ApprovedApplicationListAGC(string registrationNo = "", string requestDate = "")//LIst Of Approved Application
        {
            List<AGCModel> lstAGCDetails = new List<AGCModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            if (objSM.RollID == 8)
            {
                lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 7) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appliedDate == requestDate || requestDate == "")).ToList();
            }
            else
            {

                lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 7) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();
            }

            // var lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 7) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();

            return PartialView("_ApprovedApplicationListAGC", lstAGCDetails);
        }

        [AuthorizeAdmin(10)]
        public ActionResult RejectedApplicationAGC()//All Rejected Application
        {

            return View();
        }

        [AuthorizeAdmin(10)]
        public ActionResult RejectedApplicationListAGC(string registrationNo = "", string requestDate = "")//LIst Of Rejected Application
        {
            List<AGCModel> lstAGCDetails = new List<AGCModel>();
            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (objSM.RollID == 8)
            {
                lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 0 || m.appStatus == 5) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appliedDate == requestDate || requestDate == "")).ToList();
            }
            else
            {

                lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 0 || m.appStatus == 5) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();
            }

            //var lstAGCDetails = objCMODB.GetAllAGCList().Where(m => (m.appStatus == 0 || m.appStatus == 5) && (m.registrationNo == registrationNo || registrationNo == "") && (m.appliedDate == requestDate || requestDate == "") && (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).ToList();

            return PartialView("_RejectedApplicationListAGC", lstAGCDetails);
        }

        [AuthorizeAdmin(10)]
        public ActionResult AgeCertificateDetailAGC(string registration)//list of All Received Application
        {
            AGCModel model = new AGCModel();
            model = objCMODB.GetAGCListBYRegistrationNo(registration);

            return PartialView("_AgeCertificateDetailAGC", model);
        }

        public void SendSMSUpdateProcessAGC(string registrationNo, string inspectionDate, string mobileNo, int appstatus)
        {

            string txtmsg = "";
            string CMO = objSM.DisplayName;
            if (mobileNo != "")
            {
                ForgotPasswordModel otpChCount = new ForgotPasswordModel();

                if (appstatus == 2)
                {
                   // txtmsg = "Dear Citizen,\n\nAs per your application for Issuance of Age Certificate, a committee for Inspection has been scheduled. We request to kindly be present at Office of Chief Medical Officer on the inspection date" + inspectionDate + "and coordinate accordingly. \n\n" + objSM.DisplayName + ",\nMHFWD, UP";

                    txtmsg = "Dear Citizen,As per your application for Issuance of Age Certificate, a committee for Inspection has been scheduled. We request to kindly be present at Office of Chief Medical Officer on the inspection date " + inspectionDate + " and coordinate accordingly." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007211720840216977";
                }
                else if (appstatus == 0 || appstatus == 5)
                {
                  //  txtmsg = "Dear Citizen,\n\nYour Application Form Number " + registrationNo + " for Issuance of Age Certificate has been rejected. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details.\n\n" + objSM.DisplayName + ",\n MHFWD, UP";
                     txtmsg = "Dear Citizen,Your Application Form Number " + registrationNo + " for Issuance of Age Certificate has been rejected. Kindly get in touch with the office of Chief Medical Officer for more details. You can also login to your dashboard for more details." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007011294298369195";
                }
                else if (appstatus == 7)
                {
                   // txtmsg = "Dear Citizen,\n\nYour Application Form Number " + registrationNo + " for Issuance of Age Certificate has been approved. Please get in touch with the office of Chief Medical Officer to collect your certificate. You can also download the Certificate from your dashboard.\n\n" + objSM.DisplayName + ", \nMHFWD, UP";
                    txtmsg = "Dear Citizen,Your Application Form Number " + registrationNo + " for Issuance of Age Certificate has been approved. Please get in touch with the office of Chief Medical Officer to collect your certificate. You can also download the Certificate from your dashboard." + objSM.DisplayName + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007514674407107757";
                }
                string status = SMS.SMSSend(txtmsg, Convert.ToString(mobileNo));

                objAccDb.SMSLog(txtmsg, mobileNo, status, registrationNo);
            }

        }

        [AuthorizeAdmin(10)]
        public ActionResult BindScheduleOfCommitteeAGC(string inspectionDate = "")
        {
            if (inspectionDate != "" && inspectionDate != null)
            {
                var lstScheduleOfCommittee = objCMODB.GetScheduleOfCommitteeAGC(inspectionDate, objSM.UserID).ToList();
                return PartialView("_BindScheduleOfCommitteeAGC", lstScheduleOfCommittee);
            }
            else
            {
                return Content("NF");
            }
        }

        public ActionResult PrintApplicationFormAGC(string regisIdAGC = "")
        {
            long regisId = Convert.ToInt64(regisIdAGC);
            string Registration = "";
            AGCModel model = new AGCModel();
            model = objAGC_DB.GetAGCListBYRegistrationNo(regisId, Registration);

            if (objSM.RollID == 2 || objSM.RollID == 20)
            {
                if (model == null || objSM.districtId != model.susdistrictId)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID == 18)
            {
                var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
                int count = lstCMODistrict.Where(e => e.districtId == model.susdistrictId).ToList().Count();
                if (count == 0)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID != 8)
            {
                return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
            }

            return View(model);
        }

        #region Method Generate Certificate AGC
        [AuthorizeAdmin(10)]
        public ActionResult GeneratedCertificateAGC(string regisIdAGC)
        {
            regisIdAGC = regisIdAGC.ToString();
            string setPdfName = "", setDigitalPdfName = "";
            var data = new List<AGCReportModel>();
            //var res = objCMODB.GetCertificateDetialAGC(Convert.ToInt64(regisIdAGC));
            var res = objCMODB.GetCertificateDetialAGC(Convert.ToInt64(@OTPL_Imp.CustomCryptography.Decrypt(regisIdAGC)));

            if (res != null && objSM.districtId != res[0].susdistrictId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            var res1 = objCMODB.GetCommitteeMemberAGC(res[0].inprocessAGCId);

            if (res != null && res.Count > 0)
            {
                if (res[0].susphotoFile == "Submitted By Department" || res[0].susphotoFile == "" || res[0].susphotoFile == null || res[0].susThumbFile == null || res[0].susThumbFile == "Submitted By Department")
                {

                }
                else
                {
                    data = ConvertImageAGC(res.ToList());
                }

                try
                {

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rptCertificateAGC.rpt"));
                    if (res[0].susphotoFile == "Submitted By Department" || res[0].susphotoFile == "" || res[0].susphotoFile == null || res[0].susThumbFile == null)
                    {
                        rd.SetDataSource(res);
                    }
                    else
                    {
                        rd.SetDataSource(data);
                    }
                    ReportDocument subShows = rd.Subreports["rpt_AGCcommitteeMember.rpt"];
                    subShows.SetDataSource(res1);

                    rd.SetParameterValue("districtName", objSM.DistrictName);
                    setPdfName = "UnSigned_" + res[0].certificateNo;

                    string folderpath = "~/Content/writereaddata/UnSignedCertificate/AGC/" + objSM.DistrictName + "/";

                    if (!System.IO.Directory.Exists(Server.MapPath(folderpath)))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(folderpath));
                    }

                    string flName = folderpath + setPdfName + ".pdf";

                    rd.ExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                    rd.ExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                    rd.ExportOptions.DestinationOptions = new DiskFileDestinationOptions();
                    ((DiskFileDestinationOptions)rd.ExportOptions.DestinationOptions).DiskFileName = Server.MapPath(flName);
                    rd.Export();
                    if (System.IO.File.Exists(Server.MapPath(flName)))
                    {
                        int result = objCMODB.InsertUnSignedCertiPath_AGC(res[0].regisIdAGC, flName);
                    }
                    rd.Close();
                    rd.Dispose();

                    if (ConfigurationManager.AppSettings["IsDigitalSign"].ToString() == "N")
                    {
                        FileInfo fileInfo = new FileInfo(Server.MapPath(flName));

                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + setPdfName + ".pdf");
                        Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                        Response.WriteFile(flName);
                        Response.Flush();
                        Response.Close();
                    }
                    else
                    {
                        //digital sign

                        setDigitalPdfName = "AgeCertificateDigitalSigned" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                        string digitalFlName = folderpath + setDigitalPdfName + ".pdf";

                        var sigDetails = comndb.GetDigitalSignatureDetails(objSM.UserID);

                        float llx = 230;
                        float lly = 320;
                        float urx = 350;
                        float ury = 220;
                        Comman_Classes.DigitalCeritificateManager dcm = new Comman_Classes.DigitalCeritificateManager();
                        Comman_Classes.MetaData md = new Comman_Classes.MetaData()
                        {
                            Author = sigDetails.Author,
                            Title = "Age Certificate Authentication",
                            Subject = "Age Certificate",
                            Creator = sigDetails.Creator,
                            Producer = sigDetails.Producer,
                            Keywords = sigDetails.Keywords
                        };

                        string Signaturepath = Server.MapPath(sigDetails.Signaturepath);
                        dcm.signPDF(Server.MapPath(flName), Server.MapPath(digitalFlName), Signaturepath,
                         sigDetails.signpwd, "Authenticate Age Certificate", sigDetails.SigContact,
                         sigDetails.SigLocation, true, llx, lly, urx, ury, 1, md);

                        FileInfo fileInfo = new FileInfo(Server.MapPath(digitalFlName));
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + setDigitalPdfName + ".pdf");
                        Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                        Response.WriteFile(Server.MapPath(digitalFlName));
                        Response.Flush();
                        Response.Close();

                        if (System.IO.File.Exists(Server.MapPath(flName)))
                        {
                            System.IO.File.Delete(Server.MapPath(flName));
                        }

                        if (System.IO.File.Exists(Server.MapPath(digitalFlName)))
                        {
                            System.IO.File.Delete(Server.MapPath(digitalFlName));
                        }

                        //digital sign end
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error Occour to Downloading.";
                }
            }

            return View("DownloadFile");
        }

        private List<AGCReportModel> ConvertImageAGC(List<AGCReportModel> list)
        {
            foreach (var item in list)
            {

                imageConverter imgCon = new imageConverter();

                FileStream signfs2, signfs3;

                BinaryReader signbr2, signbr3;
                #region Photo
                String ImgPath = Server.MapPath(Convert.ToString(item.susphotoFile));
                if (!System.IO.File.Exists(ImgPath))
                {
                    ImgPath = Server.MapPath("~/Images/document.png");
                }

                signfs2 = new FileStream(ImgPath, FileMode.Open);

                try
                {
                    signbr2 = new BinaryReader(signfs2);
                    byte[] imgbyte2 = new byte[signfs2.Length + 1];
                    imgbyte2 = signbr2.ReadBytes(Convert.ToInt32((signfs2.Length)));
                    item.Photo = imgCon.ResizeImageFile(imgbyte2, 210);


                    signfs2.Close();
                    signbr2.Close();
                }
                catch { signfs2.Close(); }
                finally { signfs2.Close(); }

                #endregion
                #region Thumb Impression
                String ThumbPath = Server.MapPath(Convert.ToString(item.susThumbFile));
                if (!System.IO.File.Exists(ThumbPath))
                {
                    ThumbPath = Server.MapPath("~/Images/document.png");
                }

                signfs3 = new FileStream(ThumbPath, FileMode.Open);

                try
                {
                    signbr3 = new BinaryReader(signfs3);
                    byte[] imgbyte3 = new byte[signfs3.Length + 1];
                    imgbyte3 = signbr3.ReadBytes(Convert.ToInt32((signfs3.Length)));
                    item.Thumb = imgCon.ResizeImageFile(imgbyte3, 210);


                    signfs3.Close();
                    signbr3.Close();
                }
                catch { signfs3.Close(); }
                finally { signfs3.Close(); }

                #endregion
            }
            return list;
        }
        #endregion

        [AuthorizeAdmin(10)]
        public JsonResult UploadCertificateFileAGC(HttpPostedFileBase File)
        {
            long regisId = Convert.ToInt64(Session["registrationIdAGC"].ToString());
            var lstAGCDetails = objCMODB.GetAGCCertificate(regisId);

            string Dirpath = "~/Content/SignedCertificate/AGC/" + objSM.DistrictName + "/";
            string path = "";
            string filename = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            string ext = Path.GetExtension(File.FileName);
            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {
                filename = "Signed_" + lstAGCDetails.certificateNo + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";//"warning_Maximum 2MB file size are allowed";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";//"warning_Invalid File Format only pdf and jpg files are allow!";
            }

            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }

        [AuthorizeAdmin(10)]
        [HttpGet]
        public ActionResult UploadCertificateAGC(string regisId)
        {
            long regId = Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId));

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;

            var resultData = objCMODB.GetAllAGCList(regId).Where(m => (lstCMODistrict.Any(p => p.districtId == m.susdistrictId))).Count();

            if (resultData == 0)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            Session["registrationIdAGC"] = regId;

            return View();
        }

        [AuthorizeAdmin(10)]
        [HttpPost]
        public ActionResult UploadCertificateAGC(FormCollection frm)
        {
            var CertificatePath = frm.GetValues("hdnfileUploadCertificate");
            string IPAddress = Common.GetIPAddress();
            if (CertificatePath[0] != null || CertificatePath[0] != "")
            {
                var res = objCMODB.UpdateAGCCertificate(Convert.ToInt64(Session["registrationIdAGC"]), CertificatePath[0], objSM.UserID, IPAddress);
                if (res.Flag == 1)
                {
                    TempData["Message"] = res.Msg;
                }
                return RedirectToAction("ApprovedApplicationAGC", "CMO");
            }
            else
            {
                TempData["msg"] = "Please choose a file!";
                TempData["msgstatus"] = "warning";
                return RedirectToAction("UploadCertificateAGC");
            }
        }
        #endregion

        #region MER Riya
        [AuthorizeAdmin(8)]
        public ActionResult AppliedApplicationMER()
        {
            ProcessType model = new ProcessType();

            model = objCMODB.getApplicationCountMER(objSM.UserID, Session["RollID"].ToString());

            return View(model);
        }

        [AuthorizeAdmin(8)]
        public ActionResult ReceivedApplicationMER()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("MER").Where(m => m.Value == "-1").ToList();
            return View();
        }

        [AuthorizeAdmin(8)]
        public ActionResult ReceivedApplicationListMER(string registrationNo = "", string requestDate = "")
        {
            List<MERstatusUpdationModel> lstMERDetails = new List<MERstatusUpdationModel>();

            if (objSM.RollID == 8)
            {
                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == -1).ToList();
            }
            else
            {

                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == -1).ToList();


                if (!string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(requestDate))//111
                {

                    lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == -1 && m.registrationNo == registrationNo && m.appliedDate == requestDate).ToList();
                }
                else if (!string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(requestDate))//100
                {
                    lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.registrationNo == registrationNo && (m.appStatus == -1)).ToList();
                }

                else if (string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(requestDate))//001
                {
                    lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appliedDate == requestDate && (m.appStatus == -1)).ToList();
                }
            }



            return PartialView("_ReceivedApplicationListMER", lstMERDetails);

        }

        [AuthorizeAdmin(8)]
        [HttpGet]
        public ActionResult UpdateAppProcessMER(string regisId, int status)
        {
            MERstatusUpdationModel model = new MERstatusUpdationModel();
            regisId = OTPL_Imp.CustomCryptography.Decrypt(regisId);

            model = objCMODB.GetMERList(regisId, status);

            if (model == null)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }
            else if (objSM.RollID == 2 && objSM.districtId != model.postingDistrictId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }
            else if (objSM.RollID == 5 && objSM.UserID != (!string.IsNullOrEmpty(model.ForwardedToId) ? Convert.ToInt64(model.ForwardedToId) : 0))
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            ////////change
            if (status == 5)
            {
                MERModel modelBilll = new MERModel();
                //MER_DB objMECDB = new MER_DB();
                ViewBag.BillLis = objCMODB.getMERChild(Convert.ToInt64(model.regisIdMER));
                //ViewBag.BillList = appDB.getNUHdoctorList(Convert.ToInt64(regisId));
            }
            //change

            return View(model);
        }

        [AuthorizeAdmin(8)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAppProcessMER(MERstatusUpdationModel model, FormCollection form)//Update Status For FIC
        {
            AuditMethods objAud = new AuditMethods();
            string errormsg = "";
            bool valStatus = false;
            var result = objCMODB.GetMERList(model.registrationNo, model.appStatus);

            if (Session["PhotoPath"] != null)
            {
                string[] fileArray = Session["PhotoPath"] as string[];

                if (fileArray != null)
                {
                    model.inspReportFilePhotoPath = fileArray;
                }
            }

            ModelState["inspReportFilePhoto"].Errors.Clear();
            if (result == null)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }
            else if (objSM.RollID == 2 && objSM.districtId != result.postingDistrictId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }
            else if (objSM.RollID == 5 && objSM.UserID != (!string.IsNullOrEmpty(result.ForwardedToId) ? Convert.ToInt64(result.ForwardedToId) : 0))
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            if (!string.IsNullOrEmpty(model.inspectionRptPath))
            {

                valStatus = objAud.IsValidLink(model.inspectionRptPath, "Document File", out errormsg);
                if (!valStatus)
                {
                    setSweetAlertMsg(errormsg, "warning");
                    return View(model);
                }
            }


            model.regByUser = objSM.UserID;
            model.transIp = Common.GetIPAddress();
            if (model.appStatus == -1)//pending
            {
                if (model.rejectedRemarks != null)
                {
                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["inspectionRpt"].Errors.Clear();
                    ModelState["inspectionRejectedRemark"].Errors.Clear();
                    //ModelState["sancationAmount"].Errors.Clear();
                    //ModelState["sanctionDate"].Errors.Clear();

                    model.appStatus = 0;

                    var res = objCMODB.MERAppStatusInsertUpdate(model.regisIdMER, model.appStatus, model.rejectedRemarks, model.inspectionDate, model.departmentName, model.officerName, model.dateOfLetter, model.letterNo, model.inspectionRptPath, model.inspectionRejectedRemark, model.sancationAmount, model.sanctionDate, model.regByUser, model.transIp, "", "");
                    if (res != null)
                    {
                        setSweetAlertMsg(res.Msg.ToString(), "error");
                        TempData["msg"] = res.Msg.ToString();
                        TempData["msgstatus"] = "success";
                        SendSMSUpdateProcessMER(model.registrationNo, model.inspectionDate, res.MobileNo, model.appStatus);
                    }
                    return RedirectToAction("RejectedApplicationMER");
                }
                else
                {
                    ModelState["rejectedRemarks"].Errors.Clear();
                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["inspectionRpt"].Errors.Clear();
                    ModelState["inspectionRejectedRemark"].Errors.Clear();
                    //ModelState["sancationAmount"].Errors.Clear();
                    //ModelState["sanctionDate"].Errors.Clear();
                    model.appStatus = 1;

                    var res = objCMODB.MERAppStatusInsertUpdate(model.regisIdMER, model.appStatus, model.rejectedRemarks, model.inspectionDate, model.departmentName, model.officerName, model.dateOfLetter, model.letterNo, model.inspectionRptPath, model.inspectionRejectedRemark, model.sancationAmount, model.sanctionDate, model.regByUser, model.transIp, "", "");
                    if (res != null)
                    {
                        setSweetAlertMsg(res.Msg.ToString(), "error");
                        TempData["msg"] = res.Msg.ToString();
                        TempData["msgstatus"] = "success";

                    }
                    return RedirectToAction("InProcessApplicationMER");
                    //return RedirectToAction("UpdateAppProcessMER", new { regisId = @OTPL_Imp.CustomCryptography.Encrypt(model.registrationNo), status = model.appStatus });
                }
            }
            else if (model.appStatus == 1)//inspection schedule
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();
                ModelState["inspectionRejectedRemark"].Errors.Clear();
                //ModelState["sancationAmount"].Errors.Clear();
                //ModelState["sanctionDate"].Errors.Clear();
                model.appStatus = 3;
                ModelState["inspReportFilePhoto"].Errors.Clear();

                #region Vinod Bulkinsertion of Inspection Photo

                string XmlDataPIC = "";
                //int countPic = model.inspReportFilePhotoPath.Count();
                //XmlDataPIC = "<InspectionPICS>";
                ////  long regisByuser = objSM.UserID;
                //for (int i = 0; i < countPic; i++)
                //{
                //    if (model.inspReportFilePhotoPath[i] == "")
                //    {
                //        //XmlData = string.Empty;
                //    }
                //    else
                //    {

                //        XmlDataPIC += "<InspectionPIC><UploadPICFilePath>" + model.inspReportFilePhotoPath[i] + "</UploadPICFilePath></InspectionPIC>";
                //    }

                //}
                //XmlDataPIC += "</InspectionPICS>";
                model.XmlDataPhoto = XmlDataPIC;

                #endregion

                var res = objCMODB.MERAppStatusInsertUpdate(model.regisIdMER, model.appStatus, model.rejectedRemarks, model.inspectionDate, model.departmentName, model.officerName, model.dateOfLetter, model.letterNo, model.inspectionRptPath, model.inspectionRejectedRemark, model.sancationAmount, model.sanctionDate, model.regByUser, model.transIp, "", model.XmlDataPhoto);
                if (res != null)
                {
                    setSweetAlertMsg(res.Msg.ToString(), "error");
                    TempData["msg"] = res.Msg.ToString();
                    TempData["msgstatus"] = "success";
                }
                return RedirectToAction("UpdateAppProcessMER", new { regisId = @OTPL_Imp.CustomCryptography.Encrypt(model.registrationNo), status = model.appStatus });
            }
            else if (model.appStatus == 3)//inspection rpt uploaded
            {
                if (model.inspectionRptStatus == true)//POSITIVE
                {
                    ModelState["rejectedRemarks"].Errors.Clear();
                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["inspectionRpt"].Errors.Clear();
                    ModelState["inspectionRejectedRemark"].Errors.Clear();
                    ModelState["sancationAmount"].Errors.Clear();
                    ModelState["sanctionDate"].Errors.Clear();
                    model.appStatus = 5;//POSITIVE


                    var res = objCMODB.MERAppStatusInsertUpdate(model.regisIdMER, model.appStatus, model.rejectedRemarks, model.inspectionDate, model.departmentName, model.officerName, model.dateOfLetter, model.letterNo, model.inspectionRptPath, model.inspectionRejectedRemark, model.sancationAmount, model.sanctionDate, model.regByUser, model.transIp, "", "");
                    if (res != null)
                    {
                        setSweetAlertMsg(res.Msg.ToString(), "error");
                        TempData["msg"] = res.Msg.ToString();
                        TempData["msgstatus"] = "success";

                    }
                    return RedirectToAction("UpdateAppProcessMER", new { regisId = @OTPL_Imp.CustomCryptography.Encrypt(model.registrationNo), status = model.appStatus });
                }
                else
                {
                    if (model.inspectionRejectedRemark != null)
                    {
                        ModelState["rejectedRemarks"].Errors.Clear();
                        ModelState["inspectionDate"].Errors.Clear();
                        ModelState["inspectionRpt"].Errors.Clear();
                        ModelState["sancationAmount"].Errors.Clear();
                        ModelState["sanctionDate"].Errors.Clear();
                        model.appStatus = 4;//reject

                        var res = objCMODB.MERAppStatusInsertUpdate(model.regisIdMER, model.appStatus, model.rejectedRemarks, model.inspectionDate, model.departmentName, model.officerName, model.dateOfLetter, model.letterNo, model.inspectionRptPath, model.inspectionRejectedRemark, model.sancationAmount, model.sanctionDate, model.regByUser, model.transIp, "", "");
                        if (res != null)
                        {
                            setSweetAlertMsg(res.Msg.ToString(), "error");
                            TempData["msg"] = res.Msg.ToString();
                            TempData["msgstatus"] = "success";
                            SendSMSUpdateProcessMER(model.registrationNo, model.inspectionDate, res.MobileNo, model.appStatus);
                        }
                        return RedirectToAction("RejectedApplicationMER");
                    }
                }
            }
            else if (model.appStatus == 5)//inspection rpt uploaded
            {

                ///////////change param
                #region Bulk Insertion doctor
                var billidentity = form.GetValues("billidentity");
                var billSactionAmount = form.GetValues("billSactionAmount");
                int count1 = billidentity.Count();
                string XmlDataDoctor = "<root>";
                for (int i = 0; i < count1; i++)
                {
                    if (billidentity[i].ToString() == "")
                    {
                        //XmlDataParamedical = string.Empty;
                    }
                    else
                    {
                        XmlDataDoctor += "<Bill><billidentity>" + billidentity[i] + "</billidentity>"
                            + "<billSactionAmount>" + (billSactionAmount[i] == "" ? "0" : billSactionAmount[i]) + "</billSactionAmount>"
                             + "</Bill>";
                    }
                }
                XmlDataDoctor += "</root>";
                model.xmlBillSanction = XmlDataDoctor;
                #endregion
                /////change param

                if (model.sancationAmount != 0 && model.sanctionDate != "")
                {
                    ModelState["rejectedRemarks"].Errors.Clear();
                    ModelState["inspectionDate"].Errors.Clear();
                    ModelState["inspectionRpt"].Errors.Clear();
                    ModelState["inspectionRejectedRemark"].Errors.Clear();
                    model.appStatus = 6;

                    var res = objCMODB.MERAppStatusInsertUpdate(model.regisIdMER, model.appStatus, model.rejectedRemarks, model.inspectionDate, model.departmentName, model.officerName, model.dateOfLetter, model.letterNo, model.inspectionRptPath, model.inspectionRejectedRemark, model.sancationAmount, model.sanctionDate, model.regByUser, model.transIp, model.xmlBillSanction, "");
                    if (res != null)
                    {
                        setSweetAlertMsg(res.Msg.ToString(), "error");
                        TempData["msg"] = res.Msg.ToString();
                        TempData["msgstatus"] = "success";
                    }
                    return RedirectToAction("UpdateAppProcessMER", new { regisId = @OTPL_Imp.CustomCryptography.Encrypt(model.registrationNo), status = model.appStatus });
                }
            }
            else if (model.appStatus == 6)//inspection rpt uploaded
            {
                ModelState["rejectedRemarks"].Errors.Clear();
                ModelState["inspectionDate"].Errors.Clear();
                ModelState["inspectionRpt"].Errors.Clear();
                ModelState["inspectionRejectedRemark"].Errors.Clear();
                model.appStatus = 7;//Certificate genrate

                var res = objCMODB.MERAppStatusInsertUpdate(model.regisIdMER, model.appStatus, model.rejectedRemarks, model.inspectionDate, model.departmentName, model.officerName, model.dateOfLetter, model.letterNo, model.inspectionRptPath, model.inspectionRejectedRemark, model.sancationAmount, model.sanctionDate, model.regByUser, model.transIp, "", "");
                if (res != null)
                {
                    setSweetAlertMsg(res.Msg.ToString(), "error");
                    TempData["msg"] = res.Msg.ToString();
                    TempData["msgstatus"] = "success";
                    TempData["DatSetMER"] = res.RegisId;
                    SendSMSUpdateProcessMER(model.registrationNo, model.inspectionDate, res.MobileNo, model.appStatus);
                }
                return RedirectToAction("UpdateAppProcessMER", new { regisId = @OTPL_Imp.CustomCryptography.Encrypt(model.registrationNo), status = model.appStatus });
                // return RedirectToAction("ApprovedApplicationMER");
            }
            return View();
        }

        [AuthorizeAdmin(8)]
        public ActionResult InProcessApplicationMER()//All In process Application
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("MER").Where(m => m.Value == "1" || m.Value == "3" || m.Value == "5" || m.Value == "6").ToList();
            return View();
        }

        [AuthorizeAdmin(8)]
        public ActionResult InProcessApplicationListMER(string registrationNo = "", string status = "", string requestDate = "")//LIst Of all In Process Application
        {
            int intStatus = 0;
            List<MERstatusUpdationModel> lstMERDetails = new List<MERstatusUpdationModel>();
            int institutionTypeId = Convert.ToInt32(objSM.RollID);
            lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == 1 || m.appStatus == 3 || m.appStatus == 5 || m.appStatus == 6).ToList();

            if (!string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(requestDate))//111
            {
                intStatus = Convert.ToInt32(status);
                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == intStatus && m.registrationNo == registrationNo && m.appliedDate == requestDate).ToList();
            }
            else if (!string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(status) && string.IsNullOrEmpty(requestDate))//100
            {
                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.registrationNo == registrationNo && (m.appStatus == 1 || m.appStatus == 3 || m.appStatus == 5 || m.appStatus == 6)).ToList();
            }
            else if (string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status) && string.IsNullOrEmpty(requestDate))//010
            {
                intStatus = Convert.ToInt32(status);
                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == intStatus).ToList();
            }
            else if (string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(requestDate))//001
            {

                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appliedDate == requestDate && (m.appStatus == 1 || m.appStatus == 3 || m.appStatus == 5 || m.appStatus == 6)).ToList();
            }
            else if (!string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status) && string.IsNullOrEmpty(requestDate))//110
            {
                intStatus = Convert.ToInt32(status);
                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.registrationNo == registrationNo && m.appStatus == intStatus).ToList();
            }
            else if (string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(requestDate))//011
            {
                intStatus = Convert.ToInt32(status);
                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == intStatus && m.appliedDate == requestDate).ToList();
            }
            else if (!string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(requestDate))//101
            {

                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.registrationNo == registrationNo && m.appliedDate == requestDate && (m.appStatus == 1 || m.appStatus == 3 || m.appStatus == 5 || m.appStatus == 6)).ToList();
            }
            //if (!string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status))
            //{
            //    intStatus = Convert.ToInt32(status);
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.appStatus == intStatus && m.registrationNo == registrationNo).ToList();
            //}
            //else if (!string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(status))
            //{
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.registrationNo == registrationNo).ToList();
            //}
            //else if (string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status))
            //{
            //    intStatus = Convert.ToInt32(status);
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.appStatus == intStatus).ToList();
            //}

            return PartialView("_InProcessApplicationListMER", lstMERDetails);
        }

        [AuthorizeAdmin(8)]
        public ActionResult ApprovedApplicationMER()//All Approved Application
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("MER").Where(m => m.Value == "7").ToList();
            return View();
        }

        [AuthorizeAdmin(8)]
        public ActionResult ApprovedApplicationListMER(string registrationNo = "", string requestDate = "")//LIst Of Approved Application
        {
            int intStatus = 0;
            List<MERstatusUpdationModel> lstMERDetails = new List<MERstatusUpdationModel>();
            int institutionTypeId = Convert.ToInt32(objSM.RollID);
            lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == 7).ToList();

            if (!string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(requestDate))//11
            {

                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.registrationNo == registrationNo && m.appliedDate == requestDate && (m.appStatus == 7)).ToList();
            }
            else if (!string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(requestDate))//10
            {
                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.registrationNo == registrationNo && (m.appStatus == 7)).ToList();
            }
            else if (string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(requestDate))//01
            {

                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appliedDate == requestDate && (m.appStatus == 7)).ToList();
            }
            //if (!string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status))
            //{
            //    intStatus = Convert.ToInt32(status);
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.appStatus == intStatus && m.registrationNo == registrationNo).ToList();
            //}
            //else if (!string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(status))
            //{
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.registrationNo == registrationNo).ToList();
            //}
            //else if (string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status))
            //{
            //    intStatus = Convert.ToInt32(status);
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.appStatus == intStatus).ToList();
            //}

            return PartialView("_ApprovedApplicationListMER", lstMERDetails);
        }

        [AuthorizeAdmin(8)]
        public ActionResult RejectedApplicationMER()//All Rejected Application
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("MER").Where(m => m.Value == "0" || m.Value == "4").ToList();
            return View();
        }

        [AuthorizeAdmin(8)]
        public ActionResult RejectedApplicationListMER(string registrationNo = "", string requestDate = "")//LIst Of Rejected Application
        {
            int intStatus = 0;
            List<MERstatusUpdationModel> lstMERDetails = new List<MERstatusUpdationModel>();
            int institutionTypeId = Convert.ToInt32(objSM.RollID);
            lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appStatus == 0 || m.appStatus == 4).ToList();

            if (!string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(requestDate))//11
            {

                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.registrationNo == registrationNo && m.appliedDate == requestDate && (m.appStatus == 0 || m.appStatus == 4)).ToList();
            }
            else if (!string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(requestDate))//10
            {
                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.registrationNo == registrationNo && (m.appStatus == 0 || m.appStatus == 4)).ToList();
            }
            else if (string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(requestDate))//01
            {

                lstMERDetails = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID)).Where(m => m.appliedDate == requestDate && (m.appStatus == 0 || m.appStatus == 4)).ToList();
            }
            //if (!string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status))
            //{
            //    intStatus = Convert.ToInt32(status);
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.appStatus == intStatus && m.registrationNo == registrationNo).ToList();
            //}
            //else if (!string.IsNullOrEmpty(registrationNo) && string.IsNullOrEmpty(status))
            //{
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.registrationNo == registrationNo).ToList();
            //}
            //else if (string.IsNullOrEmpty(registrationNo) && !string.IsNullOrEmpty(status))
            //{
            //    intStatus = Convert.ToInt32(status);
            //    lstMERDetails = objCMODB.GetAllMERList().Where(m => m.appStatus == intStatus).ToList();
            //}

            return PartialView("_RejectedApplicationListMER", lstMERDetails);
        }

        [AuthorizeAdmin(8)]
        public ActionResult AgeCertificateDetailMER(string registration)//list of All Received Application
        {
            AGCModel model = new AGCModel();
            model = objCMODB.GetAGCListBYRegistrationNo(registration);

            return PartialView("_AgeCertificateDetailMER", model);
        }

        public string SendSMSUpdateProcessMER(string registrationNo, string inspectionDate, string mobileNo, int msgType)
        {
            string res = "";
            string txtmsg = "";
            string CMO = objSM.DisplayName;
            if (mobileNo != "")
            {
                ForgotPasswordModel otpChCount = new ForgotPasswordModel();

                if (registrationNo != null)
                {
                    if (msgType == 0 || msgType == 4)
                    {
                       // txtmsg = "Dear Citizen,\n\nYour Application Form Number " + registrationNo + " for Request for Payment of Medical Reimbursement has been rejected. Kindly get in touch with the office of Community Health Centre/ District Hospital for more details. You can also login to your dashboard for more details.\n\n" + CMO + "\n MHFWD, UP";
                        txtmsg = "Dear Citizen,Your Application Form Number " + registrationNo + " for Request for Payment of Medical Reimbursement has been rejected. Kindly get in touch with the office of Community Health Centre/ District Hospital for more details. You can also login to your dashboard for more details." + CMO + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007414215678924991";
                    }
                    else if (msgType == 1)
                        {
                            //txtmsg = "Dear User, your Application " + registrationNo + " has been Approved. For more informnation please login to your account.";
                        }
                    else if (msgType == 2)
                        {
                            //txtmsg = "Dear Citizen,\n\nAs per your application for Request for Payment of Medical Reimbursement, a committee for Inspection has been scheduled. We request to kindly be present at Office of Community Health Centre /District Hospital on the inspection date " + inspectionDate + " and coordinate accordingly.\n\n" + CMO + "\n MHFWD, UP";

                            txtmsg = "Dear Citizen,As per your application for Request for Payment of Medical Reimbursement, a committee for Inspection has been scheduled. We request to kindly be present at Office of Community Health Centre /District Hospital on the inspection date " + inspectionDate + " and coordinate accordingly." + CMO + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007889641787822926";
                    }
                        else if (msgType == 5)
                            {
                               // txtmsg = "Dear User, your Inspection Report for application number " + registrationNo + " has been approved. For more informnation please login to your account.";
                                txtmsg = "Dear User, your Inspection Report for application number " + registrationNo + " has been approved. For more informnation please login to your account.-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007478200045842353";
                            }
                        else if (msgType == 7)
                            {
                               // txtmsg = "Dear Citizen,\n\nYour Application Form Number " + registrationNo + " for Request for Payment of Medical Reimbursement has been approved. Please get in touch with the office of Community Health Centre/ District Hospital for more details. You can also login to your dashboard for more details.\n\n" + CMO + "\n MHFWD, UP";
                                txtmsg = "Dear Citizen,Your Application Form Number " + registrationNo + " for Request for Payment of Medical Reimbursement has been approved. Please get in touch with the office of Community Health Centre/ District Hospital for more details. You can also login to your dashboard for more details." + CMO + "-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007846853614712835";
                        }
                    string status = SMS.SMSSend(txtmsg, Convert.ToString(mobileNo));

                    objAccDb.SMSLog(txtmsg, mobileNo, status, registrationNo);
                    if (status.ToLower() == "success")
                    {
                        res = "S";
                        try
                        {

                        }
                        catch { }
                        objSM.isMSGSend = true;
                    }
                    else
                    {
                        res = "SMS Service is not working currently. please try again later. ! ";
                        objSM.isMSGSend = false;
                    }
                }
                else
                {
                    res = "OCE";
                    objSM.IsMaxLimit = true;
                    objSM.isMSGSend = false;
                }
            }
            else
            {
                res = "Current Session is terminated continue to login and try again...!";
                objSM.isMSGSend = false;
            }
            return res;
        }

        [AuthorizeAdmin(8)]
        public ActionResult MedicalReimbursementMER(string registration)//list of All Received Application
        {
            MERModel model = new MERModel();
            model = objMER_DB.getMERByRegistration(Convert.ToInt64(registration));
            model.MERModelList = objMER_DB.getMERChild(model.regisIdMER);
            return PartialView("_MedicalReimbursementMER", model);
        }

        [AuthorizeAdmin(8)]
        public JsonResult UploadFileMER(HttpPostedFileBase File)
        {
            string Dirpath = "~/Content/writereaddata/MER/InspectionReport/";
            string path = "";
            string filename = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            string ext = Path.GetExtension(File.FileName);
            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {

                filename = Path.GetFileNameWithoutExtension(File.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "warning_Maximum 2MB file size are allowed";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "warning_Invalid File Format only pdf and jpg files are allow!";
            }

            List<string> plist = new List<string> { File.FileName, path };
            return Json(plist);


        }

        public ActionResult PrintApplicationFormMER(string regisId)
        {
            regisId = OTPL_Imp.CustomCryptography.Decrypt(regisId);
            Session["regisIdMER"] = regisId;
            MERModel model = new MERModel();
            model = objMER_DB.getMERByRegistration(Convert.ToInt64(regisId));

            if (objSM.RollID == 20)
            {
                if (model == null || objSM.districtId != model.postingDistrictId)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID >= 2 && objSM.RollID <= 5)
            {
                if (model == null || objSM.districtId != model.postingDistrictId || !(objSM.RollID != 2 ? objSM.UserID == Convert.ToInt64(model.ForwardedToId) : model.TotalBillAmount >= 50000))
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID == 18)
            {
                var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
                int count = lstCMODistrict.Where(e => e.districtId == model.postingDistrictId).ToList().Count();
                if (count == 0)
                {
                    return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
                }
            }
            else if (objSM.RollID != 8)
            {
                return RedirectToAction("UnauthoriseAcess", "Home", new { isWithoutLayout = true });
            }

            return View(model);
        }

        [AuthorizeAdmin(8)]
        public ActionResult BindScheduleOfCommitteeMER(string inspectiondate = "")
        {
            if (inspectiondate != "")
            {
                var lstScheduleInspection = objCMODB.GetScheduleOfinspectonMER(inspectiondate).ToList(); //objCMODB.GetScheduleOfCommittee(committeeId).ToList();
                return PartialView("_BindScheduleOfCommitteeMER", lstScheduleInspection);
            }
            else
            {
                return Content("NF");
            }
        }

        #region Generate Certificate MER
        [AuthorizeAdmin(8)]
        public ActionResult MERgeneratedCertificate(long regisId)
        {
            string setPdfName = "", setDigitalPdfName = "";

            var res = objCMODB.GetMERdetailCertificateRpt(regisId);

            if (res == null)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }
            else if (objSM.RollID == 2 && objSM.districtId != res[0].postingDistrictId)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }
            else if (objSM.RollID == 5 && objSM.UserID != (!string.IsNullOrEmpty(res[0].ForwardedToId) ? Convert.ToInt64(res[0].ForwardedToId) : 0))
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            if (res != null && res.Count > 0)
            {
                try
                {
                    ReportDocument rd = new ReportDocument();

                    rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rptCertificateMER.rpt"));
                    rd.SetDataSource(res);
                    rd.SetParameterValue("districtName", objSM.DistrictNameHi);

                    setPdfName = "UnSigned_" + res[0].certificateNo;

                    string folderpath = "~/Content/writereaddata/UnSignedCertificate/MER/" + objSM.DistrictName + "/";

                    if (!System.IO.Directory.Exists(Server.MapPath(folderpath)))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(folderpath));
                    }

                    string flName = folderpath + setPdfName + ".pdf";

                    rd.ExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                    rd.ExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                    rd.ExportOptions.DestinationOptions = new DiskFileDestinationOptions();
                    ((DiskFileDestinationOptions)rd.ExportOptions.DestinationOptions).DiskFileName = Server.MapPath(flName);
                    rd.Export();
                    if (System.IO.File.Exists(Server.MapPath(flName)))
                    {
                        int result = objCMODB.InsertUnSignedCertiPath_MER(res[0].regisIdMER, flName);
                    }
                    rd.Close();
                    rd.Dispose();
                    if (ConfigurationManager.AppSettings["IsDigitalSign"].ToString() == "N")
                    {
                        FileInfo fileInfo = new FileInfo(Server.MapPath(flName));
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + setPdfName + ".pdf");
                        Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                        Response.WriteFile(flName);
                        Response.Flush();
                        Response.Close();
                    }
                    else
                    {
                        //digital sign

                        setDigitalPdfName = "MedicalReimbursementCertificateDigitalSigned" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                        string digitalFlName = folderpath + setDigitalPdfName + ".pdf";

                        var sigDetails = comndb.GetDigitalSignatureDetails(objSM.UserID);

                        float llx = 570;
                        float lly = 320;
                        float urx = 450;
                        float ury = 220;
                        Comman_Classes.DigitalCeritificateManager dcm = new Comman_Classes.DigitalCeritificateManager();
                        Comman_Classes.MetaData md = new Comman_Classes.MetaData()
                        {
                            Author = sigDetails.Author,
                            Title = "Medical Reimbursement Certificate Authentication",
                            Subject = "Medical Reimbursement Certificate",
                            Creator = sigDetails.Creator,
                            Producer = sigDetails.Producer,
                            Keywords = sigDetails.Keywords
                        };

                        string Signaturepath = Server.MapPath(sigDetails.Signaturepath);
                        dcm.signPDF(Server.MapPath(flName), Server.MapPath(digitalFlName), Signaturepath,
                         sigDetails.signpwd, "Authenticate Medical Reimbursement Certificate", sigDetails.SigContact,
                         sigDetails.SigLocation, true, llx, lly, urx, ury, 1, md);

                        FileInfo fileInfo = new FileInfo(Server.MapPath(digitalFlName));
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + setDigitalPdfName + ".pdf");
                        Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                        Response.WriteFile(Server.MapPath(digitalFlName));
                        Response.Flush();
                        Response.Close();

                        if (System.IO.File.Exists(Server.MapPath(flName)))
                        {
                            System.IO.File.Delete(Server.MapPath(flName));
                        }

                        if (System.IO.File.Exists(Server.MapPath(digitalFlName)))
                        {
                            System.IO.File.Delete(Server.MapPath(digitalFlName));
                        }
                        //digital sign end
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error Occour to Downloading.";
                }
            }

            return View("DownloadFile");
        }
        #endregion

        [AuthorizeAdmin(8)]
        public JsonResult UploadCertificateFileMER(HttpPostedFileBase File)
        {
            long regisId = Convert.ToInt64(Session["registrationIdAGC"].ToString());
            var lstMERDetails = objCMODB.GetMERCertificate(regisId);

            string Dirpath = "~/Content/SignedCertificate/MER/" + objSM.DistrictName + "/";
            string path = "";
            string filename = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            string ext = Path.GetExtension(File.FileName);
            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {

                filename = "Signed_" + lstMERDetails + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";//"warning_Maximum 2MB file size are allowed";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";//"warning_Invalid File Format only pdf and jpg files are allow!";
            }

            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }

        [AuthorizeAdmin(8)]
        [HttpGet]
        public ActionResult UploadCertificateMER(string regisId)
        {
            long regId = Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId));

            var resultData = objCMODB.GetAllMERList(objSM.UserID, Convert.ToString(objSM.RollID), regId).Count();

            if (resultData == 0)
            {
                return RedirectToAction("UnauthoriseAcess", "Home");
            }

            Session["registrationIdAGC"] = regId;

            return View();
        }

        [AuthorizeAdmin(8)]
        [HttpPost]
        public ActionResult UploadCertificateMER(FormCollection frm)
        {
            var CertificatePath = frm.GetValues("hdnfileUploadCertificate");
            string IPAddress = Common.GetIPAddress();
            if (CertificatePath[0] != null || CertificatePath[0] != "")
            {
                var res = objCMODB.UpdateMERCertificate(Convert.ToInt64(Session["registrationIdAGC"]), CertificatePath[0], objSM.UserID, IPAddress);
                if (res.Flag == 1)
                {
                    TempData["Message"] = res.Msg;
                }
                return RedirectToAction("ApprovedApplicationMER", "CMO");
            }
            else
            {
                TempData["msg"] = "Please choose a file!";
                TempData["msgstatus"] = "warning";
                return RedirectToAction("UploadCertificateMER");
            }
        }
        #endregion

        #region Method Download File By Path
        [AuthorizeAdmin]
        public void DownloadFileByPath(string filePath)
        {
            string decrypFilePath = OTPL_Imp.CustomCryptography.Decrypt(Server.UrlDecode(filePath));

            if (System.IO.File.Exists(Server.MapPath(decrypFilePath)))
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(decrypFilePath));
                Response.ClearContent();
                Response.ClearHeaders();
                Response.ContentType = "application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(decrypFilePath));
                Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                Response.WriteFile(decrypFilePath);
                Response.Flush();
                Response.Close();
            }
        }
        #endregion

        [AuthorizeAdmin]
        public ActionResult BindChildList()
        {
            MERModel model = new MERModel();
            model.MERModelList = objCMODB.getMERChild(Convert.ToInt64(Session["regisIdMER"].ToString()));
            return PartialView("_ViewMERChild", model.MERModelList);
        }

        [AuthorizeAdmin]
        public ActionResult ViewApplicationFAP()
        {
            ViewBag.DLLAppStatus = comndb.GetApplicationProcessByAppName("FAP").ToList();
            return View();
        }

        [AuthorizeAdmin]
        public ActionResult ViewApplicationListFAP(string registrationNo = "", string mobile = "", string requestDate = "", string status = "")
        {
            int intStatus = !string.IsNullOrEmpty(status) ? Convert.ToInt32(status) : 0;

            var lstFAPDetails = objCMODB.GetAllRegistrationFAP().Where(m => (m.registrationNo == registrationNo || registrationNo == "") && (m.claimantMobileNo == mobile || mobile == "") && (m.requestDate == requestDate || requestDate == "") && (m.appStatus.ToString() == status || status == "")).ToList();

            return PartialView("_ViewApplicationListFAP", lstFAPDetails);
        }

        #region
        [AuthorizeAdmin(1)]
        [HttpGet]
        public ActionResult CancleRegistationNUH(string regisIdNUH)
        {
            CancleNUHregistration model = new CancleNUHregistration();
            model.hdnNUHId = OTPL_Imp.CustomCryptography.Decrypt(regisIdNUH);
            return View(model);
        }

        [AuthorizeAdmin(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancleRegistationNUH(CancleNUHregistration model)
        {
            model.Ip = Common.GetIPAddress();
            model.UserID = objSM.UserID;
            model = objCMODB.CancleNUHApplicationForCMO(model);
            if (model.IsSuccess == 1)
            {
                TempData["msg"] = "Application Canceled Successfully.";
                TempData["msgstatus"] = "success";
            }
            else
            {
                TempData["msg"] = "Somthing Went Wrong !";
                TempData["msgstatus"] = "info";
            }
            return View();
        }

        [AuthorizeAdmin(1)]
        public JsonResult CancleUploadCertificateFileNUH(HttpPostedFileBase File)
        {

            string Dirpath = "~/Content/CancleRegistration/NUH/" + objSM.DistrictName + "/";
            string path = "";
            string filename = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            string ext = Path.GetExtension(File.FileName);
            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {

                filename = "CancleFile_" + DateTime.Now.Ticks + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";//"warning_Maximum 2MB file size are allowed";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";//"warning_Invalid File Format only pdf and jpg files are allow!";
            }

            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }
        #endregion




        #region New Module Hospital Investigation Add 09-01-2024


        [AuthorizeAdmin(1)]
        public ActionResult AppliedInspectionReport()
        {
            InspectionType model = new InspectionType();
            model = objCMODB.getMethodApplicationCountInspection(objSM.UserID);
            model.DistrictList = objCMODB.GetDistrictList(7, 34);
            return View(model);

        }

        [HttpGet]
        [AuthorizeAdmin(1)]
        public ActionResult CMOInvestigationReport()
        {

            return View();

        }

        [HttpPost]
        [AuthorizeAdmin(1)]
        public ActionResult CMOInvestigationReport(IPCApplicationform model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.CreateBy = objSM.UserID.ToString();
                    model.DistrictId = objSM.districtId;
                    ResultSet result = objCMODB.InsertIPCApplicationDetail(model);
                    if (result.StatusMessage == "Success")
                    {
                        ModelState.Clear();
                        setSweetAlertMsg("Data Insert Success !", "Success");
                    }

                }
                else
                {
                    setSweetAlertMsg("Please Fill all field", "warning");
                }
            }
            catch
            {
            }
            return View();

        }

        public JsonResult UploadFileIPC(HttpPostedFileBase File)
        {
            string Dirpath = "~/Content/writereaddata/IPC/InspectionReport/";
            string path = "";
            string filename = "";

            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }

            string ext = Path.GetExtension(File.FileName);

            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".pdf")
            {
                filename = Path.GetFileNameWithoutExtension(File.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ext;
                string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                if (System.IO.File.Exists(completepath))
                {
                    System.IO.File.Delete(completepath);
                }

                long size = File.ContentLength;
                if (size > 2097152)
                {
                    path = "SNV";
                }
                else
                {
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                }
            }
            else
            {
                path = "TNV";
            }

            List<string> plist = new List<string> { filename, path };
            return Json(plist);
        }
        //public JsonResult UploadFileIPC(HttpPostedFileBase File)
        //{
        //    int Year = DateTime.Now.Year;
        //    int month = DateTime.Now.Month;
        //    string Dirpath = "~/content/UploadImg/" + Year + "/" + month + "/";


        //    string path = "";
        //    string filename = "";

        //    string ext = Path.GetExtension(File.FileName);

        //    if (ext.ToLower() == ".jpg" || ext.ToLower() == ".png" || ext.ToLower() == ".jpeg" || ext.ToLower() == ".pdf")
        //    {


        //        filename = Path.GetFileNameWithoutExtension(File.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ext;
        //        string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
        //        if (System.IO.File.Exists(completepath))
        //        {
        //            System.IO.File.Delete(completepath);
        //        }

        //        long size = File.ContentLength;
        //        if (size > 50097152)
        //        {
        //            path = "SNV";
        //        }
        //        else
        //        {
        //            File.SaveAs(completepath);
        //            path = Dirpath + filename;


        //            ViewBag.PhotosPath = path;
        //        }
        //    }
        //    else
        //    {
        //        path = "TNV";
        //    }
        //    List<string> plist = new List<string> { filename, path };
        //    return Json(plist);

        //}


        #endregion End New Module Hospital Investigation Add 09-01-2024


        #region Upload Image 49 beds Aniket

        [AuthorizeAdmin(1)]
        public ActionResult UploadImageApplicationNUH()
        {
            return View();
        }

        [AuthorizeAdmin(1)]
        public ActionResult UploadImageApplicationListNUH(string registrationNo = "", string appDate = "", string uploadStatus = "0")
        {
            List<NUHDetailsModel> lstNUHDetails = new List<NUHDetailsModel>();

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (TempData["fDate"] == null && TempData["tDate"] == null)
            {
                TempData["fDate"] = "";
                TempData["tDate"] = "";
            }
            if (Convert.ToInt32(TempData["District"]) == 0)
            {
                TempData["District"] = 0;
            }
            if (objSM.RollID == 8)
            {
                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString(), Convert.ToInt32(TempData["District"])).Where(m => m.appStatus == 6 && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "")).ToList();
            }
            else
            {
                lstNUHDetails = objCMODB.GetAllNUHListForCMO_ImageUpload(0, TempData["fDate"].ToString(), TempData["tDate"].ToString(), 0, uploadStatus).Where(m => m.appStatus == 6 && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "") && (lstCMODistrict.Any(p => p.districtId == m.districtid))).ToList();
            }
            return PartialView("_UploadImageApplicationListNUH", lstNUHDetails);
        }


        public static string ValidateOnlyImage_FileExtWithSize(HttpPostedFileBase Uploadedfile, int ImageSize)
        {
            string massage;
            String fn = Path.GetFileNameWithoutExtension(Uploadedfile.FileName);
            String ext = Path.GetExtension(Uploadedfile.FileName);
            char[] SpecialChars = "!@#$%^&*()+=~`\\|/?><,\"".ToCharArray();
            int indexOf = fn.IndexOfAny(SpecialChars);
            String fileName = fn;
            int count = fileName.Split('.').Length - 1;
            if (count > 1)
            {
                massage = "Double extension not allowed in File name";
            }
            else
            {
                if (indexOf != -1)
                {
                    massage = "Special character not allowed in File name/फ़ाइल के नाम में विशेष वर्ण नहीं होने चाहिए।’";
                }
                else
                {
                    string mimetype = Uploadedfile.ContentType;
                    if ((ext.ToLower() == ".jpg" || ext.ToLower() == ".jpeg") && (mimetype == "image/jpeg" || mimetype == "image/jpg"))
                    {
                        fn = "";
                        string fFullName = Uploadedfile.FileName;
                        int len = fFullName.Length;
                        string ext1 = Path.GetExtension(fFullName);
                        string str = fFullName.Substring(fFullName.LastIndexOf("\\") + 1);
                        len = str.Length;
                        string fileN = str.Substring(0, len - ext1.Length);

                        Regex FilenameRegex = null;
                        FilenameRegex = new Regex("(.*?)\\.(jpeg|jpg|JPEG|JPG)$", RegexOptions.IgnoreCase);
                        int index = fileN.IndexOf(".");

                        if (!FilenameRegex.IsMatch(fFullName) || index != -1)
                        {
                            massage = "Please upload .jpg, .jpeg  File only";
                        }
                        else
                        {
                            string Photoname = Path.GetFileNameWithoutExtension(Uploadedfile.FileName);
                            string fileSize = Uploadedfile.ContentLength.ToString();
                            String ImageFileName = Uploadedfile.FileName;
                            Byte[] stu_image = new Byte[Uploadedfile.ContentLength];
                            Stream fs = Uploadedfile.InputStream;
                            fs.Read(stu_image, 0, Convert.ToInt32(fileSize));
                            fs.Seek(0, SeekOrigin.Begin);
                            StreamReader sr = new StreamReader(fs, true);
                            string firstLine = sr.ReadLine().ToString();
                            if ((firstLine.IndexOf("JFIF") > -1) || (firstLine.IndexOf("Exif") > -1))
                            {
                                if (Uploadedfile.ContentLength <= ImageSize)
                                {
                                    massage = "Valid";
                                }
                                else
                                {
                                    massage = "File size can not exceed " + (ImageSize / 1000) + " KB ";
                                }
                            }
                            else
                            {
                                massage = "Please upload .jpg, .jpeg File only";
                            }
                        }
                    }
                    else if (ext.ToLower() == ".png" && mimetype == "image/png")
                    {
                        fn = "";

                        string fFullName = Uploadedfile.FileName;
                        int len = fFullName.Length;
                        string ext1 = Path.GetExtension(fFullName);
                        string str = fFullName.Substring(fFullName.LastIndexOf("\\") + 1);
                        len = str.Length;
                        string fileN = str.Substring(0, len - ext1.Length);

                        Regex FilenameRegex = null;
                        FilenameRegex = new Regex("(.*?)\\.(png|PNG)$", RegexOptions.IgnoreCase);
                        int index = fileN.IndexOf(".");

                        if (!FilenameRegex.IsMatch(fFullName) || index != -1)
                        {
                            massage = "Please upload .pdf, .JPG, .jpeg, .png File only";
                        }
                        else
                        {
                            string Photoname = Path.GetFileNameWithoutExtension(Uploadedfile.FileName);
                            string fileSize = Uploadedfile.ContentLength.ToString();
                            String ImageFileName = Uploadedfile.FileName;
                            Byte[] stu_image = new Byte[Uploadedfile.ContentLength];
                            Stream fs = Uploadedfile.InputStream;
                            fs.Read(stu_image, 0, Convert.ToInt32(fileSize));
                            fs.Seek(0, SeekOrigin.Begin);
                            StreamReader sr = new StreamReader(fs, true);
                            string firstLine = sr.ReadLine().ToString();
                            if ((firstLine.IndexOf("PNG") > -1))
                            {
                                if (Uploadedfile.ContentLength <= ImageSize)
                                {
                                    massage = "Valid";
                                }
                                else
                                {
                                    massage = "File size can not exceed " + (ImageSize / 1000000) + " MB ";
                                }
                            }
                            else
                            {
                                massage = "Please upload .jpg, .jpeg, .png File only";
                            }
                        }
                    }
                    else
                    {
                        massage = "Please upload .jpg, .jpeg, .png File only";
                    }
                }
            }
            return massage;
        }
        public JsonResult UploadFiles(HttpPostedFileBase File, int size, string FileType, string FolderName, string RegNo, string RegisId)
        {
            string fold = "";
            if (FolderName.Contains("PhotoPath"))
            {
                fold = "EstablishmentPhoto";
            }

            string Dirpath = "~/Content/" + fold + "/";
            string path = "";
            string filename = "";
            string ext = Path.GetExtension(File.FileName);
            bool res = false;
            string msg = "";
            var status = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            if (FileType == "Documents")
            {
                filename = "_" + File.FileName;
                status = ValidateOnlyImage_FileExtWithSize(File, size);
            }

            if (status == "Valid")
            {
                try
                {
                    filename = Convert.ToString(RegNo) + filename;
                    string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                    if (System.IO.File.Exists(completepath))
                    {
                        System.IO.File.Delete(completepath);
                    }
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                    var result = objCMODB.SaveImageAccToRegNo(1, RegNo, path, RegisId).FirstOrDefault();
                    if (result != null && result.Remarks == "true")
                        res = true;
                    else
                        res = false;

                }
                catch (Exception ex)
                {
                    res = false;
                }

            }
            else
            {
                msg = status;
            }
            return Json(new { result = res, fpath = path, mesg = msg });
        }

        public JsonResult RemoveFiles(string RegNo, string RegisId)
        {
            bool res = false;
            try
            {
                var result = objCMODB.SaveImageAccToRegNo(2, RegNo, "", RegisId).FirstOrDefault();
                if (result != null && result.Remarks == "true")
                    res = true;
                else
                    res = false;

            }
            catch (Exception ex)
            {
                res = false;
            }
            return Json(new { result = res });
        }

        #endregion

        #region Upload Image More Than 49 Beds Aniket
        public ActionResult UploadImageApplicationMoreThanFourtNineNUH()
        {
            return View();
        }

        [AuthorizeAdmin(1)]
        public ActionResult UploadImageApplicationMoreThanFourtNineListNUH(string registrationNo = "", string appDate = "", string uploadStatus = "0")
        {
            List<NUHDetailsModel> lstNUHDetails = new List<NUHDetailsModel>();

            var lstCMODistrict = Session["CMODistricts"] as List<CMODistrictModel>;
            if (TempData["fDate"] == null && TempData["tDate"] == null)
            {
                TempData["fDate"] = "";
                TempData["tDate"] = "";
            }
            if (Convert.ToInt32(TempData["District"]) == 0)
            {
                TempData["District"] = 0;
            }
            if (objSM.RollID == 8)
            {
                lstNUHDetails = objCMODB.GetAllNUHList(0, TempData["fDate"].ToString(), TempData["tDate"].ToString(), Convert.ToInt32(TempData["District"])).Where(m => m.appStatus == 6 && (m.appliedDate == appDate || appDate == "") && (m.registrationNo == registrationNo || registrationNo == "")).ToList();
            }
            else
            {
                lstNUHDetails = objCMODB.GetAllNUHListForCMO_ImageUploadThanFourtNine(0, objSM.districtId, uploadStatus).ToList();
            }
            return PartialView("_UploadImageApplicationMoreThanFourtNineListNUH", lstNUHDetails);
        }


        public JsonResult UploadFilesForMoreThanFortyNine(HttpPostedFileBase File, int size, string FileType, string FolderName, string RegNo, string RegisId)
        {
            string fold = "";
            if (FolderName.Contains("PhotoPath"))
            {
                fold = "EstablishmentPhotoMoreThanFortyNine";
            }

            string Dirpath = "~/Content/" + fold + "/";
            string path = "";
            string filename = "";
            string ext = Path.GetExtension(File.FileName);
            bool res = false;
            string msg = "";
            var status = "";
            if (!Directory.Exists(Server.MapPath(Dirpath)))
            {
                Directory.CreateDirectory(Server.MapPath(Dirpath));
            }
            if (FileType == "Documents")
            {
                filename = "_" + File.FileName;
                status = ValidateOnlyImage_FileExtWithSize(File, size);
            }

            if (status == "Valid")
            {
                try
                {
                    filename = Convert.ToString(RegisId) + filename;
                    string completepath = Path.Combine(Server.MapPath(Dirpath), filename);
                    if (System.IO.File.Exists(completepath))
                    {
                        System.IO.File.Delete(completepath);
                    }
                    File.SaveAs(completepath);
                    path = Dirpath + filename;
                    var result = objCMODB.SaveImageOfMoreThanFortyNine(1, RegNo, path, RegisId).FirstOrDefault();
                    if (result != null && result.Remarks == "true")
                        res = true;
                    else
                        res = false;

                }
                catch (Exception ex)
                {
                    res = false;
                }

            }
            else
            {
                msg = status;
            }
            return Json(new { result = res, fpath = path, mesg = msg });
        }

        public JsonResult RemoveFilesForMoreThanFortyNine(string RegNo, string RegisId)
        {
            bool res = false;
            try
            {
                var result = objCMODB.SaveImageOfMoreThanFortyNine(2, RegNo, "", RegisId).FirstOrDefault();
                if (result != null && result.Remarks == "true")
                    res = true;
                else
                    res = false;

            }
            catch (Exception ex)
            {
                res = false;
            }
            return Json(new { result = res });
        }
        #endregion

        public static void RanSchedule(string cpatStr)
        {

            string path = @"D:\NIveshmitra3.0 Integration UP health08may2024\av-janhit-portal\CCSHealthFamilyWelfareDept\log\logformn.text";


            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine("\n\n-------Response Started at {" + DateTime.Now.ToString() + "}-------");

                writer.WriteLine(cpatStr);
                writer.WriteLine("\n\n-------Response Ended at {" + DateTime.Now.ToString() + "}-------");
            }
        }

    }
}
