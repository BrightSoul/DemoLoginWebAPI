ASP.NET Web API 2 Basic Auth & JWT Token Demo
---------------------------------------------

Questo progetto dimostrativo supporta due meccanismi di autenticazione contemporaneamente:
 * Basic Authentication
 * Token JWT
 
 Entrambi sono stati implementati come due DelegatingHandlers
 (cioè elementi della pipeline di ASP.NET Web API 2) il cui codice si trova nella
 sottodirectory "Sicurezza" del progetto.
 
 Funzionamento
 =============
 Il client è libero di inviare una richiesta ad un ApiController qualsiasi 
 fornendo username e password per autenticarsi con la Basic Authentication.
 Se le credenziali erano corrette, ASP.NET Web API 2 restituirà la risposta e verrà inoltre
 emesso un token nell'intestazione di risposta Server-Authentication che il client
 potrà raccogliere ed usare nelle successive richieste.
 
 Il token emesso in questo modo può avere una scadenza molto breve (es. 5 minuti) e verrà
 rinnovato ad ogni richiesta del client. Questo comportamento è disattivabile commentando
 la riga 44 di TokenHandler.cs.
 
 ![](auth.png)

 
 Token JWT
 =========
 Questo progetto usa il pacchetto NuGet [System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/4.0.2.206221351)
 per emettere e validare i Token.
 Un [Token JWT](https://jwt.io/) è sostanzialmente una stringa cifrati con un algoritmo simmetrico
 che contiene i *claims* dell'utente che si era autenticato in precedenza con la Basic Authentication.
 Tali token, proprio perché sono cifrati con una chiave segreta, nota solo al server, non
 possono essere manipolati dal client. Altrimenti, nel momento in cui vengono forniti al 
 server, la loro validazione fallirebbe.
 
 Dopo che il server ha validato e decifrato il contenuto del Token, potrà estrarre da esso
 il nome utente, il suo id, i suoi ruoli o qualsiasi altra informazione avremo scelto
 di inserirvi all'atto dell'emissione. E' ovvio che, più informazioni inseriamo, più
 pesante risulterà il token.