using System.Collections.Generic;
using System.Threading.Tasks;
using Warden.Web.Dto;

namespace Warden.Web.Services.DataStorage
{
    public interface IDataStorage
    {
        Task SaveIterationAsync(WardenIterationDto iteration);
        Task<IEnumerable<WardenIterationDto>> GetIterationsAsync(WardenIterationFiltersDto filters);
    }
}