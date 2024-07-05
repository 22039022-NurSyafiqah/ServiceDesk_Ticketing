using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ServiceDesk_Ticketing.Models;

public class SysUser
{
    public int User_ID { get; set; }

    public int User_Role_ID { get; set; }

    public string User_Role_Name { get; set; } = null!;

    [Required(ErrorMessage = "Please enter Full Name")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Please enter Valid NRIC Number")]
    public string IC_Num { get; set; } = null!;

    [Required(ErrorMessage = "Please enter Email")]
    [EmailAddress(ErrorMessage = "Invalid Email")]
    public string EmailAddress { get; set; } = null!;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Please enter Password")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Password must be 5 characters or more")]
    public string Password { get; set; } = null!;

    public DateTime LastLogin { get; set; }


}
