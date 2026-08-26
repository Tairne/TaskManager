using System.ComponentModel.DataAnnotations;

namespace TaskManager.DB.DTO
{
    public class UpdateUserDto
    {
        public string Login { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}
