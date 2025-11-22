using System.Net.Http.Headers;

namespace RestaurantOrderSystem.Services
{
    public class ApiJwtDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiJwtDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;

            if (context != null)
            {
                var token = context.Session.GetString("ApiJwt");

                if (!string.IsNullOrEmpty(token))
                {
                    // Only set if not already set, so you can override manually if needed.
                    if (request.Headers.Authorization == null)
                    {
                        request.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                    }
                }
                else
                {
                    // Optional: some logging
                    Console.WriteLine("ApiJwtDelegatingHandler: no JWT in session");
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
