using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }

    [SerializeField] private PlayerData currentPlayerData;
    public PlayerData CurrentPlayerData => currentPlayerData;

    private const string PlayerDataKey = "PlayerData";
    private const string FirstLaunchKey = "FirstLaunchCompleted";

    void Awake() { Instance = this; }

    public bool HasCompletedFirstLaunch()
{
        return PlayerPrefs.GetInt(FirstLaunchKey, 0) == 1;
    }

    public void SetFirstLaunchCompleted()
    {
        PlayerPrefs.SetInt(FirstLaunchKey, 1);
        PlayerPrefs.Save();
        Debug.Log("FirstLaunchCompleted flag has been set in PlayerPrefs.");
    }

    public async Task<bool> LoadDataFromCloudAsync()
    {
        try
        {
            var keysToLoad = new HashSet<string> { PlayerDataKey };
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(keysToLoad);
            if (loadedData.TryGetValue(PlayerDataKey, out Item item))
            {
                currentPlayerData = JsonUtility.FromJson<PlayerData>(item.Value.GetAsString());
                Debug.Log($"Data loaded from Cloud. Username: {currentPlayerData.username}");
                return true;
            }
            else
            {
                Debug.LogWarning("PlayerData not found on Cloud Save.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data from Cloud: {e}");
            return false;
        }
    }

    public async Task CreateAndSaveInitialDataAsync(string username, string initialDataJson)
    {
        currentPlayerData = JsonUtility.FromJson<PlayerData>(initialDataJson);
        if (currentPlayerData == null)
        {
            Debug.LogError("Failed to parse initial data from Remote Config. Creating default PlayerData.");
            currentPlayerData = new PlayerData();
        }
        var now = DateTime.UtcNow.ToString("o");
        currentPlayerData.username = username;
        currentPlayerData.lastActiveDeviceID = SystemInfo.deviceUniqueIdentifier;
        currentPlayerData.creationTime = now;
        currentPlayerData.lastSaveTime = now;
        var dataToSave = new Dictionary<string, object> { { PlayerDataKey, currentPlayerData } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
        Debug.Log("Initial data from Remote Config created and saved to Cloud.");
    }

    public async Task<bool> CheckForDeviceConflictAsync()
    {
        try
        {
            var keysToLoad = new HashSet<string> { PlayerDataKey };
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(keysToLoad);
            if (loadedData.TryGetValue(PlayerDataKey, out Item item))
            {
                var cloudPlayerData = JsonUtility.FromJson<PlayerData>(item.Value.GetAsString());
                string savedDeviceId = cloudPlayerData?.lastActiveDeviceID;
                string currentDeviceId = SystemInfo.deviceUniqueIdentifier;
                if (!string.IsNullOrEmpty(savedDeviceId) && savedDeviceId != currentDeviceId)
                {
                    Debug.LogWarning($"Device conflict detected. Cloud Device ID: {savedDeviceId}, Current: {currentDeviceId}");
                    return true;
                }
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to check for device conflict: {e}. Assuming no conflict for safety.");
            return false;
        }
    }

    public async Task<bool> ExecuteFullDataWipeAsync()
    {
        bool cloudDeleted = false;
        bool localCleared = false;
        try
        {
            await CloudSaveService.Instance.Data.Player.DeleteAsync(PlayerDataKey, new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions());
            Debug.Log("Deleted PlayerData from Cloud Save.");
            cloudDeleted = true;
        }
        catch (CloudSaveException e) when (e.Reason == CloudSaveExceptionReason.NotFound)
        {
            Debug.Log("No PlayerData to delete from Cloud Save (already gone).");
            cloudDeleted = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete cloud data: {e}");
            cloudDeleted = false;
        }
        try
        {
            PlayerPrefs.DeleteKey(FirstLaunchKey);
            PlayerPrefs.Save();
            Debug.Log("Cleared FirstLaunchCompleted flag from PlayerPrefs.");
            localCleared = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to clear PlayerPrefs: {e}");
            localCleared = false;
        }
        currentPlayerData = new PlayerData();
        return cloudDeleted && localCleared;
    }

    public async Task SaveDataToCloudAsync()
    {
        if (currentPlayerData == null) { return; }
        try
        {
            currentPlayerData.lastSaveTime = DateTime.UtcNow.ToString("o");
            var dataToSave = new Dictionary<string, object> { { "PlayerData", currentPlayerData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log("Successfully saved data to Cloud.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data to cloud: {e}");
        }
    }

        public async Task UpdateDeviceIDAfterRecoveryAsync() {
        // 復旧直後なので、クラウドから最新のデータを強制的にロードする
        bool loadSuccess = await LoadDataFromCloudAsync();
        if (!loadSuccess) {
            // この例外は通常、SignInWithGoogleForRecoveryAsyncで先に捕捉されるはずだが、念のため
            throw new Exception("Failed to load data after account recovery.");
        }

        // デバイスIDを更新して即時保存
        currentPlayerData.lastActiveDeviceID = SystemInfo.deviceUniqueIdentifier;
        await SaveDataToCloudAsync();
        Debug.Log($"Device ID updated on Cloud to: {currentPlayerData.lastActiveDeviceID}");
    }
}