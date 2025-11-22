using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ItemsController(AppDbContext context)
        {
            _context = context;
        }
        //POST: api/items
        [HttpPost]
        [Authorize(Policy = "CanManageItems")]
        public async Task<IActionResult> PostItem(Item item)
        {
            if(item == null)
            {
                return BadRequest("Payload is null");
            }
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
  
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetItem", new { id = item.Id }, item);
        }
        // GET: api/items
        [HttpGet]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            return await _context.Items.ToListAsync();
        }
        //GET: api/items/id
        [HttpGet("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }
            return item;
        }

        //DELETE: api/items/id
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageItems")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if(item == null)
            {
                return NotFound();
            }
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //PUT: api/items/id
        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageItems")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] Item updatedItem)
        {
            if (id != updatedItem.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (updatedItem.Price < 0)
                return UnprocessableEntity(new { message = "Price must be positive numbers" });

            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Item not found" });

            item.Name = updatedItem.Name;
            item.Price = updatedItem.Price;

            await _context.SaveChangesAsync();
            return Ok(item);
        }

    }
}
