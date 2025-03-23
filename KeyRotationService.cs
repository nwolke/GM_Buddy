using JWTAuthServer.Data;
using JWTAuthServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace JWTAuthServer.Services
{
    // This class defines a background service that periodically rotates cryptographic keys.
    public class KeyRotationService : BackgroundService
    {
        // Service provider is used to create a scoped service lifetime.
        private readonly IServiceProvider _serviceProvider;

        // Sets how frequently keys should be rotated; here it’s every 7 days.
        private readonly TimeSpan _rotationInterval = TimeSpan.FromDays(7);

        // Constructor that accepts a service provider for dependency injection.
        public KeyRotationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            // Create a new service scope for dependency injection.
            using var scope = _serviceProvider.CreateScope();

            // Retrieve the database context from the service provider.
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Query the database for the currently active signing key.
            var activeKey = await context.SigningKeys.FirstOrDefaultAsync(k => k.IsActive);

            // Check if there’s no active key or if the active key is about to expire.
            if (activeKey == null || activeKey.ExpiresAt <= DateTime.UtcNow.AddDays(10))
            {
                // If there's an active key, mark it as inactive.
                if (activeKey != null)
                {
                    // Mark the current key as inactive since it’s about to be replaced.
                    activeKey.IsActive = false;

                    // Update the current key in the database.
                    context.SigningKeys.Update(activeKey);
                }

                // Generate a new RSA key pair.
                using var rsa = RSA.Create(2048);

                // Export the private key as a Base64-encoded string.
                var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

                // Export the public key as a Base64-encoded string.
                var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

                // Generate a unique identifier for the new key.
                var newKeyId = Guid.NewGuid().ToString();

                // Create a new SigningKey entity with the new RSA key details.
                var newKey = new SigningKey
                {
                    KeyId = newKeyId,
                    PrivateKey = privateKey,
                    PublicKey = publicKey,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddYears(1) // Set the new key to expire in one year.
                };

                // Add the new key to the database.
                await context.SigningKeys.AddAsync(newKey);

                // Save the changes to the database.
                await context.SaveChangesAsync();
            }
        }
    }
}