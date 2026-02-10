using GM_Buddy.Business;
using GM_Buddy.Contracts;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Data;
using GM_Buddy.Server.Helpers;
using GM_Buddy.Server.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
using System.Net;
using System.Net.Security;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load Cognito settings
var cognitoSettings = builder.Configuration.GetSection("Cognito").Get<CognitoSettings>();

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
                return true; // DEV - Accept all certificates in development
#else
                return errors == SslPolicyErrors.None;
#endif
            }
        };
        return handler;
    });
});

// Add services to the container.
// Configure JWT Bearer authentication with AWS Cognito
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Cognito token validation
    options.Authority = cognitoSettings?.Authority;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = cognitoSettings?.Authority,
        ValidateAudience = false, // Cognito doesn't always set audience in access tokens
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // Map Cognito claims to standard claims
        NameClaimType = "cognito:username",
        RoleClaimType = "cognito:groups"
    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Add Response Compression for better network performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "text/json"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

// Add Memory Cache for caching frequently accessed data
builder.Services.AddMemoryCache();

// Add Output Cache for API response caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("NpcList", builder => builder.Expire(TimeSpan.FromMinutes(2)).Tag("npcs"));
    options.AddPolicy("CampaignList", builder => builder.Expire(TimeSpan.FromMinutes(2)).Tag("campaigns"));
    options.AddPolicy("ShortCache", builder => builder.Expire(TimeSpan.FromSeconds(30)));
});

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("DbSettings"));
builder.Services.AddTransient<IDbConnector, DbConnector>();
builder.Services.AddScoped<INpcLogic, NpcLogic>();
builder.Services.AddScoped<ICampaignLogic, CampaignLogic>();
builder.Services.AddScoped<INpcRepository, NpcRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<IRelationshipRepository, RelationshipRepository>();
builder.Services.AddScoped<IPcRepository, PcRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IGameSystemRepository, GameSystemRepository>();
builder.Services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthHelper, AuthHelper>();
builder.Services.AddScoped<IAuthLogic, AuthLogic>();
builder.Services.AddScoped<IAccountLogic, AccountLogic>();
builder.Services.AddScoped<INewAccountDataSeeder, NewAccountDataSeeder>();

// Register AccountRepository for Cognito user management
builder.Services.AddScoped<IAccountRepository>(sp =>
{
    var dbConnector = sp.GetRequiredService<IDbConnector>();
    return new AccountRepository(dbConnector.ConnectionString);
});

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
                "http://localhost:49505",
                "http://localhost:3000",    // Local dev (Vite or Docker React)
                "https://localhost:3000",
                "https://d2zsk9max2no60.cloudfront.net" // Production CloudFront distribution
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Allow credentials (cookies, auth headers)
    });
});

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error-development");
}
else
{
    app.UseExceptionHandler("/error");
    // Only use HTTPS redirection if not running in Docker (where we use HTTP internally)
    // or if explicitly enabled via configuration
    var runningInDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    var enableHttpsRedirect = app.Configuration.GetValue<bool>("EnableHttpsRedirect", !runningInDocker);
    if (enableHttpsRedirect)
    {
        app.UseHttpsRedirection();
    }
}

// Response Compression - should be early in the pipeline
app.UseResponseCompression();

// Metrics logging middleware - logs timing and parameters for each request
app.UseMiddleware<MetricsLoggingMiddleware>();

// CORS must come BEFORE Authentication/Authorization and MapControllers
app.UseCors("AllowSpecificOrigins");

// Output Cache for API responses
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
