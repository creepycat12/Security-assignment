using System.ComponentModel.DataAnnotations;

namespace api.ViewModels;

public class LoginViewModel 
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [MaxLength(80, ErrorMessage = "Email cannot exceed 80 characters.")]
    public string Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(30, ErrorMessage = "Password cannot be longer than 30 characters.")]
    public string Password { get; set; }
}