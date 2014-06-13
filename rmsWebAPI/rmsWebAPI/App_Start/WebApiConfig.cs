﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace rmsWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "ProtectWithTemplateApiMethod",
                routeTemplate: "api/protect/{filePath}/{templateName}",
                defaults: new
                {
                    controller = "Values",
                    action = "Protect"
                }
            );

            config.Routes.MapHttpRoute(
                name: "ProtectWithLitOfRightsApiMethod",
                routeTemplate: "api/protect/{filePath}/{ownerEmail}/{listOfRights}",
                defaults: new
                {
                    controller = "Values",
                    action = "Protect"
                }
            );

            config.Routes.MapHttpRoute(
                name: "UnprotectApiMethod",
                routeTemplate: "api/unprotect/{filePath}",
                defaults: new
                {
                    controller = "Values",
                    action = "Unprotect"
                }
            );

            config.Routes.MapHttpRoute(
               name: "IsProtectedApiMethod",
               routeTemplate: "api/isprotected/{filePath}",
               defaults: new
               {
                   controller = "Values",
                   action = "IsProtected"
               }
           );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            config.EnableSystemDiagnosticsTracing();

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
