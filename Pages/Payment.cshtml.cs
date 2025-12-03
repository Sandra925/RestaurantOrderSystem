using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Models;
using System.Text.Json;
using static RestaurantOrderSystem.Controllers.OrdersController;

namespace RestaurantOrderSystem.Pages
{
    public class PaymentModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public int OrderId { get; set; }

        [BindProperty]
        public int TableId { get; set; }

        public Order? Order { get; set; }
        public Table? Table { get; set; }

        public PaymentModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task OnGetAsync(int orderId, int tableId)
        {
            OrderId = orderId;
            TableId = tableId;
            await LoadOrderDetails();
        }

        private async Task LoadOrderDetails()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/orders/{OrderId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Order = JsonSerializer.Deserialize<Order>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading order: {ex.Message}");
            }

            try
            {
                var response = await _httpClient.GetAsync($"/api/tables/{TableId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Table = JsonSerializer.Deserialize<Table>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading table: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostPayWithCardAsync()
        {
            try
            {
                var paymentData = new PaymentRequest{ PaymentMethod = PaymentMethod.Card };
                var response = await _httpClient.PostAsJsonAsync($"/api/orders/{OrderId}/pay", paymentData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Hall");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Payment failed: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing payment: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostPayWithCashAsync()
        {
            try
            {
                var paymentData = new PaymentRequest { PaymentMethod = PaymentMethod.Cash };
                var response = await _httpClient.PostAsJsonAsync($"/api/orders/{OrderId}/pay", paymentData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Hall");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Payment failed: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing payment: {ex.Message}";
            }

            return Page();
        }
    }
}
