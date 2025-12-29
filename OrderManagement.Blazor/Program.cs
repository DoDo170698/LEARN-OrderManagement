using OrderManagement.Blazor.Components;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services
    .AddOrderManagementClient()
    .ConfigureHttpClient(async (sp, client) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var tokenService = sp.GetRequiredService<ITokenService>();

        var endpoint = config["GraphQL:Endpoint"]!;
        var token = await tokenService.GetTokenAsync();

        client.BaseAddress = new Uri(endpoint);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    })
    .ConfigureWebSocketClient(async (sp, client) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var tokenService = sp.GetRequiredService<ITokenService>();

        var wsEndpoint = config["GraphQL:WebSocketEndpoint"]!;
        var token = await tokenService.GetTokenAsync();

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
