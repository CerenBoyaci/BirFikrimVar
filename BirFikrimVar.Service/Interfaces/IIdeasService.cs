using BirFikrimVar.Core.Dtos.Admin;
using BirFikrimVar.Core.Dtos.Fikir;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BirFikrimVar.Service.Interfaces
{
    public interface IIdeasService
    {
        Task<int> CreateIdeaAsync(FikirOlusturDto model, string userId);
        Task<IEnumerable<FikirListeDto>> GetIdeasAsync(string userId, IList<string> userRoles);
        Task<FikirDetayDto> GetIdeaDetailAsync(int id, string userId, IList<string> userRoles);
        Task<string> UpdateIdeaAsync(int id, FikirGuncelleDto model, string userId);
        Task<string> SubmitForPreApprovalAsync(int id, string userId);
        Task<string> SubmitPreApprovalEvaluationAsync(int fikirId, OnOnayPuanEkleDto model, string userId);
        Task<string> UpdatePreApprovalEvaluationAsync(int fikirId, OnOnayPuanEkleDto model, string userId);

        Task<string> SubmitCommissionEvaluationAsync(int fikirId, KomisyonPuanEkleDto model, string userId);
        Task<string> UpdateCommissionEvaluationAsync(int fikirId, KomisyonPuanEkleDto model, string userId);
        Task<string> UploadIdeaAttachmentsAsync(int fikirId, List<IFormFile> files, string userId);
     
        Task<(string filePath, string fileName, string contentType)> GetAttachmentForDownloadAsync(int fikirId, int fileId, string userId, IList<string> userRoles);
        Task<IEnumerable<KategoriDto>> GetActiveCategoriesAsync();
    }
}
