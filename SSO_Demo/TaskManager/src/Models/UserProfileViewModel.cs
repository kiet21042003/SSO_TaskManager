using System;
using System.Collections.Generic;

namespace TaskManager.Models
{
    public class UserProfileViewModel
    {
        public required string UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Picture { get; set; }
        public DateTime CreatedAt { get; set; }
        public required List<TodoTask> RecentTasks { get; set; }
    }
} 