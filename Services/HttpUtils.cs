namespace Poll.Services;

public class HttpUtils
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUtils(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsPreRendering()
    {
        return _httpContextAccessor.HttpContext is not null;
    }
}