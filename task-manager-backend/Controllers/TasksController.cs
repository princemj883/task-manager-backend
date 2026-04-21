using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_manager_backend.Data;
using task_manager_backend.Models;

namespace task_manager_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController(TaskManagerContext context, ILogger<TasksController> logger)
    : ControllerBase
{
    /// <summary>
    /// Get all tasks
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
        try
        {
            var tasks = await context.Tasks.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tasks");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific task by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        try
        {
            var task = await context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound(new { message = $"Task with id {id} not found" });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting task");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskDto createTaskDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createTaskDto.Title))
            {
                return BadRequest(new { message = "Title is required" });
            }

            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description ?? string.Empty,
                Category = createTaskDto.Category ?? string.Empty,
                Priority = createTaskDto.Priority ?? "medium",
                DueDate = createTaskDto.DueDate,
                Status = createTaskDto.Status ?? "pending",
                CreatedAt = DateTime.UtcNow
            };

            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItem>> UpdateTask(int id, UpdateTaskDto updateTaskDto)
    {
        try
        {
            var task = await context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound(new { message = $"Task with id {id} not found" });
            }

            // Update only the provided fields
            if (!string.IsNullOrWhiteSpace(updateTaskDto.Title))
                task.Title = updateTaskDto.Title;

            if (!string.IsNullOrWhiteSpace(updateTaskDto.Description))
                task.Description = updateTaskDto.Description;

            if (!string.IsNullOrWhiteSpace(updateTaskDto.Category))
                task.Category = updateTaskDto.Category;

            if (!string.IsNullOrWhiteSpace(updateTaskDto.Priority))
                task.Priority = updateTaskDto.Priority;

            if (updateTaskDto.DueDate != default)
                task.DueDate = updateTaskDto.DueDate;

            if (!string.IsNullOrWhiteSpace(updateTaskDto.Status))
                task.Status = updateTaskDto.Status;

            context.Tasks.Update(task);
            await context.SaveChangesAsync();

            return Ok(task);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating task");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        try
        {
            var task = await context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound(new { message = $"Task with id {id} not found" });
            }

            context.Tasks.Remove(task);
            await context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting task");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO for creating a new task
/// </summary>
public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public DateTime DueDate { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// DTO for updating an existing task
/// </summary>
public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public DateTime DueDate { get; set; }
    public string? Status { get; set; }
}
