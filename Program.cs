using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Poll.Components;
using Poll.DAL;
using Poll.DAL.Services;
using Poll.Services;

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


builder.Services.AddHostedService<DbSeeder>();
builder.Services.AddSingleton<AppSynchronizer>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddTransient<HttpUtils>();
builder.Services.AddTransient<DatabaseWriteContextProvider>();
builder.Services.AddTransient<DatabaseReadContextProvider>();
builder.Services.AddTransient<GameService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddBlazoredLocalStorage();


/* Runtime */

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PollContext>().Database.MigrateAsync();
}

/* ASP.Net Pipeline */

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

app.Run();
