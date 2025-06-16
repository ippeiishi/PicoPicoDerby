using UnityEngine;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Firebase.Auth;

public class AuthenticationManager : MonoBehaviour {
    public static AuthenticationManager Instance { get; private set; }
    public bool IsLoggedIn => AuthenticationService.Instance.IsSignedIn;

    void Awake() { Instance = this; }

    public async Task SignInAnonymouslyIfNeeded() {
        if (IsLoggedIn) {
            Debug.Log("Already logged in.");
            return;
        }
        try {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"UGS Signed in anonymously. Player ID: {AuthenticationService.Instance.PlayerId}");
        } catch (Exception e) {
            Debug.LogError($"Anonymous sign-in failed: {e}");
            throw;
        }
    }

    public void SignOut() {
        if (!IsLoggedIn && FirebaseInitializationManager.Instance?.Auth?.CurrentUser == null) {
            return;
        }
        Debug.Log("Signing out from all services...");
        if (AuthenticationService.Instance.IsSignedIn) {
            try {
                AuthenticationService.Instance.SignOut();
                Debug.Log("UGS signed out.");
            } catch (Exception e) {
                Debug.LogError($"UGS sign out failed: {e}");
            }
        }
#if !UNITY_EDITOR
        try {
            if (GoogleSignInProvider.Instance != null) {
                GoogleSignInProvider.Instance.SignOut();
            }
        } catch (Exception e) {
            Debug.LogError($"Google sign out failed: {e}");
        }
        try {
            if (FirebaseInitializationManager.Instance?.Auth?.CurrentUser != null) {
                FirebaseInitializationManager.Instance.Auth.SignOut();
                Debug.Log("Firebase Auth signed out.");
            }
        } catch (Exception e) {
            Debug.LogError($"Firebase Auth sign out failed: {e}");
        }
#endif
    }

    public async Task<bool> LinkWithGoogleAsync() {
        if (!IsLoggedIn) {
            Debug.LogError("UGSにサインインしていません。Googleアカウントの連携はできません。");
            return false;
        }
        try {
            bool firebaseSuccess = await FirebaseInitializationManager.Instance.InitializeFirebaseIfNeeded();
            if (!firebaseSuccess) { throw new Exception("Firebase Initialization Failed on demand."); }
            string idToken = await GoogleSignInProvider.Instance.GetGoogleIdTokenAsync();
            if (string.IsNullOrEmpty(idToken)) { return false; }
            await AuthenticationService.Instance.LinkWithGoogleAsync(idToken);
            Debug.Log("Googleアカウントの連携に成功しました。");
            return true;
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked) {
            Debug.LogError("このGoogleアカウントは、すでに他のゲームアカウントに連携されています。");
            return false;
        }
        catch (Exception e) {
            Debug.LogError($"Googleアカウントの連携に失敗しました: {e}");
            return false;
        }
    }

    public async Task<bool> UnlinkFromGoogleAsync() {
        if (!IsLoggedIn) {
            Debug.LogError("UGSにサインインしていません。");
            return false;
        }
        try {
            await AuthenticationService.Instance.UnlinkGoogleAsync();
            GoogleSignInProvider.Instance.SignOut();
            if (FirebaseInitializationManager.Instance.Auth?.CurrentUser != null) {
                FirebaseInitializationManager.Instance.Auth.SignOut();
                Debug.Log("Firebase Auth signed out during unlink.");
            }
            Debug.Log("Googleアカウントとの連携を解除しました。");
            return true;
        } catch (Exception e) {
            Debug.LogError($"Googleアカウントの連携解除に失敗しました: {e}");
            return false;
        }
    }

    public async Task<bool> SignInWithGoogleForRecoveryAsync() {
        bool firebaseSuccess = await FirebaseInitializationManager.Instance.InitializeFirebaseIfNeeded();
        if (!firebaseSuccess) { throw new Exception("Firebase Initialization Failed on demand."); }
        string idToken = await GoogleSignInProvider.Instance.GetGoogleIdTokenAsync();
        if (string.IsNullOrEmpty(idToken)) {
            return false;
        }
        var options = new SignInOptions { CreateAccount = false };
        await AuthenticationService.Instance.SignInWithGoogleAsync(idToken, options);
        Debug.Log($"Googleアカウントでのデータ引き継ぎに成功しました。 PlayerID: {AuthenticationService.Instance.PlayerId}");
        return true;
    }

    public bool IsLinkedWithGoogle() {
        if (!IsLoggedIn) { return false; }
        foreach (var identity in AuthenticationService.Instance.PlayerInfo.Identities) {
            if (identity.TypeId.Equals("google.com", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }
        return false;
    }

    public bool IsAccountNotFoundException(Exception ex) {
        var requestFailedException = ex as RequestFailedException;
        if (requestFailedException != null) {
            return requestFailedException.ErrorCode == 404;
        }
        var authException = ex as AuthenticationException;
        if (authException != null) {
            return authException.ErrorCode == 1009;
        }
        return false;
    }

    public async Task EnsureGoogleSignInStateAsync() {
        if (IsLinkedWithGoogle() && FirebaseInitializationManager.Instance.Auth?.CurrentUser == null) {
            Debug.Log("Server is linked, but client is not. Attempting to sync state...");
            await FirebaseInitializationManager.Instance.InitializeFirebaseIfNeeded();
            bool silentSignInSuccess = await GoogleSignInProvider.Instance.SignInSilentlyAsync();
            if (silentSignInSuccess) {
                string idToken = await GoogleSignInProvider.Instance.GetGoogleIdTokenAsync();
                if (!string.IsNullOrEmpty(idToken)) {
                    Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
                    await FirebaseInitializationManager.Instance.Auth.SignInWithCredentialAsync(credential);
                    Debug.Log("Firebase state synced successfully via silent sign-in.");
                }
            }
        }
    }
}