using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using RestaurantOrderSystem.Services;

namespace RestaurantOrderSystem.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IJwtAuthService _jwtAuthService;
        public List<User> Users { get; set; } = new List<User>();

        [BindProperty]
        public User NewUser { get; set; } = new User();

        [BindProperty]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsGoogleSignUp { get; set; } = false;

        public SignUpModel(AppDbContext context, IJwtAuthService jwtAuthService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
        }

        public void OnGet([FromQuery] string email)
        {
            try
            {
                Users = _context.Users.ToList();
                if (!string.IsNullOrEmpty(email))
                {
                    NewUser.Email = email;
                    IsGoogleSignUp = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError(string.Empty, "Error loading page. Please try again.");
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (!IsGoogleSignUp && NewUser.Password != ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    return Page();
                }

                if (_context.Users.Any(u => u.Username == NewUser.Username))
                {
                    ModelState.AddModelError("NewUser.Username", "Username is already taken.");
                    return Page();
                }

                if (_context.Users.Any(u => u.Email == NewUser.Email))
                {
                    ModelState.AddModelError("NewUser.Email", "Email is already registered.");
                    return Page();
                }

                if (!IsGoogleSignUp && NewUser.Password.Length < 3)
                {
                    ModelState.AddModelError("NewUser.Password", "Password must be at least 3 characters long.");
                    return Page();
                }

                if (IsGoogleSignUp)
                {
                    // For Google OAuth users set a random password
                    NewUser.Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString() + DateTime.Now.Ticks);
                }
                else
                {
                    // For traditional sign-up, hash the provided password
                    NewUser.Password = BCrypt.Net.BCrypt.HashPassword(NewUser.Password);
                }
                _context.Users.Add(NewUser);
                _context.SaveChanges();

                // After creating the user, generate JWT and set cookie
                var jwt = _jwtAuthService.GenerateToken(NewUser);
                HttpContext.Session.SetString("ApiJwt", jwt);
                _jwtAuthService.SignInWithCookieAsync(HttpContext, NewUser).GetAwaiter().GetResult();

                TempData["SuccessMessage"] = "Account created successfully! You are now signed in.";
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError(string.Empty, "An error occurred while creating your account. Please try again.");
                return Page();
            }
        }
    }
}