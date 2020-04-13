using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MappingImageSampleML.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace MappingImageSampleAPI
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
            services.AddSingleton<PredictionEngine<ModelInput, ModelOutput>>(sp =>
            {
                MLContext ctx = new MLContext();

                // Register NormalizeMapping
                ctx.ComponentCatalog.RegisterAssembly(typeof(NormalizeMapping).Assembly);

                // Register LabelMapping
                ctx.ComponentCatalog.RegisterAssembly(typeof(LabelMapping).Assembly);

                var modelPath = Path.Join("bin/Debug/netcoreapp3.1/","MLModel.zip");

                ITransformer mlModel = ctx.Model.Load(modelPath, out var modelInputSchema);
                var predEngine = ctx.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

                return predEngine;
            });
            
            services.AddControllers();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
