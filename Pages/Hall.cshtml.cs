using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Models;
using System.Net.Http.Json;

namespace RestaurantOrderSystem.Pages
{
    public class HallModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public HallModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public List<Table> Tables { get; set; } = new List<Table>();
        public async Task OnGetAsync()
        {
            try
            {
                Tables = await _httpClient.GetFromJsonAsync<List<Table>>("/api/tables");
            }
            catch (Exception ex)
            {
                Tables = new List<Table>();
            }
        }

        public async Task<IActionResult> OnPostAddTable(int row, int col, int num)
        {
            var table = new Table
            {
                Row = row,
                Col = col,
                Number = num
            };

            var response = await _httpClient.PostAsJsonAsync("api/tables", table);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteTable(int row, int col)
        {
            var response = await _httpClient.DeleteAsync($"api/tables/byposition/{row}/{col}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }

            return Page();
        }
        public async Task<IActionResult> OnPostDeleteTables()
        {
            var response = await _httpClient.DeleteAsync($"api/tables");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }
            return Page();
        }

    }
}