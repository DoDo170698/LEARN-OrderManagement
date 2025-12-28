using OrderManagement.Blazor.Components;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAutoMapper(typeof(Program));

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
    })
    .ConfigureWebSocketClient((sp, client) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var wsEndpoint = config["GraphQL:WebSocketEndpoint"]!;
        var token = config["GraphQL:JwtToken"]!;

        client.Uri = new Uri(wsEndpoint);
        client.ConnectionInterceptor = new WebSocketAuthInterceptor(token);
    });

var app = builder.Build();

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
