using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Pages
{
    public class OrderModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public OrderModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public List<Order> Orders { get; set; } = new List<Order>();

        public async Task OnGetAsync()
        {

        }
    }
}
