using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CCSHealthFamilyWelfareDept.Filters;
using CCSHealthFamilyWelfareDept.Models;

namespace CCSHealthFamilyWelfareDept.DAL
{
    public class Common_DB : DbContext
    {
         #region Default Constructor
        public Common_DB()
            : base("CMSModule")
        { }
        #endregion

        #region Create Email Log
        public int EmailLog(String EmailId, String Subject, String MailBody, bool MailStatus, String ResponceMessage)
        {
            try
            {
                int count = this.Database.ExecuteSqlCommand("proc_InserEmailLog @EmailId, @Subject, @MailBody,@MailStatus,@ResponceMessage",
                    new SqlParameter("EmailId", EmailId),
                    new SqlParameter("Subject", Subject),
                    new SqlParameter("MailBody", MailBody),
                    new SqlParameter("MailStatus", MailStatus),
                    new SqlParameter("ResponceMessage", ResponceMessage)
                    );
                return count;
            }
            catch
            { return 0; }

        }
        #endregion

        #region Method Get Dropdown List
        public List<DropDownList> GetDropDownList(int procId, int Id)
        {
            var sqlparam = new SqlParameter[] { 
              new SqlParameter{ParameterName="@procId",Value=procId},
              new SqlParameter{ParameterName="@Id",Value=Id}
            };
            var _proc = @"proc_GetDataForDropDownList @procId,@Id";
            var slist = this.Database.SqlQuery<DropDownList>(_proc, sqlparam).ToList();
            return slist;
        } 
        #endregion

        #region Method Get Monday Dates
        public List<SelectListItem> GetMondayDates()
        {
            var _proc = @"proc_GetMondayDates";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc).ToList();
            return slist;
        }
        #endregion

