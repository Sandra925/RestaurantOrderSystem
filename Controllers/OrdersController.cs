using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
       private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            List<Order> orders = await _context.Orders.ToListAsync();
            if (orders.Count == 0)
            {
                return NotFound();
            }
            return orders;
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder([FromBody] Order order)
        {
            if (order == null)
                return BadRequest("Payload is null");

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var createdOrder = await _context.Orders
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return Ok(createdOrder);
        }

        //DELETE: api/items/deleteOrder/id
        [HttpDelete("deleteOrder/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //DELETE: api/orders
        [HttpDelete]
        public async Task<IActionResult> DeleteTables()
        {
            List<Order> orders = await _context.Orders.ToListAsync();
            foreach (Order order in orders)
            {
                _context.Orders.Remove(order);
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //PUT: api/items/id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order updatedOrder)
        {
            if (id != updatedOrder.Id)
                return BadRequest(new { message = "ID mismatch" });

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            order.CustomerCount = updatedOrder.CustomerCount;
            order.TableId = updatedOrder.TableId;

            await _context.SaveChangesAsync();
            return Ok(order);
        }


    }
}
