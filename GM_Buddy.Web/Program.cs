using GM_Buddy.Contracts;
using GM_Buddy.Web.Components;
using GM_Buddy.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure API settings
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();
if (string.IsNullOrEmpty(apiSettings.BaseUrl))
{
    apiSettings.BaseUrl = "http://gm_buddy_server:8080";
}
Console.WriteLine($"API BaseUrl configured: {apiSettings.BaseUrl}");

// Load Cognito settings
var cognitoSettings = builder.Configuration.GetSection("Cognito").Get<CognitoSettings>() ?? new CognitoSettings();
var isCognitoConfigured = !string.IsNullOrEmpty(cognitoSettings.ClientId) 
    && !string.IsNullOrEmpty(cognitoSettings.UserPoolId);

if (isCognitoConfigured)
{
    // Configure Cookie + OpenID Connect authentication with Cognito
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "GMBuddy.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(5);
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = cognitoSettings.Authority;
        options.ClientId = cognitoSettings.ClientId;
        options.ClientSecret = cognitoSettings.ClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        
        options.MetadataAddress = $"{cognitoSettings.Authority}/.well-known/openid-configuration";
        
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "cognito:username");
        
        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    // Map Cognito groups to roles
                    var groupsClaims = identity.FindAll("cognito:groups").ToList();
                    foreach (var group in groupsClaims)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, group.Value));
                    }
                    
                    // Sync account with backend database
                    var cognitoSub = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var email = identity.FindFirst(ClaimTypes.Email)?.Value;
                    var displayName = identity.FindFirst(ClaimTypes.Name)?.Value;
                    
                    if (!string.IsNullOrEmpty(cognitoSub))
                    {
                        try
                        {
                            var httpClient = new HttpClient { BaseAddress = new Uri(apiSettings.BaseUrl) };
                            var request = new { CognitoSub = cognitoSub, Email = email, DisplayName = displayName };
                            await httpClient.PostAsJsonAsync("/Account/sync", request);
                        }
                        catch (Exception ex)
                        {
                            // Log but don't fail authentication
                            Console.WriteLine($"Warning: Failed to sync account: {ex.Message}");
                        }
                    }
                }
            },
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                var logoutUri = $"https://{cognitoSettings.Domain}.auth.{cognitoSettings.Region}.amazoncognito.com/logout";
                logoutUri += $"?client_id={cognitoSettings.ClientId}";
                logoutUri += $"&logout_uri={Uri.EscapeDataString(context.Request.Scheme + "://" + context.Request.Host + "/")}";
                
                context.Response.Redirect(logoutUri);
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    // Development mode without Cognito - use cookie auth only
    Console.WriteLine("WARNING: Cognito not configured. Authentication is disabled.");
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "GMBuddy.Auth";
            options.LoginPath = "/login";
        });
}

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Register HttpClient for main API with token forwarding
// Note: AddHttpClient<T> also registers T as a service, so don't call AddScoped<ApiService>
builder.Services.AddHttpClient<ApiService>(client =>
{
    if (string.IsNullOrEmpty(apiSettings.BaseUrl))
    {
        throw new InvalidOperationException("ApiSettings:BaseUrl is not configured");
    }
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
    Console.WriteLine($"ApiService HttpClient configured with BaseAddress: {client.BaseAddress}");
});

// Register other services (ApiService is already registered by AddHttpClient above)
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<NpcService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Login/Logout endpoints
if (isCognitoConfigured)
{
    app.MapGet("/login", async (HttpContext context, string? returnUrl) =>
    {
        await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        });
    });

    app.MapGet("/logout", async (HttpContext context) =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    });
}
else
{
    // Dev mode - sign in with a fake dev user for testing [Authorize] pages
    // Note: Uses 'dev-user-sub' as Cognito sub - will auto-create account in database
    app.MapGet("/login", async (HttpContext context, string? returnUrl) =>
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "dev-user-sub"), // Simulates Cognito sub
            new(ClaimTypes.Name, "Dev User"),
            new(ClaimTypes.Email, "dev@localhost"),
            new(ClaimTypes.Role, "Admin") // Give dev user admin access
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        // Sync dev account with backend database
        try
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(apiSettings.BaseUrl) };
            var request = new { CognitoSub = "dev-user-sub", Email = "dev@localhost", DisplayName = "Dev User" };
            await httpClient.PostAsJsonAsync("/Account/sync", request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to sync dev account: {ex.Message}");
        }
        
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        context.Response.Redirect(returnUrl ?? "/");
    });
    
    app.MapGet("/logout", async (HttpContext context) =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Response.Redirect("/");
    });
}

app.Run();
