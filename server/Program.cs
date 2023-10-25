using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using System;
using SignalR;
namespace server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddControllers();
                        services.AddEndpointsApiExplorer();
                        services.AddSwaggerGen();
                        services.AddSignalR();
                    })
                    .Configure(app =>
                    {
                        var environment = app.ApplicationServices.GetService<IWebHostEnvironment>();
                        
                        if (environment.IsDevelopment())
                        {
                            app.UseSwagger();
                            app.UseSwaggerUI();
                        }
                        app.UseRouting();
                        app.UseHttpsRedirection();
                        app.UseCors(x =>
                            x.AllowAnyHeader()
                             .AllowAnyMethod()
                             .AllowCredentials()
                             .WithOrigins("https://localhost:4200", "https://localhost:7034", "http://localhost:3000", "https://chomaimai.onlinesignpost.com"));

                        app.UseAuthorization();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapHub<ChatHub>("/chathub");
                        });
                    });
                })
                .Build();

            host.Run();
        }
    }
}
