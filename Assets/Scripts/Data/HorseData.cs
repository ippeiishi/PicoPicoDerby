using System;
using Newtonsoft.Json;

/// <summary>
/// 競走馬の基本的なデータを保持するクラス。
/// これはプロトタイピング用の最小構成です。
/// </summary>
[Serializable]
public class HorseData {
    [JsonProperty("uid")]
    public string Uid { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("base_speed")]
    public int BaseSpeed { get; set; }

    [JsonProperty("base_stamina")]
    public int BaseStamina { get; set; }

    [JsonProperty("base_guts")]
    public int BaseGuts { get; set; }

    [JsonProperty("base_mentality")]
    public int BaseMentality { get; set; }

    public HorseData() {
        // 新規作成時にユニークIDを自動で割り振る
        Uid = Guid.NewGuid().ToString();
    }
}