using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RestaurantOrderSystem.Models
{
    public class Order
    {
        public Order() { }

        public Order(int tableId, int customerCount)
        {
            TableId = tableId;
            CustomerCount = customerCount;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        [Range(1, 20)]
        public int CustomerCount { get; set; }

        public decimal? TotalAmount => OrderItems?.Sum(oi => oi.Quantity * oi.Item.Price) ?? 0;

        [Required]
        public int TableId { get; set; }

        [JsonIgnore]
        public virtual Table? Table { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
