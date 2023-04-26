using InvoiceGeneratorApi.Auth;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Auth;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace InvoiceGeneratorApi.Services;

public class UserService : IUserService
{
    private readonly InvoiceApiDbContext _context;
    private readonly IMemoryCache _memoryCache;

    public UserService(InvoiceApiDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    public async Task<UserDTO> ChangePassword(string Email, string OldPassword, string NewPassword)
    {
        var user = await FindUser(Email, OldPassword);

        if (user is null)
            return null;

        user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        user.Password = NewPassword;

        Log.Information($"The user {user.Email} changed password -> {OldPassword} with new one -> {NewPassword}.");

        return DtoAndReverseConverter.UserToUserDto(user);
    }

    public async Task<UserDTO> DeleteUser(string Email, string PasswordConfirmation)
    {
        var user = await FindUser(Email, PasswordConfirmation);

        if (user is null)
            return null;

        user = _context.Users.Remove(user).Entity;
        await _context.SaveChangesAsync();
        user.Password = PasswordConfirmation;

        Log.Information($"The user with these credentials is deleted: Email:{user.Email} and Password:{PasswordConfirmation}.");

        _memoryCache.Remove(user.Email);
        return DtoAndReverseConverter.UserToUserDto(user);
    }

    public async Task<UserDTO> EditUser(
        string Email, string? Name,
        string? Address, string? PhoneNumber, string Password)
    {
        var user = await FindUser(Email, Password);

        if (user is null)
            return null;

        user.Name = Name is not null ? Name : user.Name;
        user.Address = Address is not null ? Address : user.Address;
        user.PhoneNumber = PhoneNumber is not null ? PhoneNumber : user.PhoneNumber;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        user = _context.Users.Update(user).Entity;
        await _context.SaveChangesAsync();
        user.Password = Password;

        Log.Information($"The user {user.Email} updated an account.");

        return DtoAndReverseConverter.UserToUserDto(user);
    }

    public async Task<UserDTO> LogInUser(string Email, string Password)
    {
        var user = await FindUser(Email, Password);

        if (user is null)
            return null;
        user.Password = Password;

        Log.Information($"The user {user.Email} logged in.");

        return DtoAndReverseConverter.UserToUserDto(user);
    }

    public async Task<UserDTO> RegisterUser(UserRegisterRequest userRequest)
    {
        var isExistUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userRequest.Email);

        if (isExistUser is not null)
            return null;

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
        user.Password = userRequest.Password;

        Log.Information($"A new user {user.Email} registered.");

        _memoryCache.Set(user.Email, user, TimeSpan.FromMinutes(10));
        return DtoAndReverseConverter.UserToUserDto(user);
    }

    private async Task<User> FindUser(string email, string password)
    {
        if(_memoryCache.TryGetValue(email, out User user))
            return user;

        user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        bool isValidPassword = false;

        if (user.Password == password)
            isValidPassword = true;

        if (user is not null && !isValidPassword)
            isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (user is null || !isValidPassword)
            return null;

        _memoryCache.Set(email, user, TimeSpan.FromMinutes(10));
        return user;
    }
}