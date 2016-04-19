using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Queries;
using Warden.Web.Core.Services;

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
        public async Task<IActionResult> Create([FromBody]WardenIterationDto iteration)
        {
            await _dataStorage.SaveIterationAsync(iteration);

            return new HttpStatusCodeResult(204);
        }

        [HttpGet]
        public async Task<IEnumerable<WardenIterationDto>> GetAll([FromUri]BrowseWardenIterations query)
        {
            var pagedResult = await _dataStorage.GetIterationsAsync(query);

            return pagedResult.Items;
        }
    }
}
