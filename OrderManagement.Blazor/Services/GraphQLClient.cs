using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderManagement.Blazor.Services;

/// <summary>
/// Strongly-typed GraphQL client for Order Management API
/// </summary>
public class GraphQLClient
{
    private readonly HttpClient _httpClient;
    private readonly string _jwtToken;

    public GraphQLClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _jwtToken = configuration["GraphQL:JwtToken"]!;

        // Set authorization header with static JWT token
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _jwtToken);
    }

    public async Task<GraphQLResponse<T>> ExecuteQueryAsync<T>(string query, object? variables = null)
    {
        var request = new GraphQLRequest
        {
            Query = query,
            Variables = variables
        };

        var response = await _httpClient.PostAsJsonAsync("", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<T>>();
        return result ?? throw new Exception("Failed to deserialize response");
    }

    public async Task<GraphQLResponse<T>> ExecuteMutationAsync<T>(string mutation, object? variables = null)
    {
        return await ExecuteQueryAsync<T>(mutation, variables);
    }
}

public class GraphQLRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("variables")]
    public object? Variables { get; set; }
}

public class GraphQLResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<GraphQLError>? Errors { get; set; }
}

public class GraphQLError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public List<string>? Path { get; set; }
}
