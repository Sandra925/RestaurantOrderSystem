using System.ComponentModel.DataAnnotations;

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
        public int Number { get; set; }
        public int Row {  get; set; }
        public int Col { get; set; }
        Order? TableOrder { get; set; }
        public int? CustomerCount { get; set; }



    }
}
