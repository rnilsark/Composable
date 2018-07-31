using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nilsark.Avatars.Shared;

namespace Nilsark.Frontend.Server.Controllers
{
    [Route("api/[controller]")]
    public class AvatarController : Controller
    {
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(Guid id)
        {
            using (var client = new AvatarClient("http://localhost:5123"))
            {
                return Ok(await client.GetAvatarAsync(id.ToString()));
            }
        }
    }
}
