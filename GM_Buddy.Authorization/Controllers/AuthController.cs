﻿using GM_Buddy.Business;
using GM_Buddy.Contracts.AuthModels.Entities;
using GM_Buddy.Contracts.AuthModels.Requests;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace GM_Buddy.Authorization.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    // Private fields to hold the configuration and database context
    // Holds configuration settings from appsettings.json or environment variables
    private readonly IConfiguration _configuration;

    // Database context for interacting with the database
    private readonly IAuthRepository _authRepository;

    // Constructor that injects IConfiguration and ApplicationDbContext via dependency injection
    public AuthController(IConfiguration configuration, IAuthRepository authRepository)
    {
        // Assign the injected IConfiguration to the private field
        _configuration = configuration;

        _authRepository = authRepository;
    }

    // Define the Login endpoint that responds to POST requests at 'api/Auth/Login'
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        // Validate the incoming model based on data annotations in LoginDTO
        if (!ModelState.IsValid)
        {
            // If the model is invalid, return a 400 Bad Request with validation errors
            return BadRequest(ModelState);
        }

        // Query the Clients table to verify if the provided ClientId exists
        Client? client = await _authRepository.GetClient(loginRequest.ClientId);
        // If the client does not exist, return a 401 Unauthorized response
        if (client == null)
        {
            return Unauthorized("Invalid client credentials.");
        }

        // Retrieve the user from the Users table by matching the email (case-insensitive)
        // Also include the UserRoles and associated Roles for later use
        User? user = await _authRepository.GetUserByEmail(loginRequest.Email);

        // If the user does not exist, return a 401 Unauthorized response
        if (user == null)
        {
            // For security reasons, avoid specifying whether the client or user was invalid
            return Unauthorized("Invalid credentials.");
        }

        // Verify the provided password against the stored hashed password using BCrypt
        bool isPasswordValid = PasswordHasher.VerifyPassword(loginRequest.Password, user.Password);

        // If the password is invalid, return a 401 Unauthorized response
        if (!isPasswordValid)
        {
            // Again, avoid specifying whether the client or user was invalid
            return Unauthorized("Invalid credentials.");
        }

        // At this point, authentication is successful. Proceed to generate a JWT token.
        string token = await GenerateJwtToken(user, client);

        // Return the generated token in a 200 OK response
        return Ok(new { Token = token });
    }

    // Private method responsible for generating a JWT token for an authenticated user
    private async Task<string> GenerateJwtToken(User user, Client client)
    {
        // Retrieve the active signing key from the SigningKeys table
        SigningKey? signingKey = await _authRepository.GetActiveSigningKeyAsync();

        // If no active signing key is found, throw an exception
        if (signingKey == null)
        {
            throw new Exception("No active signing key available.");
        }

        // Convert the Base64-encoded private key string back to a byte array
        byte[] privateKeyBytes = Convert.FromBase64String(signingKey.Private_Key);

        // Create a new RSA instance for cryptographic operations
        RSA rsa = RSA.Create();

        // Import the RSA private key into the RSA instance
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

        // Create a new RsaSecurityKey using the RSA instance
        RsaSecurityKey rsaSecurityKey = new(rsa)
        {
            // Assign the Key ID to link the JWT with the correct public key
            KeyId = signingKey.Key_Id
        };

        // Define the signing credentials using the RSA security key and specifying the algorithm
        SigningCredentials creds = new(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        // Initialize a list of claims to include in the JWT
        List<Claim> claims =
        [
                // Subject (sub) claim with the user's ID
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

                // JWT ID (jti) claim with a unique identifier for the token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                // Name claim with the user's first name
                new Claim(ClaimTypes.Name, user.First_Name),

                // NameIdentifier claim with the user's email
                new Claim(ClaimTypes.NameIdentifier, user.Email),

                // Email claim with the user's email
                new Claim(ClaimTypes.Email, user.Email)
            ];

        IEnumerable<Role> roles = await _authRepository.GetAllUserRoles(user.Id);

        // Iterate through the user's roles and add each as a Role claim
        foreach (Role userRole in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Name));
        }

        // Define the JWT token's properties, including issuer, audience, claims, expiration, and signing credentials
        JwtSecurityToken tokenDescriptor = new(
            issuer: _configuration["Jwt:Issuer"], // The token issuer, typically your application's URL
            audience: client.Client_URL, // The intended recipient of the token, typically the client's URL
            claims: claims, // The list of claims to include in the token
            expires: DateTime.UtcNow.AddHours(1), // Token expiration time set to 1 hour from now
            signingCredentials: creds // The credentials used to sign the token
        );

        // Create a JWT token handler to serialize the token
        JwtSecurityTokenHandler tokenHandler = new();

        // Serialize the token to a string
        string token = tokenHandler.WriteToken(tokenDescriptor);

        // Return the serialized JWT token
        return token;
    }
}
