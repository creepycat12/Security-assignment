namespace api.ViewModels;

public class TaskViewModel
{
    public int Id { get; set; }
    public string Task { get; set; }
    public DateTime? DueDate { get; set; }
    public bool Complete { get; set; }
    public string UserId { get; set; }
}
