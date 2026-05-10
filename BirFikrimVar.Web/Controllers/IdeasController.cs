using BirFikrimVar.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

[Authorize]
public class IdeasController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IdeasController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    private void AddTokenToHeader(HttpClient client)
    {
        var token = User.FindFirst("Token")?.Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.GetAsync("ideas");
        if (response.IsSuccessStatusCode)
        {
            var model = await response.Content.ReadFromJsonAsync<List<FikirListeViewModel>>();
            return View(model);
        }
        return View(new List<FikirListeViewModel>());
    }

  
    [HttpGet]
    public IActionResult Olustur() => View();

 
    [HttpPost]
    public async Task<IActionResult> Olustur(FikirOlusturViewModel model, List<IFormFile> Dosyalar)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.PostAsJsonAsync("ideas", model);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IdeaResult>();
            int fikirId = result.fikirId;

            if (Dosyalar != null && Dosyalar.Any())
            {
                var content = new MultipartFormDataContent();
                foreach (var file in Dosyalar)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    content.Add(fileContent, "files", file.FileName);
                }
                await client.PostAsync($"ideas/{fikirId}/attachments", content);
            }

            TempData["BasariMesaji"] = "Fikriniz başarıyla taslak olarak kaydedildi.";
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", "Fikir kaydedilirken bir hata oluştu.");
        return View(model);
    }

  
    [HttpPost]
    public async Task<IActionResult> OnOnayaGonder(int id)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.PostAsync($"ideas/{id}/submit", null);
        if (response.IsSuccessStatusCode)
            TempData["BasariMesaji"] = "Fikriniz ön onaya gönderildi.";
        else
            TempData["HataMesaji"] = "İşlem başarısız oldu.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Detay(int id)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.GetAsync($"ideas/{id}");
        if (response.IsSuccessStatusCode)
        {
            var model = await response.Content.ReadFromJsonAsync<FikirDetayViewModel>();
            return View(model);
        }

        TempData["HataMesaji"] = "Fikir detayları alınamadı.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Guncelle(FikirDetayViewModel model, List<IFormFile> YeniDosyalar)
    {
        if (model.Durum != "Taslak") return RedirectToAction("Detay", new { id = model.Id });

        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        //metindekileri güncelleme
        var response = await client.PutAsJsonAsync($"ideas/{model.Id}", model);

        if (response.IsSuccessStatusCode)
        {
            //yeni dosyalar var ise ekleme
            if (YeniDosyalar != null && YeniDosyalar.Any())
            {
                var content = new MultipartFormDataContent();
                foreach (var file in YeniDosyalar)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    content.Add(fileContent, "files", file.FileName);
                }
                await client.PostAsync($"ideas/{model.Id}/attachments", content);
            }

            TempData["BasariMesaji"] = "Fikir ve yeni dosyalar güncellendi.";
            return RedirectToAction("Detay", new { id = model.Id });
        }
        return View("Detay", model);
    }

    [HttpPost]
    public async Task<IActionResult> DosyaSil(int fikirId, int dosyaId)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.DeleteAsync($"ideas/{fikirId}/attachments/{dosyaId}");
        if (response.IsSuccessStatusCode)
            TempData["BasariMesaji"] = "Dosya başarıyla silindi.";
        else
            TempData["HataMesaji"] = "Dosya silinemedi.";

        return RedirectToAction("Detay", new { id = fikirId });
    }

    [HttpGet]
    public async Task<IActionResult> DosyaIndir(int fikirId, int dosyaId)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);


        var response = await client.GetAsync($"ideas/{fikirId}/attachments/{dosyaId}");

        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

          
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                        ?? response.Content.Headers.ContentDisposition?.FileName
                        ?? "indirilen_dosya";

            return File(stream, contentType, fileName);
        }


        TempData["HataMesaji"] = $"Dosya indirilemedi. API Hatası: {response.StatusCode}";
        return RedirectToAction("Detay", new { id = fikirId });
    } 
}
