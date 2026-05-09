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
                
                TempData["BasariMesaji"] = "Kaydınız alındı. Hesabınızı kullanabilmek için e-posta doğrulaması yapmanız gerekmektedir. (Lütfen API Konsol ekranındaki linke tıklayın)";
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
        [HttpGet]
        public async Task<IActionResult> EmailOnayla(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return View("Hata", new ApiMesajViewModel { Mesaj = "Geçersiz onay isteği." });
            }

            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");

         
            var response = await client.GetAsync($"auth/confirm-email?email={email}&token={Uri.EscapeDataString(token)}");

            if (response.IsSuccessStatusCode)
            {
                TempData["BasariMesaji"] = "E-posta adresiniz başarıyla doğrulandı! Şimdi giriş yapabilirsiniz.";
                return RedirectToAction("Giris");
            }

            ViewBag.Hata = "E-posta doğrulaması başarısız oldu. Token süresi dolmuş olabilir.";
            return View();
        }
    }
}