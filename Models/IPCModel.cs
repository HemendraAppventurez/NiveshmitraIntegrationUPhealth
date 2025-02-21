using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CCSHealthFamilyWelfareDept.Models
{
    public class IPCModel
    {
    }

    public class IPCApplicationform
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "Enter Name of Establishment")]
        public string NameEstablishment { get; set; }
        [Required(ErrorMessage = "Enter Name of Incharge/Owner")]
        public string NameofIncharge { get; set; }
        [Required(ErrorMessage = "Enter Address")]
        public string EstablishmentAddress { get; set; }
        [Required(ErrorMessage = "Enter Contact No")]
        public string ContactIncharge { get; set; }
        [Required(ErrorMessage = "Select Action")]
        public int ActionTakenId { get; set; }

        [Required(ErrorMessage = "Enter Email Id")]
        public string EmailId { get; set; }
        public string ActionTaken { get; set; }
        [Required(ErrorMessage = "Upload File")]
        public string UploadFile { get; set; }
        public string UploadFilePath { get; set; }
        [Required(ErrorMessage = "Enter Remark")]
        public string Remark { get; set; }
        [Required(ErrorMessage = "Enter Inspection Date")]
        public string InspectionDate { get; set; }
        public string CreateBy { get; set; }
        public long DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string CreateDate { get; set; }
    }

    public class IPCModelResponce
    {

        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public long totalInspection { get; set; }
        public long totalNotice { get; set; }
        public long totalSealOrder { get; set; }
        public long totalFIR { get; set; }
        public long TotalLicenseRejected { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public long rollId { get; set; }

    }


    public class IPCModelDetailResponce
    { }
}