using TaskManager.Enums;

namespace TaskManager.DB.DTO
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PriorityEnum Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssignedTo { get; set; }
    }
}
