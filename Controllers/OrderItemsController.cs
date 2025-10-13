using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Controllers
{
    [ApiController]
    [Route("api/tables/{tableId}/orders/{orderId}/items")]
    public class OrderItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/tables/{tableId}/orders/{orderId}/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems(int tableId, int orderId)
        {
            try
            {
                // Check if table exists
                var tableExists = await _context.Tables.AnyAsync(t => t.Id == tableId);
                if (!tableExists)
                {
                    return NotFound(new { message = $"Table with ID {tableId} not found" });
                }

                // Check if order exists and belongs to table
                var orderExists = await _context.Orders.AnyAsync(o => o.Id == orderId && o.TableId == tableId);
                if (!orderExists)
                {
                    return NotFound(new { message = $"Order with ID {orderId} not found for table {tableId}" });
                }

                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .Include(oi => oi.Item)
                    .ToListAsync();

                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving order items", error = ex.Message });
            }
        }

        // POST: api/tables/{tableId}/orders/{orderId}/items
        [HttpPost]
        public async Task<ActionResult<OrderItem>> AddItemToOrder(int tableId, int orderId, [FromBody] AddItemToOrderRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request payload is null" });

                // Check if table exists
                var table = await _context.Tables.FindAsync(tableId);
                if (table == null)
                {
                    return NotFound(new { message = $"Table with ID {tableId} not found" });
                }

                // Check if order exists and belongs to table
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.TableId == tableId);
                if (order == null)
                {
                    return NotFound(new { message = $"Order with ID {orderId} not found for table {tableId}" });
                }

                // Check if item exists
                var item = await _context.Items.FindAsync(request.ItemId);
                if (item == null)
                {
                    return NotFound(new { message = $"Item with ID {request.ItemId} not found" });
                }

                // Validate quantity
                if (request.Quantity <= 0)
                {
                    return BadRequest(new { message = "Quantity must be greater than 0" });
                }

                // Check if item already exists in order
                var existingOrderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ItemId == request.ItemId);

                if (existingOrderItem != null)
                {
                    // Update quantity if item already exists in order
                    existingOrderItem.Quantity += request.Quantity;
                    await _context.SaveChangesAsync();

                    // Reload the item details
                    await _context.Entry(existingOrderItem)
                        .Reference(oi => oi.Item)
                        .LoadAsync();

                    return Ok(existingOrderItem);
                }
                else
                {
                    // Create new order item
                    var orderItem = new OrderItem
                    {
                        OrderId = orderId,
                        ItemId = request.ItemId,
                        Quantity = request.Quantity
                    };

                    _context.OrderItems.Add(orderItem);
                    await _context.SaveChangesAsync();

                    // Load the item details for response
                    await _context.Entry(orderItem)
                        .Reference(oi => oi.Item)
                        .LoadAsync();

                    return CreatedAtAction(
                        nameof(GetOrderItem),
                        new { tableId = tableId, orderId = orderId, orderItemId = orderItem.Id },
                        orderItem);
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Database error while adding item to order", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        // GET: api/tables/{tableId}/orders/{orderId}/items/{orderItemId}
        [HttpGet("{orderItemId}")]
        public async Task<ActionResult<OrderItem>> GetOrderItem(int tableId, int orderId, int orderItemId)
        {
            try
            {
                // Check if table exists
                var tableExists = await _context.Tables.AnyAsync(t => t.Id == tableId);
                if (!tableExists)
                {
                    return NotFound(new { message = $"Table with ID {tableId} not found" });
                }

                // Check if order exists and belongs to table
                var orderExists = await _context.Orders.AnyAsync(o => o.Id == orderId && o.TableId == tableId);
                if (!orderExists)
                {
                    return NotFound(new { message = $"Order with ID {orderId} not found for table {tableId}" });
                }

                var orderItem = await _context.OrderItems
                    .Include(oi => oi.Item)
                    .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.OrderId == orderId);

                if (orderItem == null)
                {
                    return NotFound(new { message = $"Order item with ID {orderItemId} not found in order {orderId}" });
                }

                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving order item", error = ex.Message });
            }
        }

        // PUT: api/tables/{tableId}/orders/{orderId}/items/{orderItemId}
        [HttpPut("{orderItemId}")]
        public async Task<IActionResult> UpdateOrderItem(int tableId, int orderId, int orderItemId, [FromBody] UpdateOrderItemRequest request)
        {
            try
            {
                // Check if table exists
                var tableExists = await _context.Tables.AnyAsync(t => t.Id == tableId);
                if (!tableExists)
                {
                    return NotFound(new { message = $"Table with ID {tableId} not found" });
                }

                // Check if order exists and belongs to table
                var orderExists = await _context.Orders.AnyAsync(o => o.Id == orderId && o.TableId == tableId);
                if (!orderExists)
                {
                    return NotFound(new { message = $"Order with ID {orderId} not found for table {tableId}" });
                }

                var orderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.OrderId == orderId);

                if (orderItem == null)
                {
                    return NotFound(new { message = $"Order item with ID {orderItemId} not found in order {orderId}" });
                }

                // Validate quantity
                if (request.Quantity <= 0)
                {
                    return BadRequest(new { message = "Quantity must be greater than 0" });
                }

                // Update properties
                orderItem.Quantity = request.Quantity;

                await _context.SaveChangesAsync();

                // Reload the item details
                await _context.Entry(orderItem)
                    .Reference(oi => oi.Item)
                    .LoadAsync();

                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating order item", error = ex.Message });
            }
        }

        // DELETE: api/tables/{tableId}/orders/{orderId}/items/{orderItemId}
        [HttpDelete("{orderItemId}")]
        public async Task<IActionResult> RemoveItemFromOrder(int tableId, int orderId, int orderItemId)
        {
            try
            {
                // Check if table exists
                var tableExists = await _context.Tables.AnyAsync(t => t.Id == tableId);
                if (!tableExists)
                {
                    return NotFound(new { message = $"Table with ID {tableId} not found" });
                }

                // Check if order exists and belongs to table
                var orderExists = await _context.Orders.AnyAsync(o => o.Id == orderId && o.TableId == tableId);
                if (!orderExists)
                {
                    return NotFound(new { message = $"Order with ID {orderId} not found for table {tableId}" });
                }

                var orderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.OrderId == orderId);

                if (orderItem == null)
                {
                    return NotFound(new { message = $"Order item with ID {orderItemId} not found in order {orderId}" });
                }

                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing item from order", error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class AddItemToOrderRequest
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Notes { get; set; }
    }

    public class UpdateOrderItemRequest
    {
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}