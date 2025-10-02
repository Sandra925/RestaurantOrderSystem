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
            Status = OrderStatus.Pending;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        [Range(1, 20)]
        public int CustomerCount { get; set; }

        public decimal TotalAmount => OrderItems?.Sum(oi => oi.Quantity * oi.Item.Price) ?? 0;

        // Foreign keys and navigation properties
        [Required]
        public int TableId { get; set; }

        [JsonIgnore]
        public virtual Table Table { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Methods
        public void AddItem(Item item, int quantity = 1)
        {
            var existingItem = OrderItems.FirstOrDefault(oi => oi.ItemId == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                OrderItems.Add(new OrderItem { OrderId = Id, ItemId = item.Id, Item = item, Quantity = quantity });
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveItem(int itemId)
        {
            var itemToRemove = OrderItems.FirstOrDefault(oi => oi.ItemId == itemId);
            if (itemToRemove != null)
            {
                OrderItems.Remove(itemToRemove);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
