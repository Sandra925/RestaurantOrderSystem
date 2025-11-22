using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Controllers
{
    [ApiController]
    [Route("api/tables/{tableId}/orders")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class TableOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TableOrdersController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/tables/{tableId}/orders
        [HttpGet]
        [Authorize(Policy = "CanViewOrders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersForTable(int tableId)
        {
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return NotFound(new { message = "Table not found" });
            }

            var orders = await _context.Orders
                .Where(o => o.TableId == tableId)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/tables/{tableId}/orders/{orderId}
        [HttpGet("{orderId}")]
        [Authorize(Policy = "CanViewOrders")]
        public async Task<ActionResult<Order>> GetOrder(int tableId, int orderId)
        {
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.TableId == tableId);
            if (order == null)
            {
                return NotFound();
            }
            return order;
        }


        // POST: api/tables/{tableId}/orders
        [HttpPost]
        [Authorize(Policy = "CanCreateOrders")]
        public async Task<ActionResult<Order>> CreateOrder(int tableId, [FromBody] Order order)
        {
            if (order == null)
                return BadRequest(new { message = "Order payload is null" });

            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return NotFound(new { message = "Table not found" });
            }

            order.TableId = tableId;

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Return the created order with proper location header
            return CreatedAtAction(
                nameof(GetOrder),
                new { tableId = tableId, orderId = order.Id },
                order);
        }


        // PUT: api/tables/{tableId}/orders/{orderId}
        [HttpPut("{orderId}")]
        [Authorize(Policy = "CanCreateOrders")]
        public async Task<IActionResult> UpdateOrder(int tableId, int orderId, [FromBody] Order updatedOrder)
        {
            if (orderId != updatedOrder.Id)
                return BadRequest(new { message = "ID mismatch" });

            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return NotFound(new { message = "Table not found" });
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.TableId == tableId);

            if (order == null)
                return NotFound(new { message = "Order not found for this table" });

            order.CustomerCount = updatedOrder.CustomerCount;
            order.TableId = tableId;

            await _context.SaveChangesAsync();
            return Ok(order);
        }

        // DELETE: api/tables/{tableId}/orders/{orderId}

        [HttpDelete("{orderId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteOrder(int tableId, int orderId)
        {
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return NotFound(new { message = "Table not found" });
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.TableId == tableId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found for this table" });
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
