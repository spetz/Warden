using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace Warden.Web.Controllers
{
    [Route("api/checks")]
    public class ApiController : Controller
    {
        [HttpPost]
        public async Task Post([FromBody]Item item)
        {
        }
    }

    //Test data
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
