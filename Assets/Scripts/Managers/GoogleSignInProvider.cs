using UnityEngine;
using System;
using System.Threading.Tasks;
using Google;

/// <summary>
/// Google サインイン専用プロバイダー（シングルトン）<br/>
/// 既存の実装を尊重しつつ、成功 / キャンセル / エラーのイベント通知のみ追加。
/// </summary>
public class GoogleSignInProvider : MonoBehaviour
{
    public static GoogleSignInProvider Instance { get; private set; }

    // ★★★ あなたの Web Client ID ★★★
    private const string WebClientId = "1073674123330-ugo8mk76nmgvn1rpvn9hfvf4efrbnfpt.apps.googleusercontent.com";

    private GoogleSignInConfiguration configuration;

    // ----- 追加イベント -----
    public event Action OnGoogleSignInSuccess;                       // 成功
    public event Action OnGoogleSignInCancelled;                     // キャンセル
    public event Action<Exception> OnGoogleSignInError;              // 失敗(例外付き)

    // ----- 内部状態 -----
    private bool _isSigningIn = false;       // 多重実行ガード
    private string _cachedIdToken = null;    // 必要なら再利用用キャッシュ

    // ----------------------
    #region Unity Lifecycle
    void Awake()
    {
        Instance = this;

        configuration = new GoogleSignInConfiguration
        {
            WebClientId = WebClientId,
            RequestIdToken = true,
            UseGameSignIn = false
        };
    }
    #endregion

    /// <summary>
    /// Google ID トークンを取得して返す。（失敗・キャンセル時は null）
    /// </summary>
    public async Task<string> GetGoogleIdTokenAsync()
    {
        // すでに別スレッドでサインイン中なら待機
        if (_isSigningIn)
        {
            Debug.LogWarning("Google sign-in is already in progress.");
            return null;
        }

        // キャッシュが要る場合はここで返す
        if (!string.IsNullOrEmpty(_cachedIdToken))
            return _cachedIdToken;

        GoogleSignIn.Configuration = configuration;
        _isSigningIn = true;

        Debug.Log("Google サインインの設定を開始します…");

        try
        {
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

            if (googleUser != null && !string.IsNullOrEmpty(googleUser.IdToken))
            {
                Debug.Log("Google ID トークンの取得に成功しました。");
                _cachedIdToken = googleUser.IdToken;
                OnGoogleSignInSuccess?.Invoke();     // ★ イベント発火
                return googleUser.IdToken;
            }
            else
            {
                Debug.LogWarning("Google サインインはキャンセルされたか、ID トークンが取得できませんでした。");
                OnGoogleSignInCancelled?.Invoke();   // ★ イベント発火
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Google サインイン処理中にエラーが発生しました: {e}");
            OnGoogleSignInError?.Invoke(e);          // ★ イベント発火
            return null;
        }
        finally
        {
            _isSigningIn = false;
        }
    }

    /// <summary>
    /// Google / Firebase 側サインアウト（キャッシュも削除）
    /// </summary>
    public void SignOut()
    {
        _cachedIdToken = null;

        if (GoogleSignIn.DefaultInstance != null)
        {
            GoogleSignIn.DefaultInstance.SignOut();
            Debug.Log("Google アカウントからサインアウトしました。");
        }
    }
    
    // GoogleSignInProvider.cs に追加
public async Task<bool> SignInSilentlyAsync() {
    if (_isSigningIn) { return false; }
    _isSigningIn = true;
    try {
        GoogleSignIn.Configuration = configuration;
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
    } finally {
        _isSigningIn = false;
    }
}
}
