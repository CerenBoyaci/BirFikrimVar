using Amazon.S3;
using BirFikrimVar.Core.Entities;
using BirFikrimVar.Data.Context;
using BirFikrimVar.Service.Interfaces;
using BirFikrimVar.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Kullanici, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IIdeasService, IdeasService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = builder.Configuration["Minio:Endpoint"] ?? "http://minio:9000",
        ForcePathStyle = true
    };

    var accessKey = builder.Configuration["Minio:AccessKey"] ?? "minioadmin";
    var minioSecretKey = builder.Configuration["Minio:SecretKey"] ?? "minioadmin123";

    return new AmazonS3Client(accessKey, minioSecretKey, config);
});

builder.Services.AddScoped<IStorageService, S3StorageService>();

var app = builder.Build();

// Docker test ortamında Scalar/OpenAPI her ortamda açık kalsın.
// Production canlı sistemde istersen bunu tekrar sadece Development içine alırsın.
app.MapOpenApi();
app.MapScalarApiReference("/scalar");

// Docker içinde HTTPS redirect kapalı.
// HTTPS'i production'da Nginx / reverse proxy yönetecek.
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();

    await BirFikrimVar.API.Extensions.DbSeeder.SeedAsync(roleManager, userManager);
}

app.Run();