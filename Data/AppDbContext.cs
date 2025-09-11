using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Data
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Table> Tables { get; set; }
    }
}
