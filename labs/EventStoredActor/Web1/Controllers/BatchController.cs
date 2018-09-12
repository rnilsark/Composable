using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
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
    public class BatchController : Controller
    {
        private readonly ActorProxyFactory _actorProxyFactory;
        
        public BatchController()
        {
            _actorProxyFactory = new ActorProxyFactory();
        }


        public IActionResult Index()
        {
            return View(new HitMeUiCommand());
        }

        [HttpPost]
        public async Task<IActionResult> HitMe(HitMeUiCommand command)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < command.Count; i++)
            {
                var proxy = _actorProxyFactory.CreateActorProxy<IActor1V2>(new ActorId(Guid.NewGuid()));
                tasks.Add(proxy.CreateAsync(new CreateCommand { Name = Guid.NewGuid().ToString() }, CancellationToken.None));
            }

            await Task.WhenAll(tasks); //Crates <Count> number of actors.

            return RedirectToAction("Index", "Home");
        }
    }
}
