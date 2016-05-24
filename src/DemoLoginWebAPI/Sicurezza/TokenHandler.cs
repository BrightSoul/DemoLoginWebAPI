using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoLoginWebAPI.Sicurezza
{
    public class TokenHandler : DelegatingHandler
    {
        private const string TokenScheme = "Token";
        private static byte[] symmetricKey = Guid.Parse(ConfigurationManager.AppSettings["ChiaveSimmetrica"]).ToByteArray();
        private static TimeSpan durataToken = TimeSpan.Parse(ConfigurationManager.AppSettings["DurataToken"]);
        private static TimeSpan clockSkew = TimeSpan.Parse(ConfigurationManager.AppSettings["ClockSkew"]);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            IPrincipal principal = request.GetRequestContext().Principal;
            var authorization = request.Headers.Authorization;
            if (!principal.Identity.IsAuthenticated && authorization != null && authorization.Scheme == TokenScheme && !string.IsNullOrEmpty(authorization.Parameter))
            {
                principal = ParsaToken(authorization.Parameter);
                if (principal != null)
                    request.GetRequestContext().Principal = principal;

            }

            return base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    var response = task.Result;
                        //Altrimenti, se lo status della response è stato impostato su Unauthorized invito l'utente a loggarsi
                        principal = request.GetRequestContext().Principal;
                        //if (principal.Identity.IsAuthenticated && authorization != null && authorization.Parameter != TokenScheme)
                        if (principal.Identity.IsAuthenticated)
                        EmettiToken(response, principal, scadenza: durataToken);
                    return response;
                });
        }

        private void EmettiToken(HttpResponseMessage response, IPrincipal principal, TimeSpan scadenza)
        {
            ClaimsIdentity identità;
            if (principal.Identity is ClaimsIdentity)
            {
                identità = principal.Identity as ClaimsIdentity;
            } else
            {
                identità = new ClaimsIdentity();
                identità.AddClaim(new Claim(ClaimTypes.Name, principal.Identity.Name));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identità,
                TokenIssuerName = "self",
                Lifetime = new Lifetime(DateTime.Now, DateTime.Now.Add(scadenza)),
                SigningCredentials = new SigningCredentials(
                        new InMemorySymmetricSecurityKey(symmetricKey),
                        "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                        "http://www.w3.org/2001/04/xmlenc#sha256"),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encodedToken = tokenHandler.WriteToken(token);
            response.Headers.Add("Server-Authorization", $"Token {encodedToken}");
        }

        public IPrincipal ParsaToken(string encodedToken)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new InMemorySymmetricSecurityKey(symmetricKey),
                    ValidIssuer = "self",
                    ClockSkew = clockSkew
                };
            SecurityToken token;
            try
            {
                
                var principal = tokenHandler.ValidateToken(encodedToken, validationParameters, out token);
                return principal;
            }
            catch
            {
                return null;
            }
            
        }
    }
}
