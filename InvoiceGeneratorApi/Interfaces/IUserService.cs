using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Auth;

namespace InvoiceGeneratorApi.Interfaces;

public interface IUserService
{
    public Task<UserDTO> RegisterUser(UserRegisterRequest userRequest);
    public Task<UserDTO> LogInUser(string email, string password);
    public Task<UserDTO> EditUser(
        string Email, string? Name,
        string? Address, string? PhoneNumber, string Password);
    public Task<UserDTO> ChangePassword(string Email, string OldPassword, string NewPassword);
    public Task<UserDTO> DeleteUser(string Email, string PasswordConfirmation);
}