using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services
{
    public interface ITaskService
    {
        Task<List<TodoTask>> GetTasksByUserIdAsync(string userId);
        Task<TodoTask?> GetTaskByIdAsync(int id, string userId);
        Task<TodoTask> CreateTaskAsync(TodoTask task);
        Task<TodoTask> UpdateTaskAsync(TodoTask task);
        Task DeleteTaskAsync(int id, string userId);
    }

    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TodoTask>> GetTasksByUserIdAsync(string userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TodoTask?> GetTaskByIdAsync(int id, string userId)
        {
            return await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<TodoTask> CreateTaskAsync(TodoTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TodoTask> UpdateTaskAsync(TodoTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task DeleteTaskAsync(int id, string userId)
        {
            var task = await GetTaskByIdAsync(id, userId);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
} 