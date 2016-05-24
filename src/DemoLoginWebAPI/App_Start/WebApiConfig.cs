using DemoLoginWebAPI.Models;
using DemoLoginWebAPI.Sicurezza;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace DemoLoginWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            /* Solo a scopo dimostrativo definisco degli utenti qui nel codice */
            var utenti = new List<Utente>
            {
                new Utente { Email = "admin@example.com", IdUtente = 1, Username = "admin", Password = "password", Ruolo = "Amministratore" },
                new Utente { Email = "cliente@example.com", IdUtente = 2, Username = "cliente", Password = "password", Ruolo = "Cliente" }
            };

            //privilegiamo l'autenticazione basata su token
            config.MessageHandlers.Add(new TokenHandler());
            //...ma supportiamo anche la basic authentication
            config.MessageHandlers.Add(new BasicAuthenticationHandler(utenti.AsQueryable()));
            

        }
    }
}
