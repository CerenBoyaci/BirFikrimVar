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

        // --- ÖN ONAY ENDPOINTLERİ ---

        [HttpPost("{id}/pre-approval-evaluations")]
        [Authorize(Roles = "OnOnayci,Admin")]
        public async Task<IActionResult> EvaluatePreApproval(int id, [FromBody] OnOnayPuanEkleDto model)
        {
            var sonuc = await _ideasService.SubmitPreApprovalEvaluationAsync(id, model, GetUserId());
            return sonuc switch
            {
                "Bulunamadi" => NotFound(new { mesaj = "Fikir bulunamadı." }),
                "Basarili" => Ok(new { mesaj = "Ön onay puanınız eklendi." }),
                _ => BadRequest(new { mesaj = sonuc })
            };
        }

        [HttpPut("{id}/pre-approval-evaluations/me")]
        [Authorize(Roles = "OnOnayci,Admin")]
        public async Task<IActionResult> UpdatePreApprovalEvaluation(int id, [FromBody] OnOnayPuanEkleDto model)
        {
            var sonuc = await _ideasService.UpdatePreApprovalEvaluationAsync(id, model, GetUserId());
            return sonuc switch
            {
                "Bulunamadi" => NotFound(new { mesaj = "Fikir bulunamadı." }),
                "Basarili" => Ok(new { mesaj = "Ön onay puanınız güncellendi." }),
                _ => BadRequest(new { mesaj = sonuc })
            };
        }


      

        [HttpPost("{id}/commission-evaluations")]
        [Authorize(Roles = "KomisyonUyesi,Admin")]
        public async Task<IActionResult> EvaluateCommission(int id, [FromBody] KomisyonPuanEkleDto model)
        {
            var sonuc = await _ideasService.SubmitCommissionEvaluationAsync(id, model, GetUserId());
            return sonuc switch
            {
                "Bulunamadi" => NotFound(new { mesaj = "Fikir bulunamadı." }),
                "Basarili" => Ok(new { mesaj = "Komisyon puanınız kaydedildi. Nihai sonuç için diğer üyeler bekleniyor." }),
                "KomisyonTamamlandi" => Ok(new { mesaj = "3 üyenin değerlendirmesi tamamlandı ve fikrin nihai durumu belirlendi." }),
                _ => BadRequest(new { mesaj = sonuc })
            };
        }

        [HttpPut("{id}/commission-evaluations/me")]
        [Authorize(Roles = "KomisyonUyesi,Admin")]
        public async Task<IActionResult> UpdateCommissionEvaluation(int id, [FromBody] KomisyonPuanEkleDto model)
        {
            var sonuc = await _ideasService.UpdateCommissionEvaluationAsync(id, model, GetUserId());
            return sonuc switch
            {
                "Bulunamadi" => NotFound(new { mesaj = "Fikir bulunamadı." }),
                "Basarili" => Ok(new { mesaj = "Komisyon puanınız başarıyla güncellendi." }),
                _ => BadRequest(new { mesaj = sonuc })
            };
        }

        [HttpPost("{id}/attachments")]
        public async Task<IActionResult> UploadAttachments(int id, [FromForm] List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(new { mesaj = "Lütfen en az bir dosya seçin." });

            var sonuc = await _ideasService.UploadIdeaAttachmentsAsync(id, files, GetUserId());

            return sonuc switch
            {
                "Bulunamadi" => NotFound(new { mesaj = "Fikir bulunamadı." }),
                "Yetkisiz" => Forbid(),
                "Basarili" => Ok(new { mesaj = "Dosyalar başarıyla yüklendi." }),
                _ => BadRequest(new { mesaj = sonuc })
            };
        }

        [HttpGet("active-categories")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _ideasService.GetActiveCategoriesAsync();
            return Ok(categories);
        }
    }
}
