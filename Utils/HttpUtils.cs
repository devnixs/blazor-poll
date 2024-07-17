using Microsoft.JSInterop;

namespace Poll.Services;

public class HttpUtils
{
    private readonly IJSRuntime _runtime;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IWebHostEnvironment _hostEnvironment;

    public HttpUtils(IJSRuntime runtime, IHttpContextAccessor contextAccessor, IWebHostEnvironment hostEnvironment)
    {
        _runtime = runtime;
        _contextAccessor = contextAccessor;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<bool> IsPreRendering()
    {
        try
        {
            await _runtime.InvokeVoidAsync("jsMethod");
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    public string GetBaseUrl()
    {
        var ctx = _contextAccessor.HttpContext;
        if (ctx is null)
        {
            return string.Empty;
        }

        var hostName = ctx.Request.Host.ToString();
        var scheme = ctx.Request.Scheme;
        if (_hostEnvironment.IsProduction())
        {
            scheme = "https";
        }

        return $"{scheme}://{hostName}";
    }
}