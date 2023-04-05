using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Models;
using InvoiceGeneratorApi.Services;
using InvoiceGeneratorApi.DTO;

namespace InvoiceGeneratorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;
        private readonly IUserService _userService; 

        public UsersController(InvoiceApiDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <summary>
        /// Changes user's password with confirmation.
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="OldPassword"></param>
        /// <param name="NewPassword"></param>
        /// <param name="NewPasswordConfirmation"></param>
        /// <returns></returns>
        // PUT: api/Users/Email
        [HttpPut("Email, OldPassword, NewPassword, NewPasswordConfirmation")]
        public async Task<ActionResult<UserDTO>> ChangePassword(string Email, string OldPassword, string NewPassword, string NewPasswordConfirmation)
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
        /// Edit user's data with password confirmation
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Name"></param>
        /// <param name="Address"></param>
        /// <param name="PhoneNumber"></param>
        /// <param name="Password"></param>
        /// <param name="PasswordConfirmation"></param>
        /// <returns></returns>
        // PUT: api/Users/Email
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

            var user = await _userService.EditUser(Email, Name, Address, PhoneNumber);

            return user is not null
                ? user
                : Problem("Something went wrong");
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
        /// Delete user according to Email and Password
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="PasswordConfirmation"></param>
        /// <returns></returns>
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