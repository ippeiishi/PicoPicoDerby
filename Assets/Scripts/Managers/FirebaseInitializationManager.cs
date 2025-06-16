// FirebaseInitializationManager.cs
using UnityEngine;
using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;

public class FirebaseInitializationManager : MonoBehaviour {
    public static FirebaseInitializationManager Instance { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    private bool isInitialized = false;

    void Awake() { Instance = this; }

    public async Task<bool> InitializeFirebaseIfNeeded() {
        if (isInitialized) { return true; }

        try {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available) {
                Auth = FirebaseAuth.DefaultInstance;
                isInitialized = true;
                Debug.Log("Firebase Initialized successfully.");
                return true;
            } else {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                return false;
            }
        } catch (Exception e) {
            Debug.LogError($"Firebase Initialization Exception: {e}");
            return false;
        }
    }
}