using TaskManager.Enums;

namespace TaskManager.DB.DataModels
{
    public class Task : IEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public StatusEnum Status { get; set; }
        public PriorityEnum Priority { get; set; } = PriorityEnum.Medium;
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public User? AssignedUser { get; set; }
        public int? AssignedTo { get; set; }
    }
}
