using System.Text;
using Authentication;
using identity_server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddLogging();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured");
var key = Encoding.UTF8.GetBytes(jwtSecretKey);
var issuer = builder.Configuration["Jwt:Issuer"] ?? "identity-server";
var defaultAudience = builder.Configuration["Jwt:Audience"] ?? "api";
var validAudiences = builder.Configuration.GetSection("Jwt:Audiences").Get<string[]>() ?? [defaultAudience];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudiences = validAudiences,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["OAuth2:Google:ClientId"] ?? throw new InvalidOperationException("OAuth2:Google:ClientId is not configured");
        options.ClientSecret = builder.Configuration["OAuth2:Google:ClientSecret"] ?? throw new InvalidOperationException("OAuth2:Google:ClientSecret is not configured");
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["OAuth2:Microsoft:ClientId"] ?? throw new InvalidOperationException("OAuth2:Microsoft:ClientId is not configured");
        options.ClientSecret = builder.Configuration["OAuth2:Microsoft:ClientSecret"] ?? throw new InvalidOperationException("OAuth2:Microsoft:ClientSecret is not configured");
        options.CallbackPath = "/signin-microsoft";
        options.SaveTokens = true;
    });

// Register services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is not configured");
builder.Services.AddScoped<IUserRepository>(_ => new PostgresUserRepository(connectionString));
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IExternalLoginService, ExternalLoginService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
