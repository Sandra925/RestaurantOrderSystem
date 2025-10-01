using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Pages
{
    public class ItemModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public ItemModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }
        public List<Item> Items { get; set; } = new List<Item>();
        public async Task OnGetAsync()
        {
            try
            {
                Items = await _httpClient.GetFromJsonAsync<List<Item>>("/api/items");
            }
            catch (Exception ex)
            {
                Items = new List<Item>();
            }
        }
        public async Task<IActionResult> OnPostAddItem(string name, decimal price)
        {
            var item = new Item { Name = name, Price = price };
            var response = await _httpClient.PostAsJsonAsync("api/items", item);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }
            await OnGetAsync();
            return Page();

        }
        
    }
}
