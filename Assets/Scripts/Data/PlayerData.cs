using System;

[Serializable]
public class PlayerData
{
    public int saveVersion = 1;
    public string lastSaveTime;
    public string creationTime;
    public string username;
    public long money;
    public long gem;
    public string lastActiveDeviceID; // 「いちアカウント、いち端末」の鍵

    // デフォルトコンストラクタ
    public PlayerData() { }

    // 新規作成用コンストラクタ
    public PlayerData(string name, string deviceId)
    {
        saveVersion = 1;
        var now = DateTime.UtcNow.ToString("o"); // ISO 8601 形式
        lastSaveTime = now;
        creationTime = now;
        username = name;
        money = 100; // 初期所持金
        gem = 0;     // 初期所持ジェム
        lastActiveDeviceID = deviceId;
    }
}