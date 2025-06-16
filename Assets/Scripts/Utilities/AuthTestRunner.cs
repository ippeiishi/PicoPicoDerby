using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Firebase;
using Firebase.Auth;

public class AuthTestRunner : MonoBehaviour {
    [Header("Test Button (Optional)")]
    [SerializeField] private Button testButton;

    private FirebaseAuth auth;
    private string webClientId = "1073674123330-ugok8o0gq9kmmb4e1q5h1dj61f12i52q.apps.googleusercontent.com"; // ★★★ あなたのWeb Client IDに書き換えてください ★★★

    void Start() {
        if (testButton != null) {
            testButton.onClick.AddListener(RunFullAuthTest);
        }
    }

    // このメソッドをUIボタンから呼び出すか、直接実行します
    public async void RunFullAuthTest() {
        Debug.Log("--- 認証テスト開始 ---");

        // 1. Firebase 初期化
        if (!await InitializeFirebaseAsync()) { return; }

        // 2. UGS 初期化と匿名サインイン
        if (!await InitializeAndSignInUGSAsync()) { return; }

        // 3. Google IDトークン取得
        string idToken = await GetGoogleIdTokenAsync();
        if (string.IsNullOrEmpty(idToken)) { return; }

        // 4. UGSアカウントとGoogleをリンク
        await LinkGoogleToUGSAsync(idToken);

        Debug.Log("--- 認証テスト終了 ---");
    }

    private async Task<bool> InitializeFirebaseAsync() {
        Debug.Log("Firebase初期化開始...");
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available) {
            auth = FirebaseAuth.DefaultInstance;
            Debug.Log("Firebase初期化成功。");
            return true;
        } else {
            Debug.LogError($"Firebase初期化失敗: {dependencyStatus}");
            return false;
        }
    }

    private async Task<bool> InitializeAndSignInUGSAsync() {
        try {
            Debug.Log("UGS初期化開始...");
            await UnityServices.InitializeAsync();
            Debug.Log("UGS初期化成功。");

            if (!AuthenticationService.Instance.IsSignedIn) {
                Debug.Log("UGS匿名サインイン開始...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"UGS匿名サインイン成功。 PlayerID: {AuthenticationService.Instance.PlayerId}");
            } else {
                Debug.Log("UGSは既にサインイン済みです。");
            }
            return true;
        } catch (Exception e) {
            Debug.LogError($"UGS処理失敗: {e}");
            return false;
        }
    }

    private async Task<string> GetGoogleIdTokenAsync() {
        Debug.Log("Googleサインイン処理開始...");
        var googleConfig = new Google.GoogleSignInConfiguration {
            WebClientId = this.webClientId,
            RequestIdToken = true,
            UseGameSignIn = false
        };

        try {
            Google.GoogleSignIn.Configuration = googleConfig;
            Google.GoogleSignInUser googleUser = await Google.GoogleSignIn.DefaultInstance.SignIn();
            
            if (string.IsNullOrEmpty(googleUser.IdToken)) {
                 Debug.LogError("Googleサインインは成功しましたが、IDトークンが取得できませんでした。");
                 return null;
            }

            Debug.Log("Google IDトークン取得成功。");
            return googleUser.IdToken;
        } catch (Exception e) {
            Debug.LogError($"Googleサインイン失敗: {e}");
            return null;
        }
    }

    private async Task LinkGoogleToUGSAsync(string idToken) {
        try {
            Debug.Log("UGSアカウントへのGoogleアカウント連携試行...");
            await AuthenticationService.Instance.LinkWithGoogleAsync(idToken);
            Debug.Log("【成功】Googleアカウントの連携が完了しました。");
        } catch (Exception e) {
            Debug.LogError($"Googleアカウント連携失敗: {e}");
        }
    }
}
