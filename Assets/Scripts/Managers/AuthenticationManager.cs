using UnityEngine;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Firebase.Auth;

public class AuthenticationManager : MonoBehaviour {
    public static AuthenticationManager Instance { get; private set; }
    public bool IsLoggedIn => AuthenticationService.Instance.IsSignedIn;

    public enum RecoveryResult {
        Success,
        AccountNotFound,
        Failure
    }

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
        try {
            AuthenticationService.Instance.SignOut();
            GoogleSignInProvider.Instance.SignOutFromGoogle();
            FirebaseInitializationManager.Instance.Auth.SignOut();
        } catch (Exception e) {
            Debug.LogError($"Exception during SignOut: {e.Message}");
        }
    }

    public enum LinkResult {
        Success,
        AccountAlreadyLinked,
        Cancelled,
        Failed
    }

    public async Task<LinkResult> LinkWithGoogleAsync() {
        if (!IsLoggedIn) { Debug.LogError("UGSにサインインしていません。"); return LinkResult.Failed; }
    
        try {
            bool firebaseSuccess = await FirebaseInitializationManager.Instance.InitializeFirebaseIfNeeded();
            if (!firebaseSuccess) {
                Debug.LogError("Firebase Initialization Failed on demand.");
                return LinkResult.Failed;
            }
            string idToken = await GoogleSignInProvider.Instance.GetGoogleIdTokenAsync();
            if (string.IsNullOrEmpty(idToken)) {
                Debug.Log("Googleサインインがキャンセルされました。");
                return LinkResult.Cancelled;
            }
            await AuthenticationService.Instance.LinkWithGoogleAsync(idToken);
            Debug.Log("Googleアカウントの連携に成功しました。");
            return LinkResult.Success;
        } 
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked) {
            Debug.LogError("このGoogleアカウントは、すでに他のセーブデータに連携されています。");
            GoogleSignInProvider.Instance.SignOutFromGoogle();
            FirebaseInitializationManager.Instance.Auth.SignOut();
            return LinkResult.AccountAlreadyLinked;
        } 
        catch (OperationCanceledException) {
            Debug.Log("連携操作がキャンセルされました。");
            return LinkResult.Cancelled;
        } 
        catch (Exception e) {
            Debug.LogError($"Googleアカウントの連携に失敗しました: {e}");
            GoogleSignInProvider.Instance.SignOutFromGoogle();
            FirebaseInitializationManager.Instance.Auth.SignOut();
            return LinkResult.Failed;
        }
    }

    public async Task<bool> UnlinkFromGoogleAsync() {
        if (!IsLoggedIn) { Debug.LogError("UGSにサインインしていません。"); return false; }
        try {
            await AuthenticationService.Instance.UnlinkGoogleAsync();
            GoogleSignInProvider.Instance.SignOutFromGoogle();
            FirebaseInitializationManager.Instance.Auth.SignOut();
            Debug.Log("Googleアカウントとの連携を解除しました。");
            return true;
        } catch (Exception e) {
            Debug.LogError($"Googleアカウントの連携解除に失敗しました: {e}");
            return false;
        }
    }

    public async Task<RecoveryResult> SignInWithGoogleForRecoveryAsync() {
        try {
            bool firebaseSuccess = await FirebaseInitializationManager.Instance.InitializeFirebaseIfNeeded();
            if (!firebaseSuccess) { throw new Exception("Firebase Initialization Failed on demand."); }

            string idToken = await GoogleSignInProvider.Instance.GetGoogleIdTokenAsync();
            if (string.IsNullOrEmpty(idToken)) {
                throw new Exception("Failed to get Google ID Token. It might be cancelled or an error occurred.");
            }

            var options = new SignInOptions { CreateAccount = false };
            await AuthenticationService.Instance.SignInWithGoogleAsync(idToken, options);
            return RecoveryResult.Success;

        } catch (Exception ex) {
            if (IsAccountNotFoundException(ex)) {
                return RecoveryResult.AccountNotFound;
            }
            
            Debug.LogError($"An unexpected error occurred during Google Sign-In for recovery: {ex}");
            return RecoveryResult.Failure;
        }
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
        if (ex is RequestFailedException reqEx) {
            string msg = reqEx.Message ?? string.Empty;
            if (msg.IndexOf("user not found", StringComparison.OrdinalIgnoreCase) >= 0) { return true; }
            if (msg.IndexOf("RESOURCE_NOT_FOUND", StringComparison.OrdinalIgnoreCase) >= 0) { return true; }
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