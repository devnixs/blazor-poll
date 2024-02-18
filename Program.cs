using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Poll.Components;
using Poll.DAL;
using Poll.DAL.Services;
using Poll.Events;
using Poll.Services;
using Poll.Services.Abstractions;
using Poll.Services.EventHandlers;
using Poll.Services.PostCommitHandlers;

/* Dependency Injection Registration */

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<PollContext>();

builder.Services.AddDbContext<PollContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(cs, o => { o.MigrationsAssembly("Poll"); });

    options.ReplaceService<IExecutionStrategyFactory, PollExecutionStrategyFactory>();
    options.EnableSensitiveDataLogging();
});

builder.Services.AddLogging();
builder.Services.AddSingleton<AppSynchronizer>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddTransient<HttpUtils>();
builder.Services.AddTransient<DatabaseWriteContextProvider>();
builder.Services.AddTransient<DatabaseReadContextProvider>();
builder.Services.AddTransient<GameService>();
builder.Services.AddSingleton<GameStateCache>();
builder.Services.AddTransient<DomainEvents>();
builder.Services.AddTransient<Initializer>();
builder.Services.AddScoped<TransactionContext>();

builder.Services.AddTransient<IInitializer, DbSeeder>();
builder.Services.AddTransient<IInitializer, GameStateCache>(svc => svc.GetRequiredService<GameStateCache>());


builder.Services.AddTransient<IEventHandler<QuestionValidatedEvent>, OnQuestionValidated>();
builder.Services.AddTransient<OnQuestionValidated>();
builder.Services.AddTransient<IEventHandler<CacheRefreshedEvent>, OnCacheRefreshed>();
builder.Services.AddTransient<OnCacheRefreshed>();
builder.Services.AddTransient<IEventHandler<GameStateChangedEvent>, OnGameStateChanged>();
builder.Services.AddTransient<OnGameStateChanged>();
builder.Services.AddTransient<IEventHandler<QuestionChangedEvent>, OnQuestionChanged>();
builder.Services.AddTransient<OnQuestionChanged>();
builder.Services.AddTransient<IEventHandler<PlayersCountChangedEvent>, PlayersCountChangedEventHandler>();
builder.Services.AddTransient<OnNewAnswer>();
builder.Services.AddTransient<IEventHandler<NewAnswerEvent>, OnNewAnswer>();
builder.Services.AddTransient<OnPlayerNameChanged>();
builder.Services.AddTransient<IEventHandler<PlayerNameChangedEvent>, OnPlayerNameChanged>();

builder.Services.AddHttpContextAccessor();

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
}


app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.GetRequiredService<Initializer>().Initialize();

app.Run();
