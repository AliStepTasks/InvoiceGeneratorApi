using InvoiceGeneratorApi.Models;
using System.Security.Claims;

namespace InvoiceGeneratorApi.Auth;

public interface IJwtService
{
    string GenerateSecurityToken(int Id, string Email, string Name);
}