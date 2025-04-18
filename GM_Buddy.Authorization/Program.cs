using GM_Buddy.Authorization.Services;
using GM_Buddy.Business;
using GM_Buddy.Contracts;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Configure Authentication using JWT Bearer tokens
//builder.Services.AddAuthentication(options =>
//{
//    // This indicates the authentication scheme that will be used by default when the app attempts to authenticate a user.
//    // Which authentication handler to use for verifying who the user is by default.
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    // This indicates the authentication scheme that will be used by default when the app encounters an authentication challenge. 
//    // Which authentication handler to use for responding to failed authentication or authorization attempts.
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    // Define token validation parameters to ensure tokens are valid and trustworthy
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true, // Ensure the token was issued by a trusted issuer
//        ValidIssuer = builder.Configuration["Jwt:Issuer"], // The expected issuer value from configuration
//        ValidateAudience = false, // Disable audience validation (can be enabled as needed)
//        ValidateLifetime = true, // Ensure the token has not expired
//        ValidateIssuerSigningKey = true, // Ensure the token's signing key is valid
//                                         // Define a custom IssuerSigningKeyResolver to dynamically retrieve signing keys from the JWKS endpoint
//        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
//        {
//            //Console.WriteLine($"Received Token: {token}");
//            //Console.WriteLine($"Token Issuer: {securityToken.Issuer}");
//            //Console.WriteLine($"Key ID: {kid}");
//            //Console.WriteLine($"Validate Lifetime: {parameters.ValidateLifetime}");
//            // Initialize an HttpClient instance for fetching the JWKS
//            var httpClient = new HttpClient();
//            // Synchronously fetch the JWKS (JSON Web Key Set) from the specified URL
//            var jwks = httpClient.GetStringAsync($"{builder.Configuration["Jwt:Issuer"]}/.well-known/jwks.json").Result;
//            // Parse the fetched JWKS into a JsonWebKeySet object
//            var keys = new JsonWebKeySet(jwks);
//            // Return the collection of JsonWebKey objects for token validation
//            return keys.Keys;
//        }
//    };
//});
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Preserve property names as defined in the C# models (disable camelCase naming)
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("DbSettings"));
builder.Services.AddTransient<IDbConnector, DbConnector>();
builder.Services.AddScoped<INpcLogic, NpcLogic>();
builder.Services.AddSingleton<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthObjectResolver, AuthObjectResolver>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigins", builder =>
//    {
//        _ = builder.WithOrigins("https://localhost:49505") // Add your React app's URL
//        .AllowAnyHeader()
//        .AllowAnyMethod();
//    });
//});
builder.Services.AddHostedService<KeyRotationService>();

WebApplication app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
    _ = app.UseExceptionHandler("/error-development");
}
else
{
    _ = app.UseExceptionHandler("/error");
}
app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");
app.MapControllers();

app.Run();
