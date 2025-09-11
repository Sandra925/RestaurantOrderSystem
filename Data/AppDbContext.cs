using Microsoft.EntityFrameworkCore;
using RestoranoSistema.Models;

namespace RestoranoSistema.Data
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Table> Tables { get; set; }
    }
}
