using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using LabUI.Blazor2.Components;
using LabUI.Blazor2.Services;
using LabUI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Настройка HttpClient
builder.Services.AddHttpClient<LabUI.Blazor2.Services.IProductService<LabUI.Models.Dish>,
    LabUI.Blazor2.Services.ApiProductService>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7002/");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();