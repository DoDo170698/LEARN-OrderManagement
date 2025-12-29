using System.Text.Json;

namespace OrderManagement.Blazor.Services;

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private string? _cachedToken;

    public TokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken))
            return _cachedToken;

        var tokenEndpoint = _configuration["GraphQL:TokenEndpoint"]!;
        var response = await _httpClient.PostAsync(tokenEndpoint, null);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        _cachedToken = tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to get token");
        return _cachedToken;
    }

    private record TokenResponse(string AccessToken, int ExpiresIn);
}
