using Microsoft.JSInterop;

namespace Poll.Services;

public class HttpUtils
{
    private readonly IJSRuntime _runtime;

    public HttpUtils(IJSRuntime runtime)
    {
        _runtime = runtime;
    }

    public async Task<bool> IsPreRendering()
    {
        try
        {
            await _runtime.InvokeVoidAsync("jsMethod");
            return false;
        }
        catch (System.InvalidOperationException)
        {
            return true;
        }
    }
}