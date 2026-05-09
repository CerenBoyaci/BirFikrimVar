using BirFikrimVar.Core.Dtos.Fikir;
using BirFikrimVar.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BirFikrimVar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IdeasController : ControllerBase
    {
        private readonly IIdeasService _ideasService;

        public IdeasController(IIdeasService ideasService)
        {
            _ideasService = ideasService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        private IList<string> GetUserRoles() => User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        [HttpPost]
        public async Task<IActionResult> CreateIdea([FromBody] FikirOlusturDto model)
        {
            var fikirId = await _ideasService.CreateIdeaAsync(model, GetUserId());
            return Ok(new { mesaj = "Fikriniz taslak olarak kaydedildi.", fikirId });
        }

        [HttpGet]
        public async Task<IActionResult> GetIdeas()
        {
            var fikirler = await _ideasService.GetIdeasAsync(GetUserId(), GetUserRoles());
            return Ok(fikirler);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIdeaDetail(int id)
        {
            var fikir = await _ideasService.GetIdeaDetailAsync(id, GetUserId(), GetUserRoles());
            if (fikir == null) return NotFound(new { mesaj = "Fikir bulunamadı veya erişim yetkiniz yok." });

            return Ok(fikir);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIdea(int id, [FromBody] FikirGuncelleDto model)
        {
            if (id != model.Id) return BadRequest(new { mesaj = "ID uyuşmazlığı." });

            var sonuc = await _ideasService.UpdateIdeaAsync(id, model, GetUserId());

            return sonuc switch
            {
                "Bulunamadi" => NotFound(new { mesaj = "Fikir bulunamadı." }),
                "Yetkisiz" => Forbid(),
                "Basarili" => Ok(new { mesaj = "Fikir başarıyla güncellendi." }),
                _ => BadRequest(new { mesaj = sonuc })
            };
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitForPreApproval(int id)
        {
            var sonuc = await _ideasService.SubmitForPreApprovalAsync(id, GetUserId());

            return sonuc switch
            {
                "Bulunamadi" => NotFound(new { mesaj = "Fikir bulunamadı." }),
                "Yetkisiz" => Forbid(),
                "Basarili" => Ok(new { mesaj = "Fikriniz ön onaya gönderildi. Durumu: Ön Onay Bekliyor." }),
                _ => BadRequest(new { mesaj = sonuc })
            };
        }
    }
}
