namespace task_manager_backend.Models;

public class TaskItem
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public string Priority { get; set; } = "medium"; // low, medium, high
    
    public DateTime DueDate { get; set; }
    
    public string Status { get; set; } = "pending"; // pending, in-progress, completed
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
