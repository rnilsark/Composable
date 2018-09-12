using System;
using System.Collections.Generic;
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
            _actorServiceUri = new Uri($@"{FabricRuntime.GetActivationContext().ApplicationName}/{"Actor1V2ActorService"}");
        }

        public IActionResult Index()
        {
            return View(new CreateUiCommand { Id = Guid.NewGuid() });
        }

        public async Task<IActionResult> Rename(Guid id)
        {
            var proxy = _actorProxyFactory.CreateActorServiceProxy<IActor1V2ActorService>(
                _actorServiceUri, new ActorId(id));

            var readModel = await proxy.GetFooAsync(id, CancellationToken.None);

            return View(new RenameUiCommand { Name = readModel.Name, Id = id });
        }

        public async Task<IActionResult> History(Guid id)
        {
            var proxy = _actorProxyFactory.CreateActorServiceProxy<IActor1V2ActorService>(
                _actorServiceUri, new ActorId(id));

            var readModel = await proxy.GetHistoryAsync(id, CancellationToken.None);

            return View(new HistoryViewModel { EventNames = readModel.EventNames, Id = id });
        }

        public IActionResult HitMe(Guid id)
        {
            return View(new HitMeUiCommand { Id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUiCommand command)
        {
            var proxy = _actorProxyFactory.CreateActorProxy<IActor1V2>(new ActorId(command.Id));

            await proxy.CreateAsync(new CreateCommand { Name = command.Name }, CancellationToken.None);

            return RedirectToAction(nameof(Rename), new { command.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Rename(RenameUiCommand command)
        {
            var proxy = _actorProxyFactory.CreateActorProxy<IActor1V2>(new ActorId(command.Id));

            await proxy.RenameAsync(new RenameCommand { Name = command.Name }, CancellationToken.None);

            return RedirectToAction(nameof(Rename), new { command.Id });
        }

        [HttpPost]
        public async Task<IActionResult> HitMe(HitMeUiCommand command)
        {
            var proxy = _actorProxyFactory.CreateActorProxy<IActor1V2>(new ActorId(command.Id));

            var tasks = new List<Task>();
            for (var i = 0; i < command.Count; i++)
            {
                tasks.Add(proxy.RenameAsync(new RenameCommand { Name = Guid.NewGuid().ToString() }, CancellationToken.None));
            }

            await Task.WhenAll(tasks); //Cause <Count> number of events on a single actor (runs pll but actor access is in fact serial)

            return RedirectToAction(nameof(History), new { command.Id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
