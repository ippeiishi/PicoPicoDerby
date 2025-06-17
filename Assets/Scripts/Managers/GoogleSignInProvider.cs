using UnityEngine;
using System;
using System.Threading.Tasks;
using Google;

public class GoogleSignInProvider : MonoBehaviour {
    public static GoogleSignInProvider Instance { get; private set; }

    private const string WebClientId = "1073674123330-ugo8mk76nmgvn1rpvn9hfvf4efrbnfpt.apps.googleusercontent.com";
    private string _cachedIdToken = null;
    private GoogleSignInConfiguration configuration;

    void Awake()
    {
        Instance = this;
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = WebClientId,
            RequestIdToken = true,
        };
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false; // 通常サインイン
    }
    
    

    public async Task<string> GetGoogleIdTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedIdToken)){
            return _cachedIdToken;
        }
        Debug.Log("Google サインインの処理を開始します…");

        try {
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

            if (googleUser != null && !string.IsNullOrEmpty(googleUser.IdToken))
            {
                Debug.Log("Google ID トークンの取得に成功しました。");
                _cachedIdToken = googleUser.IdToken;
                return googleUser.IdToken;
            }
            else
            {
                Debug.LogWarning("Google サインインはキャンセルされたか、ID トークンが取得できませんでした。");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Google サインイン処理中にエラーが発生しました: {e}");
            // エラーが発生した場合もnullを返し、呼び出し元で汎用エラーとして処理させる
            return null;
        }
    }

    public void SignOutFromGoogle() {
        _cachedIdToken = null;
        if (GoogleSignIn.DefaultInstance != null) {
            GoogleSignIn.DefaultInstance.SignOut();
            Debug.Log("Google アカウントからサインアウトしました。");
        }
    }

    public async Task<bool> SignInSilentlyAsync() {
        try {
            GoogleSignInUser user = await GoogleSignIn.DefaultInstance.SignInSilently();
            if (user != null) {
                _cachedIdToken = user.IdToken;
                Debug.Log("Google silent sign-in successful.");
                return true;
            }
            return false;
        } catch (Exception e) {
            Debug.LogWarning($"Google silent sign-in failed: {e.Message}");
            return false;
        }
    }
}