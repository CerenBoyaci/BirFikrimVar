using System;
using System.Collections.Generic;
using System.Text;
using BirFikrimVar.Core.Dtos.Admin;

namespace BirFikrimVar.Service.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<KategoriDto>> GetCategoriesAsync();
        Task<string> CreateCategoryAsync(KategoriDto model);
        Task<string> UpdateCategoryAsync(int id, KategoriDto model);

        Task<string> OverrideIdeaStatusAsync(int fikirId, OverrideStatusDto model, string adminId);

        Task<object> GetIdeaMonitoringDataAsync(int fikirId);
    }
}
