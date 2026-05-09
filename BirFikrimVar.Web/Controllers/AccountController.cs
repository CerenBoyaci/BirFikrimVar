using Microsoft.AspNetCore.Mvc;
using BirFikrimVar.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BirFikrimVar.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Kayit() => View();

        [HttpPost]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            var yanit = await client.PostAsJsonAsync("auth/register", model);

            if (yanit.IsSuccessStatusCode)
            {
                TempData["BasariMesaji"] = "Kayıt başarılı! Lütfen e-posta adresinizi doğrulayarak giriş yapın.";
                return RedirectToAction("Giris");
            }

            var hata = await yanit.Content.ReadFromJsonAsync<ApiMesajViewModel>();
            ModelState.AddModelError("", hata?.Mesaj ?? "Kayıt sırasında bir hata oluştu.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Giris() => View();

        [HttpPost]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            var yanit = await client.PostAsJsonAsync("auth/login", model);

            if (yanit.IsSuccessStatusCode)
            {
                var loginData = await yanit.Content.ReadFromJsonAsync<KimlikYanitViewModel>();

                // Kullanıcı bilgilerini ve JWT token'ı Cookie'ye gömüyoruz
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginData.Email),
                    new Claim("Token", loginData.Token),
                    new Claim("RefreshToken", loginData.RefreshToken)
                };

             
                foreach (var rol in loginData.Roller.Split(','))
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "E-posta veya parola hatalı ya da hesabınız doğrulanmamış.");
            return View(model);
        }

        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Giris");
        }
    }
}