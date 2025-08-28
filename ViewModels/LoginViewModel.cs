using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.ViewModels;

public class LoginViewModel 
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [MaxLength(80, ErrorMessage = "Email cannot exceed 80 characters.")]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}