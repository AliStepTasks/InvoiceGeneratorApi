using System.Security.Claims;

namespace InvoiceGeneratorApi.Auth;

public interface IJwtService
{
    string GenerateSecurityToken(string email);
}