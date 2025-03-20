using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskManager.Models;
using TaskManager.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using System.Security.Claims;
using System.Linq;

namespace TaskManager.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProfileController> _logger;
        private readonly IAntiforgery _antiforgery;

        public ProfileController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<ProfileController> logger,
            IAntiforgery antiforgery)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _antiforgery = antiforgery;
        }

        private string NormalizeUserId(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return string.Empty;
            return userId.Contains("|") ? userId.Split('|')[1] : userId;
        }

        private async Task<User> GetOrCreateUserAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID không được để trống", nameof(userId));
            }

            var normalizedUserId = NormalizeUserId(userId);
            if (string.IsNullOrEmpty(normalizedUserId))
            {
                throw new ArgumentException("Normalized User ID không hợp lệ", nameof(userId));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == normalizedUserId);

            if (user == null)
            {
                _logger.LogInformation($"Tạo mới user với ID: {normalizedUserId}");
                
                // Lấy thông tin từ claims
                var name = User.FindFirst(ClaimTypes.Name)?.Value 
                    ?? User.FindFirst("name")?.Value 
                    ?? "Unknown";
                var email = User.FindFirst(ClaimTypes.Email)?.Value 
                    ?? User.FindFirst("email")?.Value
                    ?? string.Empty;
                var picture = User.FindFirst("picture")?.Value ?? string.Empty;

                user = new User
                {
                    Id = normalizedUserId,
                    Name = name,
                    Email = email,
                    Picture = picture,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Đã tạo user mới: {user.Id} - {user.Name}");
            }

            return user;
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var normalizedUserId = NormalizeUserId(userId);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == normalizedUserId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    user.Name = request.Name;
                }

                if (request.Picture != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(request.Picture.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new { message = "Chỉ chấp nhận file ảnh có định dạng .jpg, .jpeg, .png hoặc .gif" });
                    }

                    if (request.Picture.Length > 5 * 1024 * 1024) // 5MB
                    {
                        return BadRequest(new { message = "Kích thước file không được vượt quá 5MB" });
                    }

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                    
                    Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, "uploads"));
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Picture.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(user.Picture))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, "uploads", Path.GetFileName(user.Picture));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    user.Picture = $"/uploads/{fileName}";
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật thông tin" });
            }
        }

        [HttpGet("debug")]
        [Authorize]
        public async Task<IActionResult> DebugUsers()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("Không tìm thấy ID người dùng trong claims");
                    return Unauthorized(new { message = "Vui lòng đăng nhập lại" });
                }

                _logger.LogInformation($"Current user ID from claims: {currentUserId}");
                _logger.LogInformation($"Normalized current user ID: {NormalizeUserId(currentUserId)}");

                // Tự động tạo user nếu chưa tồn tại
                var currentUser = await GetOrCreateUserAsync(currentUserId);
                _logger.LogInformation($"User after get or create: ID={currentUser.Id}, Name={currentUser.Name}, Email={currentUser.Email}");

                var users = await _context.Users.Select(u => new { u.Id, u.Name, u.Email }).ToListAsync();
                _logger.LogInformation($"Tổng số users: {users.Count}");
                foreach (var user in users)
                {
                    _logger.LogInformation($"User - ID: {user.Id}, Name: {user.Name}, Email: {user.Email}");
                }

                return Ok(new { 
                    CurrentUserId = currentUserId,
                    NormalizedCurrentUserId = NormalizeUserId(currentUserId),
                    CurrentUser = new { currentUser.Id, currentUser.Name, currentUser.Email, currentUser.Picture },
                    Users = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thông tin users: {ex.Message}");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin users" });
            }
        }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public IFormFile? Picture { get; set; }
    }
} 