using GM_Buddy.Contracts.AuthModels.Entities;
using GM_Buddy.Contracts.Interfaces;
using System.Security.Cryptography;

namespace GM_Buddy.Authorization.Services;

// This class defines a background service that periodically rotates cryptographic keys.
public class KeyRotationService : BackgroundService
{
    // Sets how frequently keys should be rotated; here it’s every 7 days.
    private readonly TimeSpan _rotationInterval = TimeSpan.FromDays(7);
    private readonly IAuthRepository _authRepository;
    private readonly PollyRetry _pollyRetry = new();

    // Constructor that accepts a service provider for dependency injection.
    public KeyRotationService(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    // This method is executed when the background service starts.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Loop that runs until the service is stopped.
        while (!stoppingToken.IsCancellationRequested)
        {
            // Perform the key rotation logic.
            await RotateKeysAsync();

            // Wait for the configured rotation interval before running again.
            await Task.Delay(_rotationInterval, stoppingToken);
        }
    }

    // This method handles the actual key rotation logic.
    private async Task RotateKeysAsync()
    {
        // Query the database for the currently active signing key.
        SigningKey? activeKey = await _pollyRetry._retryPolicy.ExecuteAsync(()=> _authRepository.GetActiveSigningKeyAsync());

        // Check if there’s no active key or if the active key is about to expire.
        if (activeKey == null || activeKey.Expires_At <= DateTime.UtcNow.AddDays(10))
        {
            // If there's an active key, mark it as inactive.
            if (activeKey != null)
            {
                await _authRepository.DeactiveSigningKey();
            }

            // Generate a new RSA key pair.
            using RSA rsa = RSA.Create(2048);

            // Export the private key as a Base64-encoded string.
            string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            // Export the public key as a Base64-encoded string.
            string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

            // Generate a unique identifier for the new key.
            string newKeyId = Guid.NewGuid().ToString();

            // Create a new SigningKey entity with the new RSA key details.
            SigningKey newKey = new()
            {
                Key_Id = newKeyId,
                Private_Key = privateKey,
                Public_Key = publicKey,
                Is_Active = true,
                Created_At = DateTime.UtcNow,
                Expires_At = DateTime.UtcNow.AddYears(1) // Set the new key to expire in one year.
            };

            // Insert the new signing key into the database.
            await _authRepository.InsertSigningKey(newKey);
        }
    }
}