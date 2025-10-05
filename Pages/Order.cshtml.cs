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
        public Order Order { get; set; } = new Order();

        public async Task OnGetAsync(int id)
        {
            try
            {
                Order = await _httpClient.GetFromJsonAsync<Order>($"api/orders/{id}");
                Items = await _httpClient.GetFromJsonAsync<List<Item>>($"api/items");
            }
            catch(Exception ex)
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
                Status = OrderStatus.Pending
            };
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(order));
            var response = await _httpClient.PostAsJsonAsync("api/orders", order);

            if (response.IsSuccessStatusCode)
            {
                Order = await response.Content.ReadFromJsonAsync<Order>();
                return RedirectToPage(new {id = Order.Id});
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(errorContent);
                return Page();
            }

        }
    }
}
