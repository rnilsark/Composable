using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces;
using Actor1.Interfaces.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Web1.Models;

namespace Web1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ActorProxyFactory _actorProxyFactory;
        private readonly Uri _actorServiceUri;

        public HomeController()
        {

            _actorProxyFactory = new ActorProxyFactory();
            _actorServiceUri = new Uri($@"{FabricRuntime.GetActivationContext().ApplicationName}/{"Actor1ActorService"}");
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View(new SetNameUiCommand { Id = Guid.NewGuid() });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(Guid id)
        {
            var proxy = _actorProxyFactory.CreateActorServiceProxy<IActor1ActorService>(
                _actorServiceUri, new ActorId(id));

            var readModel = await proxy.GetAsync(id, CancellationToken.None);

            return View(new SetNameUiCommand { Name = readModel.Name, Id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Index(SetNameUiCommand command)
        {
            var proxy = _actorProxyFactory.CreateActorProxy<IActor1>(new ActorId(command.Id));

            await proxy.SetNameAsync(new SetNameCommand { Name = command.Name }, CancellationToken.None);

            return RedirectToAction(nameof(Index), new { command.Id });
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
