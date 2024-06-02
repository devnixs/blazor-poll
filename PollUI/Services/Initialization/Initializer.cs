namespace Poll.Services.Abstractions;

public class Initializer
{
    private readonly IEnumerable<IInitializer> _initializers;
    private readonly ILogger<Initializer> _logger;

    public Initializer(IEnumerable<IInitializer> initializers, ILogger<Initializer> logger)
    {
        _initializers = initializers;
        _logger = logger;
    }

    public async Task Initialize()
    {
        foreach (var initializer in _initializers)
        {
            _logger.LogInformation("Initializing {}", initializer.GetType());
            await initializer.OnInitialize();
        }
    }
}