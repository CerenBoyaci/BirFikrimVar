using BirFikrimVar.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
}
