using CCSHealthFamilyWelfareDept.Filters;
using CCSHealthFamilyWelfareDept.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace CCSHealthFamilyWelfareDept.DAL
{
    public class NiveshMitraAPI
    {
        SessionManager objSM = new SessionManager();
        Common_DB commonDB = new Common_DB();
        public string HitAuthorizationAPI()
        {
            string userName = ConfigurationManager.AppSettings["NiveshMitrauserName"].ToString();
            string password = ConfigurationManager.AppSettings["NiveshMitrapassword"].ToString();

            string NiveshMitraAuthenticateURL = ConfigurationManager.AppSettings["NiveshMitraAuthenticateURL"].ToString();
            var _parameterList = new Dictionary<string, object>();


            _parameterList.Add("userName", userName);
            _parameterList.Add("password", password);


            String data = JsonConvert.SerializeObject(_parameterList);
            string httpUrl = NiveshMitraAuthenticateURL;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(httpUrl);
            httpWebRequest.ContentType = "application/json";
            var _request = data;
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(_request);
                streamWriter.Flush();
            }
            string _resCode = string.Empty;
            string _output = string.Empty;
            HttpWebResponse httpResponse = null;
            commonDB.APIRequestResponseLogHistory(userName, NiveshMitraAuthenticateURL, "", "", "", "", "", "", "", "", _request, "", "", "", "", "", "","","","","",0,"","","","","","","","","","","","","","","","","","","","","","","","","","","","");
            try
            {
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                _resCode = Convert.ToString(httpResponse.StatusCode);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    _output = streamReader.ReadToEnd();
                    // called log method
                    commonDB.APIRequestResponseLogHistory(userName, NiveshMitraAuthenticateURL, "", "", "", "", "", "", _resCode, "", "", _output, "", "", "", "", "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        _resCode = Convert.ToString(response.StatusCode);
                        _output = response.StatusDescription.ToString();
                        // called log method
                        commonDB.APIRequestResponseLogHistory(userName, NiveshMitraAuthenticateURL, "", "", "", "", "", "", _resCode, "", "", "", _output, "", "", "", "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    }
                    else
                    {
                        _resCode = "No Code Available #1";
                        _output = "";
                    }
                }
                else
                {
                    _resCode = "No Code Available #2";
                    _output = "";
                }

            }


            return _output;

        }



        public string getSessionRequestValidated(string sessionkey = "")
        {

            string _resCode = string.Empty;
            string _output = string.Empty;
            string userName = ConfigurationManager.AppSettings["NiveshMitrauserName"].ToString();
            string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
            string RequestValidatedURL = ConfigurationManager.AppSettings["NiveshMitragetRequestValidatedURL"].ToString();
            if (sessionkey != "")
            {
                AuthanitcationResponse Auth = new AuthanitcationResponse();
                string accessdata = HitAuthorizationAPI();
                var dat = JsonConvert.DeserializeObject(accessdata);
                Auth = JsonConvert.DeserializeObject<AuthanitcationResponse>(accessdata);
                string AccessToca = Auth.token;


                SessionE_Data Objpayload = new SessionE_Data() { sessionkey = sessionkey };
                JavaScriptSerializer jss = new JavaScriptSerializer();
                var sObj = jss.Serialize(Objpayload);
                var data = NiveshMitraEncryptionDecryption.Encrypt(sObj, EncryptionKey);

                niveshmitra nivesh = new niveshmitra() { u_Name = userName, e_data = data };
                var pairdata = jss.Serialize(nivesh);

                string SessionURL = RequestValidatedURL;

                string httpUrl = SessionURL;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(httpUrl);
                httpWebRequest.ContentType = "application/json";
                var _request = pairdata;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + AccessToca);
                //httpWebRequest.UseDefaultCredentials = true;

                // called log method
                commonDB.APIRequestResponseLogHistory(userName, SessionURL, "", "", "", "", "", "", "", "", _request, "", "", AccessToca, sessionkey, "", "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(_request);
                    streamWriter.Flush();
                }

                HttpWebResponse httpResponse = null;
                try
                {
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    _resCode = Convert.ToString(httpResponse.StatusCode);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _output = streamReader.ReadToEnd();
                        // called log method
                        commonDB.APIRequestResponseLogHistory(userName, SessionURL, "", "", "", "", "", "", _resCode, "", "", _output, "", AccessToca, sessionkey,"", "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            _resCode = Convert.ToString(response.StatusCode);
                            _output = response.StatusDescription.ToString();
                            // called log method
                            commonDB.APIRequestResponseLogHistory(userName, SessionURL, "", "", "", "", "", "", _resCode, "", "", "", _output, AccessToca, sessionkey, "", "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                        }
                        else
                        {
                            _resCode = "No Code Available #1";
                            _output = "";
                        }
                    }
                    else
                    {
                        _resCode = "No Code Available #2";
                        _output = "";
                    }

                }

                return _output;
                // niveshmitra Objpayload = JsonConvert.DeserializeObject<niveshmitra>(someObject);
                //string dicriptstring = Objpayload.data;

            }
            return _output;


        }




        public string getApplicationDetails(string requestkey = "")
        {


            string _resCode = string.Empty;
            string _output = string.Empty;
            string userName = ConfigurationManager.AppSettings["NiveshMitrauserName"].ToString();
            string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
            string RequestValidatedURL = ConfigurationManager.AppSettings["NiveshMitraGetAppicationDetailURL"].ToString();
            if (requestkey != "")
            {
                AuthanitcationResponse Auth = new AuthanitcationResponse();
                string accessdata = HitAuthorizationAPI();
                var dat = JsonConvert.DeserializeObject(accessdata);
                Auth = JsonConvert.DeserializeObject<AuthanitcationResponse>(accessdata);
                string AccessToca = Auth.token;


                GetAppicationRequest Objpayload = new GetAppicationRequest() { requestkey = requestkey };
                JavaScriptSerializer jss = new JavaScriptSerializer();
                var sObj = jss.Serialize(Objpayload);
                var data = NiveshMitraEncryptionDecryption.Encrypt(sObj, EncryptionKey);

                niveshmitra nivesh = new niveshmitra() { u_Name = userName, e_data = data };
                var pairdata = jss.Serialize(nivesh);

                string SessionURL = RequestValidatedURL;

                string httpUrl = SessionURL;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(httpUrl);
                httpWebRequest.ContentType = "application/json";
                var _request = pairdata;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + AccessToca);
                httpWebRequest.Headers.Add("CToken", objSM.CToken);
                //httpWebRequest.UseDefaultCredentials = true;
                // called log method
                commonDB.APIRequestResponseLogHistory(userName, SessionURL, "", "", "", "", "", "", "", "", _request, "", "", AccessToca, "", "", requestkey, "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");


                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(_request);
                    streamWriter.Flush();
                }

                HttpWebResponse httpResponse = null;
                try
                {
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    _resCode = Convert.ToString(httpResponse.StatusCode);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _output = streamReader.ReadToEnd();
                        // called log method
                        commonDB.APIRequestResponseLogHistory(userName, SessionURL, "", "", "", "", "", "", _resCode, "", "", _output, "", AccessToca, "", "", requestkey, "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            _resCode = Convert.ToString(response.StatusCode);
                            _output = response.StatusDescription.ToString();
                            // called log method
                            commonDB.APIRequestResponseLogHistory(userName, SessionURL, "", "", "", "", "", "", _resCode, "", "", "", _output, AccessToca, "", "", requestkey, "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                        }
                        else
                        {
                            _resCode = "No Code Available #1";
                            _output = "";
                        }
                    }
                    else
                    {
                        _resCode = "No Code Available #2";
                        _output = "";
                    }

                }

                return _output;
                // niveshmitra Objpayload = JsonConvert.DeserializeObject<niveshmitra>(someObject);
                //string dicriptstring = Objpayload.data;

            }
            return _output;


        }


        public string getApplicationAcknowledgement(ApplicationAcknowledgementRequest modelReq)
        {

            string _resCode = string.Empty;
            string _output = string.Empty;
            string userName = ConfigurationManager.AppSettings["NiveshMitrauserName"].ToString();
            string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
            string RequestValidatedURL = ConfigurationManager.AppSettings["ApplicationAcknowledgementURL"].ToString();
            if (modelReq.ControlId != "")
            {
                AuthanitcationResponse Auth = new AuthanitcationResponse();
                string accessdata = HitAuthorizationAPI();
                var dat = JsonConvert.DeserializeObject(accessdata);
                Auth = JsonConvert.DeserializeObject<AuthanitcationResponse>(accessdata);
                string AccessToca = Auth.token;



                JavaScriptSerializer jss = new JavaScriptSerializer();
                var sObj = jss.Serialize(modelReq);
                var data = NiveshMitraEncryptionDecryption.Encrypt(sObj, EncryptionKey);

                niveshmitra nivesh = new niveshmitra() { u_Name = userName, e_data = data };
                var pairdata = jss.Serialize(nivesh);

                string SessionURL = RequestValidatedURL;

                string httpUrl = SessionURL;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(httpUrl);
                httpWebRequest.ContentType = "application/json";
                var _request = pairdata;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + AccessToca);
                httpWebRequest.Headers.Add("CToken", objSM.CToken);
                //httpWebRequest.UseDefaultCredentials = true;
                // called log method
                commonDB.APIRequestResponseLogHistory(userName, RequestValidatedURL, modelReq.ControlId, modelReq.UnitId, modelReq.RequestId, modelReq.ServiceId, modelReq.ProcessIndustryId, modelReq.ApplicationId, _resCode, "", _request, "", "", "", "", "", "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(_request);
                    streamWriter.Flush();
                }

                HttpWebResponse httpResponse = null;
                try
                {
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    _resCode = Convert.ToString(httpResponse.StatusCode);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _output = streamReader.ReadToEnd();
                        // called log method
                        commonDB.APIRequestResponseLogHistory(userName, RequestValidatedURL, modelReq.ControlId, modelReq.UnitId, modelReq.RequestId, modelReq.ServiceId, modelReq.ProcessIndustryId, modelReq.ApplicationId, _resCode, "", "", _output, "", "", "", "", "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            _resCode = Convert.ToString(response.StatusCode);
                            _output = response.StatusDescription.ToString();
                            // called log method
                            commonDB.APIRequestResponseLogHistory(userName, RequestValidatedURL, modelReq.ControlId, modelReq.UnitId, modelReq.RequestId, modelReq.ServiceId, modelReq.ProcessIndustryId, modelReq.ApplicationId, _resCode, "", "", "", _output, "", "", "", "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                        }
                        else
                        {
                            _resCode = "No Code Available #1";
                            _output = "";
                        }
                    }
                    else
                    {
                        _resCode = "No Code Available #2";
                        _output = "";
                    }

                }

                return _output;
                // niveshmitra Objpayload = JsonConvert.DeserializeObject<niveshmitra>(someObject);
                //string dicriptstring = Objpayload.data;

            }
            return _output;


        }


        public string returnServiceStatus(returnServiceStatusRequest modelReq)
        {
            string _resCode = string.Empty;
            string _output = string.Empty;
            string userName = ConfigurationManager.AppSettings["NiveshMitrauserName"].ToString();
            string EncryptionKey = ConfigurationManager.AppSettings["NiveshMitraEncrptionKey"].ToString();
            string RequestValidatedURL = ConfigurationManager.AppSettings["returnServiceStatusURL"].ToString();
            if (modelReq.ControlId != "")
            {
                AuthanitcationResponse Auth = new AuthanitcationResponse();
                string accessdata = HitAuthorizationAPI();
                var dat = JsonConvert.DeserializeObject(accessdata);
                Auth = JsonConvert.DeserializeObject<AuthanitcationResponse>(accessdata);
                string AccessToca = Auth.token;

                JavaScriptSerializer jss = new JavaScriptSerializer();
                var sObj = jss.Serialize(modelReq);
                var data = NiveshMitraEncryptionDecryption.Encrypt(sObj, EncryptionKey);

                niveshmitra nivesh = new niveshmitra() { u_Name = userName, e_data = data };
                var pairdata = jss.Serialize(nivesh);

                string SessionURL = RequestValidatedURL;

                string httpUrl = SessionURL;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(httpUrl);
                httpWebRequest.ContentType = "application/json";
                var _request = pairdata;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + AccessToca);
                //httpWebRequest.Headers.Add("CToken", objSM.CToken);
                //httpWebRequest.UseDefaultCredentials = true;
                commonDB.APIRequestResponseLogHistory(userName, RequestValidatedURL, modelReq.ControlId, modelReq.UnitId, modelReq.RequestId, modelReq.ServiceId, modelReq.ProcessIndustryId, modelReq.ApplicationId, _resCode, "", _request, "", "", AccessToca, "", modelReq.StatusCode, "", "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(_request);
                    streamWriter.Flush();
                }

                HttpWebResponse httpResponse = null;
                try
                {
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    _resCode = Convert.ToString(httpResponse.StatusCode);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        _output = streamReader.ReadToEnd();
                        commonDB.APIRequestResponseLogHistory(userName, RequestValidatedURL, modelReq.ControlId, modelReq.UnitId, modelReq.RequestId, modelReq.ServiceId, modelReq.ProcessIndustryId, modelReq.ApplicationId, _resCode, "", "", _output, "", AccessToca, "", modelReq.StatusCode, "",modelReq.DeptId,modelReq.Remarks,modelReq.PendecyLevel,modelReq.Pending_with_Officer,modelReq.feeamount,modelReq.feestatus,modelReq.NOC_Certificate_Number,modelReq.NOC_Url,modelReq.IsNocUrlActiveYesNo,modelReq.Objection_Rejection_Code,modelReq.Objection_Rejection_Url,modelReq.Is_Certificate_Valid_Life_Time,modelReq.Certificate_EXP_Date_DDMMYYYY,modelReq.D1,modelReq.D2,modelReq.D3,modelReq.D4,modelReq.D5,modelReq.D6,modelReq.D7, modelReq.D8,modelReq.D9,modelReq.D10,modelReq.D11,modelReq.D12,modelReq.D13,modelReq.D14,modelReq.D15,modelReq.D16,modelReq.D17,modelReq.D18,modelReq.D19, modelReq.D20);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            _resCode = Convert.ToString(response.StatusCode);
                            _output = response.StatusDescription.ToString();
                            commonDB.APIRequestResponseLogHistory(userName, RequestValidatedURL, modelReq.ControlId, modelReq.UnitId, modelReq.RequestId, modelReq.ServiceId, modelReq.ProcessIndustryId, modelReq.ApplicationId, _resCode, "", "", "", _output, AccessToca, "",modelReq.StatusCode, "",modelReq.DeptId,modelReq.Remarks,modelReq.PendecyLevel,modelReq.Pending_with_Officer,modelReq.feeamount,modelReq.feestatus,modelReq.NOC_Certificate_Number,modelReq.NOC_Url,modelReq.IsNocUrlActiveYesNo,modelReq.Objection_Rejection_Code,modelReq.Objection_Rejection_Url,modelReq.Is_Certificate_Valid_Life_Time,modelReq.Certificate_EXP_Date_DDMMYYYY,modelReq.D1,modelReq.D2,modelReq.D3,modelReq.D4,modelReq.D5,modelReq.D6,modelReq.D7, modelReq.D8,modelReq.D9,modelReq.D10,modelReq.D11,modelReq.D12,modelReq.D13,modelReq.D14,modelReq.D15,modelReq.D16,modelReq.D17,modelReq.D18,modelReq.D19, modelReq.D20);
                        }
                        else
                        {
                            _resCode = "No Code Available #1";
                            _output = "";
                        }
                    }
                    else
                    {
                        _resCode = "No Code Available #2";
                        _output = "";
                    }

                }

                return _output;
                // niveshmitra Objpayload = JsonConvert.DeserializeObject<niveshmitra>(someObject);
                //string dicriptstring = Objpayload.data;

            }
            return _output;


        }
    }
}