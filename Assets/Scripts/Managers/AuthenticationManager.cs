using UnityEngine;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour {
    public static AuthenticationManager Instance { get; private set; }
    public bool IsLoggedIn => AuthenticationService.Instance.IsSignedIn;

    void Awake() {
        Instance = this;
    }

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
            throw; // エラーを呼び出し元のInitializationManagerに伝える
        }
    }
}