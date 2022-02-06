// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.0

using RetailBot.Bots;
using RetailBot.Dialogs;
using RetailBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Orchestrator;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace RetailBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            OrchestratorConfig = configuration.GetSection("Orchestrator").Get<OrchestratorConfig>();
        }

        public OrchestratorConfig OrchestratorConfig { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton(InitializeOrchestrator());

            // Configure State
            ConfigureState(services);

            // Configure Dialogs
            ConfigureDialogs(services);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogBot<MainDialog>>();
        }

        public void ConfigureDialogs(IServiceCollection services)
        {
            services.AddSingleton<MainDialog>();
        }

        public void ConfigureState(IServiceCollection services)
        {
            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.) 
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. 
            services.AddSingleton<UserState>();

            // Create the Conversation state. 
            services.AddSingleton<ConversationState>();

            // Create an instance of the state service 
            services.AddSingleton<StateService>();

            // Configure Services
            services.AddSingleton<BotServices>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }

        private OrchestratorRecognizer InitializeOrchestrator()
        {
            var modelFolder = Path.GetFullPath(OrchestratorConfig.ModelFolder);
            var snapshotFile = Path.GetFullPath(OrchestratorConfig.SnapshotFile);
            var orc = new OrchestratorRecognizer()
            {
                ModelFolder = modelFolder,
                SnapshotFile = snapshotFile
            };
            return orc;
        }
    }
}
