using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Microsoft.Identity.Client;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Graph;

namespace MicrosoftGraph
{
    public class MicrosoftGraph
    {
        public static IPublicClientApplication PublicClientApp;
        const string ClientId = "d233a10f-bb53-4389-b3d6-367d41affc8d";
        const string Tenant = "consumers";
        string[] scopes = new string[] { "user.read", "files.readwrite.all" };
        public string accessToken { get; private set; }

        public MicrosoftGraph()
        {
            PublicClientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithRedirectUri("http://localhost")
                .WithAuthority(AzureCloudInstance.AzurePublic, Tenant)
                .Build();
            //tokenキャッシュ有効化
            TokenCacheHelper.EnableSerialization(PublicClientApp.UserTokenCache);
        }
        public async Task<bool> SilentAuth()
        {
            AuthenticationResult authResult = null;
            IAccount firstAccount;
            var app = PublicClientApp;
            var accounts = await app.GetAccountsAsync();
            firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await app.AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex) { return false; }
            accessToken = authResult.AccessToken;
            return true;
        }
        public async Task<bool> Auth(System.Windows.Window window)
        {
            AuthenticationResult authResult = null;
            IAccount firstAccount;
            var app = PublicClientApp;
            //firstAccount = PublicClientApplication.OperatingSystemAccount;
            var accounts = await app.GetAccountsAsync();
            firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await app.AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await app.AcquireTokenInteractive(scopes)
                        .WithAccount(firstAccount)
                        .WithParentActivityOrWindow(new WindowInteropHelper(window).Handle) // optional, used to center the browser on the window
                        .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                    //return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                //return false;
            }

            if (authResult != null)
            {
                //ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
                //DisplayBasicTokenInfo(authResult);
                //this.SignOutButton.Visibility = Visibility.Visible;
                accessToken = authResult.AccessToken;
                return true;
            }

            return false;
        }

        public async Task<GraphServiceClient> GetClient()
        {
            if (!await SilentAuth()) return null;
            DelegateAuthenticationProvider prov = new DelegateAuthenticationProvider(
                (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
                        return Task.FromResult(0);
                    }
                );
            return new GraphServiceClient(prov);

        }

        public async Task<bool> Logout()
        {
            var accounts = await PublicClientApp.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
                }
                catch (MsalException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error signing-out user: {ex.Message}");
                    return false;
                }
            }
            return true;
        }


    }

    static class TokenCacheHelper
    {
        static TokenCacheHelper()
        {
            /*try
            {
                // For packaged desktop apps (MSIX packages, also called desktop bridge) the executing assembly folder is read-only. 
                // In that case we need to use Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path + "\msalcache.bin" 
                // which is a per-app read/write folder for packaged apps.
                // See https://docs.microsoft.com/windows/msix/desktop/desktop-to-uwp-behind-the-scenes
                CacheFilePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path, ".msalcache.bin3");
            }
            catch (System.InvalidOperationException)
            {
                // Fall back for an unpackaged desktop app
                CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";
            }*/
            CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";
        }

        /// <summary>
        /// Path to the token cache
        /// </summary>
        public static string CacheFilePath { get; private set; }

        private static readonly object FileLock = new object();

        public static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(System.IO.File.Exists(CacheFilePath)
                        ? ProtectedData.Unprotect(System.IO.File.ReadAllBytes(CacheFilePath),
                                                 null,
                                                 DataProtectionScope.CurrentUser)
                        : null);
            }
        }

        public static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changesgs in the persistent store
                    System.IO.File.WriteAllBytes(CacheFilePath,
                                       ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                                                             null,
                                                             DataProtectionScope.CurrentUser)
                                      );
                }
            }
        }

        internal static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }
    }
}
