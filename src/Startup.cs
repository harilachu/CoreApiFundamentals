using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CoreCodeCamp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CampContext>();
            services.AddScoped<ICampRepository, CampRepository>();

            //Adding automapper
            services.AddAutoMapper(typeof(Startup));
            //API versioning
            services.AddApiVersioning(
                opt =>
                {
                    opt.AssumeDefaultVersionWhenUnspecified = true;
                    opt.DefaultApiVersion = new ApiVersion(1, 0); //default version 
                    opt.ReportApiVersions = true;
                    //opt.ApiVersionReader = new UrlSegmentApiVersionReader();
                    //opt.ApiVersionReader = new QueryStringApiVersionReader("ver"); //?ver=1.1
                    //opt.ApiVersionReader = new HeaderApiVersionReader("X-Version"); //from header X-Version=1.1
                }
            );

            services.AddMvc(opt => opt.EnableEndpointRouting = false) //to enable api versioning
              .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
