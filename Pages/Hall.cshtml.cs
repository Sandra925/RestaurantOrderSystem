using Microsoft.AspNetCore.Mvc;
using RestoranoSistema.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RestoranoSistema.Pages
{
    public class HallModel : PageModel
    {
        public List<Table> Tables { get; set; } = new List<Table>();
        public void OnGet()
        {
            Tables = new List<Table>
            {
                new Table(1, 1, null, 4) {Row = 1, Col = 1},
                new Table(2, 2, null, 1) {Row = 7, Col = 5}
            };
        }
        public void OnPostAddTable(int row, int col, int number)
        {
            int id = Tables.Count();
            Table table = new Table(id, number, null, 0);
            Tables.Add(table);
        }

    }
}
