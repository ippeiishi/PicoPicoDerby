// UGSInitializationManager.cs
using UnityEngine;
using System;
using Unity.Services.Core;
using System.Threading.Tasks; // Taskを使うために必要

public class UGSInitializationManager : MonoBehaviour {
    public static UGSInitializationManager Instance { get; private set; }
    private bool isInitialized = false;

    void Awake() {
        Instance = this;
    }

    public async Task<bool> InitializeUGSIfNeeded() {
        if (isInitialized) {
            return true;
        }

        try {
            await UnityServices.InitializeAsync();
            Debug.Log("UGS Initialized successfully.");
            isInitialized = true;
            return true;
        } catch (Exception e) {
            Debug.LogError($"UGS Initialization failed: {e}");
            // エラーダイアログの表示は呼び出し元に任せる
            return false;
        }
    }
}