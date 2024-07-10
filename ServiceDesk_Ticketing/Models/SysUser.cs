using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace ServiceDesk_Ticketing.Models;
public class SysUser
{
    public int User_ID { get; set; }

    [Required(ErrorMessage = "Please enter Full Name")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Please select the roles")]
    public string User_Role_Name { get; set; } = null!;

    [Required(ErrorMessage = "Please enter valid NRIC Number")]
    public string IC_num { get; set; } = null!;

    [Required(ErrorMessage = "Please enter a valid phone number")]
    [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "Phone number must be exactly 8 digits")]
    public string PhoneNumber { get; set; } = null!;


    [Required(ErrorMessage = "Please enter Email Address")]
    [EmailAddress(ErrorMessage = "Invalid Email")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@huamin\.edu\.sg$", ErrorMessage = "Email must be from huamin.edu.sg domain")]
    public string EmailAddress { get; set; } = null!;

    public bool IsActive { get; set; } = true; // Default to active

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Please enter Password")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Password must be 5 characters or more")]
    public string UserPw { get; set; } = null!;

    public DateTime LastLogin { get; set; }
}