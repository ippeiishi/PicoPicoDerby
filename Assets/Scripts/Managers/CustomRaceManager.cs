using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CustomRaceManager : MonoBehaviour {
    [Header("UI Panels")]
    [SerializeField] private GameObject _panelRaceSettings;
    [SerializeField] private GameObject _panelHorseSelection;

    [Header("Navigation Buttons")]
    [SerializeField] private Button _navigateToHorseSelectionButton;
    [SerializeField] private Button _backToRaceSettingsButton;
    [SerializeField] private Button _goToRaceButton;

    [Header("Data Containers")]
    [SerializeField] private RaceParameters _raceParameters;
    // private List<Contender> _selectedContenders;
    private List<HorseData> _mockHorseDatabase;

    [Header("Race Settings Toggles")]
    [SerializeField] private Toggle _toggleTurf;
    [SerializeField] private Toggle _toggleDirt;
    [SerializeField] private Toggle _toggleLeft;
    [SerializeField] private Toggle _toggleRight;
    [SerializeField] private Toggle _toggle1200m;
    [SerializeField] private Toggle _toggle1600m;
    [SerializeField] private Toggle _toggle2000m;
    [SerializeField] private Toggle _toggle2400m;
    [SerializeField] private Toggle _toggle3000m;
    [SerializeField] private Toggle _toggleSunny;
    [SerializeField] private Toggle _toggleCloudy;
    [SerializeField] private Toggle _toggleRain;
    [SerializeField] private Toggle _toggleStorm;
    [SerializeField] private Toggle _toggleSnow;
    [SerializeField] private Toggle _toggleFirm;
    [SerializeField] private Toggle _toggleGood;
    [SerializeField] private Toggle _toggleYielding;
    [SerializeField] private Toggle _toggleSoft;

    void Awake() {
        _raceParameters = new RaceParameters();
        // _selectedContenders = new List<Contender>();
        CreateMockHorseDatabase();
        RegisterToggleListeners();

        _navigateToHorseSelectionButton.onClick.AddListener(NavigateToHorseSelection);
        _backToRaceSettingsButton.onClick.AddListener(NavigateToRaceSettings);
        _goToRaceButton.onClick.AddListener(NavigateToRaceScene);
    }

    void OnDestroy() {
        _navigateToHorseSelectionButton.onClick.RemoveListener(NavigateToHorseSelection);
        _backToRaceSettingsButton.onClick.RemoveListener(NavigateToRaceSettings);
        _goToRaceButton.onClick.RemoveListener(NavigateToRaceScene);
    }
void OnEnable() {
    _panelRaceSettings.SetActive(true);
    _panelHorseSelection.SetActive(false);

    // トグルの現在状態に応じてRaceParametersを初期化
    if (_toggleTurf.isOn) OnTrackTypeChanged(TrackType.Turf);
    if (_toggleDirt.isOn) OnTrackTypeChanged(TrackType.Dirt);

    if (_toggleLeft.isOn) OnDirectionChanged(TrackDirection.Left);
    if (_toggleRight.isOn) OnDirectionChanged(TrackDirection.Right);

    if (_toggle1200m.isOn) OnDistanceChanged(1200);
    if (_toggle1600m.isOn) OnDistanceChanged(1600);
    if (_toggle2000m.isOn) OnDistanceChanged(2000);
    if (_toggle2400m.isOn) OnDistanceChanged(2400);
    if (_toggle3000m.isOn) OnDistanceChanged(3000);

    if (_toggleSunny.isOn) OnWeatherChanged(WeatherType.Sunny);
    if (_toggleCloudy.isOn) OnWeatherChanged(WeatherType.Cloudy);
    if (_toggleRain.isOn) OnWeatherChanged(WeatherType.Rain);
    if (_toggleStorm.isOn) OnWeatherChanged(WeatherType.Storm);
    if (_toggleSnow.isOn) OnWeatherChanged(WeatherType.Snow);

    if (_toggleFirm.isOn) OnTrackConditionChanged(TrackCondition.Firm);
    if (_toggleGood.isOn) OnTrackConditionChanged(TrackCondition.Good);
    if (_toggleYielding.isOn) OnTrackConditionChanged(TrackCondition.Yielding);
    if (_toggleSoft.isOn) OnTrackConditionChanged(TrackCondition.Soft);
}


    private void RegisterToggleListeners() {
        _toggleTurf.onValueChanged.AddListener(isOn => { if (isOn) OnTrackTypeChanged(TrackType.Turf); });
        _toggleDirt.onValueChanged.AddListener(isOn => { if (isOn) OnTrackTypeChanged(TrackType.Dirt); });
        _toggleLeft.onValueChanged.AddListener(isOn => { if (isOn) OnDirectionChanged(TrackDirection.Left); });
        _toggleRight.onValueChanged.AddListener(isOn => { if (isOn) OnDirectionChanged(TrackDirection.Right); });
        _toggle1200m.onValueChanged.AddListener(isOn => { if (isOn) OnDistanceChanged(1200); });
        _toggle1600m.onValueChanged.AddListener(isOn => { if (isOn) OnDistanceChanged(1600); });
        _toggle2000m.onValueChanged.AddListener(isOn => { if (isOn) OnDistanceChanged(2000); });
        _toggle2400m.onValueChanged.AddListener(isOn => { if (isOn) OnDistanceChanged(2400); });
        _toggle3000m.onValueChanged.AddListener(isOn => { if (isOn) OnDistanceChanged(3000); });
        _toggleSunny.onValueChanged.AddListener(isOn => { if (isOn) OnWeatherChanged(WeatherType.Sunny); });
        _toggleCloudy.onValueChanged.AddListener(isOn => { if (isOn) OnWeatherChanged(WeatherType.Cloudy); });
        _toggleRain.onValueChanged.AddListener(isOn => { if (isOn) OnWeatherChanged(WeatherType.Rain); });
        _toggleStorm.onValueChanged.AddListener(isOn => { if (isOn) OnWeatherChanged(WeatherType.Storm); });
        _toggleSnow.onValueChanged.AddListener(isOn => { if (isOn) OnWeatherChanged(WeatherType.Snow); });
        _toggleFirm.onValueChanged.AddListener(isOn => { if (isOn) OnTrackConditionChanged(TrackCondition.Firm); });
        _toggleGood.onValueChanged.AddListener(isOn => { if (isOn) OnTrackConditionChanged(TrackCondition.Good); });
        _toggleYielding.onValueChanged.AddListener(isOn => { if (isOn) OnTrackConditionChanged(TrackCondition.Yielding); });
        _toggleSoft.onValueChanged.AddListener(isOn => { if (isOn) OnTrackConditionChanged(TrackCondition.Soft); });
    }

    private void OnTrackTypeChanged(TrackType type) { _raceParameters.TrackType = type; }
    private void OnDirectionChanged(TrackDirection direction) { _raceParameters.Direction = direction; }
    private void OnDistanceChanged(int distance) { _raceParameters.Distance = distance; }
    private void OnWeatherChanged(WeatherType weather) { _raceParameters.Weather = weather; }
    private void OnTrackConditionChanged(TrackCondition condition) { _raceParameters.TrackCondition = condition; }

    private void NavigateToHorseSelection() {
        _panelRaceSettings.SetActive(false);
        _panelHorseSelection.SetActive(true);
    }

    private void NavigateToRaceSettings() {
        _panelHorseSelection.SetActive(false);
        _panelRaceSettings.SetActive(true);
    }

private void NavigateToRaceScene() {
    Debug.Log("[CustomRaceManager] Preparing race...");

    var contenders = _mockHorseDatabase.Take(5).ToList();

    LobbyManager.Instance.SwitchMode(LobbyManager.GameMode.Race);

    // PrepareRace は1回だけ
    RaceStageManager.Instance.PrepareRace(_raceParameters, contenders);
}

private void CreateMockHorseDatabase() {
    _mockHorseDatabase = new List<HorseData> {
        new HorseData { Uid = "mock-001", Name = "サイレンススズカ", sp = 100, st = 0, gt = 100, mn = 100, ft = 0 },
        new HorseData { Uid = "mock-002", Name = "ディープインパクト", sp = 100, st = 0, gt = 100, mn = 100, ft = 1 },
        new HorseData { Uid = "mock-003", Name = "オグリキャップ",     sp = 100, st = 0, gt = 100, mn = 100, ft = 2 },
        new HorseData { Uid = "mock-004", Name = "アーモンドアイ",     sp = 100, st = 0, gt = 100, mn = 100, ft = 3 },
        new HorseData { Uid = "mock-005", Name = "キタサンブラック",   sp = 100, st = 0, gt = 100, mn = 100, ft = 4 }
    };
}

}