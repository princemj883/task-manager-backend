# Task Manager API - Setup Instructions

## Prerequisites
- .NET 9.0 SDK installed
- PostgreSQL server running on localhost:5432
- PostgreSQL user "postgres" with password "welcome" (as per connection string)

## Database Setup

### 1. Create the PostgreSQL Database
If the database doesn't exist, create it using psql:

```bash
psql -U postgres -h localhost
# In psql prompt:
CREATE DATABASE "TaskManagerDatabase";
\q
```

### 2. Apply Entity Framework Migrations

Run the following commands in the project directory to create the database schema:

```bash
cd task-manager-backend

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migrations to the database
dotnet ef database update
```

If the migrations folder doesn't exist, both commands will create it automatically.

## Running the Application

1. Navigate to the project directory:
```bash
cd task-manager-backend
```

2. Build the application:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

## API Endpoints

### Task Management
- **GET** `/api/tasks` - Get all tasks
- **GET** `/api/tasks/{id}` - Get a specific task by ID
- **POST** `/api/tasks` - Create a new task
- **PUT** `/api/tasks/{id}` - Update an existing task
- **DELETE** `/api/tasks/{id}` - Delete a task

### Filtering Endpoints
- **GET** `/api/tasks/status/{status}` - Get tasks by status (pending, in-progress, completed)
- **GET** `/api/tasks/priority/{priority}` - Get tasks by priority (low, medium, high)
- **GET** `/api/tasks/category/{category}` - Get tasks by category

## Request/Response Examples

### Create a Task
**Request:**
```json
POST /api/tasks
Content-Type: application/json

{
  "title": "Complete Angular Assignment",
  "description": "Finish the task manager application with all requirements including http integration",
  "category": "education",
  "priority": "high",
  "dueDate": "2030-12-15T00:00:00.000Z",
  "status": "in-progress"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "title": "Complete Angular Assignment",
  "description": "Finish the task manager application with all requirements including http integration",
  "category": "education",
  "priority": "high",
  "dueDate": "2030-12-15T00:00:00Z",
  "status": "in-progress",
  "createdAt": "2026-04-03T12:00:00Z",
  "updatedAt": "2026-04-03T12:00:00Z"
}
```

### Update a Task
**Request:**
```json
PUT /api/tasks/1
Content-Type: application/json

{
  "status": "completed",
  "priority": "high"
}
```

**Response (200 OK):**
Returns the updated task object.

### Get All Tasks
**Request:**
```
GET /api/tasks
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": "Complete Angular Assignment",
    "description": "Finish the task manager application with all requirements including http integration",
    "category": "education",
    "priority": "high",
    "dueDate": "2030-12-15T00:00:00Z",
    "status": "in-progress",
    "createdAt": "2026-04-03T12:00:00Z",
    "updatedAt": "2026-04-03T12:00:00Z"
  }
]
```

## Database Connection String
The connection string in `appsettings.json` is:
```
Host=localhost;Port=5432;Database=TaskManagerDatabase;Username=postgres;Password=welcome
```

To change the connection parameters, update the `ConnectionStrings` section in `appsettings.json`.

## Development Tools

### View Database with Swagger
When running in development mode, Swagger UI will be available at:
- `https://localhost:5001/swagger/ui` or `http://localhost:5000/swagger/ui`

### Testing with REST Client
Use the `task-manager-backend.http` file in VS Code with the REST Client extension to test all endpoints.

## Troubleshooting

### Connection Issues
- Ensure PostgreSQL server is running on localhost:5432
- Verify the database user and password are correct
- Check firewall settings allow local connections

### Migration Issues
- If migrations fail, delete the `Migrations` folder and start again with `dotnet ef migrations add InitialCreate`
- Ensure you have Entity Framework Core tools installed: `dotnet tool install --global dotnet-ef`

### Port Already in Use
The default port is 5083. If it's already in use, update the port in `Properties/launchSettings.json`.
