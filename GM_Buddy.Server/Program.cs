using GM_Buddy.Business;
using GM_Buddy.Contracts;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpClientDefaults(config =>
{
    config.ConfigurePrimaryHttpMessageHandler(() =>
    {
        HttpClientHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) =>
            {
#if DEBUG
                return true; // DEV
#else
                         if (string.IsNullOrEmpty (config.AppBaseInfo.CertThumbprint)) {
                             return errors == SslPolicyErrors.None;
                         } else {
                             return errors == SslPolicyErrors.None &&
                                 certificate.Thumbprint.Equals (config.AppBaseInfo.CertThumbprint, StringComparison.OrdinalIgnoreCase);
                         }
#endif
            }
        };
        return handler;
    });
});

// Add services to the container.
builder.Services.AddAuthentication(options =>
{
    // This indicates the authentication scheme that will be used by default when the app attempts to authenticate a user.
    // Which authentication handler to use for verifying who the user is by default.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // This indicates the authentication scheme that will be used by default when the app encounters an authentication challenge. 
    // Which authentication handler to use for responding to failed authentication or authorization attempts.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Define token validation parameters to ensure tokens are valid and trustworthy
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Ensure the token was issued by a trusted issuer
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // The expected issuer value from configuration
        ValidateAudience = false, // Disable audience validation (can be enabled as needed)
        ValidateLifetime = true, // Ensure the token has not expired
        ValidateIssuerSigningKey = true, // Ensure the token's signing key is valid
                                         // Define a custom IssuerSigningKeyResolver to dynamically retrieve signing keys from the JWKS endpoint
        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
        {
            //Console.WriteLine($"Received Token: {token}");
            //Console.WriteLine($"Token Issuer: {securityToken.Issuer}");
            //Console.WriteLine($"Key ID: {kid}");
            //Console.WriteLine($"Validate Lifetime: {parameters.ValidateLifetime}");
            // Initialize an HttpClient instance for fetching the JWKS
            HttpClient httpClient = new(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) =>
                {
#if DEBUG
                    return true; // DEV
#else
                    if (string.IsNullOrEmpty(config.AppBaseInfo.CertThumbprint))
                    {
                        return errors == SslPolicyErrors.None;
                    }
                    else
                    {
                        return errors == SslPolicyErrors.None &&
                               certificate.Thumbprint.Equals(config.AppBaseInfo.CertThumbprint, StringComparison.OrdinalIgnoreCase);
                    }
#endif
                }
            });
            // Synchronously fetch the JWKS (JSON Web Key Set) from the specified URL
            string jwks = httpClient.GetStringAsync($"{builder.Configuration["Jwt:Issuer"]}/.well-known/jwks.json").Result;
            // Parse the fetched JWKS into a JsonWebKeySet object
            JsonWebKeySet keys = new(jwks);
            // Return the collection of JsonWebKey objects for token validation
            return keys.Keys;
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("DbSettings"));
builder.Services.AddTransient<IDbConnector, DbConnector>();
builder.Services.AddScoped<INpcLogic, NpcLogic>();
builder.Services.AddSingleton<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthObjectResolver, AuthObjectResolver>();
builder.Services.AddScoped<INpcRepository, NpcRepository>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Server", Version = "v1" });

    // Add Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    // Add Security Requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
     {
       Reference = new OpenApiReference
       {
         Type = ReferenceType.SecurityScheme,
         Id = "Bearer"
       },
       Scheme = "oauth2"
      },
            new List<string>()
        }
    });
});

// CORS Configuration - allow both HTTP and HTTPS for local dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                "https://localhost:49505",  // HTTPS Vite dev server
                "http://localhost:49505"    // HTTP Vite dev server (fallback)
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Allow credentials (cookies, auth headers)
    });
});

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/error-development");
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

// CORS must come BEFORE Authentication/Authorization and MapControllers
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
