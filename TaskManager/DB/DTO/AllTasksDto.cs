namespace TaskManager.DB.DTO
{
    public class AllTasksDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? AssignedUser { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
