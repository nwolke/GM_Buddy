using GM_Buddy.Business;
using GM_Buddy.Contracts;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Data;
using GM_Buddy.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:5001",
            ValidAudience = "https://localhost:5001",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345superSecretKey@345superSecretKey@345superSecretKey@345"))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("DbSettings"));
builder.Services.AddTransient<IDbConnector, DbConnector>();
builder.Services.AddScoped<INpcLogic, NpcLogic>();
builder.Services.AddSingleton<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthObjectResolver, AuthObjectResolver>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Add Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http
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
       }
      },
            new List<string>()
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        _ = builder.WithOrigins("https://localhost:49505") // Add your React app's URL
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

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
