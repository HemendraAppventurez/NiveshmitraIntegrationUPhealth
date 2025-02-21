﻿using CCSHealthFamilyWelfareDept.DAL;
using CCSHealthFamilyWelfareDept.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CCSHealthFamilyWelfareDept.Filters;
using System.Configuration;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;

namespace CCSHealthFamilyWelfareDept.Controllers
{
    [AuthorizeUser]
    public class DICController : Controller
    {
        Common_DB objComDB = new Common_DB();

        DIC_DB objdb = new DIC_DB();
        Common objCom = new Common();
        Common_DB OBJcOMdb = new Common_DB();
        SessionManager objSM = new SessionManager();
        Account_DB objAccDb = new Account_DB();
        CMO_DB objCMODB = new CMO_DB();
        
        #region Method Set Sweet Alert Message
        protected void setSweetAlertMsg(string msg, string msgStatus)
        {
            ViewBag.Msg = msg;
            ViewBag.MsgStatus = msgStatus;
        }
        #endregion

        #region checkfor Registration

        public ActionResult isRegister()
        {
            var res = objdb.IsRegister();
            if (res.isExists == 1)
            {
                return RedirectToAction("DICDashBoard", "DIC");
            }
            else if (res.isExists == 0)
            {
                return RedirectToAction("RegistrationInstructions", "DIC");
            }
            return View();
        }
        #endregion

        public ActionResult RegistrationInstructions()
        {
            return View();
        }

