using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;
using Poll.Services;

namespace Poll.Components.Components;

public abstract class BaseRefreshableComponent : ComponentBase, IDisposable
{
    [Inject]
    protected AppSynchronizer AppSynchronizer { get; set; }

    protected abstract ILogger Logger { get; }
    
    [Inject]
    protected DatabaseReadContextProvider DatabaseReadContextProvider { get; set; }
    
    [Inject]
    protected DatabaseWriteContextProvider DatabaseWriteContextProvider { get; set; }
    
    protected Game? CurrentGame = null;
    protected Question? CurrentQuestion = null;
    CancellationTokenSource cts = new CancellationTokenSource();
    SemaphoreSlim semaphore = new SemaphoreSlim(0);

    private bool _disposed = false;

    protected override async Task OnInitializedAsync()
    {
        AppSynchronizer.SubscribeStateChanged(OnStateChanged);

        _ = Loop();
        await Refresh(isFirstRefresh : true);
        
        await base.OnInitializedAsync();
    }
    
    private async Task Loop()
    {
        while (!cts.Token.IsCancellationRequested)
        {
            await semaphore.WaitAsync();

            await Refresh(isFirstRefresh: false);
        }
    }
    private async Task Refresh(bool isFirstRefresh)
    {
        Logger.LogInformation("Refreshing");

        await DatabaseReadContextProvider.Read<GameStateCache, int>(async cache =>
        {
            CurrentQuestion = cache.GetCurrentQuestion();
            CurrentGame = cache.GetCurrentGame();
            return 0;
        });

        await AfterRefresh(isFirstRefresh);
        
        StateHasChanged();
    }

    private void OnStateChanged()
    {
        if (_disposed)
        {
            return;
        }
        
        Logger.LogInformation("State Changed");
        semaphore.Release();
    }

    public virtual void Dispose()
    {
        _disposed = true;
        cts.Dispose();
        semaphore.Dispose();
        AppSynchronizer.UnsubscribeStateChanged(OnStateChanged);
    }

    protected virtual Task AfterRefresh(bool isFirst)
    {
        return Task.CompletedTask;
    }
}