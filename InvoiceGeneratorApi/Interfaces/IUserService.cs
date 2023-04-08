using InvoiceGeneratorApi.DTO;

namespace InvoiceGeneratorApi.Interfaces;

public interface IUserService
{
    public Task<UserDTO> RegisterUser(UserDTO userDTO);
    public Task<UserDTO> LogInUser(UserDTO userDTO);
    public Task<UserDTO> EditUser(
        string Email, string? Name,
        string? Address, string? PhoneNumber, string Password);
    public Task<UserDTO> ChangePassword(string Email, string OldPassword, string NewPassword);
    public Task<UserDTO> DeleteUser(string Email, string PasswordConfirmation);
}