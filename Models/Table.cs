using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderSystem.Models
{
    public class Table
    {
        public Table() { }

        public Table(int number, int row, int col)
        {
            Number = number;
            Row = row;
            Col = col;
            Status = TableStatus.Available;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [BindProperty]
        public int Number { get; set; }

        [Required]
        [Range(0, 9)]
        [BindProperty]
        public int Row { get; set; }

        [Required]
        [Range(0, 9)]
        [BindProperty]
        public int Col { get; set; }

        [Required]
        [BindProperty]
        public TableStatus Status { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        [NotMapped]
        public Order? CurrentOrder => Orders?.FirstOrDefault(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);
    }

    public enum OrderStatus
    {

        Pending,
        Open,
        InProgress,
        Ready,
        Served,
        Completed,
        Cancelled
    }

    public enum TableStatus
    {
        Available,
        Occupied,
        Reserved,
        Maintenance
    }
}

