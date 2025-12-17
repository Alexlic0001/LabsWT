using LabUI.Blazor;
using LabUI.Blazor.Components;
using BootstrapBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddScoped<IProductService<LabUI.Models.Dish>, ApiProductService>();

builder.Services.AddBootstrapBlazor();


builder.Services.AddHttpClient<IProductService<LabUI.Models.Dish>,
    ApiProductService>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7002/");
    });

var app = builder.Build();


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