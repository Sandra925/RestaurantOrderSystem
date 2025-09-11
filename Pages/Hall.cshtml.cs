using Microsoft.AspNetCore.Mvc;
using RestaurantOrderSystem.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Data;

namespace RestaurantOrderSystem.Pages
{
    public class HallModel : PageModel
    {
   
        private readonly AppDbContext _context;
        public HallModel(AppDbContext context)
        {
            _context = context;
        }
        public List<Table> Tables { get; set; } = new List<Table>();
        public void OnGet()
        {
            Tables = _context.Tables.ToList();
        }
        public IActionResult OnPostAddTable(int row, int col, int number)
        {
            int id = Tables.Count();
            Table table = new Table(id, number, null, 0) { Row = row, Col = col } ;
            _context.Tables.Add(table);
            _context.SaveChanges();
            return RedirectToPage();
        }

    }
}
