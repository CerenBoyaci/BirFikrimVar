using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BirFikrimVar.Core.Dtos.Auth;
using BirFikrimVar.Core.Entities;
using Microsoft.AspNetCore.Identity;
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

        public AuthController(UserManager<Kullanici> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
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

            return Ok(new
            {
                mesaj = "Kayıt başarılı. Lütfen e-posta adresinizi doğrulayın.",
                dogrulamaTokeni = token,
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
            //kullanıcı 15 günün sonunda sistemden atılmasın diye
            user.RefreshTokenEndDate = DateTime.UtcNow.AddDays(15); //her başarılı yenilemede süreyi ileri atar
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
                return Ok(new { mesaj = "E-posta adresiniz başarıyla doğrulandı. Artık giriş yapabilirsiniz." });

            return BadRequest(new { mesaj = "E-posta doğrulaması başarısız oldu veya token süresi dolmuş." });
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
    }
}
