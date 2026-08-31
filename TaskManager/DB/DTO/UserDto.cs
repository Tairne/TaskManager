namespace TaskManager.DB.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}
