namespace task_manager_backend.Models;

public class Employee
{
    public int Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Phone { get; set; } = string.Empty;
    
    public string Department { get; set; } = string.Empty;
    
    public string Designation { get; set; } = string.Empty;
    
    public decimal Salary { get; set; }
    
    public DateTime JoinDate { get; set; }
    
    public string Status { get; set; } = "active"; // active, inactive, on-leave
    
    public string Location { get; set; } = string.Empty;
    
    public int? ManagerId { get; set; }
    
    public string? Avatar { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
