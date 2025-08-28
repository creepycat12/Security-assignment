using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.ViewModels;

 public class UserRegisterViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(80, ErrorMessage = "Email cannot exceed 80 characters.")]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(30, ErrorMessage = "Password cannot exceed 30 characters.")]
        public string Password { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }
    }
