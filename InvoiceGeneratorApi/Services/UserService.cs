using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceGeneratorApi.Services;

public class UserService : IUserService
{
    private InvoiceApiDbContext _context;

    public UserService(InvoiceApiDbContext context)
    {
        _context = context;
    }

    public async Task<UserDTO> ChangePassword(string Email, string OldPassword, string NewPassword)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == Email && u.Password == OldPassword);
        if (user == null)
        {
            return null;
        }

        user.Password = NewPassword;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return UserToUserDto(user);
    }

    public async Task<UserDTO> DeleteUser(string Email, string PasswordConfirmation)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == Email && u.Password == PasswordConfirmation);
        if (user == null)
        {
            return null;
        }

        user = _context.Users.Remove(user).Entity;
        await _context.SaveChangesAsync();

        return UserToUserDto(user);
    }

    public async Task<UserDTO> EditUser(
        string Email, string? Name,
        string? Address, string? PhoneNumber)
    {
        var user = await _context.Users.FirstOrDefaultAsync(c => c.Email == Email);
        if (user is null)
        {
            return null;
        }

        user.Name = Name is not null ? Name : user.Name;
        user.Address = Address is not null ? Address : user.Address;
        user.PhoneNumber = PhoneNumber is not null ? PhoneNumber : user.PhoneNumber;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        user = _context.Users.Update(user).Entity;
        await _context.SaveChangesAsync();

        return UserToUserDto(user);
    }

    public async Task<UserDTO> LogInUser(UserDTO userDTO)
    {
        throw new NotImplementedException();
    }

    public async Task<UserDTO> RegisterUser(UserDTO userDTO)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts User to UserDTO
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private UserDTO UserToUserDto (User user)
    {
        var userDTO = new UserDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            Password = user.Password,
            PhoneNumber = user.PhoneNumber
        };

        return userDTO;
    }

    /// <summary>
    /// Converts UserDTO to User
    /// </summary>
    /// <param name="userDTO"></param>
    /// <returns></returns>
    private User UserDtoToUser(UserDTO userDTO)
    {
        var user = new User
        {
            Id = userDTO.Id,
            Name = userDTO.Name,
            Email = userDTO.Email,
            Address = userDTO.Address,
            Password = userDTO.Password,
            PhoneNumber = userDTO.PhoneNumber
        };

        return user;
    }
}