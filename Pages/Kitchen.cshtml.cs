using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Models;
using System.Text.Json;

namespace RestaurantOrderSystem.Pages
{
    [Authorize(Policy = "CookOnly")]
    public class KitchenModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public KitchenModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _configuration = configuration;
        }

        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Order> PendingOrders => Orders.Where(o => o.Status == OrderStatus.Pending).ToList();
        public List<Order> InProgressOrders => Orders.Where(o => o.Status == OrderStatus.InProgress).ToList();
        public List<Order> ReadyOrders => Orders.Where(o => o.Status == OrderStatus.Ready).ToList();

        public async Task OnGetAsync()
        {
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            try
            {
                // Get the JWT token from the cookie
                var token = Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    // Set the authorization header
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync("/api/orders");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Orders = JsonSerializer.Deserialize<List<Order>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Order>();
                }
                else
                {
                    Console.WriteLine($"Error loading orders: {response.StatusCode}");
                    Orders = new List<Order>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception loading orders: {ex.Message}");
                Orders = new List<Order>();
            }
        }

        public async Task<IActionResult> OnPostUpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            try
            {
                // Get the JWT token from the cookie
                var token = Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var updateData = new { status = newStatus };
                var response = await _httpClient.PatchAsJsonAsync($"/api/orders/{orderId}/status", updateData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Order #{orderId} status updated to {newStatus}";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to update order status: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating order status: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkInProgressAsync(int orderId)
        {
            return await OnPostUpdateOrderStatusAsync(orderId, OrderStatus.InProgress);
        }

        public async Task<IActionResult> OnPostMarkReadyAsync(int orderId)
        {
            return await OnPostUpdateOrderStatusAsync(orderId, OrderStatus.Ready);
        }

        public async Task<IActionResult> OnPostMarkServedAsync(int orderId)
        {
            return await OnPostUpdateOrderStatusAsync(orderId, OrderStatus.Served);
        }
    }
}