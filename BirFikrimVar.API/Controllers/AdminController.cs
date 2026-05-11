using BirFikrimVar.Core.Dtos.Admin;
using BirFikrimVar.Core.Dtos.Auth;
using BirFikrimVar.Core.Entities;
using BirFikrimVar.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] 
public class AdminController : ControllerBase
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAdminService _adminService;
    private readonly ILogService _logService;

    public AdminController(UserManager<Kullanici> userManager, RoleManager<IdentityRole> roleManager, IAdminService adminService, ILogService logService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _adminService = adminService;
        _logService = logService;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _logService.GetAdminLogsAsync();
        return Ok(logs);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = _userManager.Users.ToList();
        var userList = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new
            {
                user.Id,
                user.Ad,
                user.Soyad,
                user.Email,
                user.AktifMi,
                Roller = roles
            });
        }
        return Ok(userList);
    }

    [HttpPost("users/{id}/assign-role")]
    public async Task<IActionResult> AssignRole(string id, [FromBody] string roleName)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();


        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);


        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (result.Succeeded)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _logService.LogAdminActionAsync(adminId, "Rol Atama", $"{user.Email} kullanıcısına '{roleName}' rolü atandı.");

            return Ok(new { mesaj = "Rol başarıyla güncellendi." });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] AdminKullaniciOlusturDto model)
    {
        var userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null) return BadRequest(new { mesaj = "Bu e-posta adresi zaten sistemde kayıtlı." });

        var user = new Kullanici
        {
            UserName = model.Email,
            Email = model.Email,
            Ad = model.Ad,
            Soyad = model.Soyad,
            AktifMi = true,
            EmailConfirmed = true 
        };

        var result = await _userManager.CreateAsync(user, model.Parola);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new { mesaj = $"Kullanıcı oluşturulamadı: {errors}" });
        }

  
        var roleToAssign = string.IsNullOrWhiteSpace(model.Rol) ? "StandartKullanici" : model.Rol;
        await _userManager.AddToRoleAsync(user, roleToAssign);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _logService.LogAdminActionAsync(adminId, "Kullanıcı Oluşturma", $"{model.Email} e-posta adresli yeni kullanıcı oluşturuldu. (Rol: {roleToAssign})");

        return Ok(new { mesaj = "Kullanıcı başarıyla oluşturuldu." });
    }

    [HttpPost("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound(new { mesaj = "Kullanıcı bulunamadı." });

    
        user.AktifMi = !user.AktifMi;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var durumMetni = user.AktifMi ? "Aktif" : "Pasif";
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _logService.LogAdminActionAsync(adminId, "Kullanıcı Durum Güncelleme", $"{user.Email} kullanıcısının durumu '{durumMetni}' olarak güncellendi.");
            return Ok(new { mesaj = $"Kullanıcı durumu '{durumMetni}' olarak güncellendi." });
        }

        return BadRequest(new { mesaj = "Durum güncellenirken bir hata oluştu." });
    }

    [HttpPost("ideas/{id}/override-status")]
    public async Task<IActionResult> OverrideIdeaStatus(int id, [FromBody] OverrideStatusDto model)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sonuc = await _adminService.OverrideIdeaStatusAsync(id, model, adminId);

        if (sonuc == "Bulunamadi") return NotFound(new { mesaj = "Fikir bulunamadı." });

        await _logService.LogAdminActionAsync(adminId, "Fikir Durum Ezme (Override)", $"ID'si {id} olan fikrin durumu manuel olarak '{model.YeniDurum}' yapıldı. Sebep: {model.Aciklama}");

        return Ok(new { mesaj = "Fikir durumu admin tarafından başarıyla değiştirildi ve sisteme loglandı." });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var kategoriler = await _adminService.GetCategoriesAsync();
        return Ok(kategoriler);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] KategoriDto model)
    {
        await _adminService.CreateCategoryAsync(model);
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _logService.LogAdminActionAsync(adminId, "Kategori Ekleme", $"'{model.Ad}' adlı yeni kategori eklendi.");
        return Ok(new { mesaj = "Kategori başarıyla eklendi." });
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] KategoriDto model)
    {
        var sonuc = await _adminService.UpdateCategoryAsync(id, model);
        if (sonuc == "Bulunamadi") return NotFound(new { mesaj = "Kategori bulunamadı." });

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var durum = model.AktifMi ? "Aktif" : "Pasif";
        await _logService.LogAdminActionAsync(adminId, "Kategori Güncelleme", $"ID'si {id} olan kategori güncellendi. (Yeni Ad: {model.Ad}, Durum: {durum})");

        return Ok(new { mesaj = "Kategori başarıyla güncellendi." });
    }

    [HttpGet("ideas/{id}/monitoring")]
    public async Task<IActionResult> GetIdeaMonitoringData(int id)
    {
        var monitoringData = await _adminService.GetIdeaMonitoringDataAsync(id);
        if (monitoringData == null) return NotFound(new { mesaj = "Fikir bulunamadı." });

        return Ok(monitoringData);
    }
}
