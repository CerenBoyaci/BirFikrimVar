using System;
using System.Collections.Generic;
using System.Text;
using BirFikrimVar.Core.Dtos.Fikir;

namespace BirFikrimVar.Service.Interfaces
{
    public interface IIdeasService
    {
        Task<int> CreateIdeaAsync(FikirOlusturDto model, string userId);
        Task<IEnumerable<FikirListeDto>> GetIdeasAsync(string userId, IList<string> userRoles);
        Task<FikirDetayDto> GetIdeaDetailAsync(int id, string userId, IList<string> userRoles);
        Task<string> UpdateIdeaAsync(int id, FikirGuncelleDto model, string userId);
        Task<string> SubmitForPreApprovalAsync(int id, string userId);
    }
}
