using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// プレイヤー（オーナー）の永続データを保持するクラス。
/// UGS Cloud SaveでJSONとして保存されることを想定しています。
/// </summary>
[Serializable]
public class OwnerData
{
    // --- 基本情報 ---
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("family_name")]
    public string FamilyName { get; set; }

    [JsonProperty("family_name_position")]
    public int FamilyNamePosition { get; set; }

    [JsonProperty("created_at")]
    public string CreatedAt { get; set; }

    [JsonProperty("last_updated_at")]
    public string LastUpdatedAt { get; set; }

    // --- 通貨・スロット ---
    [JsonProperty("money")]
    public long Money { get; set; }

    [JsonProperty("gem")]
    public int Gem { get; set; }

    [JsonProperty("active_slot_count")]
    public int ActiveSlotCount { get; set; }

    [JsonProperty("legend_slot_count")]
    public int LegendSlotCount { get; set; }

    // --- フラグ・カウンター ---
    [JsonProperty("max_fever_mode_chain")]
    public int MaxFeverModeChain { get; set; } // 旧: tutorial

    [JsonProperty("is_training_lobby_list_view")]
    public bool IsTrainingLobbyListView { get; set; } // 旧: flg1

    [JsonProperty("is_legend_lobby_list_view")]
    public bool IsLegendLobbyListView { get; set; } // 旧: flg2

    [JsonProperty("true_fever_mode_count")]
    public int TrueFeverModeCount { get; set; } // 旧: flg3

    [JsonProperty("ultimate_fever_mode_count")]
    public int UltimateFeverModeCount { get; set; } // 旧: flg4

    [JsonProperty("has_used_daily_gem")]
    public bool HasUsedDailyGem { get; set; } // 旧: flg_daily

    // --- 統計情報 (Stats / Analytics) ---
    [JsonProperty("total_horses_produced")]
    public int TotalHorsesProduced { get; set; } // 旧: total_gen

    [JsonProperty("total_successors_produced")]
    public int TotalSuccessorsProduced { get; set; } // 旧: total_regen

    [JsonProperty("max_extra_bonus")]
    public int MaxExtraBonus { get; set; } // 旧: max_extra

    [JsonProperty("total_extra_bonus_used")]
    public int TotalExtraBonusUsed { get; set; } // 旧: total_extra

    [JsonProperty("total_bonus_7_rolled")]
    public int TotalBonus7Rolled { get; set; } // 旧: total_7

    [JsonProperty("total_bonus_3_rolled")]
    public int TotalBonus3Rolled { get; set; } // 旧: total_3

    [JsonProperty("freeze_count")]
    public int FreezeCount { get; set; } // 旧: flg5

    [JsonProperty("max_money_held")]
    public long MaxMoneyHeld { get; set; }

    [JsonProperty("total_prize_earned")]
    public long TotalPrizeEarned { get; set; }

    [JsonProperty("total_races_entered")]
    public int TotalRacesEntered { get; set; }

    [JsonProperty("total_races_won")]
    public int TotalRacesWon { get; set; }

    [JsonProperty("max_training_chain")]
    public int MaxTrainingChain { get; set; } // 旧: max_tre

    [JsonProperty("total_successions")]
    public int TotalSuccessions { get; set; } // 旧: total_inheri

    [JsonProperty("total_weeks_elapsed")]
    public int TotalWeeksElapsed { get; set; }

    [JsonProperty("total_injuries")]
    public int TotalInjuries { get; set; }

    [JsonProperty("total_deaths")]
    public int TotalDeaths { get; set; }

    // --- データリスト ---
    [JsonProperty("encountered_rival_ids")]
    public List<int> EncounteredRivalIds { get; set; } // 質問: IDリストであるならint型が適切と考えますが、いかがでしょうか。

    [JsonProperty("g1_race_records")]
    public List<RaceRecord> G1RaceRecords { get; set; }

    [JsonProperty("g2_race_records")]
    public List<RaceRecord> G2RaceRecords { get; set; }

    [JsonProperty("g3_race_records")]
    public List<RaceRecord> G3RaceRecords { get; set; }

    [JsonProperty("overseas_race_records")]
    public List<RaceRecord> OverseasRaceRecords { get; set; } // 旧: gw

    [JsonProperty("unlocked_ability_ids")]
    public List<int> UnlockedAbilityIds { get; set; }

    [JsonProperty("unlocked_unique_item_ids")]
    public List<int> UnlockedUniqueItemIds { get; set; }

    // [分離推奨] アイテム所持データは肥大化しやすいため、OwnerDataとは別のCloud Saveキーで管理することを強く推奨します。
    [JsonProperty("owned_item_instance_ids")]
    public List<int> OwnedItemInstanceIds { get; set; }

     [JsonProperty("mission_progress")]
    public List<int> MissionProgress { get; set; }

    // 修正点: 空行を削除
    [JsonProperty("last_active_device_id")]
    public string LastActiveDeviceId { get; set; }

    // 修正点: インデントとK&Rスタイルを修正
    public OwnerData() {
        // コンストラクタでデフォルト値を設定
        Name = "New Owner";
        FamilyName = "アリマ";
        CreatedAt = DateTime.UtcNow.ToString("o");
        LastUpdatedAt = DateTime.UtcNow.ToString("o");
        Money = 5000;
        ActiveSlotCount = 3;
        LegendSlotCount = 3;
        
        // boolはfalse, int/longは0で自動的に初期化される
        // Listは空のリストで初期化が必要
        EncounteredRivalIds = new List<int>();
        G1RaceRecords = new List<RaceRecord>();
        G2RaceRecords = new List<RaceRecord>();
        G3RaceRecords = new List<RaceRecord>();
        OverseasRaceRecords = new List<RaceRecord>();
        UnlockedAbilityIds = new List<int>();
        UnlockedUniqueItemIds = new List<int>();
        OwnedItemInstanceIds = new List<int>();
        MissionProgress = new List<int>();
    }
}

[Serializable]
public class RaceRecord
{
    [JsonProperty("wins")]
    public int Wins { get; set; }

    [JsonProperty("entries")]
    public int Entries { get; set; }
}