using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wizdom.Client;

namespace Wizdom.Client.Extensions
{
    public class DeviceCodeTokenHandler : ITokenHandler
    {
        private IPublicClientApplication _authContext;
        public IPublicClientApplication AuthContext
        {
            get
            {
                if (_authContext == null && !string.IsNullOrEmpty(clientId)) _authContext = PublicClientApplicationBuilder
                    .Create(clientId) //Note - a bit messy - but clientid must always be set, before accessing this property...
                    .WithAuthority(Authority)
                    .WithDefaultRedirectUri()
                    .Build();
                return _authContext;
            }
        }

        public DeviceCodeTokenHandler()
        {
            //TODO: get output as delegate and send all communication through that instead of console.writeline
        }

        private string clientId;
        private const string Authority = "https://login.microsoftonline.com/common";

        public async Task<string> GetTokenAsync(string clientId, string resourceId = null)
        {
            this.clientId = clientId;

            if (AuthContext == null) return null;

            var accounts = await AuthContext?.GetAccountsAsync();

            //        new KeyValuePair<string, string>("scope", resourceId != null ? resourceId + "/.default offline_access" : clientId + "/.default offline_access") //Ensure resourceid ends with / and add another for the .default scope so it ends up being //
            string[] scopes = new string[] { "offline_access", resourceId != null ? resourceId + "/.default" : clientId + "/.default" };

            // All AcquireToken* methods store the tokens in the cache, so check the cache first
            try
            {
                var tokenResult = await AuthContext?.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
                return tokenResult.AccessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                // No token found in the cache or AAD insists that a form interactive auth is required (e.g. the tenant admin turned on MFA)
                // If you want to provide a more complex user experience, check out ex.Classification 

                var tokenResult = await AcquireByDeviceCodeAsync(AuthContext, scopes);
                return tokenResult.AccessToken;
            }
        }


        private async Task<AuthenticationResult> AcquireByDeviceCodeAsync(IPublicClientApplication pca, string[] scopes)
        {
            try
            {
                var result = await pca?.AcquireTokenWithDeviceCode(scopes, deviceCodeResult =>
                {
                    // This will print the message on the console which tells the user where to go sign-in using 
                    // a separate browser and the code to enter once they sign in.
                    // The AcquireTokenWithDeviceCode() method will poll the server after firing this
                    // device code callback to look for the successful login of the user via that browser.
                    // This background polling (whose interval and timeout data is also provided as fields in the 
                    // deviceCodeCallback class) will occur until:
                    // * The user has successfully logged in via browser and entered the proper code
                    // * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
                    // * The developing application calls the Cancel() method on a CancellationToken sent into the method.
                    //   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).
                    Console.WriteLine(deviceCodeResult.Message);
                    return Task.FromResult(0);
                }).ExecuteAsync();

                //Console.WriteLine(result.Account.Username);
                return result;
            }
            // TODO: handle or throw all these exceptions
            catch (MsalServiceException ex)
            {
                // Kind of errors you could have (in ex.Message)

                // AADSTS50059: No tenant-identifying information found in either the request or implied by any provided credentials.
                // Mitigation: as explained in the message from Azure AD, the authoriy needs to be tenanted. you have probably created
                // your public client application with the following authorities:
                // https://login.microsoftonline.com/common or https://login.microsoftonline.com/organizations

                // AADSTS90133: Device Code flow is not supported under /common or /consumers endpoint.
                // Mitigation: as explained in the message from Azure AD, the authority needs to be tenanted

                // AADSTS90002: Tenant <tenantId or domain you used in the authority> not found. This may happen if there are 
                // no active subscriptions for the tenant. Check with your subscription administrator.
                // Mitigation: if you have an active subscription for the tenant this might be that you have a typo in the 
                // tenantId (GUID) or tenant domain name.
            }
            catch (OperationCanceledException ex)
            {
                // If you use a CancellationToken, and call the Cancel() method on it, then this *may* be triggered
                // to indicate that the operation was cancelled. 
                // See https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads 
                // for more detailed information on how C# supports cancellation in managed threads.
            }
            catch (MsalClientException ex)
            {
                // Possible cause - verification code expired before contacting the server
                // This exception will occur if the user does not manage to sign-in before a time out (15 mins) and the
                // call to `AcquireTokenWithDeviceCode` is not cancelled in between
            }
            return null;
        }

        public async Task LogOutAsync()
        {
            if (AuthContext == null) return;

            var accounts = await AuthContext?.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await AuthContext?.RemoveAsync(account);
            }
        }

        //public async Task<string> GetToken(string clientId, string resourceId = null)
        //{
        //    var OAuthServiceUrl = $"https://login.microsoftonline.com/common/oauth2/v2.0/devicecode";
        //    HttpContent content = new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string, string>("client_id", clientId),
        //        new KeyValuePair<string, string>("scope", resourceId != null ? resourceId + "/.default offline_access" : clientId + "/.default offline_access") //Ensure resourceid ends with / and add another for the .default scope so it ends up being //
        //    });

        //    HttpClient httpClient = new HttpClient();
        //    HttpResponseMessage response = await httpClient.PostAsync(OAuthServiceUrl, content);
        //    //TODO: Error handling - check response code!
        //    CodeResponse codeResponse = JsonConvert.DeserializeObject<CodeResponse>(await response.Content.ReadAsStringAsync());
        //    Console.WriteLine($"Please open {codeResponse.verification_uri} in a browser and type {codeResponse.user_code} and then proceed to login");


        //    var token = await PollForToken(httpClient, clientId, codeResponse.device_code);

        //    return token.access_token;
        //}

        //private static async Task<TokenResponse> PollForToken(HttpClient httpClient, string clientId, string deviceCode)
        //{
        //    while (true)
        //    {
        //        string pollUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
        //        HttpContent pollContent = new FormUrlEncodedContent(new[]
        //        {
        //            new KeyValuePair<string, string>("tenant", "common"),
        //            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code"),
        //            new KeyValuePair<string, string>("client_id", clientId),
        //            new KeyValuePair<string, string>("device_code", deviceCode)
        //        });

        //        var response = await httpClient.PostAsync(pollUrl, pollContent);
        //        //Console.WriteLine(response.StatusCode);
        //        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        //        {
        //            PollResponse pollResponse = JsonConvert.DeserializeObject<PollResponse>(await response.Content.ReadAsStringAsync());
        //        }
        //        else
        //        {
        //            TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
        //            return tokenResponse;
        //        }

        //        Thread.Sleep(2000); //Wait a bit before polling again...
        //        //TODO: Check codeResponse. time valid thing - then break if time passed...
        //    }
        //}

        //public void ClearCache()
        //{
        //    //TODO
        //}

        //public class TokenResponse
        //{
        //    public string token_type { get; set; }
        //    public string scope { get; set; }
        //    public int expires_in { get; set; }
        //    public string access_token { get; set; }
        //    public string refresh_token { get; set; }
        //    public string id_token { get; set; }
        //}


        //public class PollResponse
        //{
        //    public string error { get; set; }
        //    public string error_description { get; set; }
        //    public int[] error_codes { get; set; }
        //    public string timestamp { get; set; }
        //    public string trace_id { get; set; }
        //    public string correlation_id { get; set; }
        //    public string error_uri { get; set; }
        //}


        //public class CodeResponse
        //{
        //    public string user_code { get; set; }
        //    public string device_code { get; set; }
        //    public string verification_uri { get; set; }
        //    public int expires_in { get; set; }
        //    public int interval { get; set; }
        //    public string message { get; set; }
        //}



    }
}
