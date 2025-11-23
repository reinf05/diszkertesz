using diszkerteszClient.Models;
using Microsoft.Identity.Client;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Services
{
    public class AuthenticationService
    {
        private const string ClientId = "ee879ed8-b52e-4100-9d08-0a530ca01dab";
        private const string TenantName = "diszkerteszentra";
        private const string PolicySignInSignUp = "singin_signup";
        private object? ParentWindow;
        private const string BaseUrl = "https://ca-diszkertesz-gerwest-dev-001.politeocean-b59cb8a8.westeurope.azurecontainerapps.io/User/";
        private HttpClient httpClient;
        public static object? ParentActivity { get; set; }

        private static readonly string[] Scopes = new string[]
        {
            "openid",
            "offline_access",
            "api://0534bddb-0348-4be0-a028-dec801b97388/user_access"
        };

        private IPublicClientApplication application;

        public AuthenticationService()
        {
            httpClient = new();

            application = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority($"https://{TenantName}.ciamlogin.com/{TenantName}.onmicrosoft.com/{PolicySignInSignUp}")
                .WithRedirectUri($"msal{ClientId}://auth")
                .Build();

        }
        public async Task<bool> CreateUserAsync()
        {
            string URL = BaseUrl + "create-user";
            string accessToken = null;
            try
            {
                accessToken = await GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentUserList Failed: {ex.Message}");
                return false;
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.PostAsync(URL, new StringContent(""));

            if (response.IsSuccessStatusCode)
            {
                return true;

            }
            return false;
        }

        public async Task<AuthenticationResult?> SignInAsync()
        {
            AuthenticationResult? authResult = null;

            try
            {
                // Attempt to silently acquire a token (user is already logged in)
                IEnumerable<IAccount> accounts = await application.GetAccountsAsync();
                authResult = await application.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // A UI is required, so interactively acquire a token (first-time login or session expired)
                try
                {
                    //var parentWindow = GetParentWindow();
                    authResult = await application.AcquireTokenInteractive(Scopes)
                        .WithParentActivityOrWindow(ParentActivity)
                        .ExecuteAsync();
                }
                catch (MsalException ex)
                {
                    // Handle specific interactive login errors
                    System.Diagnostics.Debug.WriteLine($"Interactive Login Failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Silent Token Acquisition Failed: {ex.Message}");
            }

            if (authResult is not null)
            {
                bool createUser = await CreateUserAsync();

                if (!createUser)
                {
                    throw new Exception("Error in registering user in database");
                }
            }

            return authResult;
        }

        public async Task<AuthenticationResult?> SignUpAsync()
        {
            AuthenticationResult? signUpResult = null;
            try
            {
                // Interactively acquire a token for sign-up
                signUpResult = await application.AcquireTokenInteractive(Scopes)
                    .WithParentActivityOrWindow(ParentActivity)
                    .WithPrompt(Prompt.Create)
                    .ExecuteAsync();
            }
            catch (MsalException ex)
            {
                // Handle specific interactive login errors
                System.Diagnostics.Debug.WriteLine($"Interactive Sign-Up Failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sign-Up Failed: {ex.Message}");
            }

            if (signUpResult is not null)
            {
                bool createUser = await CreateUserAsync();

                if (!createUser)
                {
                    throw new Exception("Error in registering user in database");
                }
            }

            return signUpResult;
        }

        public async Task SignOutAsync()
        {
            try
            {
                IEnumerable<IAccount> accounts = await application.GetAccountsAsync();

                // Loop through all accounts and remove them from the cache
                while (accounts.Any())
                {
                    await application.RemoveAsync(accounts.FirstOrDefault());
                    accounts = await application.GetAccountsAsync(); // Re-fetch remaining accounts
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sign Out Failed: {ex.Message}");
            }
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            try
            {
                // Get the account that was logged in
                IEnumerable<IAccount> accounts = await application.GetAccountsAsync();
                IAccount? account = accounts.FirstOrDefault();

                if (account == null)
                {
                    // No user is logged in
                    return null;
                }

                // Silently refresh the access token if needed
                AuthenticationResult authResult = await application.AcquireTokenSilent(Scopes, account)
                    .ExecuteAsync();

                return authResult.AccessToken;
            }
            catch (MsalException ex)
            {
                // The token is expired and cannot be refreshed silently (e.g., refresh token is gone).
                // The calling code should catch this and force a new SignInAsync() call.
                System.Diagnostics.Debug.WriteLine($"Token Acquisition Silent Failed: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserItem>> GetCurrentUserList()
        {
            string URL = BaseUrl + "user-list";
            string accessToken = null;
            try
            {
                accessToken = await GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentUserList Failed: {ex.Message}");
            }

            if (accessToken is null)
            {
                await SignInAsync();
            }

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync(URL);

            if (response.IsSuccessStatusCode)
            {
                var userList = await response.Content.ReadFromJsonAsync<List<UserItem>>();
                if (userList is not null)
                {
                    return userList;
                }
                else 
                {
                    System.Diagnostics.Debug.WriteLine($"GetCurrentUserList Failed: {response.ReasonPhrase}");
                    throw new Exception("User list is null.");
                }
            }
            throw new Exception("Failed to retrieve user list.");
        }

        public async Task<bool> UploadItemAsync(UserItem userItem)
        {
            string URL = BaseUrl + "post-list";

            try
            {
                await GetHeaderAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UploadItemAsync Failed: {ex.Message}");
                return false;
            }

            string userItemJson = System.Text.Json.JsonSerializer.Serialize(userItem);

            var response = await httpClient.PostAsync(URL, new StringContent(userItemJson, System.Text.Encoding.UTF8, "application/json"));
            if(response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"UploadItemAsync Failed: {response.ReasonPhrase}");
                return false;
            }
        }

        private async Task GetHeaderAsync()
        {
            string accessToken = null;
            try
            {
                accessToken = await GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentUserList Failed: {ex.Message}");
            }

            if (accessToken is null)
            {
                await SignInAsync();
            }

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}
