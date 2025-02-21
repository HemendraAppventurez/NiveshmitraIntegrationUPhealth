﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace CCSHealthFamilyWelfareDept.EDistrictSVC {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ServiceSoap", Namespace="http://tempuri.org/")]
    public partial class Service : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback SendRequestOperationCompleted;
        
        private System.Threading.SendOrPostCallback SendResponseOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Service() {
            this.Url = global::CCSHealthFamilyWelfareDept.Properties.Settings.Default.CCSHealthFamilyWelfareDept_EDistrictSVC_Service;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event SendRequestCompletedEventHandler SendRequestCompleted;
        
        /// <remarks/>
        public event SendResponseCompletedEventHandler SendResponseCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/SendRequest", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string SendRequest(string RequestKey, string DeptRegistraionID) {
            object[] results = this.Invoke("SendRequest", new object[] {
                        RequestKey,
                        DeptRegistraionID});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void SendRequestAsync(string RequestKey, string DeptRegistraionID) {
            this.SendRequestAsync(RequestKey, DeptRegistraionID, null);
        }
        
        /// <remarks/>
        public void SendRequestAsync(string RequestKey, string DeptRegistraionID, object userState) {
            if ((this.SendRequestOperationCompleted == null)) {
                this.SendRequestOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSendRequestOperationCompleted);
            }
            this.InvokeAsync("SendRequest", new object[] {
                        RequestKey,
                        DeptRegistraionID}, this.SendRequestOperationCompleted, userState);
        }
        
        private void OnSendRequestOperationCompleted(object arg) {
            if ((this.SendRequestCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SendRequestCompleted(this, new SendRequestCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/SendResponse", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string SendResponse(string RequestKey, string DeptRegistraionID, string ApplicationNo, string serviceCode) {
            object[] results = this.Invoke("SendResponse", new object[] {
                        RequestKey,
                        DeptRegistraionID,
                        ApplicationNo,
                        serviceCode});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void SendResponseAsync(string RequestKey, string DeptRegistraionID, string ApplicationNo, string serviceCode) {
            this.SendResponseAsync(RequestKey, DeptRegistraionID, ApplicationNo, serviceCode, null);
        }
        
        /// <remarks/>
        public void SendResponseAsync(string RequestKey, string DeptRegistraionID, string ApplicationNo, string serviceCode, object userState) {
            if ((this.SendResponseOperationCompleted == null)) {
                this.SendResponseOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSendResponseOperationCompleted);
            }
            this.InvokeAsync("SendResponse", new object[] {
                        RequestKey,
                        DeptRegistraionID,
                        ApplicationNo,
                        serviceCode}, this.SendResponseOperationCompleted, userState);
        }
        
        private void OnSendResponseOperationCompleted(object arg) {
            if ((this.SendResponseCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SendResponseCompleted(this, new SendResponseCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void SendRequestCompletedEventHandler(object sender, SendRequestCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SendRequestCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal SendRequestCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void SendResponseCompletedEventHandler(object sender, SendResponseCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SendResponseCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal SendResponseCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591