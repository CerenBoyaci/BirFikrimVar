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
}
