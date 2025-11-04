using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public string StoreName { get; set; } = string.Empty;
    }
}