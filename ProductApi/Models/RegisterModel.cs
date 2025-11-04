using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models
{
    public class RegisterModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string StoreName { get; set; } = string.Empty;
    }
}