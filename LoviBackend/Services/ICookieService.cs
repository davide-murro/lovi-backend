namespace LoviBackend.Services
{
    public interface ICookieService
    {
        void Append(HttpResponse response, string key, string value, DateTime expires);
        void Delete(HttpResponse response, string key);
        string? Read(HttpRequest request, string key);
    }
}
