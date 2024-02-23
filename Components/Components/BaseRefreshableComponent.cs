using Microsoft.AspNetCore.Components;
using Poll.Services;

namespace Poll.Components.Components;

public abstract class BaseRefreshableComponent : ComponentBase, IDisposable
{
    [Inject]
    protected GameStateAccessor GameStateAccessor { get; set; }

    [Parameter]
    public string? GameId { get; set; }
    
    protected abstract ILogger Logger { get; }
    
    
    protected GameState? CurrentGame = null;
    
    private bool _disposed = false;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(GameId) && Guid.TryParse(GameId, out var gameIdGuid))
        {
            CurrentGame = GameStateAccessor.GetGame(gameIdGuid);
        }

        CurrentGame?.SubscribeStateChanged(OnStateChanged);

        await base.OnInitializedAsync();
    }
    
    private void OnStateChanged()
    {
        if (_disposed)
        {
            return;
        }
        
        Logger.LogInformation("State Changed");
        AfterRefresh();
        InvokeAsync(StateHasChanged);
    }

    public virtual void Dispose()
    {
        _disposed = true;
        CurrentGame?.SubscribeStateChanged(OnStateChanged);
    }

    protected virtual void AfterRefresh()
    {
    }
}