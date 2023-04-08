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

        /// <summary>
        /// Initializes a new instance of the UsersController class.
        /// </summary>
        /// <param name="context">The InvoiceApiDbContext used to access the database.</param>
        /// <param name="userService">The IUserService used to perform operations on users.</param>
        public UsersController(InvoiceApiDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
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
            if (_context.Users == null)
            {
                return NotFound();
            }
            if(NewPassword != NewPasswordConfirmation)
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
        /// Add a new user to the database.
        /// </summary>
        /// <param name="user">The user to add to the database.</param>
        /// <returns>The created user.</returns>
        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
          if (_context.Users == null)
          {
              return Problem("Entity set 'InvoiceApiDbContext.Users'  is null.");
          }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
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
            if (_context.Users == null)
            {
                return NotFound();
            }

            var user = await _userService.DeleteUser(Email, PasswordConfirmation);

            return user is not null
                ? user
                : Problem("Something went wrong");
        }
    }
}