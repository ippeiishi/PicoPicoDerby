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
    public string lastActiveDeviceID;

    public PlayerData() { }

}