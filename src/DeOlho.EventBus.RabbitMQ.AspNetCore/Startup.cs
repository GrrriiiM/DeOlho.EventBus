using System;
using System.Threading.Tasks;
using DeOlho.EventBus.MediatR;
using DeOlho.EventBus.Message;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using DeOlho.EventBus.RabbitMQ.DependencyInjection;

namespace DeOlho.EventBus.RabbitMQ.AspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEventBusRabbitMQ(c => 
            {
                c.HostName = "localhost";
                c.Port = 11002;
                c.UserName = "deolho";
                c.Password = "deolho";
                c.SubscribeMediatorConsumers(this.GetType().Assembly);
            });

            services.AddMediatR(this.GetType());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "DeOlho EventBus", Version = "v1" });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public class TestConsummerHandler : EventBusConsumerHandler<MessageTest>
        {
            public override Task<Unit> Handle(MessageTest message, System.Threading.CancellationToken cancellationToken)
            {
                System.Diagnostics.Debug.WriteLine(message.Testando);
                throw new Exception();
                return Unit.Task;
            }
        }

        public class TestConsummerFailHandler : EventBusConsumerFailHandler<MessageTest>
        {
            public override Task<Unit> Handle(MessageTest message, string[] exceptionStack, System.Threading.CancellationToken cancellationToken)
            {
                System.Diagnostics.Debug.WriteLine(exceptionStack[0]);
                return Unit.Task;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeOlho EventBus API V1");
                c.RoutePrefix = string.Empty;
            });

            //app.UseHttpsRedirection();
            
            app.UseMvc();
        }
    }


    public class MessageTest : EventBusMessage
    {
        public MessageTest(string messageId) : base(messageId)
        {
        }

        public string Testando { get; set; }
    }
}
