﻿using CCSHealthFamilyWelfareDept.DAL;
using CCSHealthFamilyWelfareDept.Filters;
using CCSHealthFamilyWelfareDept.Models;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace CCSHealthFamilyWelfareDept.Controllers
{ 
    [AuthorizeUser]
    public class MERController : Controller
    { 
        Common_DB objComDB = new Common_DB();
        Common objCom = new Common();
        MER_DB objMECDB = new MER_DB();
        Account_DB objAccDb = new Account_DB();
        SessionManager objSM = new SessionManager();

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
            var res = objMECDB.IsRegister();
            if (res.isExists == 1)
            {
                return RedirectToAction("MedicalReimbursementDashBoard", "MER");
            }
            else if (res.isExists == 0)
            {
                return RedirectToAction("RegistrationInstructions", "MER");
            }
            return View();
        }
        #endregion

        public ActionResult MedicalReimbursementDashBoard()
        {
            return View();
        }

        public ActionResult RegistrationInstructions()
        { 
            return View();
        }

        public JsonResult Dropdown()
        {
            var res = objComDB.GetDropDownList(23, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadFile(HttpPostedFileBase File)
        {
            string Dirpath = "~/Content/writereaddata/MER/";
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

        [HttpGet]
        public ActionResult MERRegistration()
        {
            ViewBag.TreatmentType = objComDB.GetDropDownList(22, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.District = objComDB.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.State = objComDB.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.State1 = objComDB.GetDropDownList(6, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.District1 = Enumerable.Empty<SelectListItem>();

            ViewBag.BillType = objComDB.GetDropDownList(23, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            ViewBag.Submit = "Submit";
            ViewBag.Cancel = "Cancel";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MERRegistration(MERModel model, FormCollection form)
        {
            AuditMethods objAud = new AuditMethods();
            string errormsg = "";
            bool valStatus = false;
            if (model.isAdvanceTaken == "No")
            {
                ModelState["advanceAmount"].Errors.Clear();
                ModelState["advanceLetterNo"].Errors.Clear();
                ModelState["advanceDate"].Errors.Clear();
            }
            if (model.isRetirement == false)
            {
                ModelState["ppotreament"].Errors.Clear();
            }
            else{
                ModelState["officeName"].Errors.Clear();
                ModelState["officeInchargeName"].Errors.Clear();
            }
            ModelState["appMobile"].Errors.Clear();
            long userId = objSM.UserID;
            string IpAddress = Common.GetIPAddress();

            #region Bulk Insertion
            var billId = form.GetValues("billId");
            var billNo = form.GetValues("billNo");
            var billdate = form.GetValues("billdate");
            var billAmount = form.GetValues("billAmount");
            var billFilePath = form.GetValues("billFilePath");
            string XmlData = "<Bills>";


            for (int i = 0; i < billId.Count(); i++)
            {
                if (billId[i].ToString() == "" && billNo[i] == "" && billdate[i] == "" && billAmount[i] == "" && billFilePath[i] == "")
                {
                    //XmlData = string.Empty;
                }
                else
                {
                    valStatus = objAud.IsValidLink(billFilePath[i], "Attached Document", out errormsg);
                    if (!valStatus)
                    {
                        setSweetAlertMsg(errormsg, "warning");
                        return View(model);
                    }

                    XmlData += "<Bill><BillId>" + billId[i] + "</BillId>"
                          + "<BillNo>" + objAud.FilterForAlphabetNumaric(billNo[i]) + "</BillNo>"
                          + "<BillDate>" + billdate[i] + "</BillDate>"
                           + "<BillAmount>" + billAmount[i] + "</BillAmount>" +
                           "<BillFilePath>" + billFilePath[i] + "</BillFilePath>"
                            + "</Bill>";
                }
            }
            XmlData += "</Bills>";

            #endregion
            if (ModelState.IsValid)
            { 
                if (objCom.IsValidDateFormat(model.patienttreatmentFromDate))
                {
                    model.patienttreatmentFromDate = Convert.ToDateTime(model.patienttreatmentFromDate).ToString();
                    if (objCom.IsValidDateFormat(model.patienttreatmentToDate))
                    {
                        model.patienttreatmentToDate = Convert.ToDateTime(model.patienttreatmentToDate).ToString();
                        if ((model.patientType == "Self" && model.manavSampda_AadharNo == model.patientAadhaarNo) || (model.patientType == "Dependent" && model.manavSampda_AadharNo != model.patientAadhaarNo))
                        { 
                            var res = objMECDB.MER_Insertion(1, model.regisIdMER, userId, model.treatmentId,
                                model.empfullName, model.designation, model.manavSampda_AadharNo, model.isRetirement, model.ppotreament, model.father_HusbandName, model.officeName, model.deptName, model.basicSalary, model.dob, model.gender, model.mobileNo,
                                model.postingAddress, model.postingDistrictId, model.postingPinCode, model.postingStateId, model.permAddress, model.permDistrictId, model.permPinCode, model.permStateId, model.patientType, model.patientName,
                                model.patientage, model.patientgender, model.patientrelationsWithEmployee, model.patientAadhaarNo, model.patientdiseaseName, model.patienttreatmentFromDate, model.patienttreatmentToDate, model.patientplaceOfDisease,
                                model.patienthospitalName, model.patientdoctorName, model.bankName, model.branchName, model.accountNo, model.ifscCode, XmlData, IpAddress, IpAddress, objSM.AppRequestKey,
                                model.officeInchargeName, model.hospitalType, model.isAdvanceTaken, model.advanceAmount, model.advanceLetterNo, model.advanceDate,
                                model.ForwardedToId);
                            try
                            {
                                if (res.RegisId > 0 && res.RegistrationNo != "")
                                {
                                    if (!string.IsNullOrEmpty(objSM.AppRequestKey) && ConfigurationManager.AppSettings["AllowEDistrict"].ToString() == "Y")
                                    {
                                        EDistrictServiceClass ed = new EDistrictServiceClass();

                                        int serviceCode = Convert.ToInt32(EDistrict_ServiceCode.MER);

                                        bool result = ed.postServiceResponse(objSM.AppRequestKey, res.RegistrationNo, serviceCode.ToString(), "MER");

                                        if (result)
                                        {
                                            SendSMS(res.RegistrationNo, res.MobileNo);
                                            TempData["RegisId"] = res.RegisId;
                                            TempData["RegistrationNo"] = res.RegistrationNo;
                                            return RedirectToAction("RegistrationConfirmation");
                                        }
                                        else
                                        {
                                            int exeRsult = objMECDB.DeleteRegistrationMER(res.RegisId);
                                            setSweetAlertMsg("Invalid Request or Service Unavailable", "warning");
                                            ModelState.Clear();
                                        }
                                    }
                                    else
                                    {
                                        SendSMS(res.RegistrationNo, res.MobileNo);
                                        TempData["RegisId"] = res.RegisId;
                                        TempData["RegistrationNo"] = res.RegistrationNo;
                                        ModelState.Clear();
                                        return RedirectToAction("RegistrationConfirmation");
                                    }
                                }
                                else
                                {
                                    setSweetAlertMsg("some error occur!", "error");
                                }
                            }
                            catch (Exception E)
                            {
                                setSweetAlertMsg("Record not save ", "error");
                            }
                            ViewBag.TreatmentType = objComDB.GetDropDownList(22, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                            ViewBag.District = objComDB.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                            ViewBag.State = objComDB.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                            ViewBag.BillType = objComDB.GetDropDownList(23, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                            ViewBag.Submit = "Submit";
                            ViewBag.Cancel = "Cancel";
                        }
                        else
                        {

                            ViewBag.TreatmentType = objComDB.GetDropDownList(22, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                            ViewBag.District = objComDB.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                            ViewBag.State = objComDB.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                            ViewBag.BillType = objComDB.GetDropDownList(23, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                            ViewBag.Submit = "Submit";
                            ViewBag.Cancel = "Cancel";
                            TempData["msg"] = " Employee`s Aadhaar Number must be different from Dependent`s Aadhaar Number!";
                            TempData["msgstatus"] = "warning";
                        }
                    }
                    else
                    {
                        TempData["msg"] = "Treatment Period (To) Date is incorrect format, Date format should be DD/MM/YYYY !";
                        TempData["msgstatus"] = "warning";
                    }
                }
                else
                {
                    TempData["msg"] = "Treatment Period (From) Date is incorrect format, Date format should be DD/MM/YYYY !";
                    TempData["msgstatus"] = "warning";
                } 
            }
            else
            {
                setSweetAlertMsg("Invalid Data Entered!", "error");
                ViewBag.TreatmentType = objComDB.GetDropDownList(22, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                ViewBag.District = objComDB.GetDropDownList(7, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                ViewBag.State = objComDB.GetDropDownList(6, 34).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                ViewBag.BillType = objComDB.GetDropDownList(23, 0).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                ViewBag.Submit = "Submit";
                ViewBag.Cancel = "Cancel";
            }

            return View();
        }
        public JsonResult binddist(int permStateId)
        {
            var res = objComDB.GetDropDownList(7, permStateId).Select(m => new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CheckMobileExistence(string mobileNo)
        {
            var user = objMECDB.CheckEmailMobileExistence(mobileNo, "M", objSM.UserID);
            bool isExisting = user.isExists == 0;
            return Json(isExisting);
        }
         
        public ActionResult RegistrationConfirmation()
        {
            ViewBag.MERRegistrationNo = TempData["RegistrationNo"];
            ViewBag.regisId = TempData["RegisId"];
            return View();
        }

        public ActionResult ViewMERList()
        {
            long userId = objSM.UserID;
            MERModel model = new MERModel();
            model.MERModelList = objMECDB.getMERList(userId);

            return View(model.MERModelList);
        }

        public ActionResult MERDetails(string regisId)
        {
            regisId = OTPL_Imp.CustomCryptography.Decrypt(regisId);
            Session["regisIdMER"] = regisId;
            MERModel model = new MERModel();

            model = objMECDB.getMERByRegistration(Convert.ToInt64(regisId));

            if (model == null || objSM.UserID != model.regByUser)
            {
                return RedirectToAction("UserUnauthoriseAcess", "Home");
            }

            return View(model);
        }

        public ActionResult BindChildList()
        {
            MERModel model = new MERModel();

            model.MERModelList = objMECDB.getMERChild(Convert.ToInt64(Session["regisIdMER"].ToString()));
            return PartialView("_ViewMERChild", model.MERModelList);
        }

        public ActionResult PrintApplicationForm(string regisId)
        {
            regisId = OTPL_Imp.CustomCryptography.Decrypt(regisId);
            Session["regisIdMER"] = regisId;
            MERModel model = new MERModel();

            model = objMECDB.getMERByRegistration(Convert.ToInt64(regisId));

            if (model == null || objSM.UserID != model.regByUser)
            {
                return RedirectToAction("UserUnauthoriseAcess", "Home", new { isWithoutLayout = true });
            }

            return View(model); 
        }

        void SendSMS(string registrationNo, string mobileNo)
        { 
            if (mobileNo != "")
            {
                ForgotPasswordModel otpChCount = new ForgotPasswordModel();
                string txtmsg = "";

                //txtmsg = "Dear Citizen,\n\nYour Application form has been submitted successfully. Your Application Form Number is " + registrationNo + ", kindly use this further.\n\n Thanks";
               // txtmsg = "Dear Citizen,\n\nYour Application form for Payment of Medical Reimbursement has been submitted successfully. Your Application Number is " + registrationNo + ".\nPlease use this Application Form Number for further references.\n\nTechnical Team\nMHFWD, UP";
                txtmsg = "Dear Citizen,Your Application form for Payment of Medical Reimbursement has been submitted successfully. Your Application Number is " + registrationNo + ".Please use this Application Form Number for further references.-UPDGMH IT CELL&entityid=1001959722217619726&templateid=1007801521063114165";
                string status = SMS.SMSSend(txtmsg, Convert.ToString(mobileNo));//commented on 19/07/2023

                objAccDb.SMSLog(txtmsg, mobileNo, status, registrationNo);
            } 
        }
         
        #region Affidavate Report
        public ActionResult MERAffidavateReport(string regisId)
        {
            string stausMessage = "";
            string setPdfName = "";

            var res = objMECDB.GetMERdetailRpt(Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisId)));
            //var res = objMECDB.GetMERdetailRpt(Convert.ToInt64(regisId));
            var res2 = objMECDB.GetMERCHILD(res[0].regisIdMER);
            try
            {
                ReportDocument rd = new ReportDocument();
                if (res[0].treatmentId == 1)
                {
                    rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rpt_MERAffidaviteA.rpt"));
                }
                else if (res[0].treatmentId == 2)
                {
                    rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rpt_MERAffidaviteB.rpt"));
                }
                else if (res[0].treatmentId == 3)
                {
                    rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rpt_MERAffidaviteBoth.rpt"));
                    ReportDocument subShows1 = rd.Subreports["MERaffidavitChildA.rpt - 01"];
                    subShows1.SetDataSource(res2);

                }

                rd.SetDataSource(res);
                ReportDocument subShows = rd.Subreports["MERaffidavitChildA.rpt"];
                subShows.SetDataSource(res2);
                String dtnow = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                setPdfName = "MedicalReimbursement" + "_" + dtnow;
                string folderpath = "~/Content/writereaddata/PDF/";
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
                FileInfo fileInfo = new FileInfo(Server.MapPath(flName));
                rd.Close();
                rd.Dispose();
                {
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
            }
            catch (Exception ex)
            {
                stausMessage = "error_Error Occour to Downloading, Please try again.";
            }
            return RedirectToAction("MedicalReimbursementDashBoard");
        }
        #endregion

        #region Certificate Rpt Riya
        public ActionResult MERgeneratedCertificate(string regisIdMER, string certGenrBy)
        {
            int x = Convert.ToInt32(OTPL_Imp.CustomCryptography.Decrypt(certGenrBy));

            string stausMessage = "";
            string setPdfName = "", setDigitalPdfName = "";
            var res = objMECDB.GetMERdetailCertificateRpt(Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(regisIdMER)));


            try
            {
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/RPT"), "rptCertificateMER.rpt"));
                rd.SetDataSource(res);


                String dtnow = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                setPdfName = "MedicalReimbursement" + "_" + dtnow;
                setDigitalPdfName = "MedicalReimbursementCertificateDigitalSigned" + "_" + dtnow;
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

                    var sigDetails = objComDB.GetDigitalSignatureDetails(Convert.ToInt64(OTPL_Imp.CustomCryptography.Decrypt(certGenrBy)));

                    float llx = 580;
                    float lly = 290;
                    float urx = 440;
                    float ury = 190;
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

                    //////////////////////////digital sign end
                }
            }
            catch (Exception ex)
            {
                stausMessage = "error_Error Occour to Downloading, Please try again.";
            }
            return RedirectToAction("ViewNUHList");
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
