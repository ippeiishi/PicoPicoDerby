// RemoteConfigManager.cs
using UnityEngine;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class RemoteConfigManager : MonoBehaviour {
    public static RemoteConfigManager Instance { get; private set; }

    public string DefaultPlayerDataJson { get; private set; }
    public string TipsJson { get; private set; }

    private const string DefaultPlayerDataKey = "defaultPlayerData";
    private const string TipsDataKey = "tips_ja";

    private struct UserAttributes {}
    private struct AppAttributes {}

    void Awake() { Instance = this; }

    public async Task FetchConfigsAsync() {
        if (!AuthenticationService.Instance.IsSignedIn) {
            Debug.LogError("RemoteConfig: Authentication is required before fetching configs.");
            return;
        }

        await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());

        DefaultPlayerDataJson = RemoteConfigService.Instance.appConfig.GetJson(DefaultPlayerDataKey, "{}");
        TipsJson = RemoteConfigService.Instance.appConfig.GetJson(TipsDataKey, "[]");
        
        if (string.IsNullOrEmpty(DefaultPlayerDataJson) || DefaultPlayerDataJson == "{}") {
            Debug.LogWarning($"RemoteConfig: Could not retrieve '{DefaultPlayerDataKey}'. Using empty JSON.");
        } else {
            Debug.Log($"RemoteConfig: Successfully fetched '{DefaultPlayerDataKey}'.");
        }

        if (string.IsNullOrEmpty(TipsJson) || TipsJson == "[]") {
            Debug.LogWarning($"RemoteConfig: Could not retrieve '{TipsDataKey}'. Using empty JSON.");
        } else {
            Debug.Log($"RemoteConfig: Successfully fetched '{TipsDataKey}'.");
        }
    }
}