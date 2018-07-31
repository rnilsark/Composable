using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Mime;
using Composable.DependencyInjection;
using Composable.Messaging.Buses;

namespace Nilsark.Frontend.Server
{
    public class Startup
    {
        private IEndpointHost _host;
        private IEndpoint _clientEndpoint;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            _host = EndpointHost.Production.Create(DependencyInjectionContainer.Create);
            _clientEndpoint = _host.RegisterClientEndpoint(builder =>
            {
                Consultants.Shared.TypeMapper.MapTypes(builder.TypeMapper);
            });

            _host.Start();
            services.AddScoped(_ =>
            {
                IRemoteApiNavigatorSession bus = null;
                _clientEndpoint.ServiceLocator.ExecuteInIsolatedScope(() => bus = _clientEndpoint.ServiceLocator.Resolve<IRemoteApiNavigatorSession>());
                return bus;
            });
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller}/{action}/{id?}");
            });

            app.UseBlazor<Client.Program>();
        }
    }
}
