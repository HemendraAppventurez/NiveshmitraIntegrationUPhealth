﻿using CCSHealthFamilyWelfareDept.DAL;
using CCSHealthFamilyWelfareDept.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CCSHealthFamilyWelfareDept.Filters;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Configuration;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace CCSHealthFamilyWelfareDept.Controllers
{
    [AuthorizeUser]
    public class ICCController : Controller
    {
        Common_DB objComDB = new Common_DB();

        ICC_DB objdb = new ICC_DB();
        Common objCom = new Common();
        Account_DB objAccDb = new Account_DB();
        SessionManager objSM = new SessionManager();
        CMO_DB objCMODB = new CMO_DB();

        #region Method Set Sweet Alert Message
        protected void setSweetAlertMsg(string msg, string msgStatus)
        {
            ViewBag.Msg = msg;
            ViewBag.MsgStatus = msgStatus;
        }
        #endregion

        public ActionResult isRegister()
        {
            var res = objdb.IsRegister();
            if (res.isExists == 1)
            {
                return RedirectToAction("ICCDashBoard", "ICC");
            }
            else if (res.isExists == 0)
            {
                return RedirectToAction("RegistrationInstructions", "ICC");
            }
            return View();
        }
      
        public ActionResult RegistrationInstructions()
        {
            return View();
        }

        public ActionResult ICCDashBoard()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ImmunizationChildrenCertificate()
        {
            long userId = objSM.UserID;

            ICCModel model = new ICCModel();
            ViewBag.DistrictCHCPCH = objComDB.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.State = objComDB.GetDropDownList(6, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.District = Enumerable.Empty<SelectListItem>();
            ViewBag.forwardTo = Enumerable.Empty<SelectListItem>();
            ViewBag.forwardTypes = objCMODB.rblforwardType().Where(m => m.forwardtypeId == 3 || m.forwardtypeId == 4 || m.forwardtypeId == 5).ToList();
            ViewBag.ImmunizationType = objComDB.GetDropDownList(52, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            model.appImmunList = objdb.BindImmunizationDetails();

            return View(model);
        }

        public JsonResult BindDistrictlist(int stateId)
        {
            var res = objComDB.GetDropDownList(7, stateId).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BindForwardDropdownlist(long rollId, int opdDistrictId)
        {
            var res = objdb.bindDropdownlistICC(rollId, opdDistrictId).Select(m => new SelectListItem { Text = m.forwardtoName, Value = m.forwardtoId.ToString() });
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CheckMobileExistence(string mobileNo)
        {
            var user = objdb.CheckEmailMobileExistence(mobileNo, "M");
            bool isExisting = user.isExists == 0;
            return Json(isExisting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImmunizationChildrenCertificate(ICCModel model, FormCollection form)
        {
            AuditMethods objAud = new AuditMethods();
            string errormsg = "";
            bool valStatus = false;
            ModelState["applicantMobileNo"].Errors.Clear();
            model.regisByuser = objSM.UserID;
            model.regBytransIp = Common.GetIPAddress();
            model.transIP = Common.GetIPAddress();
            model.requestKey = objSM.AppRequestKey;

            //#region Bulk Insertion
            ////var immuId = form.GetValues("isExsImmuNamehdn");
            //var isExsistImmuName = form.GetValues("isExsImmuName");
            //var dateOfImmunization = form.GetValues("dateOfImmunization");
            //int count = isExsistImmuName.Count();
            //string XmlData = "<ChildVaccine>";
            //if (count == 0)
            //{
            //    TempData["msg"] = "Please Select Atleast One Immunization Detail ";
            //    TempData["msgstatus"] = "warning";
            //    return RedirectToAction("ImmunizationChildrenCertificate");
            //}
            //for (int i = 0; i < count; i++)
            //{
            //    if (dateOfImmunization[i] != "")
            //    {
            //        XmlData += "<CHild><immuId>" + isExsistImmuName[i] + "</immuId>"
            //         + "<isExsistImmuName>1</isExsistImmuName>"
            //         + "<dateOfImmunization>" + dateOfImmunization[i] + "</dateOfImmunization>"
            //           + "</CHild>";
            //    }
            //    else
            //    {
            //        //TempData["msgalert"] = "Checked Date Required!";
            //        //setSweetAlertMsg("Checked Date Required!", "warning");
            //        TempData["msg"] = "Immunization Date Required!";
            //        TempData["msgstatus"] = "warning";
            //        return RedirectToAction("ImmunizationChildrenCertificate");
            //    }


            //}

            //XmlData += "</ChildVaccine>";

            //#endregion

            valStatus = objAud.IsValidLink(model.immunizationBookpath, "Upload Front Side Scan Copy Of Immunization Book/Card", out errormsg);
            if (!valStatus)
            {
                setSweetAlertMsg(errormsg, "warning");
                return View(model);
            }

            valStatus = objAud.IsValidLink(model.immunizationBackSideBookpath, "Upload Back Side Scan Copy Of Immunization Book", out errormsg);
            if (!valStatus)
            {
                setSweetAlertMsg(errormsg, "warning");
                return View(model);
            }

            #region Bulk Insertion
            //var immuId = form.GetValues("isExsImmuNamehdn");
            var isExsistImmuName = form.GetValues("addimmunId");
            var dateOfImmunization = form.GetValues("adddateOfImmunization");
            int count = isExsistImmuName.Count();
            string XmlData = "<ChildVaccine>";
            if (count == 0)
            {
                TempData["msg"] = "Please Select Atleast One Immunization Detail ";
                TempData["msgstatus"] = "warning";
                return RedirectToAction("ImmunizationChildrenCertificate");
            }
            for (int i = 0; i < count; i++)
            {
                if (dateOfImmunization[i] != "")
                {
                    XmlData += "<CHild><immuId>" + isExsistImmuName[i] + "</immuId>"
                     + "<isExsistImmuName>1</isExsistImmuName>"
                     + "<dateOfImmunization>" + dateOfImmunization[i] + "</dateOfImmunization>"
                       + "</CHild>";
                }
                else
                {
                    //TempData["msgalert"] = "Checked Date Required!";
                    //setSweetAlertMsg("Checked Date Required!", "warning");
                    TempData["msg"] = "Immunization Date Required!";
                    TempData["msgstatus"] = "warning";
                    return RedirectToAction("ImmunizationChildrenCertificate");
                }


            }

            XmlData += "</ChildVaccine>";

            #endregion
            model.XmlData = XmlData;
            var res = objdb.ICCInsertUpdate(model);

            try
            {
                if (res.RegisId > 0 && res.RegistrationNo != "")
                {
                    if (!string.IsNullOrEmpty(objSM.AppRequestKey) && ConfigurationManager.AppSettings["AllowEDistrict"].ToString() == "Y")
                    {
                        EDistrictServiceClass ed = new EDistrictServiceClass();

                        int serviceCode = Convert.ToInt32(EDistrict_ServiceCode.ICC);

                        bool result = ed.postServiceResponse(objSM.AppRequestKey, res.RegistrationNo, serviceCode.ToString(), "ICC");

                        if (result)
                        {
                            SendSMS(res.RegistrationNo, res.MobileNo);
                            TempData["RegisIdICC"] = res.RegisId;
                            TempData["RegistrationNo"] = res.RegistrationNo;
                            return RedirectToAction("RegistrationConfirmation");
                        }
                        else
                        {
                            int exeRsult = objdb.DeleteRegistrationICC(res.RegisId);
                            setSweetAlertMsg("Invalid Request or Service Unavailable", "warning");
                            ModelState.Clear();
                        }
                    }
                    else
                    {
                        SendSMS(res.RegistrationNo, res.MobileNo);
                        TempData["RegisIdICC"] = res.RegisId;
                        TempData["RegistrationNo"] = res.RegistrationNo;
                        return RedirectToAction("RegistrationConfirmation");
                    }
                }
                else
                {
                    setSweetAlertMsg(res.Msg.ToString(), "error");
                    TempData["msg"] = res.Msg.ToString();
                    TempData["msgstatus"] = "success";

                }
            }
            catch (Exception E)
            {
                setSweetAlertMsg("Record not save ", "error");
                TempData["msg"] = res.Msg.ToString();
                TempData["msgstatus"] = "success";

            }


            ViewBag.State = objComDB.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.District = objComDB.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.forwardTo = Enumerable.Empty<SelectListItem>();
            ViewBag.forwardTypes = objCMODB.rblforwardType().Where(m => m.forwardtypeId == 3 || m.forwardtypeId == 4 || m.forwardtypeId == 5).ToList();

            return RedirectToAction("ImmunizationChildrenCertificate");
        }

        public ActionResult ViewICCList()
        {

            long regisByuser = objSM.UserID;
            ICCModel model = new ICCModel();
            model.ICCModelList = objdb.GetICCList(regisByuser);
            return View(model.ICCModelList);

        }

        public ActionResult ICCDetails(string registrationNo, string regisIdICC)
        {
            long regisByuser = objSM.UserID;
            registrationNo = OTPL_Imp.CustomCryptography.Decrypt(registrationNo);
            TempData["reg"] = registrationNo;
            ICCModel model = new ICCModel();

            model = objdb.GetICCListBYRegistrationNo(regisByuser, registrationNo);

            if (model == null || objSM.UserID != model.regisByuser)
            {
                return RedirectToAction("UserUnauthoriseAcess", "Home");
            }

            return View(model);
        }

        public ActionResult BindICCChildList(string registrationNo)
        {
            ICCModel model = new ICCModel();
            model.appImmunList = objdb.getICCChild(registrationNo);
            return PartialView("_ViewICCChild", model.appImmunList);
        }

        public ActionResult PrintApplicationForm(string registrationNo = "", string regisIdICC = "")
        {
            long regisByuser = objSM.UserID;
            registrationNo = OTPL_Imp.CustomCryptography.Decrypt(registrationNo);
            TempData["reg"] = registrationNo;
            ICCModel model = new ICCModel();

            model = objdb.GetICCListBYRegistrationNo(regisByuser, registrationNo);

            if (model == null || objSM.UserID != model.regisByuser)
            {
                return RedirectToAction("UserUnauthoriseAcess", "Home", new { isWithoutLayout = true });
            }

            return View(model);
        }

        public JsonResult UploadFile(HttpPostedFileBase File)
        {


            string Dirpath = "~/Content/writereaddata/ICC/";
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

        public ActionResult bindImmunization()
        {
            ICCModel model = new ICCModel();
            model.appImmunList = objdb.BindImmunizationDetails();
            return PartialView("_ICCimmunizationType", model);
        }

        public ActionResult RegistrationConfirmation()
        {
            ViewBag.ICCRegisId = OTPL_Imp.CustomCryptography.Encrypt(TempData["RegisIdICC"].ToString());
            ViewBag.ICCRegistrationNo = TempData["RegistrationNo"];
            return View();
        }

        void SendSMS(string registrationNo, string mobileNo)
        {

            if (mobileNo != "")
            {
                ForgotPasswordModel otpChCount = new ForgotPasswordModel();
                string txtmsg = "";


                //txtmsg = "Dear Citizen,\n\nYour Application form for Issuance of Immunization Certificate for Children  has been submitted successfully. Your Application Number is " + registrationNo + ".\nPlease use this Application Number for further references.\n\nTechnical Team\n MHFWD, UP";

                txtmsg = "Dear Citizen,Your Application form for Issuance of Immunization Certificate for Children  has been submitted successfully. Your Application Number is " + registrationNo + ".Please use this Application Number for further references.-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007998151714992510";
                string status = SMS.SMSSend(txtmsg, Convert.ToString(mobileNo));//commented on 19/07/2023

                objAccDb.SMSLog(txtmsg, mobileNo, status, registrationNo);
            }

        }

        #region Immunization Certificate for Child Muheeb 20/07/2018
        public ActionResult ICCgeneratedCertificate(string regisIdICC)
        {
            string stausMessage = "";
            string setPdfName = "", setDigitalPdfName = "";
            regisIdICC = OTPL_Imp.CustomCryptography.Decrypt(regisIdICC);
            var res = objdb.GetDetailcertificate(Convert.ToInt64(regisIdICC));
            try
            {
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rptCertificateICC.rpt"));
                rd.SetDataSource(res);
                //ReportDocument subShows = rd.Subreports["rpt_NUHcertificateChild.rpt"];
                //subShows.SetDataSource(res2);
                String dtnow = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                setPdfName = "ImmunizationChildCertificate" + "_" + dtnow;
                string folderpath = "~/Content/writereaddata/PDF/";
                if (!System.IO.Directory.Exists(Server.MapPath(folderpath)))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath(folderpath));
                }
                string flName = folderpath + setPdfName + ".pdf";
                string digitalFlName = folderpath + setDigitalPdfName + ".pdf";
                rd.ExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                rd.ExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                rd.ExportOptions.DestinationOptions = new DiskFileDestinationOptions();
                ((DiskFileDestinationOptions)rd.ExportOptions.DestinationOptions).DiskFileName = Server.MapPath(flName);
                rd.Export();
                //FileInfo fileInfo = new FileInfo(Server.MapPath(flName));
                rd.Close();
                rd.Dispose();

                FileInfo fileInfo = new FileInfo(Server.MapPath(flName));
                Response.ClearContent();
                Response.ClearHeaders();
                Response.ContentType = "application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + setPdfName + ".pdf");
                Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
                Response.WriteFile(flName);
                Response.Flush();
                Response.Close();
                if (System.IO.File.Exists(Server.MapPath(flName)))
                {
                    System.IO.File.Delete(Server.MapPath(flName));
                }



                //////////////////////////digital sign end

            }

            catch (Exception ex)
            {
                stausMessage = "error_Error Occour to Downloading, Please try again.";
            }
            return RedirectToAction("ApprovedApplicationNUH");
        }
       
        #endregion

        #region Method Download File By Path
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
    }
}
