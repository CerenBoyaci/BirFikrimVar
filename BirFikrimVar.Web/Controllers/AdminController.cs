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
           
            var users = await response.Content.ReadFromJsonAsync<List<dynamic>>();
            return View(users);
        }
        return View();
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
}
