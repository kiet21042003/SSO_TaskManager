using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using System.Security.Claims;

namespace TaskManager.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy userId hiện tại
        private string GetCurrentUserId()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Claims: {Claims}", string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));
            _logger.LogInformation("Current user ID: {UserId}", userId);
            return userId ?? string.Empty;
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            try 
            {
                _logger.LogInformation("Received create task request: {@Request}", request);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID is empty");
                    return BadRequest("User ID not found");
                }

                var task = new TodoTask
                {
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task created successfully: {TaskId}", task.Id);
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/tasks/move
        [HttpPost("move")]
        public async Task<IActionResult> Move([FromBody] MoveTaskRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.UserId == userId);

                if (task == null)
                {
                    _logger.LogWarning("Task not found: {TaskId}", request.TaskId);
                    return NotFound();
                }

                task.Status = (Models.TaskStatus)request.NewStatus;
                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Task status updated: {TaskId} -> {NewStatus}", task.Id, task.Status);

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving task");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    _logger.LogWarning("Task not found for deletion: {TaskId}", id);
                    return NotFound();
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task deleted: {TaskId}", id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    _logger.LogWarning("Task not found for update: {TaskId}", id);
                    return NotFound();
                }

                task.Title = request.Title;
                task.Description = request.Description;
                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Task updated: {TaskId}", id);

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }

    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Models.TaskStatus Status { get; set; }
    }

    public class MoveTaskRequest
    {
        public int TaskId { get; set; }
        public int NewStatus { get; set; }
    }

    public class UpdateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
} 