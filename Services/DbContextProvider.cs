using Poll.DAL;

namespace Poll.Services;

public class DbContextProvider
{
    private readonly IServiceProvider _serviceProvider;

    public DbContextProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDisposable ProvidePollContext(out PollContext ctx)
    {
        var scope = _serviceProvider.CreateScope();
        ctx = scope.ServiceProvider.GetRequiredService<PollContext>();
        return scope;
    }
}
