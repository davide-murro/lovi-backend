namespace LoviBackend.Services
{
    public class CookieService : ICookieService
    {
        private readonly IConfiguration _configuration;

        public CookieService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
        }

        public void Append(HttpResponse response, string key, string value, DateTime expires)
        {
            var options = new CookieOptions
            {
                HttpOnly = Convert.ToBoolean(_configuration["CookieOptions:HttpOnly"]),
                Secure = Convert.ToBoolean(_configuration["CookieOptions:Secure"]),
                SameSite = Enum.Parse<SameSiteMode>(_configuration["CookieOptions:SameSite"]!),
                Path = _configuration["CookieOptions:Path"],
                Expires = expires
            };
            response.Cookies.Append(key, value, options);
        }

        public void Delete(HttpResponse response, string key)
        {
            var options = new CookieOptions
            {
                HttpOnly = Convert.ToBoolean(_configuration["CookieOptions:HttpOnly"]),
                Secure = Convert.ToBoolean(_configuration["CookieOptions:Secure"]),
                SameSite = Enum.Parse<SameSiteMode>(_configuration["CookieOptions:SameSite"]!),
                Path = _configuration["CookieOptions:Path"],
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            // Set expired cookie to instruct browser to delete
            response.Cookies.Append(key, string.Empty, options);
        }

        public string? Read(HttpRequest request, string key)
        {
            request.Cookies.TryGetValue(key, out var val);
            return val;
        }
    }
}
