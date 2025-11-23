using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;
using RestaurantOrderSystem.Pages;
using RestaurantOrderSystem.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Add services
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// JWT Configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtConfig = jwtSection.Get<JwtOptions>() ?? new JwtOptions
{
    Secret = "fallback-secret-key-for-development-12345",
    ExpirationMinutes = 120
};

builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
        .Build();

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("StaffOnly", policy =>
        policy.RequireRole("Admin", "Waiter", "Cook"));

    options.AddPolicy("WaiterOnly", policy =>
        policy.RequireRole("Admin", "Waiter"));

    options.AddPolicy("CookOnly", policy =>
        policy.RequireRole("Admin", "Cook"));

    options.AddPolicy("CanManageTables", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CanManageItems", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CanCreateOrders", policy =>
        policy.RequireRole("Admin", "Waiter"));

    options.AddPolicy("CanUpdateOrderStatus", policy =>
        policy.RequireRole("Admin", "Cook"));

    options.AddPolicy("CanViewOrders", policy =>
        policy.RequireRole("Admin", "Waiter", "Cook"));
});

// Clear and Simple Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    // Default scheme for Razor Pages - uses Cookies
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";
    options.Cookie.Name = ".RestaurantAuth";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
        ValidateIssuer = !string.IsNullOrWhiteSpace(jwtConfig.Issuer),
        ValidIssuer = jwtConfig.Issuer,
        ValidateAudience = !string.IsNullOrWhiteSpace(jwtConfig.Audience),
        ValidAudience = jwtConfig.Audience,
        ValidateLifetime = true,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
})
.AddOpenIdConnect("Google", options =>
{
    options.Authority = "https://accounts.google.com";
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;

    options.CallbackPath = "/signin-google";
    options.SignedOutCallbackPath = "/signout-callback-google";

    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.Scope.Add("openid");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.RedirectUri =
                $"{context.Request.Scheme}://{context.Request.Host}{options.CallbackPath}";
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            try
            {
                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value
                            ?? context.Principal?.FindFirst("email")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return;
                }

                using var scope = context.HttpContext.RequestServices.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var jwtService = scope.ServiceProvider.GetRequiredService<IJwtAuthService>();

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = email,
                        Username = GenerateUsernameFromEmail(email),
                        Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                        Role = Role.Unassigned
                    };

                    dbContext.Users.Add(user);
                    await dbContext.SaveChangesAsync();
                }

                // Sign into cookie auth for UI
                await jwtService.SignInWithCookieAsync(context.HttpContext, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation error: {ex.Message}");
            }
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            context.Response.Redirect("/Login?error=auth_failed");
            context.HandleResponse();
            return Task.CompletedTask;
        },
        OnAccessDenied = context =>
        {
            context.Response.Redirect("/Login?error=access_denied");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

static string GenerateUsernameFromEmail(string email)
{
    var username = email.Split('@')[0];
    username = System.Text.RegularExpressions.Regex.Replace(username, @"[^a-zA-Z0-9]", "");
    if (string.IsNullOrEmpty(username))
    {
        username = "user" + DateTime.Now.Ticks.ToString()[10..];
    }
    return username;
}

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RestaurantSession";
});

// Database
if (builder.Environment.IsProduction())
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? "Data Source=restaurant.db";
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// HTTP Clients
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
})
.AddHttpMessageHandler<ApiJwtDelegatingHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<ApiJwtDelegatingHandler>();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Restaurant Order System API",
        Description = "API for Restaurant Order System"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrated successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API v1");
    options.RoutePrefix = "swagger";
    options.OAuthClientId("swagger-ui");
    options.OAuthAppName("Swagger UI");
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints with clear scheme separation
app.MapRazorPages(); // Uses Cookie authentication (default)

// API controllers can use either scheme, but specify JWT as default for APIs
app.MapControllers();

app.Run();