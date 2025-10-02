using Microsoft.AspNetCore.Mvc;
using RestaurantOrderSystem.Data;

namespace RestaurantOrderSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
       private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

    }
}
