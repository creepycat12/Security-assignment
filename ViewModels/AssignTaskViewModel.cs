using System.ComponentModel.DataAnnotations;

namespace api.ViewModels;

public class AssignTaskViewModel : TodoPostViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email{ get; set; }  
}
