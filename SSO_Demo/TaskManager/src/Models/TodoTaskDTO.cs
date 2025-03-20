using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class TodoTaskDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(100, ErrorMessage = "Tiêu đề không được vượt quá 100 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        public TodoTaskStatus Status { get; set; }
    }
} 