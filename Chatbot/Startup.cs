// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using Chatbot.Bots;
using Chatbot.Dialogs;
using Chatbot.Interfaces;
using Chatbot.Models;
using Chatbot.Recognizers;
using Chatbot.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Chatbot
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            QueryFactory queryFactory = new QueryFactory(new SqlConnection(Configuration.GetConnectionString("ApplicationDatabase")), new SqlServerCompiler());
            Query query = new Query()
                .Select("TABLE_NAME", "COLUMN_NAME", "DATA_TYPE")
                .From("INFORMATION_SCHEMA.COLUMNS")
                .Where("COLUMN_NAME", "!=", "id");

            var xQuery = queryFactory.FromQuery(query);
            var result = xQuery.Get();

            foreach (var row in result)
            {

                string tableName = "";
                string columnName = "";
                string columnType = "";

                var properties = (IDictionary<string, object>)row;

                foreach (var property in properties)
                {
                    if (property.Key == "TABLE_NAME") tableName = property.Value.ToString();
                    else if (property.Key == "COLUMN_NAME") columnName = property.Value.ToString();
                    else if (property.Key == "DATA_TYPE") columnType = property.Value.ToString();
                }

                ColumnTypeContainer.Instance.AddColumn(tableName,columnName,columnType);
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Register LUIS recognizers
            services.AddSingleton<SimpleStatementRecognizer>();
            services.AddSingleton<ComplexStatementRecognizer>();

            // Register dialogs
            services.AddSingleton<SimpleParsingDialog>();
            services.AddSingleton<ComplexParsingDialog>();
            services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();

            // Register QueryHandlers
            services.AddSingleton<ISimpleQueryHandler, SimpleQueryHandler>();
            services.AddSingleton<IComplexQueryHandler, ComplexQueryHandler>();

            // Add SqlKata Execution
            services.AddSingleton(factory =>
            {
                var connection = new SqlConnection(Configuration.GetConnectionString("ApplicationDatabase"));
                var compiler = new SqlServerCompiler();
                return new QueryFactory(connection, compiler);
            });

            //services.AddSingleton<ColumnTypeContainer>();
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
    }
}