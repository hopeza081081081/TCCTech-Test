using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(16)]
        public string ProductCode { get; set; } = string.Empty;

        public int StoreId { get; set; } // Foreign Key
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; } = null!;
    }
}