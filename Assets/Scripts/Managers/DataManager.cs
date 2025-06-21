// ファイル名を CloudSaveManager.cs から DataManager.cs に変更してください。
// DataManager.cs

using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour {
    public static DataManager Instance { get; private set; }

    private OwnerData _ownerData;

    public OwnerData OwnerData => _ownerData;
    public long Money => _ownerData?.Money ?? 0;
    public int Gem => _ownerData?.Gem ?? 0;
    public string OwnerName => _ownerData?.Name ?? "No Name";

    private const string OwnerDataKey = "owner-data";
    private const string FirstLaunchKey = "FirstLaunchCompleted";

    void Awake() { Instance = this; }

    public bool HasCompletedFirstLaunch() {
        return PlayerPrefs.GetInt(FirstLaunchKey, 0) == 1;
    }

    public void SetFirstLaunchCompleted() {
        PlayerPrefs.SetInt(FirstLaunchKey, 1);
        PlayerPrefs.Save();
        Debug.Log("FirstLaunchCompleted flag has been set in PlayerPrefs.");
    }

    public async Task<bool> LoadOwnerDataAsync() {
        try {
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { OwnerDataKey });

            if (loadedData.TryGetValue(OwnerDataKey, out var item)) {
                _ownerData = JsonConvert.DeserializeObject<OwnerData>(item.Value.GetAsString());
                Debug.Log($"OwnerData loaded from Cloud. Owner Name: {_ownerData.Name}");
                return true;
            } else {
                Debug.Log("No OwnerData found on Cloud Save. A new data object will be created upon first save.");
                _ownerData = null;
                return false;
            }
        } catch (Exception e) {
            Debug.LogError($"Failed to load OwnerData: {e}");
            _ownerData = new OwnerData();
            return false;
        }
    }

    public async Task SaveOwnerDataAsync() {
        if (_ownerData == null) {
            Debug.LogWarning("Cannot save null OwnerData. A new data object will be created.");
            _ownerData = new OwnerData();
            
        }

        try {
            _ownerData.LastUpdatedAt = DateTime.UtcNow.ToString("o");
            _ownerData.LastActiveDeviceId = SystemInfo.deviceUniqueIdentifier; // 保存時にデバイスIDを更新
            string dataString = JsonConvert.SerializeObject(_ownerData);
            var dataToSave = new Dictionary<string, object> { { OwnerDataKey, dataString } };

            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log("Successfully saved OwnerData to Cloud.");
        } catch (Exception e) {
            Debug.LogError($"Failed to save OwnerData: {e}");
            throw;
        }
    }

    public async Task CreateAndSaveInitialDataAsync(string ownerName, string familyName) {
        _ownerData = new OwnerData {
            Name = ownerName,
            FamilyName = familyName
        };
        // 初回保存時にもデバイスIDを記録
        await SaveOwnerDataAsync();
        Debug.Log("Initial OwnerData created and saved to Cloud.");
    }

    public async Task<bool> CheckForDeviceConflictAsync() {
        // データがまだロードされていない、またはクラウドに存在しない場合は競合なし
        if (_ownerData == null) {
            bool hasData = await LoadOwnerDataAsync();
            if (!hasData) { return false; }
        }

        string savedDeviceId = _ownerData.LastActiveDeviceId;
        string currentDeviceId = SystemInfo.deviceUniqueIdentifier;

        if (!string.IsNullOrEmpty(savedDeviceId) && savedDeviceId != currentDeviceId) {
            Debug.LogWarning($"Device conflict detected. Cloud Device ID: {savedDeviceId}, Current: {currentDeviceId}");
            return true; // 競合あり
        }
        return false; // 競合なし
    }

    public async Task UpdateDeviceIDAfterRecoveryAsync() {
        if (_ownerData == null) {
            bool loadSuccess = await LoadOwnerDataAsync();
            if (!loadSuccess) {
                throw new Exception("Failed to load data before updating device ID after recovery.");
            }
        }
        // SaveOwnerDataAsyncがデバイスIDの更新と保存を両方行う
        await SaveOwnerDataAsync();
        Debug.Log($"Device ID updated on Cloud to: {SystemInfo.deviceUniqueIdentifier} after recovery.");
    }

    public async Task<bool> ExecuteFullDataWipeAsync() {
        bool cloudDeleted = false;
        bool localCleared = false;

        try {
            await CloudSaveService.Instance.Data.Player.DeleteAsync(
                OwnerDataKey,
                new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions()
            );
            Debug.Log("Deleted OwnerData from Cloud Save.");
            cloudDeleted = true;
        } catch (CloudSaveException e) when (e.Reason == CloudSaveExceptionReason.NotFound) {
            Debug.Log("No OwnerData to delete from Cloud Save (already gone).");
            cloudDeleted = true;
        } catch (Exception e) {
            Debug.LogError($"Failed to delete cloud data: {e}");
            cloudDeleted = false;
        }

        try {
            PlayerPrefs.DeleteKey(FirstLaunchKey);
            PlayerPrefs.Save();
            Debug.Log("Cleared FirstLaunchCompleted flag from PlayerPrefs.");
            localCleared = true;
        } catch (Exception e) {
            Debug.LogError($"Failed to clear PlayerPrefs: {e}");
            localCleared = false;
        }

        _ownerData = new OwnerData();
        return cloudDeleted && localCleared;
    }
}