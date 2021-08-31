using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireTry
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(x =>
            x.UsePostgreSqlStorage("User name = postgres; Password = 123; Host = localhost; Port = 5432; Database = hangfireTry; Pooling = true; Persist Security Info = true;"));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var queues = Enumerable.Range(1, 10).Select(i => $"default{i}");
            foreach (var queue in queues)
            {
                var options = new BackgroundJobServerOptions
                {
                    WorkerCount = 1,
                    Queues = new[] { queue }
                };
                app.UseHangfireServer(options);
            }

            var recurringJobIds = Enumerable.Range(1, 100).Select(i => $"DeleteOldNotifications{i}");
            foreach (var recurringJobId in recurringJobIds)
            {
                RecurringJob.AddOrUpdate(recurringJobId, () => Console.WriteLine(recurringJobId),
                    "*/10 * * * *");
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
