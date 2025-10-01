using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string Name { get; set; }
        public decimal Price { get; set; }

    }

   
}
