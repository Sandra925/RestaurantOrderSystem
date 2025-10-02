using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RestaurantOrderSystem.Models
{
    public class Item
    {
        public Item() { }
        public Item(int id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        public decimal Price { get; set; }
        [JsonIgnore]
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }

   
}
