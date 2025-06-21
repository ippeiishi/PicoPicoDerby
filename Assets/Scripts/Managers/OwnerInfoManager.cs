using UnityEngine;
using TMPro;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class OwnerInfoManager : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statsText;

    private bool _isShowingStats = true; // true: 実績表示, false: 情報表示

    private void OnEnable() {
        // モーダルが開かれたときは、必ずデフォルトの実績表示に戻す
        _isShowingStats = true;
        UpdateUI();
    }

    /// <summary>
    /// デバッグボタンから呼び出すためのメソッド。表示モードを切り替える。
    /// </summary>
    public void ToggleDisplayMode() {
        _isShowingStats = !_isShowingStats;
        UpdateUI();
    }

    public void UpdateUI() {
        OwnerData data = DataManager.Instance.OwnerData;
        if (data == null) {
            statsText.text = "実績データをロードできませんでした。";
            return;
        }

        if (_isShowingStats) {
            ShowStats(data);
        } else {
            ShowDebugInfo(data);
        }
    }

    private void ShowStats(OwnerData data) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>【オーナー実績】</b>");
        sb.AppendLine($"総生産数: {FormatStat(data.TotalHorsesProduced)}");
        sb.AppendLine($"総後継馬生産数: {FormatStat(data.TotalSuccessorsProduced)}");
        sb.AppendLine($"最大エクストラボーナス: {FormatStat(data.MaxExtraBonus)}");
        sb.AppendLine($"総エクストラボーナス使用数: {FormatStat(data.TotalExtraBonusUsed)}");
        sb.AppendLine($"総ALL7: {FormatStat(data.TotalBonus7Rolled)}");
        sb.AppendLine($"総ALL3: {FormatStat(data.TotalBonus3Rolled)}");
        sb.AppendLine($"総フリーズ: {FormatStat(data.FreezeCount)}");
        sb.AppendLine($"最高到達総資産: {FormatMoney(data.MaxMoneyHeld)}");
        sb.AppendLine($"総獲得本賞金: {FormatMoney(data.TotalPrizeEarned)}");
        sb.AppendLine($"総出走数: {FormatStat(data.TotalRacesEntered)}");
        sb.AppendLine($"総勝利数: {FormatStat(data.TotalRacesWon)}");
        sb.AppendLine($"総勝率: {FormatWinRate(data.TotalRacesWon, data.TotalRacesEntered)}");
        sb.AppendLine($"GⅢ戦績: {FormatRaceRecord(data.G3RaceRecords)}");
        sb.AppendLine($"GⅡ戦績: {FormatRaceRecord(data.G2RaceRecords)}");
        sb.AppendLine($"GⅠ戦績: {FormatRaceRecord(data.G1RaceRecords)}");
        sb.AppendLine($"海外戦績: {FormatRaceRecord(data.OverseasRaceRecords)}");
        sb.AppendLine($"最高調教連チャン数: {FormatStat(data.MaxTrainingChain)}");
        sb.AppendLine($"最高開花連チャン数: {FormatStat(data.MaxFeverModeChain)}");
        sb.AppendLine($"能力開花・真の当選回数: {FormatStat(data.TrueFeverModeCount)}");
        sb.AppendLine($"能力開花・極の当選回数: {FormatStat(data.UltimateFeverModeCount)}");
        sb.AppendLine($"最高後継: {FormatSuccession(data.TotalSuccessions)}");
        sb.AppendLine($"総経過週: {FormatStat(data.TotalWeeksElapsed)}");
        sb.AppendLine($"怪我させた回数: {FormatStat(data.TotalInjuries)}");
        sb.AppendLine($"予後不良の回数: {FormatStat(data.TotalDeaths)}");

        statsText.text = sb.ToString();
    }

    private void ShowDebugInfo(OwnerData data) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>【オーナー情報 (Debug)】</b>");
        sb.AppendLine($"オーナー名: {data.Name}");
        sb.AppendLine($"ファミリーネーム: {data.FamilyName}");
        sb.AppendLine($"ファミリーネーム位置: {data.FamilyNamePosition}");
        sb.AppendLine($"作成日時: {data.CreatedAt}");
        sb.AppendLine($"最終更新日時: {data.LastUpdatedAt}");
        sb.AppendLine($"最終使用デバイスID: {data.LastActiveDeviceId}");
        sb.AppendLine("---");
        sb.AppendLine($"アクティブスロット数: {data.ActiveSlotCount}");
        sb.AppendLine($"レジェンドスロット数: {data.LegendSlotCount}");
        sb.AppendLine("---");
        sb.AppendLine($"育成ロビーリスト表示: {data.IsTrainingLobbyListView}");
        sb.AppendLine($"レジェンドロビーリスト表示: {data.IsLegendLobbyListView}");
        sb.AppendLine($"デイリーGem使用済み: {data.HasUsedDailyGem}");
        sb.AppendLine("---");
        sb.AppendLine($"遭遇済みライバルID数: {data.EncounteredRivalIds?.Count ?? 0}");
        sb.AppendLine($"所持アビリティID数: {data.UnlockedAbilityIds?.Count ?? 0}");
        sb.AppendLine($"所持ユニークアイテムID数: {data.UnlockedUniqueItemIds?.Count ?? 0}");
        sb.AppendLine($"所持アイテムインスタンスID数: {data.OwnedItemInstanceIds?.Count ?? 0}");
        sb.AppendLine($"ミッション進捗数: {data.MissionProgress?.Count ?? 0}");
        
        statsText.text = sb.ToString();
    }

    // --- ヘルパーメソッド ---

    private string FormatStat(int value) {
        return value == 0 ? "-" : value.ToString();
    }

    private string FormatMoney(long value) {
        return value == 0 ? "-" : value.ToString("N0");
    }

    private string FormatWinRate(int wins, int entries) {
        if (entries == 0) {
            return "-";
        }
        float winRate = (float)wins / entries;
        return winRate.ToString("P2");
    }

    private string FormatRaceRecord(List<RaceRecord> records) {
        if (records == null || records.All(r => r.Entries == 0)) {
            return "-";
        }
        long totalEntries = records.Sum(r => (long)r.Entries);
        long totalWins = records.Sum(r => (long)r.Wins);
        return $"{totalEntries}戦 {totalWins}勝";
    }

    private string FormatSuccession(int successions) {
        return successions == 0 ? "-" : (successions + 1).ToString() + "世代";
    }
}