using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CCSHealthFamilyWelfareDept.Filters
{
    public class niveshmitra
    {

        public string u_Name { get; set; }
        public string e_data { get; set; }
        //public string sessionKey { get; set; }
        //public string bkToken { get; set; }
        //public string CToken { get; set; }
    }

    public class BackToNMSWPModel
    {
        public string API_UserId { get; set; }
        public string sessionKey { get; set; }
        public string bkToken { get; set; }
        public string CToken { get; set; }
    }


    public class SessionE_Data
    {
        public string sessionkey { get; set; }
    }



    public class AuthanitcationResponse
    {
        public string token { get; set; }
        public string refreshToken { get; set; }
        public string isSuccess { get; set; }
        public string reason { get; set; }

    }

    public class GetValidateResponse
    {
        public string enc_data { get; set; }
        public string isSuccess { get; set; }
        public string errorMsg { get; set; }

    }

    public class ResponseDecryptedData
    {

        public string ControlId { get; set; }
        public string UnitId { get; set; }
        public string DeptId { get; set; }
        public string ServiceId { get; set; }
        public string RequestId { get; set; }
        public string CToken { get; set; }
        public string bkToken { get; set; }
    }

    public class GetAppicationRequest
    {
        public string requestkey { get; set; }
    }

    public class GetAppicationResponse
    {
        public string Control_ID { get; set; }
        public string COMPANY_ENTERPRISE_NAME { get; set; }
        public string Occupier_Name { get; set; }
        public string Occupier_First_Name { get; set; }
        public string Occupier_Middle_Name { get; set; }
        public string Occupier_Last_Name { get; set; }
        public string Occupier_DOB { get; set; }
        public string Website_Name { get; set; }
        public string Occupier_PAN { get; set; }
        public string Occupier_Email_ID { get; set; }
        public string Occupier_Mobile_No { get; set; }
        public string Occupier_Father_Mother_Name { get; set; }
        public string Occupier_Gender { get; set; }
        public string Occupier_District_Name { get; set; }
        public string Occupier_Address { get; set; }
        public string Occupier_Pin_Code { get; set; }
        public string Occupier_Applied_Date { get; set; }
        public string Occupier_District_ID { get; set; }
        public string Occupier_Country_Id { get; set; }
        public string Occupier_State_Id { get; set; }

        public string Unit_Id { get; set; }
        public string Company_Name { get; set; }
        public string Nature_of_Activity { get; set; }
        public string INSTALLED_CAPACITY { get; set; }
        public string Employees { get; set; }
        public string Nature_of_Operation { get; set; }
        public string Project_Cost { get; set; }
        public string Organization_Type { get; set; }
        public string Industry_Type_Name { get; set; }
        public string Expected_date_construction { get; set; }
        public string PROJECT_STATUS { get; set; }
        public string INDUSTRY_COLOR { get; set; }
        public string Expected_date_production { get; set; }
        public string UNIT_CATEGORY { get; set; }
        public string Items_Manufactured { get; set; }
        public string Annual_Turnover { get; set; }
        public string Unit_Tehsil { get; set; }
        public string Industry_Address { get; set; }
        public string UNIT_ADDRESS { get; set; }
        public string Industry_District { get; set; }
        public string Pin_Code { get; set; }
        public string UNIT_LANDLINENO { get; set; }
        public string UNIT_FAXNO { get; set; }
        public string UNIT_MOBILENO { get; set; }
        public string UNIT_EMAILID { get; set; }
        public string UNIT_AUTHORIZED_PERSON_NAME { get; set; }
        public string UNIT_AUTHORIZED_PERSON_EMAIL { get; set; }
        public string UNIT_AUTHORIZED_PERSON_ADDRESS { get; set; }
        public string UNIT_AUTHORIZED_PERSON_MOBILENO { get; set; }
        public string UNIT_AUTHORIZED_PERSON_COMPANY_WEBSITE { get; set; }
        public string IS_PrecuredLand { get; set; }
        public string Land_AcquiredFrom_Dept_ID { get; set; }
        public string IS_Land_Purchaged { get; set; }
        public string UNIT_ORGANIZATION_NAME { get; set; }
        public string Is_New_Unit { get; set; }
        public string UNIT_APPLIED_DATE { get; set; }
        public string Industry_District_Id { get; set; }
        public string Organization_Type_ID { get; set; }
        public string Industry_Type_ID { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public string Service_ID { get; set; }
        public string SERVICE_NAME { get; set; }
        public string Request_ID { get; set; }
        public string Form_Type { get; set; }
        public string Status_Code { get; set; }
        public string Status_Name { get; set; }
        public string Status_Description { get; set; }
        public string REQUEST_DATE { get; set; }
        public string Remarks { get; set; }
        public string Dept_ID { get; set; }
        public string Trans_Status { get; set; }
        public string Transaction_ID { get; set; }
        public string TransactionDate { get; set; }
        public string Fee_Amount { get; set; }
        public string Fee_Status { get; set; }

        public string Status_Updated_Date { get; set; }
        public string Payment_ID { get; set; }

    }



    public class ApplicationAcknowledgementRequest
    {
        public string ControlId { get; set; }
        public string UnitId { get; set; }
        public string Dept_ID { get; set; }
        public string ServiceId { get; set; }
        public string RequestId { get; set; }
        public string ProcessIndustryId { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationURL { get; set; }
        public string Remarks { get; set; }
        public string Action_Taken_Time { get; set; }

        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public string D4 { get; set; }
        public string D5 { get; set; }
        public string D6 { get; set; }
        public string D7 { get; set; }
        public string D8 { get; set; }
        public string D9 { get; set; }
        public string D10 { get; set; }

        public string D11 { get; set; }
        public string D12 { get; set; }
        public string D13 { get; set; }
        public string D14 { get; set; }
        public string D15 { get; set; }
        public string D16 { get; set; }
        public string D17 { get; set; }
        public string D18 { get; set; }
        public string D19 { get; set; }
        public string D20 { get; set; }
    }

    public class ResponseResult
    {

        public string isSuccess { get; set; }
        public string errorMsg { get; set; }

    }


    public class returnServiceStatusRequest
    {
        public string ControlId { get; set; }
        public string UnitId { get; set; }
        public string DeptId { get; set; }
        public string ServiceId { get; set; }
        public string RequestId { get; set; }

        public string ApplicationId { get; set; }
        public string ProcessIndustryId { get; set; }
        public string StatusCode { get; set; }
        public string Remarks { get; set; }
        public string ApplicationURL { get; set; }
        public string PendecyLevel { get; set; }
        public string Pending_with_Officer { get; set; }
        public decimal feeamount { get; set; }
        public string feestatus { get; set; }
        public string NOC_Certificate_Number { get; set; }
        public string NOC_Url { get; set; }
        public string IsNocUrlActiveYesNo { get; set; }
        public string Objection_Rejection_Code { get; set; }
        public string Objection_Rejection_Url { get; set; }
        public string Is_Certificate_Valid_Life_Time { get; set; }
        public string Certificate_EXP_Date_DDMMYYYY { get; set; }
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public string D4 { get; set; }
        public string D5 { get; set; }
        public string D6 { get; set; }
        public string D7 { get; set; }
        public string D8 { get; set; }
        public string D9 { get; set; }
        public string D10 { get; set; }

        public string D11 { get; set; }
        public string D12 { get; set; }
        public string D13 { get; set; }
        public string D14 { get; set; }
        public string D15 { get; set; }
        public string D16 { get; set; }
        public string D17 { get; set; }
        public string D18 { get; set; }
        public string D19 { get; set; }
        public string D20 { get; set; }

        public List<returnServiceStatusRequest> returnServiceStatusRequestList { get; set; }
        //public string SendDate { get; set; }
        //public string ResStatus { get; set; }
        //public string ServiceStatus { get; set; }
        //public int StepId { get; set; }

    }

    public class returnServiceStatusResponse
    {
        public string ControlId { get; set; }
        public string UnitId { get; set; }
        public string DeptId { get; set; }
        public string ServiceId { get; set; }
        public string RequestId { get; set; }

        public string ApplicationId { get; set; }
        public string ProcessIndustryId { get; set; }
        public string StatusCode { get; set; }
        public string Remarks { get; set; }
        public string ApplicationURL { get; set; }
        public string PendecyLevel { get; set; }
        public string Pending_with_Officer { get; set; }
        public decimal feeamount { get; set; }
        public string feestatus { get; set; }
        public string NOC_Certificate_Number { get; set; }
        public string NOC_Url { get; set; }
        public string IsNocUrlActiveYesNo { get; set; }
        public string Objection_Rejection_Code { get; set; }
        public string Objection_Rejection_Url { get; set; }
        public string Is_Certificate_Valid_Life_Time { get; set; }
        public string Certificate_EXP_Date_DDMMYYYY { get; set; }
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public string D4 { get; set; }
        public string D5 { get; set; }
        public string D6 { get; set; }
        public string D7 { get; set; }
        public string D8 { get; set; }
        public string D9 { get; set; }
        public string D10 { get; set; }

        public string D11 { get; set; }
        public string D12 { get; set; }
        public string D13 { get; set; }
        public string D14 { get; set; }
        public string D15 { get; set; }
        public string D16 { get; set; }
        public string D17 { get; set; }
        public string D18 { get; set; }
        public string D19 { get; set; }
        public string D20 { get; set; }
        public List<returnServiceStatusResponse> returnServiceStatusResponseList { get; set; }

        public DateTime SendDate { get; set; }
        public string ResStatus { get; set; }
        public string ServiceStatus { get; set; }
        public int StepId { get; set; }



    }
}