using BirFikrimVar.Core.Dtos.Auth;
using BirFikrimVar.Core.Entities;
using BirFikrimVar.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BirFikrimVar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Kullanici> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(UserManager<Kullanici> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] KayitDto model)
        {
            if (model.Parola != model.ParolaTekrar)
                return BadRequest(new { mesaj = "Parolalar eşleşmiyor." });

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest(new { mesaj = "Bu e-posta adresi zaten kullanımda." });

            var user = new Kullanici
            {
                UserName = model.Email,
                Email = model.Email,
                Ad = model.Ad,
                Soyad = model.Soyad,
                AktifMi = true
            };

            var result = await _userManager.CreateAsync(user, model.Parola);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { mesaj = $"Kullanıcı oluşturulamadı: {errors}" });
            }

            await _userManager.AddToRoleAsync(user, "StandartKullanici");


            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var webMvcUrl = "https://localhost:7112";
            var confirmationLink = $"{webMvcUrl}/Account/EmailOnayla?email={user.Email}&token={Uri.EscapeDataString(token)}";


            string subject = "BirFikrimVar - Hesabınızı Doğrulayın";
            string body = $@"
    <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
        <h2 style='color: #007bff;'>Hoş Geldin {user.Ad}!</h2>
        <p>BirFikrimVar ailesine katıldığın için teşekkürler. Hesabını aktif etmek için lütfen aşağıdaki butona tıkla:</p>
        <div style='margin: 30px 0;'>
            <a href='{confirmationLink}' style='background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Hesabımı Doğrula</a>
        </div>
        <p style='font-size: 12px; color: #777;'>Eğer buton çalışmazsa şu bağlantıyı tarayıcına yapıştırabilirsin:</p>
        <p style='font-size: 12px; color: #777;'>{confirmationLink}</p>
    </div>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
          
                Console.WriteLine($"E-posta gönderim hatası: {ex.Message}");
            }
         
            Console.WriteLine($"\nONAY LİNKİ: {confirmationLink}\n");

            return Ok(new
            {
                mesaj = "Kayıt başarılı. Lütfen e-posta adresinize gönderilen linke tıklayarak hesabınızı doğrulayın.",
                email = user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] GirisDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Parola))
                return Unauthorized(new { mesaj = "E-posta veya parola hatalı." });

            if (!user.AktifMi)
                return Unauthorized(new { mesaj = "Hesabınız pasife alınmış." });

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new { mesaj = "Lütfen giriş yapmadan önce e-posta adresinizi doğrulayın." });

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.Ad} {user.Soyad}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = CreateToken(authClaims);
           
            var refreshToken = Guid.NewGuid().ToString();


            //refresh token mantığı
         
            user.RefreshToken = refreshToken;
            user.RefreshTokenEndDate = DateTime.UtcNow.AddDays(7);

         
            await _userManager.UpdateAsync(user);

            
            var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpireMinutes"]));

            var response = new DogrulamaCevapDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken, 
                TokenExpireDate = expiration,
                Email = user.Email,
                Roller = string.Join(",", userRoles)
            };

            return Ok(response);
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
        {
          
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == model.RefreshToken);

            if (user == null || user.RefreshTokenEndDate <= DateTime.UtcNow)
                return Unauthorized(new { mesaj = "Geçersiz veya süresi dolmuş oturum." });

         
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, $"{user.Ad} {user.Soyad}"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };
            foreach (var role in userRoles) authClaims.Add(new Claim(ClaimTypes.Role, role));

            
            var newAccessToken = CreateToken(authClaims);
            var newRefreshToken = Guid.NewGuid().ToString();

           
            user.RefreshToken = newRefreshToken;
            //kullanıcı 7 günün sonunda sistemden atılmasın diye
            user.RefreshTokenEndDate = DateTime.UtcNow.AddDays(7); //her başarılı yenilemede süreyi ileri atar
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpireMinutes"]))
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return BadRequest(new { mesaj = "E-posta veya token eksik." });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { mesaj = "Kullanıcı bulunamadı." });

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                return Ok(new { mesaj = "E-posta başarıyla doğrulandı." }); 

            return BadRequest(new { mesaj = "Doğrulama başarısız." });
        }

        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpireMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        //kullanıcı giriş yapmak istiyor ama e postası doğrulanmamış o zaman gidip doğrulaması gerekiyor
        [HttpGet("resend-confirmation-token")]
        public async Task<IActionResult> ResendConfirmationToken([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new { mesaj = "Kullanıcı bulunamadı." });

            if (user.EmailConfirmed) return BadRequest(new { mesaj = "Bu e-posta zaten doğrulanmış." });

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

           //token ı döndürüyorum
            return Ok(new
            {
                mesaj = "Doğrulama bağlantısı oluşturuldu.",
                token = token,
                email = user.Email
            });
        }

        //kullanıcının mevcut bilgilerini getirmek için kullandım
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Ad,
                user.Soyad,
                user.Email
            });
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfilGuncelleDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound(new { mesaj = "Kullanıcı bulunamadı." });

      
            if (!string.IsNullOrEmpty(model.YeniParola) && model.MevcutParola == model.YeniParola)
            {
                return BadRequest(new { mesaj = "Yeni şifreniz mevcut şifreniz ile aynı olamaz." });
            }

           
            user.Ad = model.Ad;
            user.Soyad = model.Soyad;
            user.Email = model.Email;
            user.UserName = model.Email;

       
            if (!string.IsNullOrEmpty(model.YeniParola))
            {
                if (string.IsNullOrEmpty(model.MevcutParola))
                    return BadRequest(new { mesaj = "Şifre değiştirmek için mevcut şifrenizi girmelisiniz." });

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.MevcutParola, model.YeniParola);

                if (!passwordResult.Succeeded)
                {
              
                    var error = passwordResult.Errors.FirstOrDefault();
                    string mesaj = error?.Code switch
                    {
                        "PasswordTooShort" => "Şifre en az 8 karakter olmalıdır.",
                        "PasswordRequiresUpper" => "Şifre en az bir büyük harf içermelidir.",
                        "PasswordRequiresLower" => "Şifre en az bir küçük harf içermelidir.",
                        "PasswordRequiresDigit" => "Şifre en az bir rakam içermelidir.",
                        "PasswordRequiresNonAlphanumeric" => "Şifre en az bir özel karakter içermelidir.",
                        "PasswordMismatch" => "Mevcut şifrenizi hatalı girdiniz.",
                        _ => error?.Description ?? "Şifre kurallara uygun değil."
                    };
                    return BadRequest(new { mesaj });
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { mesaj = "Profil başarıyla güncellendi." });

            return BadRequest(new { mesaj = "Güncelleme başarısız." });
        }
    }
}
