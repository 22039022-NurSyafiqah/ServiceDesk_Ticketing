using System.ComponentModel.DataAnnotations;

namespace ServiceDesk_Ticketing.Models;

public class UserLogin
{
    [Required(ErrorMessage = "Please enter Email Address")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [DataType(DataType.EmailAddress)]
    public string email { get; set; } = null!;

    [Required(ErrorMessage = "Please enter Password")]
    [DataType(DataType.Password)]
    public string password { get; set; } = null!;
}
