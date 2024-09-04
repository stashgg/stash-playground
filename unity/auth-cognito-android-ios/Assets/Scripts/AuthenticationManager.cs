using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Random = UnityEngine.Random;


public class AuthenticationManager : MonoBehaviour
{
   public static string CachePath;
   
   // In production, should probably keep these in a config file
   private const string AppClientID = "1cvcpanmc6nhash0q0hed6lnvh"; // App client ID, found under App Client Settings
   private const string AuthCognitoDomainPrefix = "drive2survive"; // Found under App Integration -> Domain Name. Changing this means it must be updated in all linked Social providers redirect and javascript origins
   private const string RedirectUrl = "http://localhost";
   private const string Region = "eu-north-1"; // Update with the AWS Region that contains your services

   private const string AuthCodeGrantType = "authorization_code";
   private const string RefreshTokenGrantType = "refresh_token";
   private const string CognitoAuthUrl = ".auth." + Region + ".amazoncognito.com";
   private const string TokenEndpointPath = "/oauth2/token";

   private static string _userid = "";
   
   private static string _codeChallenge;
   private static string _codeVerifier;
   private static string _state;

   public async Task<bool> ExchangeAuthCodeForAccessToken(string rawUrlWithGrantCode)
   {
      // Debug.Log("rawUrlWithGrantCode: " + rawUrlWithGrantCode);

      // raw url looks like https://somedomain.com/?code=c91d8bf4-1cb6-46e5-b43a-8def466f3c55
      string allQueryParams = rawUrlWithGrantCode.Split('?')[1];

      // it's likely there won't be more than one param
      string[] paramsSplit = allQueryParams.Split('&');

      foreach (string param in paramsSplit)
      {
         // Debug.Log("param: " + param);

         // find the code parameter and its value
         if (param.StartsWith("code"))
         {
            string grantCode = param.Split('=')[1];
            string grantCodeCleaned = grantCode.removeAllNonAlphanumericCharsExceptDashes(); // sometimes the url has a # at the end of the string
            return await CallCodeExchangeEndpoint(grantCodeCleaned);
         }
         else
         {
            Debug.Log("Code not found");
         }
      }
      return false;
   }

   // exchanges grant code for tokens
   private async Task<bool> CallCodeExchangeEndpoint(string grantCode)
   {
      WWWForm form = new WWWForm();
      form.AddField("grant_type", AuthCodeGrantType);
      form.AddField("client_id", AppClientID);
      form.AddField("code_verifier", _codeVerifier);
      form.AddField("code", grantCode);
      form.AddField("redirect_uri", RedirectUrl);

      // DOCS: https://docs.aws.amazon.com/cognito/latest/developerguide/token-endpoint.html
      string requestPath = "https://" + AuthCognitoDomainPrefix + CognitoAuthUrl + TokenEndpointPath;

      UnityWebRequest webRequest = UnityWebRequest.Post(requestPath, form);
      await webRequest.SendWebRequest();

      if (webRequest.result != UnityWebRequest.Result.Success)
      {
         Debug.Log("Code exchange failed: " + webRequest.error + "\n" + webRequest.result + "\n" + webRequest.responseCode);
         webRequest.Dispose();
      }
      else
      {
         Debug.Log("Success, Code exchange complete!");

         BADAuthenticationResultType authenticationResultType = JsonUtility.FromJson<BADAuthenticationResultType>(webRequest.downloadHandler.text);
         Debug.Log("ID token: " + authenticationResultType.id_token);
         
         PlayerPrefs.SetString("idtoken", authenticationResultType.id_token);

         _userid = AuthUtilities.GetUserSubFromIdToken(authenticationResultType.id_token);

         // update session cache
         SaveDataManager.SaveJsonData(new UserSessionCache(authenticationResultType, _userid));
         webRequest.Dispose();
         return true;
      }
      return false;
   }

