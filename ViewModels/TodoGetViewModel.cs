namespace api.ViewModels;

public class TodoGetViewModel
{
    public int Id { get; set; }
    public string Task { get; set; }
    public DateTime DueDate { get; set; }
    public bool Complete { get; set; }
}
