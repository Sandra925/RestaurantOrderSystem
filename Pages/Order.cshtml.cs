using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Pages
{
    public class OrderModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public OrderModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Order Order { get; set; } = new Order();

        public async Task OnGetAsync(int id)
        {
            try
            {
                Console.WriteLine($"Loading order with ID: {id}");

                Order = await _httpClient.GetFromJsonAsync<Order>($"api/orders/{id}");
                Console.WriteLine($"Order loaded: {Order?.Id}, Table: {Order?.TableId}");

                if (Order != null)
                {
                    Items = await _httpClient.GetFromJsonAsync<List<Item>>("api/items") ?? new List<Item>();
                    Console.WriteLine($"Available items loaded: {Items.Count}");

                    await LoadOrderItems(Order.TableId, Order.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading order: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Order = new Order();
                Items = new List<Item>();
                OrderItems = new List<OrderItem>();
            }
        }

        private async Task LoadOrderItems(int tableId, int orderId)
        {
            try
            {
                Console.WriteLine($"Loading order items for Table: {tableId}, Order: {orderId}");

                var response = await _httpClient.GetAsync($"api/tables/{tableId}/orders/{orderId}/items");

                if (response.IsSuccessStatusCode)
                {
                    OrderItems = await response.Content.ReadFromJsonAsync<List<OrderItem>>() ?? new List<OrderItem>();
                    Console.WriteLine($"Order items loaded: {OrderItems.Count}");

                    // Debug: Print each order item
                    foreach (var orderItem in OrderItems)
                    {
                        Console.WriteLine($"OrderItem: ID={orderItem.Id}, ItemId={orderItem.ItemId}, Quantity={orderItem.Quantity}");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error loading order items: {response.StatusCode} - {errorContent}");
                    OrderItems = new List<OrderItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception loading order items: {ex.Message}");
                OrderItems = new List<OrderItem>();
            }
        }

        public async Task<IActionResult> OnPostAddOrder(int customerNum, int tableId)
        {
            var order = new Order
            {
                CreatedAt = DateTime.Now,
                TableId = tableId,
                CustomerCount = customerNum,
                Status = OrderStatus.Open
            };

            var response = await _httpClient.PostAsJsonAsync("api/orders", order);

            if (response.IsSuccessStatusCode)
            {
                Order = await response.Content.ReadFromJsonAsync<Order>();
                return RedirectToPage(new { id = Order.Id });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creating order: {errorContent}");
                ModelState.AddModelError(string.Empty, "Failed to create order");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAddItemsToOrder(int itemID, int orderID, int tableID)
        {
            try
            {
                Console.WriteLine($"Adding item {itemID} to order {orderID} on table {tableID}");

                var itemData = new
                {
                    ItemId = itemID,
                    Quantity = 1
                };

                var response = await _httpClient.PostAsJsonAsync($"api/tables/{tableID}/orders/{orderID}/items", itemData);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Item added successfully, redirecting...");
                    return RedirectToPage(new { id = orderID });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error adding item: {error}");
                    TempData["Error"] = "Failed to add item to order";
                    return RedirectToPage(new { id = orderID });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception adding item: {ex.Message}");
                TempData["Error"] = "An error occurred while adding the item";
                return RedirectToPage(new { id = orderID });
            }
        }

        public async Task<IActionResult> OnPostUpdateOrderItem(int orderItemId, int orderId, int tableId, int quantity)
        {
            try
            {
                var updateData = new { Quantity = quantity };
                var response = await _httpClient.PutAsJsonAsync($"api/tables/{tableId}/orders/{orderId}/items/{orderItemId}", updateData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(new { id = orderId });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating item: {error}");
                    return RedirectToPage(new { id = orderId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating item: {ex.Message}");
                return RedirectToPage(new { id = orderId });
            }
        }

        public async Task<IActionResult> OnPostRemoveItemFromOrder(int orderItemId, int orderId, int tableId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tables/{tableId}/orders/{orderId}/items/{orderItemId}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(new { id = orderId });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error removing item: {error}");
                    return RedirectToPage(new { id = orderId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing item: {ex.Message}");
                return RedirectToPage(new { id = orderId });
            }
        }
    }
}