using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;

namespace TaskManager.ViewComponents
{
    public class UserNameViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public UserNameViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        private string NormalizeUserId(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return string.Empty;
            return userId.Contains("|") ? userId.Split('|')[1] : userId;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!(User is ClaimsPrincipal claimsPrincipal))
            {
                return View("Default", "Unknown");
            }

            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var normalizedUserId = NormalizeUserId(userId);
            if (string.IsNullOrEmpty(normalizedUserId))
            {
                return View("Default", claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == normalizedUserId);
            var name = user?.Name ?? claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            
            return View("Default", name);
        }
    }
} 