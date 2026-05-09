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
}
