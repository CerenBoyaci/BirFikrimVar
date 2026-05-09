using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BirFikrimVar.Web.Models;
using System.Net.Http.Headers;

namespace BirFikrimVar.Web.Controllers
{
    [Authorize(Roles = "OnOnayci,Admin")]
    public class ApprovalController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApprovalController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private void AddTokenToHeader(HttpClient client)
        {
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PreApprovalList()
        {
            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            AddTokenToHeader(client);

            var response = await client.GetAsync("ideas");

            if (response.IsSuccessStatusCode)
            {
                var model = await response.Content.ReadFromJsonAsync<List<FikirListeViewModel>>();

              
                if (model != null)
                {
                    model = model.Where(f => f.Durum == "OnOnayBekliyor").ToList();
                }

                return View(model ?? new List<FikirListeViewModel>());
            }

            return View(new List<FikirListeViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Evaluate(int id)
        {
            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            AddTokenToHeader(client);

            var response = await client.GetAsync($"ideas/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound("Fikir bulunamadı veya erişim yetkiniz yok.");

            var fikir = await response.Content.ReadFromJsonAsync<FikirDetayViewModel>();

         
            var model = new OnOnayDegerlendirmeViewModel
            {
                FikirId = id,
                Fikir = fikir,
                KategoriPuanlari = new List<KategoriPuanViewModel>
                {
                    new KategoriPuanViewModel { KategoriId = 1, KategoriAdi = "Stratejik Uyum" },
                    new KategoriPuanViewModel { KategoriId = 2, KategoriAdi = "Yenilikçilik" },
                    new KategoriPuanViewModel { KategoriId = 3, KategoriAdi = "Uygulanabilirlik" },
                    new KategoriPuanViewModel { KategoriId = 4, KategoriAdi = "Beklenen Fayda" },
                    new KategoriPuanViewModel { KategoriId = 5, KategoriAdi = "Risk ve Sürdürülebilirlik" },
                    new KategoriPuanViewModel { KategoriId = 6, KategoriAdi = "Kaynak İhtiyacı" }
                }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Evaluate(OnOnayDegerlendirmeViewModel model)
        {
            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            AddTokenToHeader(client);

            var payload = new
            {
                KategoriPuanlari = model.KategoriPuanlari.Select(k => new { k.KategoriId, k.Puan }).ToList()
            };

            var response = await client.PostAsJsonAsync($"ideas/{model.FikirId}/pre-approval-evaluations", payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["BasariMesaji"] = "Değerlendirme başarıyla kaydedildi ve durum güncellendi.";
                return RedirectToAction("PreApprovalList");
            }

         
            var errorContent = await response.Content.ReadAsStringAsync();
            string goruntulenecekHata = "Değerlendirme gönderilirken bir hata oluştu.";

            try
            {
           
                var errorData = System.Text.Json.JsonSerializer.Deserialize<ApiMesajViewModel>(
                    errorContent,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (errorData != null && !string.IsNullOrWhiteSpace(errorData.Mesaj))
                {
                    goruntulenecekHata = errorData.Mesaj;
                }
                else
                {
                    goruntulenecekHata += $" (Sunucu Detayı: {errorContent})";
                }
            }
            catch
            {
               
                goruntulenecekHata += $" (Sunucu Yanıtı: {errorContent})";
            }

            ModelState.AddModelError("", goruntulenecekHata);
        
            var fikirResponse = await client.GetAsync($"ideas/{model.FikirId}");
            if (fikirResponse.IsSuccessStatusCode)
            {
                model.Fikir = await fikirResponse.Content.ReadFromJsonAsync<FikirDetayViewModel>();
            }

            return View(model);
        }
    }
}
