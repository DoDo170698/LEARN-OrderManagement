using System.Net.Http.Headers;

namespace OrderManagement.Blazor.Services;

public class GraphQLHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GraphQLHttpClientFactory(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("OrderManagementClient");

        var endpoint = _configuration["GraphQL:Endpoint"];
        var token = _configuration["GraphQL:JwtToken"];

        if (!string.IsNullOrEmpty(endpoint))
        {
            client.BaseAddress = new Uri(endpoint);
        }

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }
}
