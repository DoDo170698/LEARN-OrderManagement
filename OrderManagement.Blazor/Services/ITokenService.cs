namespace OrderManagement.Blazor.Services;

public interface ITokenService
{
    Task<string> GetTokenAsync();
}
