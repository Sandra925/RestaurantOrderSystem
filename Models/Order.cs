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
            Status = OrderStatus.Open;
            PaymentStatus = PaymentStatus.Unpaid;
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

        [Required]
        public int TableId { get; set; }

        [JsonIgnore]
        public  Table? Table { get; set; }

        public  ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [Required]
        public PaymentStatus PaymentStatus { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public DateTime? PaidAt { get; set; }
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid,
        Cancelled
    }

    public enum PaymentMethod
    {
        Card,
        Cash
    }
}
