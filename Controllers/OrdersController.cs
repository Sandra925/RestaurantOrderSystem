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
    public class OrdersController : ControllerBase
    {
       private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        [Authorize(Policy = "CanViewOrders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            List<Order> orders = await _context.Orders.Include(x=>x.OrderItems).ToListAsync();
            if (orders.Count == 0)
            {
                return NotFound();
            }
            return orders;
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewOrders")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        [Authorize(Policy = "CanCreateOrders")]
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

            return CreatedAtAction("GetOrder", new { id = order.Id }, createdOrder);
        }

        //DELETE: api/orders/id
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "CanCreateOrders")]
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

        [HttpPatch("{id}/status")]
        [Authorize(Policy = "CanUpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound(new { message = "Order not found" });

                order.Status = request.Status;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Order status updated successfully",
                    order = new
                    {
                        order.Id,
                        order.TableId,
                        order.Status,
                        order.CustomerCount,
                        order.CreatedAt,
                        order.UpdatedAt,
                        OrderItems = order.OrderItems.Select(oi => new
                        {
                            oi.Id,
                            oi.Quantity,
                            Item = new { oi.Item.Id, oi.Item.Name, oi.Item.Price }
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating order status", error = ex.Message });
            }
        }

        // POST: api/orders/{id}/pay
        [HttpPost("{id}/pay")]
        [Authorize(Policy = "CanCreateOrders")]
        public async Task<IActionResult> PayOrder(int id, [FromBody] PaymentRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound(new { message = "Order not found" });

                if (order.PaymentStatus == PaymentStatus.Paid)
                    return BadRequest(new { message = "Order has already been paid" });

                order.PaymentStatus = PaymentStatus.Paid;
                order.PaymentMethod = request.PaymentMethod;
                order.PaidAt = DateTime.UtcNow;

                // Update table status to Available when order is paid
                if (order.Table != null)
                {
                    order.Table.Status = TableStatus.Available;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Order paid successfully",
                    order = new
                    {
                        order.Id,
                        order.TableId,
                        order.Status,
                        order.PaymentStatus,
                        order.PaymentMethod,
                        order.PaidAt,
                        order.CreatedAt,
                        order.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing payment", error = ex.Message });
            }
        }

        // GET: api/orders/{id}/payment-status
        [HttpGet("{id}/payment-status")]
        [Authorize(Policy = "CanViewOrders")]
        public async Task<IActionResult> GetPaymentStatus(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);

                if (order == null)
                    return NotFound(new { message = "Order not found" });

                return Ok(new
                {
                    orderId = order.Id,
                    paymentStatus = order.PaymentStatus,
                    paymentMethod = order.PaymentMethod,
                    paidAt = order.PaidAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving payment status", error = ex.Message });
            }
        }

        public class OrderStatusUpdateRequest
        {
            public OrderStatus Status { get; set; }
        }

        public class PaymentRequest
        {
            public PaymentMethod PaymentMethod { get; set; }
        }
    }
}
