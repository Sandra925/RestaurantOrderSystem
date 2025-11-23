using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;
using RestaurantOrderSystem.Services;

namespace RestaurantOrderSystem.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IJwtAuthService _jwtAuthService;

        public LoginModel(AppDbContext context, IJwtAuthService jwtAuthService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
        }

        [BindProperty]
        public LoginInput LoginInput { get; set; } = new LoginInput();

        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(
            string returnUrl = "/",
            [FromQuery] bool returnToken = false)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == LoginInput.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(LoginInput.Password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return Page();
            }


            var tokenString = _jwtAuthService.GenerateToken(user);

            HttpContext.Session.SetString("ApiJwt", tokenString);
            await _jwtAuthService.SignInWithCookieAsync(HttpContext, user);


            if (returnToken || Request.Headers["Accept"].ToString()
                    .Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new
                {
                    token = tokenString,
                    user_id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    role = user.Role.ToString()
                });
            }

            switch (user.Role)
            {
                case Role.Cook:
                    return LocalRedirect("/Kitchen");

                case Role.Waiter:
                    return LocalRedirect("/Hall");

                case Role.Admin:
                    return LocalRedirect("/Hall");

                default:
                    return LocalRedirect("/Index");
            }
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }
    }

    public class LoginInput
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
