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
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

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
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tables/{row}/{col}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Table at row {row}, column {col} has been deleted successfully.";
                    return RedirectToPage();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["Error"] = $"Table at row {row}, column {col} does not exist. Cannot delete a non-existent table.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Failed to delete table: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                TempData["Error"] = "An error occurred while trying to delete the table.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteTables()
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tables");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "All tables have been deleted successfully.";
                    return RedirectToPage();
                }
                else
                {
                    TempData["Error"] = "Failed to delete all tables.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                TempData["Error"] = "An error occurred while trying to delete tables.";
            }

            return RedirectToPage();
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