   public async Task<bool> CallRefreshTokenEndpoint()
   {
      UserSessionCache userSessionCache = new UserSessionCache();
      SaveDataManager.LoadJsonData(userSessionCache);

      string preservedRefreshToken = "";

      if (userSessionCache != null && userSessionCache._refreshToken != null && userSessionCache._refreshToken != "")
      {
         // DOCS: https://docs.aws.amazon.com/cognito/latest/developerguide/token-endpoint.html
         string refreshTokenUrl = "https://" + AuthCognitoDomainPrefix + CognitoAuthUrl + TokenEndpointPath;
         // Debug.Log(refreshTokenUrl);

         preservedRefreshToken = userSessionCache._refreshToken;

         WWWForm form = new WWWForm();
         form.AddField("grant_type", RefreshTokenGrantType);
         form.AddField("client_id", AppClientID);
         form.AddField("refresh_token", userSessionCache._refreshToken);

         UnityWebRequest webRequest = UnityWebRequest.Post(refreshTokenUrl, form);
         webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

         await webRequest.SendWebRequest();


         if (webRequest.result != UnityWebRequest.Result.Success)
         {
            Debug.Log("Refresh token call failed: " + webRequest.error + "\n" + webRequest.result + "\n" + webRequest.responseCode);
            // clear out invalid user session data to force re-authentication
            ClearUserSessionData();
            webRequest.Dispose();
         }
         else
         {
            Debug.Log("Success, Refresh token call complete!");
            // Debug.Log(webRequest.downloadHandler.text);

            BADAuthenticationResultType authenticationResultType = JsonUtility.FromJson<BADAuthenticationResultType>(webRequest.downloadHandler.text);

            // token endpoint to get refreshed access token does NOT return the refresh token, so manually save it from before.
            authenticationResultType.refresh_token = preservedRefreshToken;

            _userid = AuthUtilities.GetUserSubFromIdToken(authenticationResultType.id_token);

            // update session cache
            SaveDataManager.SaveJsonData(new UserSessionCache(authenticationResultType, _userid));
            webRequest.Dispose();
            return true;
         }
      }
      return false;
   }

   // Revokes refresh token and any access tokens issued from the refresh token.  Forces user to re-authenticate.
   private async Task<bool> RevokeRefreshToken()
   {
      UserSessionCache userSessionCache = new UserSessionCache();
      SaveDataManager.LoadJsonData(userSessionCache);

      if (userSessionCache != null && userSessionCache._refreshToken != null && userSessionCache._refreshToken != "")
      {
         // DOCS (WARNING these docs are not accurate at the time of this implementation): https://docs.aws.amazon.com/cognito/latest/developerguide/revocation-endpoint.html
         // These were more accurate: https://docs.aws.amazon.com/cognito-user-identity-pools/latest/APIReference/API_RevokeToken.html
         // Also, the Enable token revocation option must be enabled for this to work under User Pool -> App Clients tab.
         string revokeTokenEndpoint = "https://" + AuthCognitoDomainPrefix + CognitoAuthUrl + "/oauth2/revoke";
         // Debug.Log(revokeTokenEndpoint);

         WWWForm form = new WWWForm();
         form.AddField("client_id", AppClientID);
         form.AddField("token", userSessionCache._refreshToken);

         UnityWebRequest webRequest = UnityWebRequest.Post(revokeTokenEndpoint, form);
         webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

         await webRequest.SendWebRequest();

         if (webRequest.result != UnityWebRequest.Result.Success)
         {
            Debug.Log("Revoke token call failed: " + webRequest.error + "\n" + webRequest.result + "\n" + webRequest.responseCode);
            webRequest.Dispose();
         }
         else
         {
            Debug.Log("Success, Revoke token call complete!");
            webRequest.Dispose();
            return true;
         }
      }
      return false;
   }

   public async void Logout()
   {
      bool logoutSuccess = await RevokeRefreshToken();

      // Important! Make sure to remove the local stored tokens.
      ClearUserSessionData();
      Debug.Log("user logged out.");
   }

   // Saves an empty user session object that will clear out all locally saved tokens.
   private void ClearUserSessionData()
   {
      UserSessionCache userSessionCache = new UserSessionCache();
      SaveDataManager.SaveJsonData(userSessionCache);
   }
   

   public string GetIdToken()
   {
      UserSessionCache userSessionCache = new UserSessionCache();
      SaveDataManager.LoadJsonData(userSessionCache);
      return userSessionCache.getIdToken();
   }
   
   public void GenerateCodeChallenge()
   {
      //Generate code challenge and verifier
      var(challenge, verifier) = Pkce.Generate();
      _codeChallenge = challenge;
      _codeVerifier = verifier;
      
      //Generate random State
      _state = CreateRandomString(8);
   }
   
   public string GetLoginUrl()
   {
      //Generate code challenge+verifier and random state.
      GenerateCodeChallenge();
      string loginUrl = "https://" + AuthCognitoDomainPrefix + CognitoAuthUrl
         + "/oauth2/authorize?response_type=code&client_id=" + AppClientID + 
         "&redirect_uri=" + RedirectUrl + 
         "&state=" + _state +
         "&code_challenge_method=S256&code_challenge=" + _codeChallenge;
      return loginUrl;
      //TODO: Desktop app can start pooling...
   }

   void Awake()
   {
      CachePath = Application.persistentDataPath;
   }
   
   private string CreateRandomString(int stringLength = 10) {
      int _stringLength = stringLength - 1;
      string randomString = "";
      string[] characters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
      for (int i = 0; i <= _stringLength; i++) {
         randomString = randomString + characters[Random.Range(0, characters.Length)];
      }
      return randomString;
   }
}
