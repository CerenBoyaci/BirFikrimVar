using BirFikrimVar.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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
                
                TempData["BasariMesaji"] = "Kayıt başarılı! Lütfen e-posta adresinize gönderilen doğrulama bağlantısına tıklayın.";

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
            else if (yanit.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var hata = await yanit.Content.ReadFromJsonAsync<ApiMesajViewModel>();

                //doğrulayın içeriyorsa linki tekrar üret
                if (hata?.Mesaj.Contains("doğrulayın") == true)
                {
                    //api ye git
                    var tokenResponse = await client.GetAsync($"auth/resend-confirmation-token?email={model.Email}");

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        var tokenData = await tokenResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                        var token = tokenData.GetProperty("token").GetString();

                       
                        var webUrl = "https://localhost:7112"; 
                        TempData["DevModeOnayLinki"] = $"{webUrl}/Account/EmailOnayla?email={model.Email}&token={Uri.EscapeDataString(token)}";

                        ModelState.AddModelError("", "E-postanız henüz doğrulanmamış. Sayfanın üstündeki test butonunu kullanarak hesabınızı onaylayabilirsiniz.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hesabınız doğrulanmamış ve yeni onay linki oluşturulamadı.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", hata?.Mesaj ?? "E-posta veya parola hatalı.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Giriş işlemi sırasında bir hata oluştu.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EpostaDogrula(string email)
        {
            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");

            //api den yeni token alma
            var response = await client.GetAsync($"auth/resend-confirmation-token?email={email}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>();
                string token = data.GetProperty("token").GetString();

                //bu token ile doğrulama işlemi yapma
                var confirmResponse = await client.GetAsync($"auth/confirm-email?email={email}&token={Uri.EscapeDataString(token)}");

                if (confirmResponse.IsSuccessStatusCode)
                {
                    TempData["BasariMesaji"] = "E-postanız başarıyla doğrulandı! Şimdi giriş yapabilirsiniz.";
                    return RedirectToAction("Giris");
                }
            }

            TempData["HataMesaji"] = "Doğrulama işlemi sırasında bir hata oluştu.";
            return RedirectToAction("Giris");
        }

        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Giris");
        }
        [HttpGet]
        public async Task<IActionResult> EmailOnayla(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                TempData["HataMesaji"] = "Geçersiz veya eksik doğrulama bağlantısı.";
                return RedirectToAction("Giris");
            }

            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");

            
            var response = await client.GetAsync($"auth/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");

            if (response.IsSuccessStatusCode)
            {
                TempData["BasariMesaji"] = "E-posta adresiniz başarıyla doğrulandı. Artık giriş yapabilirsiniz.";
            }
            else
            {
             
                var error = await response.Content.ReadFromJsonAsync<ApiMesajViewModel>();
                TempData["HataMesaji"] = error?.Mesaj ?? "E-posta doğrulama işlemi başarısız oldu veya token süresi doldu.";
            }

            
            return RedirectToAction("Giris");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");

            //headera token ekledim
            var token = User.FindFirst("Token")?.Value;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("auth/me");

            if (response.IsSuccessStatusCode)
            {
                var userDetail = await response.Content.ReadFromJsonAsync<ProfilGuncelleViewModel>();
                return View(userDetail);
            }

            TempData["HataMesaji"] = "Profil bilgileri yüklenemedi.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profil(ProfilGuncelleViewModel model)
        {

            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            var token = User.FindFirst("Token")?.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsJsonAsync("auth/update-profile", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["BasariMesaji"] = "Profiliniz güncellendi.";
                return RedirectToAction("Profil"); 
            }

         
            var errorData = await response.Content.ReadFromJsonAsync<ApiMesajViewModel>();
            ModelState.AddModelError("", errorData?.Mesaj ?? "Güncelleme sırasında bir hata oluştu.");

            return View(model);
        }

    }
}