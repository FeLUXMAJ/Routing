﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Dispatcher;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Dispatcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DispatcherSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDispatcher();

            // This is a temporary layering issue, don't worry about :)
            services.AddRouting();
            services.AddSingleton<RouteTemplateUrlGenerator>();
            services.AddSingleton<IDefaultDispatcherFactory, TreeDispatcherFactory>();

            // Imagine this was done by MVC or another framework.
            services.AddSingleton<DispatcherDataSource>(ConfigureDispatcher());
            services.AddSingleton<EndpointSelector, TemplateEndpointSelector>();
            services.AddSingleton<EndpointSelector, HttpMethodEndpointSelector>();

        }

        public DefaultDispatcherDataSource ConfigureDispatcher()
        {
            return new DefaultDispatcherDataSource()
            {
                Addresses =
                {
                    new TemplateAddress("{controller=Home}/{action=Index}/{id?}", new { controller = "Home", action = "Index", }, "Home:Index()"),
                    new TemplateAddress("{controller=Home}/{action=Index}/{id?}", new { controller = "Home", action = "About", }, "Home:About()"),
                    new TemplateAddress("{controller=Home}/{action=Index}/{id?}", new { controller = "Admin", action = "Index", }, "Admin:Index()"),
                    new TemplateAddress("{controller=Home}/{action=Index}/{id?}", new { controller = "Admin", action = "Users", }, "Admin:GetUsers()/Admin:EditUsers()"),
                },
                Endpoints =
                {
                    new TemplateEndpoint("{controller=Home}/{action=Index}/{id?}", new { controller = "Home", action = "Index", }, Home_Index, "Home:Index()"),
                    new TemplateEndpoint("{controller=Home}/{action=Index}/{id?}", new { controller = "Home", action = "About", }, Home_About, "Home:About()"),
                    new TemplateEndpoint("{controller=Home}/{action=Index}/{id?}", new { controller = "Admin", action = "Index", }, Admin_Index, "Admin:Index()"),
                    new TemplateEndpoint("{controller=Home}/{action=Index}/{id?}", new { controller = "Admin", action = "Users", }, "GET", Admin_GetUsers, "Admin:GetUsers()", new AuthorizationPolicyMetadata("Admin")),
                    new TemplateEndpoint("{controller=Home}/{action=Index}/{id?}", new { controller = "Admin", action = "Users", }, "POST", Admin_EditUsers, "Admin:EditUsers()", new AuthorizationPolicyMetadata("Admin")),
                },
            };
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            app.UseDispatcher();

            app.Use(async (context, next) =>
            {
                logger.LogInformation("Executing fake CORS middleware");

                var feature = context.Features.Get<IDispatcherFeature>();
                var policy = feature.Endpoint?.Metadata.OfType<ICorsPolicyMetadata>().LastOrDefault();
                logger.LogInformation("using CORS policy {PolicyName}", policy?.Name ?? "default");

                await next.Invoke();
            });

            app.Use(async (context, next) =>
            {
                logger.LogInformation("Executing fake AuthZ middleware");

                var feature = context.Features.Get<IDispatcherFeature>();
                var policy = feature.Endpoint?.Metadata.OfType<IAuthorizationPolicyMetadata>().LastOrDefault();
                if (policy != null)
                {
                    logger.LogInformation("using Auth policy {PolicyName}", policy.Name);
                }

                await next.Invoke();
            });
        }

        public static Task Home_Index(HttpContext httpContext)
        {
            var url = httpContext.RequestServices.GetService<RouteTemplateUrlGenerator>();
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<h1>Some links you can visit</h1>" +
                $"<p><a href=\"{url.GenerateUrl(httpContext, new { controller = "Home", action = "Index", })}\">Home:Index()</a></p>" +
                $"<p><a href=\"{url.GenerateUrl(httpContext, new { controller = "Home", action = "About", })}\">Home:About()</a></p>" +
                $"<p><a href=\"{url.GenerateUrl(httpContext, new { controller = "Admin", action = "Index", })}\">Admin:Index()</a></p>" +
                $"<p><a href=\"{url.GenerateUrl(httpContext, new { controller = "Admin", action = "Users", })}\">Admin:GetUsers()/Admin:EditUsers()</a></p>" +
                $"</body>" +
                $"</html>");
        }

        public static Task Home_About(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<p>This is a dispatcher sample.</p>" +
                $"</body>" +
                $"</html>");
        }

        public static Task Admin_Index(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<p>This is the admin page.</p>" +
                $"</body>" +
                $"</html>");
        }

        public static Task Admin_GetUsers(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<p>Users: rynowak, jbagga</p>" +
                $"</body>" +
                $"</html>");
        }

        public static Task Admin_EditUsers(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<p>blerp</p>" +
                $"</body>" +
                $"</html>");
        }
    }
}