using GM_Buddy.Business;
using GM_Buddy.Contracts.AuthModels.DbModels;
using GM_Buddy.Contracts.AuthModels.DTOs;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;
public class UserController : Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IAuthRepository _authRepository;
        private IAuthObjectResolver _authObjectResolver;

        // Constructor injecting the ApplicationDbContext
        public UsersController(IAuthRepository authRepository, IAuthObjectResolver authObjectResolver)
        {
            _authRepository = authRepository;
            _authObjectResolver = authObjectResolver;
        }
        // Registers a new user.
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            // Validate the incoming model.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if the email already exists.
            var existingUser = await _authRepository.GetUserByEmail(registerDto.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "Email is already registered." });
            }
            // Hash the password using BCrypt.
            var salt = PasswordHasher.GenerateSalt();
            var hashedPassword = PasswordHasher.HashPassword(registerDto.Password, salt);
            // Create a new user entity.
            var newUser = new User
            {
                FirstName = registerDto.Firstname,
                LastName = registerDto.Lastname,
                Email = registerDto.Email,
                Password = hashedPassword,
                Salt = salt
            };
            // Add the new user to the database.
            var newUserId = await _authRepository.InsertNewUser(newUser);
            // Optionally, assign a default role to the new user.
            // For example, assign the "User" role.
            var userRole = await _authRepository.GetRole("User");
            if (userRole != null)
            {
                await _authRepository.InsertUserRole(newUserId, userRole.Id);
            }
            return CreatedAtAction(nameof(GetProfile), new { id = newUserId }, new { message = "User registered successfully." });
        }

        // Retrieves the authenticated user's profile.
        [HttpGet("GetProfile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // Extract the user's email from the JWT token claims.
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: Email claim missing." });
            }
            string userEmail = emailClaim.Value;
            // Retrieve the user from the database, including roles.
            var profile = await _authObjectResolver.GetUserProfile(userEmail);
            if (profile == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(profile);
        }
        // Updates the authenticated user's profile.
        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO updateDto)
        {
            // Validate the incoming model.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Extract the user's email from the JWT token claims.
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: Email claim missing." });
            }
            string userEmail = emailClaim.Value;
            // Retrieve the user from the database.
            var user = await _authRepository.GetUserByEmail(userEmail);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            // Update fields if provided.
            if (!string.IsNullOrEmpty(updateDto.Firstname))
            {
                user.FirstName = updateDto.Firstname;
            }
            if (!string.IsNullOrEmpty(updateDto.Lastname))
            {
                user.LastName = updateDto.Lastname;
            }
            if (!string.IsNullOrEmpty(updateDto.Email))
            {
                // Check if the new email is already taken by another user.
                var emailExists = (await _authRepository.GetUserByEmail(updateDto.Email))?.Id != user.Id;
                if (emailExists)
                {
                    return Conflict(new { message = "Email is already in use by another account." });
                }
                user.Email = updateDto.Email;
            }
            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                // Hash the new password before storing.
                string hashedPassword = PasswordHasher.HashPassword(updateDto.Password, user.Salt);
                user.Password = hashedPassword;
            }
            // Save the changes to the database.
            await _authRepository.UpdateUser(user);
            return Ok(new { message = "Profile updated successfully." });
        }
    }
}
