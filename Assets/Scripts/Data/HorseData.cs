using System;
using Newtonsoft.Json;

/// <summary>
/// レース用に必要な最低限の能力値を持つ競走馬データクラス。
/// プロトタイピング・シミュレーション用の最小構成。
/// </summary>
[Serializable]
public class HorseData {
    public string Uid { get; set; }
    public string Name { get; set; }

    public int sp { get; set; }  // boost加速度
    public int st { get; set; }  // boost回数
    public int gt { get; set; }  // 根性
    public int mn { get; set; }  // 気性（出遅れ/かかり）
    public int ft { get; set; }  // 脚質（0〜3）

    public HorseData() {
        Uid = Guid.NewGuid().ToString();
    }
}
