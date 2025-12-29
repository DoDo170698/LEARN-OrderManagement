using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace OrderManagement.Application.UseCases.Auth.Commands;

public record GenerateTokenCommand(string SecretKey, string Issuer, string Audience) : IRequest<GenerateTokenResponse>;

public record GenerateTokenResponse(string AccessToken, int ExpiresIn);

public class GenerateTokenCommandHandler : IRequestHandler<GenerateTokenCommand, GenerateTokenResponse>
{
    public Task<GenerateTokenResponse> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "mock-admin-user"),
            new Claim(ClaimTypes.Name, "Mock Admin User"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var tokenHandler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddYears(1),
            Issuer = request.Issuer,
            Audience = request.Audience,
            SigningCredentials = credentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Task.FromResult(new GenerateTokenResponse(token, 31536000));
    }
}
