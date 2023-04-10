using InvoiceGeneratorApi.Auth;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Auth;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceGeneratorApi.Services;

public class UserService : IUserService
{
    private readonly InvoiceApiDbContext _context;

    public UserService(InvoiceApiDbContext context)
    {
        _context = context;
    }

    public async Task<UserDTO> ChangePassword(string Email, string OldPassword, string NewPassword)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == Email);
        var isValidPassword = BCrypt.Net.BCrypt.Verify(OldPassword, user.Password);

        if (isValidPassword)
        {
            return null;
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.UserToUserDto(user);
    }

    public async Task<UserDTO> DeleteUser(string Email, string PasswordConfirmation)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == Email);
        var isValidPassword = BCrypt.Net.BCrypt.Verify(PasswordConfirmation, user.Password);

        if (isValidPassword)
        {
            return null;
        }

        user = _context.Users.Remove(user).Entity;
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.UserToUserDto(user);
    }

    public async Task<UserDTO> EditUser(
        string Email, string? Name,
        string? Address, string? PhoneNumber, string Password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(c => c.Email == Email);
        var isValidPassword = BCrypt.Net.BCrypt.Verify(Password, user.Password);

        if (isValidPassword)
        {
            return null;
        }

        user.Name = Name is not null ? Name : user.Name;
        user.Address = Address is not null ? Address : user.Address;
        user.PhoneNumber = PhoneNumber is not null ? PhoneNumber : user.PhoneNumber;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        user = _context.Users.Update(user).Entity;
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.UserToUserDto(user);
    }

    public async Task<UserDTO> LogInUser(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        var isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (user is null || !isValidPassword)
        {
            return null;
        }

        return DtoAndReverseConverter.UserToUserDto(user);
    }

    public async Task<UserDTO> RegisterUser(UserRegisterRequest userRequest)
    {
        var isExistUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userRequest.Email);

        if (isExistUser is not null)
        {
            return null;
        }

        var user = new User
        {
            Name = userRequest.Name,
            Email = userRequest.Email,
            Address = userRequest.Address,
            PhoneNumber = userRequest.PhoneNumber,
            Password = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = DateTimeOffset.Now
        };

        user = _context.Users.Add(user).Entity;
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.UserToUserDto(user);
    }
}