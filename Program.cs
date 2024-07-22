using System.Reflection;
using System.Text.Json;
using Blazored.LocalStorage;
using dotenv.net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Poll.Components;
using Poll.Components.Pages.NewGame;
using Poll.DAL;
using Poll.DAL.Services;
using Poll.Middlewares;
using Poll.Services;
using Poll.Services.Abstractions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.SystemConsole.Themes;

Console.WriteLine("Starting app");

DotEnv.Load();

Log.Logger = new LoggerConfiguration()
    .CreateBootstrapLogger();

/* Dependency Injection Registration */

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSerilog((services, lc) =>
{
    var config = lc
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(theme: AnsiConsoleTheme.Code);

    var graylogUrl = builder.Configuration["GRAYLOG_URL"];
    if (!string.IsNullOrEmpty(graylogUrl))
    {
        Console.WriteLine("Sending logs to " + graylogUrl);
        var assemblyName = Assembly.GetEntryAssembly()?.GetName();
        config = config.WriteTo.Graylog(new GraylogSinkOptions
        {
            HostnameOrAddress = graylogUrl,
            Port = 22201,
            Facility = assemblyName?.Name,
        });
    }
});

builder.Services.AddDbContext<PollContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(cs, o => { o.MigrationsAssembly("Poll"); });

    options.ReplaceService<IExecutionStrategyFactory, PollExecutionStrategyFactory>();
    options.EnableSensitiveDataLogging();
});

builder.Services.AddLogging();
builder.Services.AddScoped<FileManager>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<GameCreatorService>();
builder.Services.AddTransient<GameTemplateRepository>();
builder.Services.AddSingleton<GameStateAccessor>();
builder.Services.AddHostedService(i => i.GetRequiredService<GameStateAccessor>());
builder.Services.AddTransient<HttpUtils>();
builder.Services.AddTransient<DatabaseWriteContextProvider>();
builder.Services.AddTransient<DatabaseReadContextProvider>();
builder.Services.AddTransient<GameService>();
builder.Services.AddTransient<Initializer>();
builder.Services.AddScoped<TransactionContext>();
builder.Services.AddScoped<IValidator<NewGameModel>, NewGameValidator>();

builder.Services.AddTransient<IInitializer, DbSeeder>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

builder.Services.AddBlazoredLocalStorage();


/* Runtime */
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PollContext>().Database.MigrateAsync();
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseNoCacheMiddleware();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
    options.EnrichDiagnosticContext = (context, httpContext) =>
    {
        context.Set("HttpRequestClientIP", httpContext.Connection.RemoteIpAddress);
        var xForwardedFor = httpContext.Request.Headers["X-Forwarded-For"];
        context.Set("OriginalIpAddress", string.Join(";", xForwardedFor.Select(i => i ?? "")));
        context.Set("Headers", JsonSerializer.Serialize(httpContext.Request.Headers));
    };
});
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.GetRequiredService<Initializer>().Initialize();

app.Run();