using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Nancy.Owin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Owin;

namespace TestMetaServer
{
    public class Startup
    {
        private readonly IConfiguration config;
        
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                              .SetBasePath(env.ContentRootPath);

            config = builder.Build();
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(x => x.UseNancy(opt => opt.Bootstrapper = new CustomBoostrapper()));
        }
    }
}
