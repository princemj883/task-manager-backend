# Task Manager API - Quick Reference Guide

## ✅ Project Setup Complete

Your task manager API is now fully configured with Entity Framework Core and PostgreSQL support.

## Project Structure

```
task-manager-backend/
├── Controllers/
│   └── TasksController.cs          # CRUD endpoints
├── Models/
│   └── Task.cs                     # TaskItem entity model
├── Data/
│   └── TaskManagerContext.cs       # EF Core DbContext
├── Migrations/
│   └── InitialCreate/              # Database schema migrations
├── Program.cs                      # Application configuration
├── appsettings.json               # Connection string
└── task-manager-backend.http      # API testing examples
```

## 📋 Database Schema

**Table: tasks**
| Column | Type | Notes |
|--------|------|-------|
| id | serial | Primary key, auto-increment |
| title | varchar(255) | Required, task name |
| description | varchar(2000) | Optional, task details |
| category | varchar(100) | Required, task category |
| priority | varchar(20) | low/medium/high, default: medium |
| due_date | timestamp | Task deadline |
| status | varchar(20) | pending/in-progress/completed, default: pending |
| created_at | timestamp | Auto-set to current time |
| updated_at | timestamp | Updated when task is modified |

## 🚀 Quick Start

### 1. Set Up Database
```bash
# Ensure PostgreSQL is running on localhost:5432
# Then create the database:
psql -U postgres -h localhost
CREATE DATABASE "TaskManagerDatabase";
\q

# Apply migrations
cd task-manager-backend
dotnet ef database update
```

### 2. Run the Application
```bash
dotnet run
```

The API will be available at:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger UI:** http://localhost:5000/swagger/ui

## 📡 API Endpoints

### Core CRUD Operations

| Method | Endpoint | Description | Status Code |
|--------|----------|-------------|------------|
| GET | `/api/tasks` | Get all tasks | 200 |
| GET | `/api/tasks/{id}` | Get task by ID | 200/404 |
| POST | `/api/tasks` | Create new task | 201 |
| PUT | `/api/tasks/{id}` | Update task | 200/404 |
| DELETE | `/api/tasks/{id}` | Delete task | 204/404 |

### Filter Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tasks/status/{status}` | Get tasks by status |
| GET | `/api/tasks/priority/{priority}` | Get tasks by priority |
| GET | `/api/tasks/category/{category}` | Get tasks by category |

## 💡 Request/Response Examples

### Create Task
```json
POST /api/tasks
Content-Type: application/json

{
  "title": "Complete Angular Assignment",
  "description": "Finish the task manager application with all requirements",
  "category": "education",
  "priority": "high",
  "dueDate": "2030-12-15T00:00:00.000Z",
  "status": "in-progress"
}

Response (201 Created):
{
  "id": 1,
  "title": "Complete Angular Assignment",
  "description": "Finish the task manager application with all requirements",
  "category": "education",
  "priority": "high",
  "dueDate": "2030-12-15T00:00:00Z",
  "status": "in-progress",
  "createdAt": "2026-04-03T06:44:12Z",
  "updatedAt": "2026-04-03T06:44:12Z"
}
```

### Update Task
```json
PUT /api/tasks/1
Content-Type: application/json

{
  "status": "completed"
}

Response (200 OK):
{
  "id": 1,
  "title": "Complete Angular Assignment",
  ...
  "status": "completed",
  "updatedAt": "2026-04-03T06:50:00Z"
}
```

### Get All Tasks
```
GET /api/tasks
Response (200 OK): Array of all tasks
```

### Get Tasks by Status
```
GET /api/tasks/status/in-progress
Response (200 OK): Array of tasks with status "in-progress"
```

## 🔌 Connection String

Located in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=TaskManagerDatabase;Username=postgres;Password=welcome"
}
```

To change PostgreSQL credentials, update this connection string.

## 📝 Testing the API

### Option 1: Use Swagger UI
Navigate to `http://localhost:5000/swagger/ui` to test all endpoints interactively.

### Option 2: Use REST Client Extension (VS Code)
Open `task-manager-backend.http` and click "Send Request" above each endpoint.

### Option 3: Use cURL
```bash
# Get all tasks
curl -X GET http://localhost:5000/api/tasks

# Create a task
curl -X POST http://localhost:5000/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"New Task","category":"work","dueDate":"2026-04-10T00:00:00Z"}'

# Update a task
curl -X PUT http://localhost:5000/api/tasks/1 \
  -H "Content-Type: application/json" \
  -d '{"status":"completed"}'

# Delete a task
curl -X DELETE http://localhost:5000/api/tasks/1
```

## 🛠️ Technologies Used

- **.NET 9.0** - Web framework
- **Entity Framework Core 9.0** - ORM
- **Npgsql** - PostgreSQL provider
- **Swashbuckle** - Swagger/OpenAPI support
- **PostgreSQL** - Database

## 📚 Model Definition

```csharp
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Priority { get; set; }      // low, medium, high
    public DateTime DueDate { get; set; }
    public string Status { get; set; }        // pending, in-progress, completed
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

## ⚙️ Configuration

### Enable CORS (Enabled by Default)
The API allows requests from any origin. To restrict:
```csharp
// In Program.cs, modify CorsPolicy
policy.WithOrigins("https://yourdomain.com")
```

### Logging
Managed through `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

## 🐛 Troubleshooting

**Database Connection Failed:**
- Ensure PostgreSQL is running: `systemctl status postgresql`
- Check credentials in `appsettings.json`
- Verify database exists: `psql -U postgres -l`

**Port Already in Use:**
- Change port in `Properties/launchSettings.json`

**Migration Issues:**
- Delete `Migrations` folder and rerun: `dotnet ef migrations add InitialCreate`

**Entity Framework Tools Not Found:**
```bash
dotnet tool install --global dotnet-ef
```

## 📖 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
