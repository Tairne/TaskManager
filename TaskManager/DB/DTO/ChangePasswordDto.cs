namespace TaskManager.DB.DTO
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPasswordDto { get; set; } = string.Empty;
    }
}
