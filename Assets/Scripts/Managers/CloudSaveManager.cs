using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }

    // メモリ上で保持する現在のプレイヤーデータ
    [SerializeField] private PlayerData currentPlayerData;
    public PlayerData CurrentPlayerData => currentPlayerData;

    private const string PlayerDataKey = "PlayerData";

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Cloudからデータをロードし、メモリに展開します。
    /// </summary>
    /// <returns>ロードに成功すればtrue、データが存在しない等の理由で失敗すればfalseを返します。</returns>
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

    /// <summary>
    /// 新規ユーザーの初期データを作成し、Cloudに保存します。
    /// </summary>
    /// <param name="username">ユーザーが入力した名前</param>
    public async Task CreateAndSaveInitialDataAsync(string username)
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        currentPlayerData = new PlayerData(username, deviceId);
        
        // PlayerDataクラス自体をオブジェクトとして渡す
        var dataToSave = new Dictionary<string, object> { { PlayerDataKey, currentPlayerData } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
        
        Debug.Log("Initial data created and saved to Cloud.");
    }

    /// <summary>
    /// クラウド上のデータと現在のデバイスIDを比較し、衝突をチェックします。
    /// </summary>
    /// <returns>衝突が検出された場合はtrueを返します。</returns>
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
                    return true; // 衝突あり
                }
            }
            return false; // 衝突なし、またはデータなし
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to check for device conflict: {e}. Assuming no conflict for safety.");
            return false; // エラー時は安全側に倒し、衝突なしと判断
        }
    }
}