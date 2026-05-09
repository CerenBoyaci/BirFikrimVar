using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BirFikrimVar.Web.Models;
using System.Net.Http.Headers;

namespace BirFikrimVar.Web.Controllers
{
    [Authorize(Roles = "OnOnayci,KomisyonUyesi,Admin")]
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

         
            var categoryResponse = await client.GetAsync("ideas/active-categories");
            var activeCategories = new List<KategoriViewModel>();

            if (categoryResponse.IsSuccessStatusCode)
            {
                activeCategories = await categoryResponse.Content.ReadFromJsonAsync<List<KategoriViewModel>>();
            }

          
            var model = new OnOnayDegerlendirmeViewModel
            {
                FikirId = id,
                Fikir = fikir,
                KategoriPuanlari = activeCategories.Select(k => new KategoriPuanViewModel
                {
                    KategoriId = k.Id,
                    KategoriAdi = k.Ad
                }).ToList()
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

              
                    if (goruntulenecekHata.Contains("PUT") || goruntulenecekHata.Contains("zaten"))
                    {
                        var putResponse = await client.PutAsJsonAsync($"ideas/{model.FikirId}/pre-approval-evaluations/me", payload);

                        if (putResponse.IsSuccessStatusCode)
                        {
                            var putData = await putResponse.Content.ReadFromJsonAsync<ApiMesajViewModel>();
                            TempData["BasariMesaji"] = putData?.Mesaj ?? "Ön onay değerlendirmeniz başarıyla güncellendi.";
                            return RedirectToAction("PreApprovalList");
                        }
                        else
                        {
                            var putErrorContent = await putResponse.Content.ReadAsStringAsync();
                            var putErrorData = System.Text.Json.JsonSerializer.Deserialize<ApiMesajViewModel>(
                                putErrorContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            goruntulenecekHata = putErrorData?.Mesaj ?? "Güncelleme sırasında hata oluştu.";
                        }
                    }
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


        [HttpGet]
        public async Task<IActionResult> CommissionList()
        {
            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            AddTokenToHeader(client);

            var response = await client.GetAsync("ideas");

            if (response.IsSuccessStatusCode)
            {
                var model = await response.Content.ReadFromJsonAsync<List<FikirListeViewModel>>();

                // Sadece Komisyon Onayı Bekleyenleri filtrele (Admin için de bu sayfa sadece bunları göstermeli)
                if (model != null)
                {
                    model = model.Where(f => f.Durum == "KomisyonOnayiBekliyor").ToList();
                }

                return View(model ?? new List<FikirListeViewModel>());
            }

            return View(new List<FikirListeViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> CommissionEvaluate(int id)
        {
            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            AddTokenToHeader(client);

            var response = await client.GetAsync($"ideas/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound("Fikir bulunamadı veya erişim yetkiniz yok.");

            var fikir = await response.Content.ReadFromJsonAsync<FikirDetayViewModel>();

            var model = new KomisyonDegerlendirmeViewModel
            {
                FikirId = id,
                Fikir = fikir
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CommissionEvaluate(KomisyonDegerlendirmeViewModel model)
        {
            var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
            AddTokenToHeader(client);

            var payload = new { Puan = model.Puan };

        
            var response = await client.PostAsJsonAsync($"ideas/{model.FikirId}/commission-evaluations", payload);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<ApiMesajViewModel>();
                TempData["BasariMesaji"] = responseData?.Mesaj ?? "Komisyon değerlendirmesi başarıyla kaydedildi.";
                return RedirectToAction("CommissionList");
            }

        
            var errorContent = await response.Content.ReadAsStringAsync();
            string errorMessage = "Değerlendirme gönderilirken bir hata oluştu.";

            try
            {
                var errorData = System.Text.Json.JsonSerializer.Deserialize<ApiMesajViewModel>(
                    errorContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (errorData != null && !string.IsNullOrWhiteSpace(errorData.Mesaj))
                {
                    errorMessage = errorData.Mesaj;

                   
                    if (errorMessage.Contains("PUT") || errorMessage.Contains("zaten"))
                    {
                        var putResponse = await client.PutAsJsonAsync($"ideas/{model.FikirId}/commission-evaluations/me", payload);
                        if (putResponse.IsSuccessStatusCode)
                        {
                            var putData = await putResponse.Content.ReadFromJsonAsync<ApiMesajViewModel>();
                            TempData["BasariMesaji"] = putData?.Mesaj ?? "Komisyon değerlendirmeniz başarıyla güncellendi.";
                            return RedirectToAction("CommissionList");
                        }
                        else
                        {
                            var putErrorContent = await putResponse.Content.ReadAsStringAsync();
                            var putErrorData = System.Text.Json.JsonSerializer.Deserialize<ApiMesajViewModel>(
                                putErrorContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            errorMessage = putErrorData?.Mesaj ?? "Güncelleme sırasında hata oluştu.";
                        }
                    }
                }
                else
                {
                    errorMessage += $" (Sunucu: {errorContent})";
                }
            }
            catch
            {
                errorMessage += $" (Sunucu Yanıtı: {errorContent})";
            }

            ModelState.AddModelError("", errorMessage);

         
            var fikirResponse = await client.GetAsync($"ideas/{model.FikirId}");
            if (fikirResponse.IsSuccessStatusCode)
            {
                model.Fikir = await fikirResponse.Content.ReadFromJsonAsync<FikirDetayViewModel>();
            }

            return View(model);
        }
    }
}
