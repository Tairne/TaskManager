using TaskManager.DB.DataModels;
using TaskManager.Enums;

namespace TaskManager.DB.DTO
{
    public class TaskExportDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string AssignedUser { get; set; } = string.Empty;
    }
}
