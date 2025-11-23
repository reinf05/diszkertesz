using Microsoft.Identity.Client;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static object? ParentActivity { get; set; }

        private static readonly string[] Scopes = new string[]
        {
            "openid",
            "offline_access"
        };

        private IPublicClientApplication application;

        public AuthenticationService()
        {

            application = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority($"https://{TenantName}.ciamlogin.com/{TenantName}.onmicrosoft.com/{PolicySignInSignUp}")
                .WithRedirectUri($"msal{ClientId}://auth")
                .Build();

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
                    await Shell.Current.DisplayAlert("Ok", "ok", "ok");
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

            return authResult;
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
    }
}
