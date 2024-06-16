using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ServiceDesk_Ticketing.Models;

public class SysUser
{

    [Required(ErrorMessage = "Please enter Email")]
    [EmailAddress(ErrorMessage = "Invalid Email")]
    public string Email { get; set; } = null!;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Please enter Password")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Password must be 5 characters or more")]
    public string UserPw { get; set; } = null!;

    [Required(ErrorMessage = "Please enter Full Name")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Please enter User ID")]
    public string UserId { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public DateTime LastLogin { get; set; }
}
