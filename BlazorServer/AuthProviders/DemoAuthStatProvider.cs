using BlazorServer.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazorServer.AuthProviders
{
    public class DemoAuthStatProvider : AuthenticationStateProvider
    {
        /*
         * On débute la démo avec un utilisateur anonyme, d'ou le constructeur sans param de ClaimsIdentity 
         * On y reviendra plus tard pour modifier le type d'authentification
         */

        public IJSRuntime _js { get; set; }
        public DemoAuthStatProvider(IJSRuntime js)
        {
            _js = js;
        }

        public string token { get; set; }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            List<Claim> claims = new List<Claim>();
            //Test.token = await _js.InvokeAsync<string>("localStorage.getItem", "token");

            if (string.IsNullOrWhiteSpace(token))
            {
                ClaimsIdentity anonymousUser = new ClaimsIdentity();
                return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymousUser)));
            }

            JwtSecurityToken jwt = new JwtSecurityToken(token);


            foreach (Claim claim in jwt.Claims)
            {
                claims.Add(claim);
            }

            int timestamp = int.Parse(jwt.Claims.FirstOrDefault(x => x.Type == "exp").Value);
            DateTime expireDate = new DateTime(1970, 1, 1).AddSeconds(timestamp);

            if (expireDate < DateTime.Now)
            {
                ClaimsIdentity anonymousUser = new ClaimsIdentity();
                return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymousUser)));
            }

            ClaimsIdentity currentUser = new ClaimsIdentity(claims, "TestAuthSystem");

            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(currentUser))).Result;

        }

        public void NotifyUserChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
