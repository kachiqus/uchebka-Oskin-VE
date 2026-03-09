using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using oskin_site.Data;
using oskin_site.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + NewtonsoftJson (нужно для JsonPatchDocument)
builder.Services
    .AddControllers()
    .AddNewtonsoftJson();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(cs);
});

// DI сервисов auth
builder.Services.AddScoped<PasswordHasherService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<RawMaterialCalculatorService>();

// JWT auth
var jwt = builder.Configuration.GetSection("Jwt");
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];
var key = jwt["Key"];

if (string.IsNullOrWhiteSpace(key))
    throw new InvalidOperationException("Jwt:Key is missing in appsettings.json");

// AuthN + AuthZ
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// ВАЖНО: сначала Authentication, потом Authorization
app.UseAuthentication();
app.UseAuthorization();

// Web UI
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
