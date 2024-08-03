using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ServiceDesk_Ticketing.Models
{
    public class Home
    {
        public int Homeid { get; set; }

        [Required(ErrorMessage = "Enter a description of the issue")]
        [StringLength(500, ErrorMessage = "Maximum is 500 characters")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Enter the Sticker Tag")]
        [StringLength(20, ErrorMessage = "Maximum is 20 characters")]
        public string StickerTag { get; set; } = null!;

        [Required(ErrorMessage = "Enter the Serial Number")]
        [StringLength(20, ErrorMessage = "Maximum is 20 characters")]
        public string SerialNumber { get; set; } = null!;

        [Required(ErrorMessage = "Enter the date of the issue")]
        [DataType(DataType.Date)]
        public DateTime DateOfIssue { get; set; }

        [Required(ErrorMessage = "Enter the time of the issue")]
        [DataType(DataType.Time)]
        public TimeSpan TimeOfIssue { get; set; }

        [Required(ErrorMessage = "Enter the venue of the issue")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string Venue { get; set; } = null!;

        [Required(ErrorMessage = "Upload the photo")]
        [DataType(DataType.Upload)]
        public IFormFile? Photo { get; set; }

        [Required(ErrorMessage = "Select at least one equipment type")]
        public List<EquipmentTypeEnum> EquipmentType { get; set; } = new List<EquipmentTypeEnum>();

        public enum EquipmentTypeEnum
        {
            ProjectorPatchPanel,
            Visualizers,
            Laptop,
            iPad,
            AppleTV
        }

        [Required(ErrorMessage = "Enter the IC number")]
        [StringLength(20, ErrorMessage = "Maximum is 20 characters")]
        public string ICNum { get; set; } = null!;

        [Required(ErrorMessage = "Enter the quantity to increase")]
        public int IncreaseQty { get; set; }

        [Required(ErrorMessage = "Enter the reason for increase")]
        [StringLength(200, ErrorMessage = "Maximum is 200 characters")]
        public string ReasonForIncrease { get; set; } = null!;

        // New fields for Account Activation for New Staff
        [Required(ErrorMessage = "Enter the full name")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Enter the NRIC")]
        [StringLength(20, ErrorMessage = "Maximum is 20 characters")]
        public string Nric { get; set; } = null!;

        [Required(ErrorMessage = "Select the employment type")]
        public string EmploymentType { get; set; } = null!;

        [Required(ErrorMessage = "Select the app required")]
        public string AppRequired { get; set; } = null!;

        [Required(ErrorMessage = "Enter the start date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }



        // New fields for Request for Equipment
        [Required(ErrorMessage = "Select at least one equipment")]
        public string EquipmentSelection { get; set; } = null!;

        [Required(ErrorMessage = "Enter the quantity")]
        public int EquipmentQuantity { get; set; }

        [Required(ErrorMessage = "Enter the venue")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string EquipmentVenue { get; set; } = null!;

        [Required(ErrorMessage = "Enter the reason for the request")]
        [StringLength(500, ErrorMessage = "Maximum is 500 characters")]
        public string ReasonForRequest { get; set; } = null!;

        // New fields for Facebook Post Session
        [Required(ErrorMessage = "Select picture/compiled document to upload")]
        public List<IFormFile> Photos { get; set; } = new List<IFormFile>();

        [Required(ErrorMessage = "Enter a caption for each photo")]
        [StringLength(200, ErrorMessage = "Maximum is 200 characters")]
        public string Caption { get; set; } = null!;

        // New fields for Website Update Session
        [Required(ErrorMessage = "Select a document to upload")]
        public List<IFormFile> Document { get; set; } = new List<IFormFile>();

        [Required(ErrorMessage = "Enter the URL links to update")]
        [StringLength(500, ErrorMessage = "Maximum is 500 characters")]
        public string UrlLinks { get; set; } = null!;

        [Required(ErrorMessage = "Select any attachments to upload")]
        public IFormFile? Attachment { get; set; }

        [Required(ErrorMessage = "Enter the date of the update")]
        [DataType(DataType.Date)]
        public DateTime DateOfUpdate { get; set; }

        // Fields for App/Software Installation
        [Required(ErrorMessage = "Select the installation type")]
        public string InstallationType { get; set; } = null!;

        // App Installation on iPad fields
        [Required(ErrorMessage = "Enter the application name")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string ApplicationName { get; set; } = null!;

        [Required(ErrorMessage = "Select the subscription type")]
        public List<string> SubscriptionType { get; set; } = new List<string>();

        [Required(ErrorMessage = "Enter the paid amount")]
        [StringLength(100, ErrorMessage = "If free,type NA")]
        public string? PaidAmount { get; set; }

        [Required(ErrorMessage = "Enter the quantity")]
        public int AppQuantity { get; set; }

        // Software Installation on SSOE devices fields
        [Required(ErrorMessage = "Enter the software name")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string SoftwareName { get; set; } = null!;

        [Required(ErrorMessage = "Enter the paid amount")]
        [StringLength(100, ErrorMessage = "If free,type NA")]
        public string? PaidAmount2 { get; set; }

        [Required(ErrorMessage = "Enter the purpose of installation")]
        [StringLength(200, ErrorMessage = "Maximum is 200 characters")]
        public string PurposeOfInstallation { get; set; } = null!;

        [Required(ErrorMessage = "Select where to install")]
        public string WhereToInstall { get; set; } = null!;

        [Required(ErrorMessage = "Enter the implementation date")]
        [DataType(DataType.Date)]
        public DateTime ImplementationDate { get; set; }

        [Required(ErrorMessage = "Enter the upload software link")]
        [StringLength(200, ErrorMessage = "Maximum is 200 characters")]
        public string UploadSoftware { get; set; } = null!;

        [Required(ErrorMessage = "Enter the manual guide link")]
        [StringLength(200, ErrorMessage = "Maximum is 200 characters")]
        public string ManualGuide { get; set; } = null!;

        // New fields for Event/ICT Support
        [Required(ErrorMessage = "Select the support type")]
        public string SupportType { get; set; } = null!;

        [Required(ErrorMessage = "Select the event support type")]
        public List<string> EventSupportType { get; set; } = new List<string>();

        [Required(ErrorMessage = "Describe the photo type")]
        [StringLength(500, ErrorMessage = "Maximum is 500 characters")]
        public string PhotoDescription { get; set; } = null!;

        [Required(ErrorMessage = "Enter the name of the event")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string NameOfEvent { get; set; } = null!;

        [Required(ErrorMessage = "Enter the date of the event")]
        [DataType(DataType.Date)]
        public DateTime DateOfEvent { get; set; }

        [Required(ErrorMessage = "Enter the time of the event")]
        [StringLength(50, ErrorMessage = "Maximum is 50 characters")]
        public string TimeOfEvent { get; set; } = null!;

        [Required(ErrorMessage = "Enter the event incharge")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string EventIncharge { get; set; } = null!;

        [Required(ErrorMessage = "Select if there is any rehearsal")]
        public string AnyRehearsal { get; set; } = null!;

        [Required(ErrorMessage = "Enter the date of rehearsal")]
        [DataType(DataType.Date)]
        public DateTime DateOfRehearsal { get; set; }

        [Required(ErrorMessage = "Enter the time of rehearsal")]
        [StringLength(50, ErrorMessage = "Maximum is 50 characters")]
        public string TimeOfRehearsal { get; set; } = null!;

        [Required(ErrorMessage = "Select the ICT event type")]
        public List<string> ICTEventType { get; set; } = new List<string>();

        [Required(ErrorMessage = "Select the primary level")]
        public List<int> PrimaryLevel { get; set; } = new List<int>();

        [Required(ErrorMessage = "Select the logistic request")]
        public List<string> LogisticRequest { get; set; } = new List<string>();

        [Required(ErrorMessage = "Enter the location")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string Location { get; set; } = null!;

        [Required(ErrorMessage = "Select the equipment needed")]
        public List<string> EquipmentNeeded { get; set; } = new List<string>();

        [Required(ErrorMessage = "Please select Equipment Type")]
        public string EquipmentReqType { get; set; } = null!;







        // New fields for Account Activation for New Staff
        public int AccID { get; set; }


        [Required(ErrorMessage = "Enter your Full Name")]
        [StringLength(100, ErrorMessage = "Maximum is 100 characters")]
        public string NewAcc_NewStaffName { get; set; } = null!;

        [Required(ErrorMessage = "Enter your NRIC Number")]
        [StringLength(9, ErrorMessage = "Maximum is 9 characters")]
        [RegularExpression(@"^[STFG]\d{7}[A-Z]$", ErrorMessage = "NRIC number must start with 'S', 'T', 'F', or 'G', followed by 7 digits, and end with an alphabetic character.")]
        [DataType(DataType.Password)]
        public string NewAcc_NewStaffNRIC { get; set; } = null!;

        public int EmpID { get; set; }

        public string Emp_Type { get; set; } = null!;


        public int AppReqID { get; set; }

        public string AppName { get; set; } = null!;


        [Required(ErrorMessage = "Enter the Start Date")]
        [DataType(DataType.Date)]
        public DateTime NewAcc_StartDate { get; set; }

    }
}