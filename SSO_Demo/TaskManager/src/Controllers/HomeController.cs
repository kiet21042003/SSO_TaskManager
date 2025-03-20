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
using Microsoft.Extensions.Configuration;
using TaskManager.Data;
using TaskManager.Models;
using System.Linq;
using System.Collections.Generic;

namespace TaskManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Lấy thông tin user từ database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == NormalizeUserId(userId));
            var name = user?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;

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
            
            // Lấy thông tin user từ database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == NormalizeUserId(userId));
            var name = user?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;
            var email = user?.Email ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var picture = user?.Picture ?? User.FindFirst("picture")?.Value;

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
                CreatedAt = user?.CreatedAt ?? DateTime.Now,
                RecentTasks = recentTasks
            };

            return View(viewModel);
        }

        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Người dùng đăng xuất");

            // Đăng xuất khỏi cookie authentication trước
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Tạo URL return sau khi đăng xuất
            var returnUrl = Url.Action("Index", "Home", values: null, protocol: Request.Scheme);

            // Đăng xuất khỏi Auth0
            return SignOut(
                new AuthenticationProperties 
                { 
                    RedirectUri = $"https://{_configuration["Auth0:Domain"]}/v2/logout?client_id={_configuration["Auth0:ClientId"]}&returnTo={returnUrl}"
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            );
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

        private string NormalizeUserId(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return string.Empty;
            return userId.Contains("|") ? userId.Split('|')[1] : userId;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            var normalizedUserId = NormalizeUserId(userId);
            if (string.IsNullOrEmpty(normalizedUserId)) return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == normalizedUserId);
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
} 