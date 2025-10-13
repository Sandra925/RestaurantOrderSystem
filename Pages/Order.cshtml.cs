using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public Order Order { get; set; } = new Order();

        public async Task OnGetAsync(int id)
        {
            try
            {
                Order = await _httpClient.GetFromJsonAsync<Order>($"api/orders/{id}");
                Items = await _httpClient.GetFromJsonAsync<List<Item>>($"api/items");
            }
            catch (Exception ex)
            {
                Order Order = new Order();
            }
        }
        public async Task<IActionResult> OnPostAddOrder(int customerNum, int tableId)
        {
            var order = new Order
            {
                CreatedAt = DateTime.Now.Date,
                TableId = tableId,
                CustomerCount = customerNum,
                Status = OrderStatus.Open
            };
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(order));
            var response = await _httpClient.PostAsJsonAsync("api/orders", order);

            if (response.IsSuccessStatusCode)
            {
                Order = await response.Content.ReadFromJsonAsync<Order>();
                return RedirectToPage(new { id = Order.Id });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(errorContent);
                return Page();
            }
        }
        public async Task<IActionResult> OnPostAddItemsToOrder(int itemID, int orderID, int tableID)
        {
            try
            {
                var itemData = new
                {
                    ItemId = itemID,
                    Quantity = 1
                };
                var response = await _httpClient.PostAsJsonAsync($"api/tables/{tableID}/orders/{orderID}/items", itemData);

                if (response.IsSuccessStatusCode)
                {
                    Order = await _httpClient.GetFromJsonAsync<Order>($"api/orders/{orderID}");
                    return RedirectToPage(new { id = orderID });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(error);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item {ex.Message}");
                return Page();

            }
        }
    }
}
