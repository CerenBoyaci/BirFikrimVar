using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BirFikrimVar.Service.Interfaces
{
    public interface ILogService
    {
        Task LogAdminActionAsync(string adminId, string islemTipi, string aciklama);
        Task<IEnumerable<object>> GetAdminLogsAsync();
    }
}
