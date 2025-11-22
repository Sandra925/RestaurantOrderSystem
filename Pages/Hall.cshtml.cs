using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RestaurantOrderSystem.Pages
{
    [Authorize(Policy = "StaffOnly")]
    public class HallModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HallModel(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _httpContextAccessor = httpContextAccessor;
        }

        public List<Table> Tables { get; set; } = new List<Table>();
        public async Task OnGetAsync()
        {
            var response = await _httpClient.GetAsync("/api/tables");
                Console.WriteLine($"API Response Status: {response.StatusCode}"); // Debug log

                if (response.IsSuccessStatusCode)
                {
                    Tables = await response.Content.ReadFromJsonAsync<List<Table>>();
                    Console.WriteLine($"Loaded {Tables?.Count ?? 0} tables"); // Debug log
                }
        }
        

        public async Task<IActionResult> OnPostAddTable(int row, int col, int num)
        {
            try
            {
                var table = new Table
                {
                    Row = row,
                    Col = col,
                    Number = num,
                    Status = TableStatus.Available
                };

                var response = await _httpClient.PostAsJsonAsync("api/tables", table);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error adding table: {errorContent}");
                    TempData["Error"] = "Failed to add table";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                TempData["Error"] = "Error occurred while adding table";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteTable(int row, int col)
        {
            var response = await _httpClient.DeleteAsync($"api/tables/{row}/{col}");

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
        public async Task<IActionResult> OnPostUpdateTableAsync(int id, int row, int col, int num)
        {
            var updatedTable = new Table
            {
                Id = id,
                Row = row,
                Col = col,
                Number = num
            };

            var response = await _httpClient.PutAsJsonAsync($"api/tables/{id}", updatedTable);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }

            return Page();
        }

    }
}