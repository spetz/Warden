using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Dto;
using Warden.Web.Services.DataStorage;

namespace Warden.Web.Controllers
{
    [Route("api/data/iterations")]
    public class IterationDataApiController : Controller
    {
        private readonly IDataStorage _dataStorage;

        public IterationDataApiController(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]WardenIterationDto iterationDto)
        {
            await _dataStorage.SaveIterationAsync(iterationDto);

            return new HttpStatusCodeResult(204);
        }
    }
}
