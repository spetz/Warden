using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Warden.Web.Dto;

namespace Warden.Web.Controllers
{
    [Route("api/data/iterations")]
    public class IterationDataApiController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]WardenIterationDto iterationDto)
        {
            //TODO: Store in database

            return new HttpStatusCodeResult(204);
        }
    }
}
