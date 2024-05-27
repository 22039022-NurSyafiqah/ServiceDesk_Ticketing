using System.ComponentModel.DataAnnotations;

namespace ServiceDesk_Ticketing.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Please enter your Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
