using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RestaurantOrderSystem.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(1, 50)]
        public int Quantity { get; set; }

        [JsonIgnore]
        public virtual Order Order { get; set; }

        public virtual Item Item { get; set; }
    }

}
