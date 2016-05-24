using DemoLoginWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoLoginWebAPI.Sicurezza
{
    public class BasicAuthenticationHandler : DelegatingHandler
    {
        private const string WWWAuthenticateHeader = "WWW-Authenticate";
        private const string BasicScheme = "Basic";
        private static bool inviaChallenge = bool.Parse(ConfigurationManager.AppSettings["InviaChallenge"]);

        private IQueryable<Utente> utenti;
        public BasicAuthenticationHandler(IQueryable<Utente> utenti)
        {
            this.utenti = utenti;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                               CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization != null && request.Headers.Authorization.Scheme == BasicScheme)
            {

                var credenziali = ParsaIntestazioneAuthorization(request.Headers.Authorization.Parameter);
                var utente = TrovaUtente(credenziali);

                if (utente != null)
                {
                    var identità = new ClaimsIdentity(BasicScheme);
                    identità.AddClaim(new Claim("id", utente.IdUtente.ToString()));
                    identità.AddClaim(new Claim(ClaimTypes.Name, utente.Username));
                    identità.AddClaim(new Claim(ClaimTypes.Role, utente.Ruolo));

                    var principal = new ClaimsPrincipal(identità);

                    //Imposto l'identità per la richiesta corrente
                    request.GetRequestContext().Principal = principal;
                    return base.SendAsync(request, cancellationToken);
                }
            }

            return base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    var response = task.Result;
                    //Altrimenti, se lo status della response è stato impostato su Unauthorized invito l'utente a loggarsi
                    if (response.StatusCode == HttpStatusCode.Unauthorized && inviaChallenge)
                        InviaChallenge(request, response);

                    return response;
                });
        }

        private NetworkCredential ParsaIntestazioneAuthorization(string credenziali)
        {

            if (string.IsNullOrEmpty(credenziali))
                return null;

            const string separatoreUsernamePassword = ":";
            //Decode da Base64
            credenziali = Encoding.Default.GetString(Convert.FromBase64String(credenziali));

            if (!credenziali.Contains(separatoreUsernamePassword))
                return null;

            var posizioneSeparatore = credenziali.IndexOf(separatoreUsernamePassword, StringComparison.Ordinal);
            return new NetworkCredential(
                userName: credenziali.Substring(0, posizioneSeparatore),
                password: credenziali.Substring(posizioneSeparatore+1)
                );

        }

        private Utente TrovaUtente(NetworkCredential credenziali)
        {
            return utenti.SingleOrDefault(utente => utente.Username == credenziali.UserName && utente.Password == credenziali.Password);
        }



        /// <summary>
        /// Send the Authentication Challenge request
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionContext"></param>
        void InviaChallenge(HttpRequestMessage request, HttpResponseMessage response)
        {
            var host = request.RequestUri.DnsSafeHost;
            response.Headers.Add(WWWAuthenticateHeader, string.Format("Basic realm=\"{0}\"", host));
        }

    }
}
