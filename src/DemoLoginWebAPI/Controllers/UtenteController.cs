using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace DemoLoginWebAPI.Controllers
{
    //Che si sia autenticato con la basic authentication (fornendo username e password)
    //o attraverso il token non mi importa. L'importante è che questo controller sia
    //protetto dall'accesso degli utenti anonimi grazie all'attributo [Authorize]
    [Authorize]
    public class UtenteController : ApiController
    {
        public IHttpActionResult Get()
        {
            

            return Ok("Accesso consentito");
        }
    }
}
