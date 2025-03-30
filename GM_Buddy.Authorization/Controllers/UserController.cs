using GM_Buddy.Business;
using GM_Buddy.Contracts.AuthModels.DbModels;
using GM_Buddy.Contracts.AuthModels.DTOs;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Authorization.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly IAuthObjectResolver _authObjectResolver;

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
        User? existingUser = await _authRepository.GetUserByEmail(registerDto.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "Email is already registered." });
        }
        // Hash the password using BCrypt.
        string salt = PasswordHasher.GenerateSalt();
        string hashedPassword = PasswordHasher.HashPassword(registerDto.Password, salt);
        // Create a new user entity.
        User newUser = new()
        {
            First_Name = registerDto.Firstname,
            Last_Name = registerDto.Lastname,
            Email = registerDto.Email,
            Password = hashedPassword,
            Salt = salt
        };
        // Add the new user to the database.
        int newUserId = await _authRepository.InsertNewUser(newUser);
        // Optionally, assign a default role to the new user.
        // For example, assign the "User" role.
        Role? userRole = await _authRepository.GetRole("User");
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
        System.Security.Claims.Claim? emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
        if (emailClaim == null)
        {
            return Unauthorized(new { message = "Invalid token: Email claim missing." });
        }
        string userEmail = emailClaim.Value;
        // Retrieve the user from the database, including roles.
        ProfileDTO? profile = await _authObjectResolver.GetUserProfile(userEmail);
        return profile == null ? NotFound(new { message = "User not found." }) : Ok(profile);
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
        System.Security.Claims.Claim? emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
        if (emailClaim == null)
        {
            return Unauthorized(new { message = "Invalid token: Email claim missing." });
        }
        string userEmail = emailClaim.Value;
        // Retrieve the user from the database.
        User? user = await _authRepository.GetUserByEmail(userEmail);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }
        // Update fields if provided.
        if (!string.IsNullOrEmpty(updateDto.Firstname))
        {
            user.First_Name = updateDto.Firstname;
        }
        if (!string.IsNullOrEmpty(updateDto.Lastname))
        {
            user.Last_Name = updateDto.Lastname;
        }
        if (!string.IsNullOrEmpty(updateDto.Email))
        {
            // Check if the new email is already taken by another user.
            bool emailExists = (await _authRepository.GetUserByEmail(updateDto.Email))?.Id != user.Id;
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

