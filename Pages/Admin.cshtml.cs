using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RestaurantOrderSystem.Pages
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
