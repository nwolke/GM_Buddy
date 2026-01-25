namespace GM_Buddy.Contracts;

/// <summary>
/// Configuration settings for AWS Cognito authentication.
/// </summary>
public class CognitoSettings
{
    /// <summary>
    /// The AWS region where the Cognito User Pool is located (e.g., "us-east-1")
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// The Cognito User Pool ID (e.g., "us-east-1_xxxxxxxxx")
    /// </summary>
    public string UserPoolId { get; set; } = string.Empty;

    /// <summary>
    /// The Cognito App Client ID for this application
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The Cognito App Client Secret (optional, depends on app client configuration)
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// The Cognito domain for hosted UI (optional, for OAuth flows)
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets the authority URL for JWT validation
    /// </summary>
    public string Authority => $"https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}";

    /// <summary>
    /// Gets the JWKS URL for token validation
    /// </summary>
    public string JwksUrl => $"{Authority}/.well-known/jwks.json";

    /// <summary>
    /// Gets the token endpoint URL
    /// </summary>
    public string TokenEndpoint => $"https://{Domain}.auth.{Region}.amazoncognito.com/oauth2/token";
}
