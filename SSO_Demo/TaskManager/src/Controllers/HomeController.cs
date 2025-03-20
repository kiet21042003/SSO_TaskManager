using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Data;
using TaskManager.Models;
using System.Linq;

namespace TaskManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;

            var taskCounts = await _context.Tasks
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.UserName = name;
            ViewBag.TodoCount = taskCounts.FirstOrDefault(t => t.Status == Models.TaskStatus.Todo)?.Count ?? 0;
            ViewBag.InProgressCount = taskCounts.FirstOrDefault(t => t.Status == Models.TaskStatus.InProgress)?.Count ?? 0;
            ViewBag.CompletedCount = taskCounts.FirstOrDefault(t => t.Status == Models.TaskStatus.Completed)?.Count ?? 0;
            ViewBag.CancelledCount = taskCounts.FirstOrDefault(t => t.Status == Models.TaskStatus.Cancelled)?.Count ?? 0;

            var recentTasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentTasks = recentTasks;

            return View();
        }

        public async Task<IActionResult> Tasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"Loading tasks for user: {userId}");

            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            _logger.LogInformation($"Found {tasks.Count} tasks");
            return View(tasks);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var picture = User.FindFirst("picture")?.Value;

            var recentTasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var taskCounts = await _context.Tasks
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.TodoCount = taskCounts.FirstOrDefault(t => t.Status == Models.TaskStatus.Todo)?.Count ?? 0;
            ViewBag.InProgressCount = taskCounts.FirstOrDefault(t => t.Status == Models.TaskStatus.InProgress)?.Count ?? 0;
            ViewBag.CompletedCount = taskCounts.FirstOrDefault(t => t.Status == Models.TaskStatus.Completed)?.Count ?? 0;
            ViewBag.CancelledCount = taskCounts.FirstOrDefault(t => t.Status == Models.TaskStatus.Cancelled)?.Count ?? 0;

            var viewModel = new UserProfileViewModel
            {
                UserId = userId ?? string.Empty,
                Name = name ?? string.Empty,
                Email = email ?? string.Empty,
                Picture = picture ?? "https://via.placeholder.com/150",
                CreatedAt = DateTime.Now, // Trong thực tế, bạn nên lấy ngày tạo tài khoản từ cơ sở dữ liệu
                RecentTasks = recentTasks
            };

            return View(viewModel);
        }

        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("OpenIdConnect");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
} 