        public ActionResult DICDashBoard()
        {
            var resultData = objdb.GetRegisterDetails();
            if (resultData != null)
            {
                ViewBag.RegisNo = OTPL_Imp.CustomCryptography.Encrypt(resultData.registrationNo.ToString());
            }
            
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DisabilityCertificate(string RegType="")
        {
            long userId = objSM.UserID;
            DICModel model = new DICModel();
           // TempData["appType"] =
            model.appType = OTPL_Imp.CustomCryptography.Decrypt(RegType);
            ViewBag.Category = objComDB.GetDropDownList(8, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.State = OBJcOMdb.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.District = OBJcOMdb.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.IdProof = OBJcOMdb.GetDropDownList(35, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.AddressProof = OBJcOMdb.GetDropDownList(36, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.Disability = OBJcOMdb.GetDropDownList(40, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.DLLMondayDates = OBJcOMdb.GetMondayDates();

            return View(model);
        }

        [HttpPost]
        public JsonResult CheckMobileExistence(string mobileNo)
        { 
            var user = objdb.CheckEmailMobileExistence(mobileNo, "M", objSM.UserID); 
            bool isExisting = user.isExists == 0;
            return Json(isExisting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DisabilityCertificate(DICModel model)
        {
            AuditMethods objAud = new AuditMethods();
            string errormsg = "";
            bool valStatus = false;
            if (model.appType == "new")
            {
                model.ApplyingFor = "1";
                ModelState["releventProof"].Errors.Clear();
            }
            else
            {
                model.ApplyingFor = "2";
            }
            if (model.disabilityTypeId != 6)
            {
                ModelState["disabilityType"].Errors.Clear();
            }
            if (model.photoPathpath!=null)
            {
            valStatus = objAud.IsValidLink(model.photoPathpath, "Upload Photograph to show as disability proof", out errormsg);
            if (!valStatus)
            {
                setSweetAlertMsg(errormsg, "warning");
                return View(model);
            }
            }
            valStatus = objAud.IsValidLink(model.passportsizephotopath, " Passport Size Photo", out errormsg);
            if (!valStatus)
            {
                setSweetAlertMsg(errormsg, "warning");
                return View(model);
            }

            valStatus = objAud.IsValidLink(model.idProofPathpath, "Photo Id Proof File", out errormsg);
            if (!valStatus)
            {
                setSweetAlertMsg(errormsg, "warning");
                return View(model);
            }

            valStatus = objAud.IsValidLink(model.documentPathpath, "Address Proof File", out errormsg);
            if (!valStatus)
            {
                setSweetAlertMsg(errormsg, "warning");
                return View(model);
            }


            model.regByUser = objSM.UserID;
            model.regByTransIp = Common.GetIPAddress();
            model.transIp = Common.GetIPAddress();
            model.requestKey = objSM.AppRequestKey;
            ResultSet res;
            if( model.oldCertificateNumber != "" && model.oldCertificateNumber != null)//for reg from this portal
            {
                model.isCertFromPortal = "Yes";
                model.regisIdDIC = Convert.ToInt64(Session["regisIdDIC"]);
                res = objdb.DICInsertUpdateForRenewal(model);
            }
            else//for new reg and reg for cert not from this portal
            {
                res = objdb.DICInsertUpdate(model);
            }



            try
            {
                if (res.RegisIdDIC > 0 && res.RegistrationNo != "")
                {
                    if (!string.IsNullOrEmpty(objSM.AppRequestKey) && ConfigurationManager.AppSettings["AllowEDistrict"].ToString() == "Y")
                    {
                        EDistrictServiceClass ed = new EDistrictServiceClass();

                        int serviceCode = Convert.ToInt32(EDistrict_ServiceCode.DIC);

                        bool result = ed.postServiceResponse(objSM.AppRequestKey, res.RegistrationNo, serviceCode.ToString(), "DIC");

                        if (result)
                        {
                            SendSMS(res.RegistrationNo, res.MobileNo);
                            TempData["RegisIdDIC"] = res.RegisIdDIC;
                            TempData["RegistrationNo"] = res.RegistrationNo;
                            return RedirectToAction("RegistrationConfirmation");
                        }
                        else
                        {
                            int exeRsult = objdb.DeleteRegistrationDIC(res.RegisIdDIC);
                            setSweetAlertMsg("Invalid Request or Service Unavailable", "warning");
                            ModelState.Clear();
                        }
                    }
                    else
                    {
                        SendSMS(res.RegistrationNo, res.MobileNo);
                        TempData["RegisIdDIC"] = res.RegisIdDIC;
                        TempData["RegistrationNo"] = res.RegistrationNo;
                        return RedirectToAction("RegistrationConfirmation");
                    }
                }
                else
                {
                    setSweetAlertMsg(res.Msg.ToString(), "error");
                    ModelState.Clear();
                }
            }
            catch (Exception E)
            {
                setSweetAlertMsg("Record not save ", "error");
            }

            ViewBag.Category = objComDB.GetDropDownList(8, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.State = OBJcOMdb.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.District = OBJcOMdb.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            ViewBag.Disability = OBJcOMdb.GetDropDownList(40, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });


            return RedirectToAction("DisabilityCertificate");


        }

        [HttpGet]
        public ActionResult ViewDICList()
        {

            long regisByuser = objSM.UserID;
            DICModel model = new DICModel();
            model.DICModelList = objdb.GetDICList(regisByuser);
            return View(model.DICModelList);

        }

        public ActionResult DICDetails(string registrationNo, string regisIdDIC)
        {
            long regisByuser = objSM.UserID;
            // var resultData = objdb.GetRegisterDetails();
            // registrationNo = resultData.registrationNo.ToString();
            registrationNo = OTPL_Imp.CustomCryptography.Decrypt(registrationNo);

            Session["DIC_registration"] = registrationNo;
            Session["regisIdDIC"] = regisIdDIC;
            DICModel model = new DICModel();

            model = objdb.GetDICListBYRegistrationNo(regisByuser, registrationNo);

            if (model == null || objSM.UserID != model.regByUser)
            {
                return RedirectToAction("UserUnauthoriseAcess", "Home");
            }

            return View(model);
        }

        public ActionResult PrintApplicationForm(string registrationNo, string regisIdDIC)
        {
            long regisByuser = objSM.UserID;
            //var resultData = objdb.GetRegisterDetails();
            // registrationNo = resultData.registrationNo.ToString();
            registrationNo = OTPL_Imp.CustomCryptography.Decrypt(registrationNo);

            Session["DIC_registration"] = registrationNo;
            Session["regisIdDIC"] = regisIdDIC;
            DICModel model = new DICModel();

            model = objdb.GetDICListBYRegistrationNo(regisByuser, registrationNo);

            if (model == null || objSM.UserID != model.regByUser)
            {
                return RedirectToAction("UserUnauthoriseAcess", "Home", new { isWithoutLayout = true });
            }

            return View(model);
        }

        public JsonResult UploadFile(HttpPostedFileBase File)
        {


            string Dirpath = "~/Content/writereaddata/DIC/";
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

        public ActionResult RegistrationConfirmation()
        {
            ViewBag.DICRegisId = OTPL_Imp.CustomCryptography.Encrypt(TempData["RegisIdDIC"].ToString());
            ViewBag.DICRegistrationNo = TempData["RegistrationNo"];
            return View();
        }

        void SendSMS(string registrationNo, string mobileNo)
        {
            if (mobileNo != "")
            {
                ForgotPasswordModel otpChCount = new ForgotPasswordModel();
                string txtmsg = "";

                //txtmsg = "Dear Citizen,\n\nYour Application form has been submitted successfully. Your Application Form Number is " + registrationNo + ", kindly use this further.\n\n Thanks";
               // txtmsg = "Dear Citizen,\n\nYour Application form for Issuance of Disability Certificate has been submitted successfully. Your Application Number is " + registrationNo + ".\nPlease use this Application Number for further references.\n\nTechnical Team\nMHFWD, UP";
                txtmsg = "Dear Citizen,Your Application form for Issuance of Disability Certificate has been submitted successfully. Your Application Number is " + registrationNo + ".Please use this Application Number for further references.-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007455617727351484";

                string status = SMS.SMSSend(txtmsg, Convert.ToString(mobileNo));

                objAccDb.SMSLog(txtmsg, mobileNo, status, registrationNo);
            }
        }

        public JsonResult GetDICdetailByCertNo(string oldCertificateNumber)
        {
           
            var resultData = objdb.GetDICdetailByCertNo(oldCertificateNumber,objSM.UserID);
            if (resultData != null)
            {
                Session["regisIdDIC"] = resultData.regisIdDIC;
                return Json(resultData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                DICModel model = new DICModel();
                model.regisIdDIC = 0;
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDICNewCaseForm()
        {
            ViewBag.Category = objComDB.GetDropDownList(8, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.State = OBJcOMdb.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.District = OBJcOMdb.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.IdProof = OBJcOMdb.GetDropDownList(35, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.AddressProof = OBJcOMdb.GetDropDownList(36, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.DLLMondayDates = OBJcOMdb.GetMondayDates();
            DICModel model = new DICModel();
            return PartialView("_DICnewCertificate", model);

        }

        public ActionResult GetDICrevisedCaseForm()
        {
            ViewBag.Category = objComDB.GetDropDownList(8, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.State = OBJcOMdb.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.District = OBJcOMdb.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.IdProof = OBJcOMdb.GetDropDownList(35, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.AddressProof = OBJcOMdb.GetDropDownList(36, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.DLLMondayDates = OBJcOMdb.GetMondayDates();
            DICModel model = new DICModel();
            return PartialView("_DICRevisedCertificate", model);

        }

        #region Method Generate Certificate DIC
        public ActionResult GenerateCertificateDIC(string regisId, string certGenrBy)
        {
            string stausMessage = "";
            string setPdfName = "", setDigitalPdfName = "";
            var res = objCMODB.GetCertificateDetialDIC(Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId)));
            try
            {
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rptCertificateDIC.rpt"));
                rd.SetDataSource(res);
                String dtnow = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                setPdfName = "DisabilityCertificate" + "_" + dtnow;
                setDigitalPdfName = "DisabilityCertificateDigitalSigned" + "_" + dtnow;
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
                    if (System.IO.File.Exists(Server.MapPath(flName)))
                    {
                        System.IO.File.Delete(Server.MapPath(flName));
                    }
                }
                else
                {
                    ////////////digital sign

                    var sigDetails = OBJcOMdb.GetDigitalSignatureDetails(Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(certGenrBy)));

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

                    //////////////////////////digital sign end
                }
            }
            catch (Exception ex)
            {
                setSweetAlertMsg(ex.Message, "warning");
            }
            return RedirectToAction("ViewDICList");
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
