using Blazored.LocalStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Poll.Components;
using Poll.DAL;
using Poll.DAL.Services;
using Poll.Services;
using Poll.Services.Abstractions;

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
builder.Services.AddScoped<PlayerService>();
builder.Services.AddSingleton<GameStateAccessor>();
builder.Services.AddHostedService(i => i.GetRequiredService<GameStateAccessor>());
builder.Services.AddTransient<HttpUtils>();
builder.Services.AddTransient<DatabaseWriteContextProvider>();
builder.Services.AddTransient<DatabaseReadContextProvider>();
builder.Services.AddTransient<GameService>();
builder.Services.AddTransient<Initializer>();
builder.Services.AddScoped<TransactionContext>();

builder.Services.AddTransient<IInitializer, DbSeeder>();

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
