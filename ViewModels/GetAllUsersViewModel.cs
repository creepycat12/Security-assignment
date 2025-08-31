namespace api.ViewModels;

public class GetAllUsersViewModel
{
    public string Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public List<TodoGetViewModel> Tasks { get; set; }
}

