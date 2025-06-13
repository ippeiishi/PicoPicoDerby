using UnityEngine;
using System.IO;
using System;

public class LocalSaveManager : MonoBehaviour {
    public static LocalSaveManager Instance { get; private set; }

    [SerializeField] private PlayerData currentPlayerData;
    public PlayerData CurrentPlayerData => currentPlayerData;
    
    private string savePath;
    private const string FirstLaunchKey = "FirstLaunchCompleted"; // PlayerPrefsで使うキーを定数化

    void Awake() {
        Instance = this;
        savePath = Path.Combine(Application.persistentDataPath, "playerdata.json");
    }

    public void LoadData() {
        if (!File.Exists(savePath)) {
            currentPlayerData = new PlayerData();
            return;
        }
        
        try {
            string json = File.ReadAllText(savePath);
            currentPlayerData = JsonUtility.FromJson<PlayerData>(json);
        } catch (Exception e) {
            Debug.LogError($"Failed to load data. Error: {e.Message}");
            currentPlayerData = new PlayerData();
        }
    }

    public void SaveData() {
        try {
            currentPlayerData.lastSaveTime = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(currentPlayerData, true);
            File.WriteAllText(savePath, json);
        } catch (Exception e) {
            Debug.LogError($"Failed to save data. Error: {e.Message}");
        }
    }

    public void CreateNewData(string username) {
        currentPlayerData = new PlayerData(username, SystemInfo.deviceUniqueIdentifier);
        SaveData();
        
        // ★★★ 最も重要な変更点 ★★★
        // データ作成完了の証として、PlayerPrefsにフラグを立てる
        PlayerPrefs.SetInt(FirstLaunchKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 初回起動かどうかをPlayerPrefsのフラグで判定する。
    /// </summary>
    /// <returns>データ作成が完了している場合はtrue、初回起動の場合はfalseを返す。</returns>
    public bool HasCompletedFirstLaunch() {
        return PlayerPrefs.GetInt(FirstLaunchKey, 0) == 1;
    }

    // (任意) デバッグ用にPlayerPrefsと物理ファイルを両方削除するメソッド
    public void ClearAllSaveDataForDebug() {
        PlayerPrefs.DeleteKey(FirstLaunchKey);
        if (File.Exists(savePath)) {
            File.Delete(savePath);
        }
        Debug.Log("All local save data (PlayerPrefs flag and JSON file) have been cleared.");
    }
}