using System.Net.Http.Headers;

namespace OrderManagement.Blazor.Handlers;

public class AuthorizationHeaderHandler : DelegatingHandler
{
    private readonly IConfiguration _configuration;

    public AuthorizationHeaderHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _configuration["GraphQL:JwtToken"];
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
