using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace PP.Signicat.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.Routes.MapHttpRoute("ActionApi",
              "api/{controller}/{action}",
              null,
              new { action = @"[a-zA-Z]+" });

            config.Routes.MapHttpRoute("WithActionApi",
              "api/{controller}/{id}/{action}");

            config.Routes.MapHttpRoute("DefaultApi",
              "api/{controller}/{id}",
              new { id = RouteParameter.Optional, action = "Default" },
              new { id = @"\d*" });
        }
    }
}
