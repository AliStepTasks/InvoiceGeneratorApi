using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Models;
using InvoiceGeneratorApi.DTO;
using Microsoft.Build.Framework;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using InvoiceGeneratorApi.Auth;
using Serilog;
using System.Security.Claims;

namespace InvoiceGeneratorApi.Controllers
{
    /// <summary>
    /// UsersController is a controller that handles HTTP requests related to users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the UsersController class.
        /// </summary>
        /// <param name="context">The InvoiceApiDbContext used to access the database.</param>
        /// <param name="userService">The IUserService used to perform operations on users.</param>
        public UsersController(
            InvoiceApiDbContext context, IUserService userService,
            IJwtService jwtService)
        {
            _context = context;
            _userService = userService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Change a user's password.
        /// </summary>
        /// <param name="Email">The email of the user whose password is to be changed.</param>
        /// <param name="OldPassword">The old password of the user.</param>
        /// <param name="NewPassword">The new password to set for the user.</param>
        /// <param name="NewPasswordConfirmation">Confirmation of the new password.</param>
        /// <returns>The updated user information as a UserDTO.</returns>
        // PUT: api/Users/Email
        [HttpPut("Email, OldPassword, NewPassword, NewPasswordConfirmation")]
        public async Task<ActionResult<UserDTO>> ChangePassword(
            string Email, string OldPassword,
            string NewPassword, string NewPasswordConfirmation)
        {
            if (_context.Users is null)
            {
                Log.Error("There is no any user in database.");
                return Problem("There is no any user in database.");
            }

            if (NewPassword != NewPasswordConfirmation)
            {
                return Problem("Passwords don't match.");
            }

            var user = await _userService.ChangePassword(Email, OldPassword, NewPassword);

            return user is not null
                ? user
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Edit a user's information.
        /// </summary>
        /// <param name="Email">The email of the user whose information is to be edited.</param>
        /// <param name="Name">The name to set for the user.</param>
        /// <param name="Address">The address to set for the user.</param>
        /// <param name="PhoneNumber">The phone number to set for the user.</param>
        /// <param name="Password">The user's current password.</param>
        /// <param name="PasswordConfirmation">Confirmation of the user's current password.</param>
        /// <returns>The updated user information as a UserDTO.</returns>
        // PUT: api/Users/Email
        [HttpPut("Email, Name, Address, PhoneNumber, Password, PasswordConfirmation")]
        public async Task<ActionResult<UserDTO>> EditUser(
            string Email, string? Name,
            string? Address, string? PhoneNumber,
            string Password, string PasswordConfirmation)
        {
            if (_context.Users is null)
            {
                Log.Error("There is no any user in database.");
                return Problem("There is no any user in database.");
            }

            if (Email is null || Password is null || PasswordConfirmation is null || Password != PasswordConfirmation)
            {
                return BadRequest();
            }

            var user = await _userService.EditUser(Email, Name, Address, PhoneNumber, Password);

            return user is not null
                ? user
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Registers a new user with the specified details and returns a security token if successful.
        /// </summary>
        /// <param name="userRequest">The details of the user to register.</param>
        /// <returns>A security token if registration is successful, or a bad request status code otherwise.</returns>
        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<string>> RegisterUser([FromBody] UserRegisterRequest userRequest)
        {
            if (_context.Users is null)
            {
                Log.Error("There is no any user in database.");
                return Problem("There is no any user in database.");
            }

            if (userRequest is null)
            {
                return BadRequest();
            }

            var user = await _userService.RegisterUser(userRequest);
            if(user is null)
            {
                return Problem("Something went wrong");
            }

            var token = _jwtService.GenerateSecurityToken(user.Id, user.Email, user.Name);
            if(token is not null)
                Log.Information($"The access token -> {token} is generated for user {user.Email}");

            return token;
        }

        /// <summary>
        /// Logs in a user with the specified email and password.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The security token for the logged in user.</returns>
        /// <response code="200">Returns the security token for the logged in user.</response>
        /// <response code="400">If either the email or password is null.</response>
        /// <response code="500">If an error occurs while logging in the user.</response>
        [HttpGet("LogIn")]
        public async Task<ActionResult<string>> LogInUser(string email, string password)
        {
            if (_context.Users is null)
            {
                Log.Error("There is no any user in database.");
                return Problem("There is no any user in database.");
            }

            var user = await _userService.LogInUser(email, password);

            if (user is null)
            {
                Log.Error("There is no such a user in database.");
                return Problem("There is no such a user in database.");
            }

            var token = _jwtService.GenerateSecurityToken(user.Id, user.Email, user.Name);
            if (token is not null)
                Log.Information($"The access token -> {token} is generated for user {user.Email}");

            return token;
        }

        /// <summary>
        /// Delete a user from the database.
        /// </summary>
        /// <param name="Email">The email of the user to delete.</param>
        /// <param name="PasswordConfirmation">Confirmation of the user's password.</param>
        /// <returns>The deleted user information as a UserDTO.</returns>
        // DELETE: api/Users/Email
        [HttpDelete("Email, PasswordConfirmation")]
        public async Task<ActionResult<UserDTO>> DeleteUser(string Email, string PasswordConfirmation)
        {
            if (_context.Users is null)
            {
                Log.Error("There is no any user in database.");
                return Problem("There is no any user in database.");
            }

            var user = await _userService.DeleteUser(Email, PasswordConfirmation);

            return user is not null
                ? user
                : Problem("Something went wrong");
        }
    }
}