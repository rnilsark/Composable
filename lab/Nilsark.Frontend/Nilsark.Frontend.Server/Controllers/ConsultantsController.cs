using System;
using System.Linq;
using System.Threading.Tasks;
using Composable.Messaging.Buses;
using Microsoft.AspNetCore.Mvc;
using Nilsark.Consultants.Shared.Commands;
using Nilsark.Consultants.Shared.Queries;
using Nilsark.Frontend.Shared;

namespace Nilsark.Frontend.Server.Controllers
{
    [Route("api/[controller]")]
    public class ConsultantsController : Controller
    {
        private readonly IRemoteApiNavigatorSession _bus;
        
        public ConsultantsController(IRemoteApiNavigatorSession bus)
        {
            _bus = bus;
        }

        [HttpGet]
        public async Task<ActionResult<ConsultantViewModel[]>> Get()
        {
            // todo: aggreagation with azure search.
            var consultantIdsQueryModel = await _bus.GetAsync(new GetConsultantIdsQuery());
            var allConsultants = await Task.WhenAll(consultantIdsQueryModel.ConsultantIds.Select(GetConsultantViewModelAsync));
            return Ok(allConsultants.ToList());

        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultantViewModel>> Get(Guid id)
        {
            return Ok(await GetConsultantViewModelAsync(id));
        }

        private async Task<ConsultantViewModel> GetConsultantViewModelAsync(Guid id)
        {
            var queryModel = await _bus.GetAsync(new GetConsultantQuery(id));
            return new ConsultantViewModel { ConsultantId = queryModel.Id, FullName = queryModel.FullName, Email = queryModel.Email };
        }

        [HttpPost]
        public void Post([FromBody] EnrolConsultantCommand command)
        {
            if (ModelState.IsValid)
            {
                _bus.Post(command);
            }
        }
    }
}
