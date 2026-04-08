using Microsoft.EntityFrameworkCore;
using task_manager_backend.Models;

namespace task_manager_backend.Data;

public class TaskManagerContext : DbContext
{
    public TaskManagerContext(DbContextOptions<TaskManagerContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }
    
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table name
        modelBuilder.Entity<TaskItem>().ToTable("tasks");

        // Configure primary key
        modelBuilder.Entity<TaskItem>()
            .HasKey(t => t.Id);

        // Configure properties
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Description)
            .HasMaxLength(2000);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Priority)
            .IsRequired()
            .HasDefaultValue("medium")
            .HasMaxLength(20);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Status)
            .IsRequired()
            .HasDefaultValue("pending")
            .HasMaxLength(20);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Configure Employee entity
        modelBuilder.Entity<Employee>().ToTable("employees");

        modelBuilder.Entity<Employee>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Employee>()
            .Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Employee>()
            .Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Phone)
            .HasMaxLength(20);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Department)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Designation)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Salary)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Status)
            .IsRequired()
            .HasDefaultValue("active")
            .HasMaxLength(20);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Location)
            .HasMaxLength(100);

        modelBuilder.Entity<Employee>()
            .Property(e => e.Avatar)
            .HasMaxLength(500);

        modelBuilder.Entity<Employee>()
            .Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}

