namespace TaskManager.DB.DTO
{
    public class CreateUserDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}
