using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TablesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/tables
        [HttpGet]
        [Authorize(Policy = "StaffOnly")]
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
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<Table>> GetTable(int id)
        {
            var table = await _context.Tables.Include(t => t.Orders)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound();
            }

            return table;
        }

        // POST: api/tables
        [HttpPost]
        [Authorize(Policy = "CanManageTables")]
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

        // DELETE: api/tables/1/2
        [HttpDelete("{row}/{col}")]
        [Authorize(Policy = "CanManageTables")]
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
        [Authorize(Policy = "CanManageTables")]
        public async Task<IActionResult> DeleteTables()
        {
            List<Table> tables = await _context.Tables.ToListAsync();
            if(tables == null)
            {
                return NotFound();
            }
            foreach (Table table in tables)
            {
                _context.Tables.Remove(table);
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //PUT: api/tables/id
        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageTables")]
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