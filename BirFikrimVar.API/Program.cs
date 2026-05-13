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

/*En az 1 büyük harf
En az 1 rakam
En az 1 özel karakter
Minimum 8 karakter*/

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

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
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
builder.Services.AddScoped<BirFikrimVar.Service.Interfaces.IIdeasService, BirFikrimVar.Service.Services.IdeasService>();
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        // Minio yerelde genellikle 9000 portunda çalışır
        ServiceURL = builder.Configuration["Minio:Endpoint"] ?? "http://localhost:9000",
        ForcePathStyle = true // Minio kullanımı için bu ayar zorunludur
    };

    var accessKey = builder.Configuration["Minio:AccessKey"] ?? "minioadmin";
    var secretKey = builder.Configuration["Minio:SecretKey"] ?? "minioadmin";

    return new AmazonS3Client(accessKey, secretKey, config);
});


/*if (builder.Environment.IsProduction())
{
  
    builder.Services.AddScoped<IStorageService, S3StorageService>();
}*/
// geliştirme ortamında Minio'yu test etmek için şartı 'true' yapıyoruz
if (true)
{
    builder.Services.AddScoped<IStorageService, S3StorageService>();
}
else
{
   
    builder.Services.AddScoped<IStorageService, LocalStorageService>();
}
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 5. Seed Data İşlemi
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
    await BirFikrimVar.API.Extensions.DbSeeder.SeedAsync(roleManager, userManager);
}

app.Run();