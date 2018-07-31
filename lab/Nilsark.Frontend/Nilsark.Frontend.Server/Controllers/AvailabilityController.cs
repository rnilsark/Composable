using System;
using Microsoft.AspNetCore.Mvc;

namespace Nilsark.Frontend.Server.Controllers
{
    [Route("api/[controller]")]
    public class AvailabilityController : Controller
    {
        public AvailabilityController() { }

        [HttpGet("{id}")]
        public ActionResult<bool> Get(Guid id)
        {
            return Ok(true);
        }
    }
}