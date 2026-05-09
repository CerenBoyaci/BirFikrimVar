using BirFikrimVar.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BirFikrimVar.Core.Dtos.Auth;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] 
public class AdminController : ControllerBase
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<Kullanici> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
        if (result.Succeeded) return Ok(new { mesaj = "Rol başarıyla güncellendi." });

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
            return Ok(new { mesaj = $"Kullanıcı durumu '{durumMetni}' olarak güncellendi." });
        }

        return BadRequest(new { mesaj = "Durum güncellenirken bir hata oluştu." });
    }
}
