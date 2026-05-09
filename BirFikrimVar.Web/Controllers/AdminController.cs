using BirFikrimVar.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private void AddTokenToHeader(HttpClient client)
    {
        var token = User.FindFirst("Token")?.Value;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<IActionResult> Users()
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.GetAsync("admin/users");
        if (response.IsSuccessStatusCode)
        {
            //json element kullanma sebebim yüksek performans ve düşük bellek tüketimi
            var users = await response.Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();
            return View(users);
        }

        return View(new List<System.Text.Json.JsonElement>());
    }

    [HttpPost]
    public async Task<IActionResult> RolGuncelle(string userId, string roleName)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.PostAsJsonAsync($"admin/users/{userId}/assign-role", roleName);
        if (response.IsSuccessStatusCode)
            TempData["BasariMesaji"] = "Kullanıcı rolü güncellendi.";
        else
            TempData["HataMesaji"] = "Rol güncellenirken bir hata oluştu.";

        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> AllIdeas()
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.GetAsync("ideas");

        if (response.IsSuccessStatusCode)
        {
            var model = await response.Content.ReadFromJsonAsync<List<FikirListeViewModel>>();
            return View(model ?? new List<FikirListeViewModel>());
        }

        return View(new List<FikirListeViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> KullaniciOlustur(AdminKullaniciOlusturViewModel model)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.PostAsJsonAsync("admin/users", model);
        if (response.IsSuccessStatusCode)
        {
            TempData["BasariMesaji"] = "Kullanıcı başarıyla oluşturuldu.";
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ApiMesajViewModel>();
            TempData["HataMesaji"] = error?.Mesaj ?? "Kullanıcı oluşturulurken bir hata meydana geldi.";
        }

        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> DurumDegistir(string userId)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.PostAsync($"admin/users/{userId}/toggle-status", null);
        if (response.IsSuccessStatusCode)
        {
            TempData["BasariMesaji"] = "Kullanıcı durumu başarıyla güncellendi.";
        }
        else
        {
            TempData["HataMesaji"] = "Durum güncellenirken bir hata oluştu.";
        }

        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> Categories()
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.GetAsync("admin/categories");
        if (response.IsSuccessStatusCode)
        {
            var categories = await response.Content.ReadFromJsonAsync<List<KategoriViewModel>>();
            return View(categories ?? new List<KategoriViewModel>());
        }
        return View(new List<KategoriViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> KaydetKategori(KategoriViewModel model)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        HttpResponseMessage response;
        if (model.Id == 0)
            response = await client.PostAsJsonAsync("admin/categories", model);
        else
            response = await client.PutAsJsonAsync($"admin/categories/{model.Id}", model);

        if (response.IsSuccessStatusCode)
            TempData["BasariMesaji"] = "Kategori işlemi başarılı.";
        else
            TempData["HataMesaji"] = "Kategori kaydedilirken hata oluştu.";

        return RedirectToAction("Categories");
    }

    [HttpPost]
    public async Task<IActionResult> OverrideDurum(OverrideStatusViewModel model)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

       
        var payload = new { YeniDurum = model.YeniDurum, Aciklama = model.Aciklama };
        var response = await client.PostAsJsonAsync($"admin/ideas/{model.FikirId}/override-status", payload);

        if (response.IsSuccessStatusCode)
            TempData["BasariMesaji"] = "Sürece başarıyla müdahale edildi ve loglandı.";
        else
            TempData["HataMesaji"] = "Durum değiştirilirken bir hata oluştu.";

        return RedirectToAction("AllIdeas");
    }

    [HttpGet]
    public async Task<IActionResult> IdeaMonitoring(int id)
    {
        var client = _httpClientFactory.CreateClient("BirFikrimVarAPI");
        AddTokenToHeader(client);

        var response = await client.GetAsync($"admin/ideas/{id}/monitoring");
        if (response.IsSuccessStatusCode)
        {
            //performans odaklı yaklaşımına uygun olarak JsonElement ile
            var data = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            return View(data);
        }

        TempData["HataMesaji"] = "İzleme verisi alınamadı.";
        return RedirectToAction("AllIdeas");
    }
}
