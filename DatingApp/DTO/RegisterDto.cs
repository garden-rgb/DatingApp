using System.ComponentModel.DataAnnotations;

namespace DatingApp.DTO
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Password { get; set; }
    }
}
