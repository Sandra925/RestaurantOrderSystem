using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderSystem.Models
{
    public class Table
    {
        public Table() { }
        public Table(int id, int number, Order tableOrder, int customerCount)
        {
            Id = id;
            Number = number;
            TableOrder = tableOrder;
            CustomerCount = customerCount;
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public int Number { get; set; }
        [Required]
        [Range(0,9)]
        public int Row {  get; set; }
        [Required]
        [Range(0,9)]
        public int Col { get; set; }
        Order? TableOrder { get; set; }
        public int? CustomerCount { get; set; }



    }
}
