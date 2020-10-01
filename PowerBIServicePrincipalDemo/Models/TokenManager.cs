using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Security;
using System.Reflection;
using Microsoft.Rest;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;

namespace PowerBIServicePrincipalDemo.Models {


  class TokenManager {

    public const string urlPowerBiServiceApiRoot = "https://api.powerbi.com/";
    private const string tenantCommonAuthority = "https://login.microsoftonline.com/organizations";

    private static string confidentialApplicationId = ConfigurationManager.AppSettings["confidential-application-id"];
    private static string confidentialApplicationSecret = ConfigurationManager.AppSettings["confidential-application-secret"];
    private static string tenantName = ConfigurationManager.AppSettings["tenant-name"];
    private readonly static string tenantSpecificAuthority = "https://login.microsoftonline.com/" + tenantName;

    static class TokenCacheHelper {

      private static readonly string CacheFilePath = Assembly.GetExecutingAssembly().Location + ".tokencache.json";
      private static readonly object FileLock = new object();

      public static void EnableSerialization(ITokenCache tokenCache) {
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
      }

      private static void BeforeAccessNotification(TokenCacheNotificationArgs args) {
        lock (FileLock) {
          // repopulate token cache from persisted store
          args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath) ? File.ReadAllBytes(CacheFilePath) : null);
        }
      }

      private static void AfterAccessNotification(TokenCacheNotificationArgs args) {
        // if the access operation resulted in a cache update
        if (args.HasStateChanged) {
          lock (FileLock) {
            // write token cache changes to persistent store
            File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
          }
        }
      }
    }

    public static string GetAppOnlyAccessToken() {

      var appConfidential = ConfidentialClientApplicationBuilder.Create(confidentialApplicationId)
                              .WithClientSecret(confidentialApplicationSecret)
                              .WithAuthority(tenantSpecificAuthority)
                              .Build();

      TokenCacheHelper.EnableSerialization(appConfidential.UserTokenCache);


      string[] scopesDefault = new string[] { "https://analysis.windows.net/powerbi/api/.default" };

      var authResult = appConfidential.AcquireTokenForClient(scopesDefault).ExecuteAsync().Result;
      return authResult.AccessToken;
    }

    public static PowerBIClient GetPowerBiAppOnlyClient() {
      var tokenCredentials = new TokenCredentials(GetAppOnlyAccessToken(), "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
    }

  }



}
