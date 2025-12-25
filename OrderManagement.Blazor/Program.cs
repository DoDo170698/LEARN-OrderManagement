using OrderManagement.Blazor.Components;
using OrderManagement.Blazor.GraphQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add StrawberryShake GraphQL Client
builder.Services
    .AddOrderManagementClient()
    .ConfigureHttpClient((sp, client) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var endpoint = config["GraphQL:Endpoint"]!;
        var token = config["GraphQL:JwtToken"]!;

        client.BaseAddress = new Uri(endpoint);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
