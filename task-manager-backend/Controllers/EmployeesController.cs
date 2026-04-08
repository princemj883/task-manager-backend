using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_manager_backend.Data;
using task_manager_backend.Models;

namespace task_manager_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly TaskManagerContext _context;
    private readonly ILogger<EmployeesController> _logger;

    // Constants for valid options
    public static readonly string[] Departments = { "Engineering", "Sales", "Marketing", "HR", "Finance", "Operations", "Support" };
    public static readonly string[] Statuses = { "active", "inactive", "on-leave" };
    public static readonly string[] Locations = { "Bangalore", "Mumbai", "Delhi", "Hyderabad", "Chennai", "Pune", "Kolkata" };

    public EmployeesController(TaskManagerContext context, ILogger<EmployeesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get available employee options (departments, statuses, locations)
    /// </summary>
    [HttpGet("options")]
    public ActionResult<EmployeeOptionsDto> GetEmployeeOptions()
    {
        return Ok(new EmployeeOptionsDto
        {
            Departments = Departments,
            Statuses = Statuses,
            Locations = Locations
        });
    }

    /// <summary>
    /// Get all employees with filtering, searching, sorting and pagination
    /// </summary>
    /// <param name="q">Full-text search across name, email, department, designation, location</param>
    /// <param name="department">Filter by department</param>
    /// <param name="status">Filter by status (active, on-leave, terminated)</param>
    /// <param name="location">Filter by location</param>
    /// <param name="joinDateFrom">Filter by join date (from)</param>
    /// <param name="joinDateTo">Filter by join date (to)</param>
    /// <param name="salaryMin">Filter by salary (minimum)</param>
    /// <param name="salaryMax">Filter by salary (maximum)</param>
    /// <param name="_sort">Column(s) to sort by (comma-separated for multiple columns)</param>
    /// <param name="_order">Sort order: asc or desc (comma-separated for multiple columns)</param>
    /// <param name="_page">Page number (default: 1)</param>
    /// <param name="_limit">Items per page (default: 10, max: 100)</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees(
        string? q = null,
        string? department = null,
        string? status = null,
        string? location = null,
        DateTime? joinDateFrom = null,
        DateTime? joinDateTo = null,
        decimal? salaryMin = null,
        decimal? salaryMax = null,
        string? _sort = null,
        string? _order = null,
        int _page = 1,
        int _limit = 100)
    {
        try
        {
            if (_page < 1) _page = 1;
            if (_limit < 1) _limit = 10;
            if (_limit > 100) _limit = 100;

            var query = _context.Employees.AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchTerm = q.ToLower();
                query = query.Where(e =>
                    e.FirstName.ToLower().Contains(searchTerm) ||
                    e.LastName.ToLower().Contains(searchTerm) ||
                    (e.FirstName.ToLower() + " " + e.LastName.ToLower()).Contains(searchTerm) ||
                    e.Email.ToLower().Contains(searchTerm) ||
                    e.Department.ToLower().Contains(searchTerm) ||
                    e.Designation.ToLower().Contains(searchTerm) ||
                    e.Location.ToLower().Contains(searchTerm)
                );
            }

            // Department filter
            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(e => e.Department.ToLower() == department.ToLower());
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status.ToLower() == status.ToLower());
            }

            // Location filter
            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e => e.Location.ToLower() == location.ToLower());
            }

            // Join date range filter
            if (joinDateFrom.HasValue)
            {
                query = query.Where(e => e.JoinDate >= joinDateFrom.Value);
            }

            if (joinDateTo.HasValue)
            {
                query = query.Where(e => e.JoinDate <= joinDateTo.Value);
            }

            // Salary range filter
            if (salaryMin.HasValue)
            {
                query = query.Where(e => e.Salary >= salaryMin.Value);
            }

            if (salaryMax.HasValue)
            {
                query = query.Where(e => e.Salary <= salaryMax.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrWhiteSpace(_sort))
            {
                var sortColumns = _sort.Split(',').Select(s => s.Trim()).ToArray();
                var orderValues = string.IsNullOrWhiteSpace(_order) 
                    ? Enumerable.Repeat("asc", sortColumns.Length).ToArray()
                    : _order.Split(',').Select(o => o.Trim().ToLower()).ToArray();

                bool isFirstSort = true;
                IOrderedQueryable<Employee>? orderedQuery = null;

                for (int i = 0; i < sortColumns.Length; i++)
                {
                    var column = sortColumns[i].ToLower();
                    var order = i < orderValues.Length ? orderValues[i] : "asc";
                    var isDescending = order == "desc";

                    if (isFirstSort)
                    {
                        orderedQuery = column switch
                        {
                            "id" => isDescending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id),
                            "firstname" => isDescending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
                            "lastname" => isDescending ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName),
                            "email" => isDescending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
                            "department" => isDescending ? query.OrderByDescending(e => e.Department) : query.OrderBy(e => e.Department),
                            "designation" => isDescending ? query.OrderByDescending(e => e.Designation) : query.OrderBy(e => e.Designation),
                            "salary" => isDescending ? query.OrderByDescending(e => e.Salary) : query.OrderBy(e => e.Salary),
                            "joindate" => isDescending ? query.OrderByDescending(e => e.JoinDate) : query.OrderBy(e => e.JoinDate),
                            "status" => isDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                            "location" => isDescending ? query.OrderByDescending(e => e.Location) : query.OrderBy(e => e.Location),
                            "createdat" => isDescending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
                            _ => query.OrderByDescending(e => e.CreatedAt)
                        };
                        isFirstSort = false;
                    }
                    else if (orderedQuery != null)
                    {
                        orderedQuery = column switch
                        {
                            "id" => isDescending ? orderedQuery.ThenByDescending(e => e.Id) : orderedQuery.ThenBy(e => e.Id),
                            "firstname" => isDescending ? orderedQuery.ThenByDescending(e => e.FirstName) : orderedQuery.ThenBy(e => e.FirstName),
                            "lastname" => isDescending ? orderedQuery.ThenByDescending(e => e.LastName) : orderedQuery.ThenBy(e => e.LastName),
                            "email" => isDescending ? orderedQuery.ThenByDescending(e => e.Email) : orderedQuery.ThenBy(e => e.Email),
                            "department" => isDescending ? orderedQuery.ThenByDescending(e => e.Department) : orderedQuery.ThenBy(e => e.Department),
                            "designation" => isDescending ? orderedQuery.ThenByDescending(e => e.Designation) : orderedQuery.ThenBy(e => e.Designation),
                            "salary" => isDescending ? orderedQuery.ThenByDescending(e => e.Salary) : orderedQuery.ThenBy(e => e.Salary),
                            "joindate" => isDescending ? orderedQuery.ThenByDescending(e => e.JoinDate) : orderedQuery.ThenBy(e => e.JoinDate),
                            "status" => isDescending ? orderedQuery.ThenByDescending(e => e.Status) : orderedQuery.ThenBy(e => e.Status),
                            "location" => isDescending ? orderedQuery.ThenByDescending(e => e.Location) : orderedQuery.ThenBy(e => e.Location),
                            "createdat" => isDescending ? orderedQuery.ThenByDescending(e => e.CreatedAt) : orderedQuery.ThenBy(e => e.CreatedAt),
                            _ => orderedQuery
                        };
                    }
                }

                if (orderedQuery != null)
                {
                    query = orderedQuery;
                }
            }
            else
            {
                // Default sorting
                query = query.OrderByDescending(e => e.CreatedAt);
            }

            // Pagination
            var employees = await query
                .Skip((_page - 1) * _limit)
                .Take(_limit)
                .ToListAsync();

            // Add X-Total-Count header
            Response.Headers.Add("X-Total-Count", totalCount.ToString());

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific employee by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = $"Employee with id {id} not found" });
            }

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
    {
        try
        {
            if (createEmployeeDto == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            if (string.IsNullOrWhiteSpace(createEmployeeDto.FirstName) || string.IsNullOrWhiteSpace(createEmployeeDto.LastName))
            {
                return BadRequest(new { message = "First name and last name are required" });
            }

            if (string.IsNullOrWhiteSpace(createEmployeeDto.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            var employee = new Employee
            {
                FirstName = createEmployeeDto.FirstName,
                LastName = createEmployeeDto.LastName,
                Email = createEmployeeDto.Email,
                Phone = createEmployeeDto.Phone ?? string.Empty,
                Department = createEmployeeDto.Department ?? string.Empty,
                Designation = createEmployeeDto.Designation ?? string.Empty,
                Salary = createEmployeeDto.Salary,
                JoinDate = createEmployeeDto.JoinDate,
                Status = createEmployeeDto.Status ?? "active",
                Location = createEmployeeDto.Location ?? string.Empty,
                ManagerId = createEmployeeDto.ManagerId,
                Avatar = createEmployeeDto.Avatar,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Create multiple employees in batch
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<IEnumerable<Employee>>> CreateEmployeesBatch([FromBody] List<CreateEmployeeDto> employeeDtos)
    {
        try
        {
            if (employeeDtos == null || employeeDtos.Count == 0)
            {
                return BadRequest(new { message = "At least one employee is required" });
            }

            var employees = new List<Employee>();

            foreach (var dto in employeeDtos)
            {
                if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
                {
                    return BadRequest(new { message = "First name and last name are required for all employees" });
                }

                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    return BadRequest(new { message = "Email is required for all employees" });
                }

                var employee = new Employee
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Phone = dto.Phone ?? string.Empty,
                    Department = dto.Department ?? string.Empty,
                    Designation = dto.Designation ?? string.Empty,
                    Salary = dto.Salary,
                    JoinDate = dto.JoinDate,
                    Status = dto.Status ?? "active",
                    Location = dto.Location ?? string.Empty,
                    ManagerId = dto.ManagerId,
                    Avatar = dto.Avatar,
                    CreatedAt = DateTime.UtcNow
                };

                employees.Add(employee);
            }

            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployees), employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employees in batch");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Employee>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
    {
        try
        {
            if (updateEmployeeDto == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = $"Employee with id {id} not found" });
            }

            // Update only the provided fields
            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.FirstName))
                employee.FirstName = updateEmployeeDto.FirstName;

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.LastName))
                employee.LastName = updateEmployeeDto.LastName;

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Email))
                employee.Email = updateEmployeeDto.Email;

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Phone))
                employee.Phone = updateEmployeeDto.Phone;

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Department))
                employee.Department = updateEmployeeDto.Department;

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Designation))
                employee.Designation = updateEmployeeDto.Designation;

            if (updateEmployeeDto.Salary.HasValue && updateEmployeeDto.Salary > 0)
                employee.Salary = updateEmployeeDto.Salary.Value;

            if (updateEmployeeDto.JoinDate != default)
                employee.JoinDate = updateEmployeeDto.JoinDate;

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Status))
                employee.Status = updateEmployeeDto.Status;

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Location))
                employee.Location = updateEmployeeDto.Location;

            if (updateEmployeeDto.ManagerId.HasValue)
                employee.ManagerId = updateEmployeeDto.ManagerId;

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Avatar))
                employee.Avatar = updateEmployeeDto.Avatar;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an employee
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = $"Employee with id {id} not found" });
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get employees by department
    /// </summary>
    [HttpGet("department/{department}")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByDepartment(string department)
    {
        try
        {
            var employees = await _context.Employees
                .Where(e => e.Department.ToLower() == department.ToLower())
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by department");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get employees by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByStatus(string status)
    {
        try
        {
            var employees = await _context.Employees
                .Where(e => e.Status.ToLower() == status.ToLower())
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by status");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get employees by location
    /// </summary>
    [HttpGet("location/{location}")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByLocation(string location)
    {
        try
        {
            var employees = await _context.Employees
                .Where(e => e.Location.ToLower() == location.ToLower())
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by location");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get employees by manager id
    /// </summary>
    [HttpGet("manager/{managerId}")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByManager(int managerId)
    {
        try
        {
            var employees = await _context.Employees
                .Where(e => e.ManagerId == managerId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by manager");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Export employees with filtering, searching, and sorting as CSV
    /// </summary>
    /// <param name="q">Full-text search across name, email, department, designation, location</param>
    /// <param name="department">Filter by department</param>
    /// <param name="status">Filter by status (active, on-leave, terminated)</param>
    /// <param name="location">Filter by location</param>
    /// <param name="joinDateFrom">Filter by join date (from)</param>
    /// <param name="joinDateTo">Filter by join date (to)</param>
    /// <param name="salaryMin">Filter by salary (minimum)</param>
    /// <param name="salaryMax">Filter by salary (maximum)</param>
    /// <param name="_sort">Column(s) to sort by (comma-separated for multiple columns)</param>
    /// <param name="_order">Sort order: asc or desc (comma-separated for multiple columns)</param>
    [HttpGet("export")]
    public async Task<ActionResult> ExportEmployees(
        string? q = null,
        string? department = null,
        string? status = null,
        string? location = null,
        DateTime? joinDateFrom = null,
        DateTime? joinDateTo = null,
        decimal? salaryMin = null,
        decimal? salaryMax = null,
        string? _sort = null,
        string? _order = null)
    {
        try
        {
            var query = _context.Employees.AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchTerm = q.ToLower();
                query = query.Where(e =>
                    e.FirstName.ToLower().Contains(searchTerm) ||
                    e.LastName.ToLower().Contains(searchTerm) ||
                    (e.FirstName.ToLower() + " " + e.LastName.ToLower()).Contains(searchTerm) ||
                    e.Email.ToLower().Contains(searchTerm) ||
                    e.Department.ToLower().Contains(searchTerm) ||
                    e.Designation.ToLower().Contains(searchTerm) ||
                    e.Location.ToLower().Contains(searchTerm)
                );
            }

            // Department filter
            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(e => e.Department.ToLower() == department.ToLower());
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status.ToLower() == status.ToLower());
            }

            // Location filter
            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e => e.Location.ToLower() == location.ToLower());
            }

            // Join date range filter
            if (joinDateFrom.HasValue)
            {
                query = query.Where(e => e.JoinDate >= joinDateFrom.Value);
            }

            if (joinDateTo.HasValue)
            {
                query = query.Where(e => e.JoinDate <= joinDateTo.Value);
            }

            // Salary range filter
            if (salaryMin.HasValue)
            {
                query = query.Where(e => e.Salary >= salaryMin.Value);
            }

            if (salaryMax.HasValue)
            {
                query = query.Where(e => e.Salary <= salaryMax.Value);
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(_sort))
            {
                var sortColumns = _sort.Split(',').Select(s => s.Trim()).ToArray();
                var orderValues = string.IsNullOrWhiteSpace(_order)
                    ? Enumerable.Repeat("asc", sortColumns.Length).ToArray()
                    : _order.Split(',').Select(o => o.Trim().ToLower()).ToArray();

                bool isFirstSort = true;
                IOrderedQueryable<Employee>? orderedQuery = null;

                for (int i = 0; i < sortColumns.Length; i++)
                {
                    var column = sortColumns[i].ToLower();
                    var order = i < orderValues.Length ? orderValues[i] : "asc";
                    var isDescending = order == "desc";

                    if (isFirstSort)
                    {
                        orderedQuery = column switch
                        {
                            "id" => isDescending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id),
                            "firstname" => isDescending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
                            "lastname" => isDescending ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName),
                            "email" => isDescending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
                            "department" => isDescending ? query.OrderByDescending(e => e.Department) : query.OrderBy(e => e.Department),
                            "designation" => isDescending ? query.OrderByDescending(e => e.Designation) : query.OrderBy(e => e.Designation),
                            "salary" => isDescending ? query.OrderByDescending(e => e.Salary) : query.OrderBy(e => e.Salary),
                            "joindate" => isDescending ? query.OrderByDescending(e => e.JoinDate) : query.OrderBy(e => e.JoinDate),
                            "status" => isDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                            "location" => isDescending ? query.OrderByDescending(e => e.Location) : query.OrderBy(e => e.Location),
                            "createdat" => isDescending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
                            _ => query.OrderByDescending(e => e.CreatedAt)
                        };
                        isFirstSort = false;
                    }
                    else if (orderedQuery != null)
                    {
                        orderedQuery = column switch
                        {
                            "id" => isDescending ? orderedQuery.ThenByDescending(e => e.Id) : orderedQuery.ThenBy(e => e.Id),
                            "firstname" => isDescending ? orderedQuery.ThenByDescending(e => e.FirstName) : orderedQuery.ThenBy(e => e.FirstName),
                            "lastname" => isDescending ? orderedQuery.ThenByDescending(e => e.LastName) : orderedQuery.ThenBy(e => e.LastName),
                            "email" => isDescending ? orderedQuery.ThenByDescending(e => e.Email) : orderedQuery.ThenBy(e => e.Email),
                            "department" => isDescending ? orderedQuery.ThenByDescending(e => e.Department) : orderedQuery.ThenBy(e => e.Department),
                            "designation" => isDescending ? orderedQuery.ThenByDescending(e => e.Designation) : orderedQuery.ThenBy(e => e.Designation),
                            "salary" => isDescending ? orderedQuery.ThenByDescending(e => e.Salary) : orderedQuery.ThenBy(e => e.Salary),
                            "joindate" => isDescending ? orderedQuery.ThenByDescending(e => e.JoinDate) : orderedQuery.ThenBy(e => e.JoinDate),
                            "status" => isDescending ? orderedQuery.ThenByDescending(e => e.Status) : orderedQuery.ThenBy(e => e.Status),
                            "location" => isDescending ? orderedQuery.ThenByDescending(e => e.Location) : orderedQuery.ThenBy(e => e.Location),
                            "createdat" => isDescending ? orderedQuery.ThenByDescending(e => e.CreatedAt) : orderedQuery.ThenBy(e => e.CreatedAt),
                            _ => orderedQuery
                        };
                    }
                }

                if (orderedQuery != null)
                {
                    query = orderedQuery;
                }
            }
            else
            {
                // Default sorting
                query = query.OrderByDescending(e => e.CreatedAt);
            }

            var employees = await query.ToListAsync();

            // Generate CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,First Name,Last Name,Email,Phone,Department,Designation,Salary,Join Date,Status,Location,Manager ID,Avatar,Created At");

            foreach (var emp in employees)
            {
                var joinDateStr = emp.JoinDate.ToString("yyyy-MM-dd");
                var createdAtStr = emp.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                var managerIdStr = emp.ManagerId?.ToString() ?? "";
                
                // Escape CSV values that contain commas or quotes
                var escapeCsv = (string? value) => string.IsNullOrEmpty(value) ? "" : $"\"{value.Replace("\"", "\"\"")}\"";

                csv.AppendLine($"{emp.Id}," +
                    $"{escapeCsv(emp.FirstName)}," +
                    $"{escapeCsv(emp.LastName)}," +
                    $"{escapeCsv(emp.Email)}," +
                    $"{escapeCsv(emp.Phone)}," +
                    $"{escapeCsv(emp.Department)}," +
                    $"{escapeCsv(emp.Designation)}," +
                    $"{emp.Salary}," +
                    $"{joinDateStr}," +
                    $"{escapeCsv(emp.Status)}," +
                    $"{escapeCsv(emp.Location)}," +
                    $"{managerIdStr}," +
                    $"{escapeCsv(emp.Avatar)}," +
                    $"{createdAtStr}");
            }

            var fileName = $"employees_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var content = csv.ToString();
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting employees");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO for creating a new employee
/// </summary>
public class CreateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
    public int? ManagerId { get; set; }
    public string? Avatar { get; set; }
}

/// <summary>
/// DTO for updating an existing employee
/// </summary>
public class UpdateEmployeeDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public decimal? Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
    public int? ManagerId { get; set; }
    public string? Avatar { get; set; }
}

/// <summary>
/// DTO for employee options
/// </summary>
public class EmployeeOptionsDto
{
    public string[] Departments { get; set; } = Array.Empty<string>();
    public string[] Statuses { get; set; } = Array.Empty<string>();
    public string[] Locations { get; set; } = Array.Empty<string>();
}
