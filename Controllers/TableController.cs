using Microsoft.AspNetCore.Mvc;
using RestaurantOrderSystem.Models;
using RestaurantOrderSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace RestaurantOrderSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TablesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/tables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Table>>> GetTables()
        {
            List<Table> tables = await _context.Tables.ToListAsync();
            if(tables.Count == 0)
            {
                return NotFound();
            }
            return tables;
        }

        // GET: api/tables/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Table>> GetTable(int id)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null)
            {
                return NotFound();
            }

            return table;
        }

        // POST: api/tables
        [HttpPost]
        public async Task<ActionResult<Table>> PostTable(Table table)
        {
            if(table == null)
            {
                return BadRequest("Payload is null");
            }
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            if (table.Id == 0)
            {
                var random = new Random();
                int id;
                do
                {
                    id = random.Next(1, 20);
                } while (await _context.Tables.AnyAsync(t => t.Id == id));

                table.Id = id;
            }

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTable", new { id = table.Id }, table);
        }

        // DELETE: api/tables/byposition/1/2
        [HttpDelete("byposition/{row}/{col}")]
        public async Task<IActionResult> DeleteTableByPosition(int row, int col)
        {
            var table = await _context.Tables.FirstOrDefaultAsync(t => t.Row == row && t.Col == col);
            if (table == null)
            {
                return NotFound();
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //DELETE: api/tables
        [HttpDelete]
        public async Task<IActionResult> DeleteTables()
        {
            List<Table> tables = await _context.Tables.ToListAsync();
            foreach (Table table in tables)
            {
                _context.Tables.Remove(table);
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] Table updatedTable)
        {
            if (id != updatedTable.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (updatedTable.Row < 0 || updatedTable.Col < 0)
                return UnprocessableEntity(new { message = "Row and Col must be positive numbers" });

            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return NotFound(new { message = "Table not found" });

            table.Row = updatedTable.Row;
            table.Col = updatedTable.Col;
            table.Number = updatedTable.Number;

            await _context.SaveChangesAsync();
            return Ok(table);
        }

    }
}