        #region Method Get Application Process By Application Name
        public List<SelectListItem> GetApplicationProcessByAppName(string applicationName)
        {
            var sqlparam = new SqlParameter[] { 
              new SqlParameter{ParameterName="@applicationName",Value=applicationName}
            };
            var _proc = @"proc_GetApplicationProcessByAppName @applicationName";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc, sqlparam).ToList();
            return slist;
        }
        #endregion

        #region Method Get Committee Details For DLL
        public List<SelectListItem> GetCommitteeDetailsForDLL()
        {
            var _proc = @"proc_GetCommitteeDetailsForDLL";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc).ToList();
            return slist;
        }
        #endregion

        #region Method Get Committee Details For DLL
        public List<SelectListItem> GetCommitteeDetailsForDLL_DIC()
        {
            var _proc = @"proc_GetCommitteeDetailsForDLL_DIC";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc).ToList();
            return slist;
        }
        #endregion

        #region Method Get Test Type For DLL
        public List<SelectListItem> GetTestTypeForDLL_DIC()
        {
            var _proc = @"proc_GetTestTypeForDLL_DIC";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc).ToList();
            return slist;
        }
        #endregion

        #region Method Get Committee Member DIC
        public List<DropDownList> GetCommitteeMember_DIC(long regisIdDIC)
        {
            var sqlparam = new SqlParameter[] { 
              new SqlParameter{ParameterName="@regisIdDIC",Value=regisIdDIC}
            };
            var _proc = @"proc_GetCommitteeMember_DIC @regisIdDIC";
            var slist = this.Database.SqlQuery<DropDownList>(_proc, sqlparam).ToList();
            return slist;
        }
        #endregion

        #region Method Get Committee Member DIC
        public DICAppProcessModel GetInspReportPersentageDIC(long regisId)
        {
            var sqlparams = new SqlParameter[] { 
            new SqlParameter{ParameterName="@regisId",Value=regisId}
            };
            var _proc = @"proc_GetInspReportPersentage_DIC @regisId";
            var slist = this.Database.SqlQuery<DICAppProcessModel>(_proc, sqlparams).FirstOrDefault();
            return slist;
        }
        #endregion

        #region Method Get Digital Signature Details
        public DigitalSignatureModel GetDigitalSignatureDetails(long userId)
        {
            SqlParameter param = new SqlParameter("@userId", userId);
            var sqlProc = "proc_GetDigitalSignatureDetails @userId";
            var sList = this.Database.SqlQuery<DigitalSignatureModel>(sqlProc, param).FirstOrDefault();
            return sList;
        } 
        #endregion

        #region Insert Send Request
        public int InsertSendRequest(ServiceModel model)
        {
            int result = 0;

            var sqlParams = new SqlParameter[] { 
                            new SqlParameter{ParameterName="@requestKey",Value=model.requestKey},
                            new SqlParameter{ParameterName="@deptRegistrationId",Value=model.deptRegistrationId},
                            new SqlParameter{ParameterName="@serviceResponse",Value=model.serviceResponse},
                            new SqlParameter{ParameterName="@transIp",Value=model.transIp}
            };

            var query = "proc_InsertSendRequest_EDistrict @requestKey,@deptRegistrationId,@serviceResponse,@transIp";
            result = this.Database.ExecuteSqlCommand(query, sqlParams);
            return result;
        }
        #endregion

        #region Insert Send Request
        public int InsertSendResponse(ServiceModel model)
        {
            int result = 0;

            var sqlParams = new SqlParameter[] { 
                            new SqlParameter{ParameterName="@requestKey",Value=model.requestKey},
                            new SqlParameter{ParameterName="@deptRegistrationId",Value=model.deptRegistrationId},
                            new SqlParameter{ParameterName="@serviceResponse",Value=model.serviceResponse},
                            new SqlParameter{ParameterName="@applicationNumber",Value=model.applicationNumber},
                            new SqlParameter{ParameterName="@serviceCode",Value=model.serviceCode},
                            new SqlParameter{ParameterName="@applicationType",Value=model.applicationType},
                            new SqlParameter{ParameterName="@transIp",Value=model.transIp}
            };

            var query = "proc_InsertSendResponse_EDistrict @requestKey,@deptRegistrationId,@serviceResponse,@applicationNumber,@serviceCode,@applicationType,@transIp";
            result = this.Database.ExecuteSqlCommand(query, sqlParams);
            return result;
        }
        #endregion

        #region Method Get CMO District Mapping
        public List<CMODistrictModel> GetCMODistrictMapping(long userId)
        {
            var sqlparam = new SqlParameter[] { 
              new SqlParameter{ParameterName="@userId",Value=userId}
            };
            var _proc = @"proc_GetCMODistrictMapping @userId";
            var slist = this.Database.SqlQuery<CMODistrictModel>(_proc, sqlparam).ToList();
            return slist;
        }
        #endregion

        #region Method Get Unsigned Certificate Details of DIC
        public List<DICCertificateDetailModel> GetUnsignedCerti_DIC(long cmoProfileId)
        {
            var sqlParam = new SqlParameter[] { 
               new SqlParameter("@cmoProfileId", cmoProfileId)
            };
            var _proc = @"proc_GetUnsignedCerti_DIC @cmoProfileId";
            var slist = this.Database.SqlQuery<DICCertificateDetailModel>(_proc, sqlParam).ToList();
            return slist;
        } 
        #endregion

        #region Method Get Unsigned Certificate Details of AGC
        public List<AGCModel> GetUnsignedCerti_AGC(long cmoProfileId)
        {
            var sqlParam = new SqlParameter[] { 
                    new SqlParameter{ParameterName="@cmoProfileId",Value=cmoProfileId} 
             };
            var _proc = @"proc_GetUnsignedCerti_AGC @cmoProfileId";
            var slist = this.Database.SqlQuery<AGCModel>(_proc, sqlParam).ToList();
            return slist;
        }
        #endregion

        #region Method Get Unsigned Certificate Details of MER
        public List<MERrptModel> GetUnsignedCerti_MER(long cmoProfileId)
        {
            var sqlparams = new SqlParameter[] { 
                    new SqlParameter{ParameterName="@cmoProfileId",Value=cmoProfileId}
             };
            var _proc = @"proc_GetUnsignedCerti_MER  @cmoProfileId";
            var slist = this.Database.SqlQuery<MERrptModel>(_proc, sqlparams).ToList();
            return slist;
        }
        #endregion

        #region Get OTP Count
        public virtual IEnumerable<DropDownList> GetOTPCount(string mobileNo)
        {
            var sqlParams = new SqlParameter[] { 
                 new SqlParameter { ParameterName = "@mobileNo", Value = mobileNo }
              };
            var sqlQuery = @"proc_GetOTPCount @mobileNo";
            var sList = this.Database.SqlQuery<DropDownList>(sqlQuery, sqlParams);
            return sList;
        }
        #endregion

        #region set OTP is Used
        public virtual IEnumerable<DropDownList> UpdateOTPUsedStatus(string mobileNo)
        {
            var sqlParams = new SqlParameter[] { 
                 new SqlParameter { ParameterName = "@mobileNo", Value = mobileNo }
              };
            var sqlQuery = @"proc_UpdateOTPUsedStatus @mobileNo";
            var sList = this.Database.SqlQuery<DropDownList>(sqlQuery, sqlParams);
            return sList;
        }
        #endregion


        #region Method Get Dropdown List PARAM
        public List<DropDownList> GetDropDownListFilled(int procId, Int64 Id)
        {
            var sqlparam = new SqlParameter[] { 
              new SqlParameter{ParameterName="@procId",Value=procId},
              new SqlParameter{ParameterName="@reregisIdNUH",Value=Id}
            };
            var _proc = @"proc_GetDataForDropDownListFilled @procId,@reregisIdNUH";
            var slist = this.Database.SqlQuery<DropDownList>(_proc, sqlparam).ToList();
            return slist;
        }
        #endregion

        #region Method Get Zone Details For DLL
        public List<SelectListItem> GetZoneForDLL()
        {
            var _proc = @"proc_GetZoneForDLL";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc).ToList();
            return slist;
        }
        #endregion

        #region Method Get District Details For DLL
        public List<SelectListItem> GetDistrictForDLL()
        {
            var _proc = @"proc_GetDistrictForDLL";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc).ToList();
            return slist;
        }
        #endregion

        #region Method Get Services Details For DLL
        public List<SelectListItem> GetSevicesForDLL()
        {
            var _proc = @"proc_GetServicesForDLL";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc).ToList();
            return slist;
        }
        #endregion

        #region Method Get Roll DLL
        public List<SelectListItem> GetRollForDLL()
        {
            var _proc = @"GetRollMasterDLL";
            var slist = this.Database.SqlQuery<SelectListItem>(_proc).ToList();
            return slist;
        }
        #endregion

        #region Nivesh Mitra API Request Response Log History 
        public int APIRequestResponseLogHistory(string userName, string APIUrl, string ControllId, string UnitId, string RequestId, string ServiceId, string ProcessIndustryId, string ApplicationId, string StatusCode, string RequestString, string DecryptRequestString, string ResponseString, string DecryptResponseString, string token, string SessionKey, string AppicationStatusCode, string RequestKey, string DeptId, string Remarks, string PendecyLevel, string Pending_with_Officer, decimal feeamount, string feestatus, string NOC_Certificate_Number, string NOC_Url, string IsNocUrlActiveYesNo, string Objection_Rejection_Code, string Objection_Rejection_Url, string Is_Certificate_Valid_Life_Time, string Certificate_EXP_Date_DDMMYYYY, string D1, string D2, string D3, string D4, string D5, string D6, string D7, string D8, string D9, string D10, string D11, string D12, string D13, string D14, string D15, string D16, string D17, string D18, string D19, string D20)
        {
            int result = 0;

            try
            {
                var sqlParams = new SqlParameter[] {
                            new SqlParameter{ParameterName="@userName",Value= (userName == null)?"":userName},
                            new SqlParameter{ParameterName="@APIUrl",Value=(APIUrl== null)?"":APIUrl},
                            new SqlParameter{ParameterName="@ControllId",Value=(ControllId ==  null)?"":ControllId},
                            new SqlParameter{ParameterName="@UnitId",Value=(UnitId == null)?"":UnitId},
                            new SqlParameter{ParameterName="@RequestId",Value=(RequestId ==  null)?"":RequestId},
                            new SqlParameter{ParameterName="@ServiceId",Value=(ServiceId == null)?"":ServiceId},
                            new SqlParameter{ParameterName="@ProcessIndustryId",Value=(ProcessIndustryId==null)?"":ProcessIndustryId},
                             new SqlParameter{ParameterName="@ApplicationId",Value=(ApplicationId==null)?"":ApplicationId},
                            new SqlParameter{ParameterName="@StatusCode",Value=(StatusCode== null)?"":StatusCode},
                            new SqlParameter{ParameterName="@RequestString",Value=(RequestString==null)?"":RequestString},
                            new SqlParameter{ParameterName="@DecryptRequestString",Value=(DecryptRequestString==null)?"":DecryptRequestString},
                            new SqlParameter{ParameterName="@ResponseString",Value=(ResponseString==null)?"":ResponseString},
                            new SqlParameter{ParameterName="@DecryptResponseString",Value=(DecryptResponseString==null)?"":DecryptResponseString},
                            new SqlParameter{ParameterName="@token",Value=(token== null)?"":token},
                            new SqlParameter{ParameterName="@SessionKey",Value=(SessionKey==null)?"":SessionKey},
                             new SqlParameter{ParameterName="@AppicationStatusCode",Value=(AppicationStatusCode==null)?"":AppicationStatusCode},
                                  new SqlParameter{ParameterName="@RequestKey",Value=(RequestKey==null)?"":RequestKey},
                                  new SqlParameter{ParameterName="@DeptId",Value=(DeptId==null)?"":DeptId},
                                  new SqlParameter{ParameterName="@Remarks",Value=(Remarks==null)?"":Remarks},
                                  new SqlParameter{ParameterName="@PendecyLevel",Value=(PendecyLevel==null)?"":PendecyLevel},
                                  new SqlParameter{ParameterName="@Pending_with_Officer",Value=(Pending_with_Officer==null)?"":Pending_with_Officer},
                                  new SqlParameter{ParameterName="@feeamount",Value=(feeamount==null)?0:feeamount},
                                  new SqlParameter{ParameterName="@feestatus",Value=(feestatus==null)?"":feestatus},
                                   new SqlParameter{ParameterName="@NOC_Certificate_Number",Value=(NOC_Certificate_Number==null)?"":NOC_Certificate_Number},
                                   new SqlParameter{ParameterName="@NOC_Url",Value=(NOC_Url==null)?"":NOC_Url},
                                   
                                  new SqlParameter{ParameterName="@IsNocUrlActiveYesNo",Value=(IsNocUrlActiveYesNo==null)?"":IsNocUrlActiveYesNo},

                                  new SqlParameter{ParameterName="@Objection_Rejection_Code",Value=(Objection_Rejection_Code==null)?"":Objection_Rejection_Code},
                                   new SqlParameter{ParameterName="@Objection_Rejection_Url",Value=(Objection_Rejection_Url==null)?"":Objection_Rejection_Url},
                                   new SqlParameter{ParameterName="@Is_Certificate_Valid_Life_Time",Value=(Is_Certificate_Valid_Life_Time==null)?"":Is_Certificate_Valid_Life_Time},
                                   new SqlParameter{ParameterName="@Certificate_EXP_Date_DDMMYYYY",Value=(Certificate_EXP_Date_DDMMYYYY==null)?"":Certificate_EXP_Date_DDMMYYYY},
                                   new SqlParameter{ParameterName="@D1",Value=(D1==null)?"":D1},
                                   new SqlParameter{ParameterName="@D2",Value=(D2==null)?"":D2},
                                    new SqlParameter{ParameterName="@D3",Value=(D3==null)?"":D3},
                                    new SqlParameter{ParameterName="@D4",Value=(D4==null)?"":D4},
                                    new SqlParameter{ParameterName="@D5",Value=(D5==null)?"":D5},
                                    new SqlParameter{ParameterName="@D6",Value=(D6==null)?"":D6},
                                    new SqlParameter{ParameterName="@D7",Value=(D7==null)?"":D7},
                                    new SqlParameter{ParameterName="@D8",Value=(D8==null)?"":D8},
                                    new SqlParameter{ParameterName="@D9",Value=(D9==null)?"":D9},
                                    new SqlParameter{ParameterName="@D10",Value=(D10==null)?"":D10},
                                    new SqlParameter{ParameterName="@D11",Value=(D11==null)?"":D11},
                                    new SqlParameter{ParameterName="@D12",Value=(D12==null)?"":D12},
                                    new SqlParameter{ParameterName="@D13",Value=(D13==null)?"":D13},
                                    new SqlParameter{ParameterName="@D14",Value=(D14==null)?"":D14},
                                    new SqlParameter{ParameterName="@D15",Value=(D15==null)?"":D15},
                                    new SqlParameter{ParameterName="@D16",Value=(D16==null)?"":D16},
                                    new SqlParameter{ParameterName="@D17",Value=(D17==null)?"":D17},
                                    new SqlParameter{ParameterName="@D18",Value=(D18==null)?"":D18},
                                    new SqlParameter{ParameterName="@D19",Value=(D19==null)?"":D19},
                                    new SqlParameter{ParameterName="@D20",Value=(D20==null)?"":D20}                                                                  
            };

                var query = "proc_LogHistory @userName,@APIUrl,@ControllId,@UnitId,@RequestId,@ServiceId,@ProcessIndustryId,@ApplicationId,@StatusCode,@RequestString,@DecryptRequestString,@ResponseString,@DecryptResponseString,@token,@SessionKey,@AppicationStatusCode,@RequestKey,@DeptId,@Remarks,@PendecyLevel,@Pending_with_Officer@feeamount,@feestatus,@NOC_Certificate_Number,@NOC_Url,@IsNocUrlActiveYesNo,@Objection_Rejection_Code,@Objection_Rejection_Url,@Is_Certificate_Valid_Life_Time,@Certificate_EXP_Date_DDMMYYYY,@D1,@D2,@D3,@D4,@D5,@D6,@D7,@D8,@D9,@D10,@D11,@D12,@D13,@D14,@D15,@D16,@D17,@D18,@D19,@D20";
                result = this.Database.ExecuteSqlCommand(query, sqlParams);
                return result;
            }

            catch { return 0; }


        }
        #endregion

        #region  Nivesh Mitra API Reconciliation

        public List<returnServiceStatusResponse> GetListForReconciliation()
        {
            var sqlparams = new SqlParameter[] { 
            
            //new SqlParameter{ParameterName="@procId",Value=1},
            //new SqlParameter{ParameterName="@userId",Value=userId}
             };
            var _proc = @"procGetListForReconciliation";
            var slist = this.Database.SqlQuery<returnServiceStatusResponse>(_proc, sqlparams).ToList();
            return slist;
        }


        public int UpdateReconcilation(string ApplicationId)
        {
            int result = 0;
            var sqlparams = new SqlParameter[] { 
            
            new SqlParameter{ParameterName="@ApplicationId",Value=ApplicationId},
            //new SqlParameter{ParameterName="@userId",Value=userId}
             };
            var _proc = @"procGetListForReconciliation";
             result = this.Database.ExecuteSqlCommand(_proc, sqlparams);
            return result;
        
        }
        #endregion

    }
}