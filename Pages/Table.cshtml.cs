using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Pages
{
    public class TableModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public Table Table { get; set; } = new Table();

        public TableModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }
        public async Task OnGetAsync(int id)
        {
            try
            {
                Table = await _httpClient.GetFromJsonAsync<Table>($"api/tables/{id}");
            }
            catch( Exception ex ) 
            {
                Table table = new Table();
            }
        }
    }
}
