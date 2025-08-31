using System.ComponentModel.DataAnnotations;

namespace api.ViewModels;

public class TodoPostViewModel
{
    [Required]
    [MaxLength(80, ErrorMessage = "Title cannot exceed 80 characters.")]
    public string Task { get; set; }

    public DateTime? DueDate { get; set; }

    
}