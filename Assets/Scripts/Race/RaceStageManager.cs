using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class RaceStageManager : MonoBehaviour {
    //================================================================================
    // シングルトン
    //================================================================================
    public static RaceStageManager Instance { get; private set; }

    //================================================================================
    // シリアライズされたフィールド
    //================================================================================
    [Header("Component References")]
    [SerializeField] private RaceVisualizer _raceVisualizer;

    [Header("Scene References")]
    [SerializeField] private RectTransform _containerMovableBG;
    [SerializeField] private RectTransform _containerGate;

    [Header("UI References")]
    [SerializeField] private Button _btnStartRace;
    [SerializeField] private Button _btnEndRace;
    [SerializeField] private Button _btnSkip;

    //================================================================================
    // プライベートフィールド
    //================================================================================
    private RaceSimulationResult _raceResult;
    private RaceParameters _raceParameters;
    private bool _btnSkipShown = false;
    private const int PIXELS_PER_METRE = 10;

    //================================================================================
    // Unityライフサイクルメソッド
    //================================================================================
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnEnable() {
        _raceVisualizer.OnCameraStartedMoving += ShowSkipButton;
        _btnStartRace.onClick.AddListener(StartRaceAnimation);
        _btnEndRace.onClick.AddListener(EndRace);
        _btnSkip.onClick.AddListener(OnSkip);
    }

    private void OnDisable() {
        _raceVisualizer.OnCameraStartedMoving -= ShowSkipButton;
        _btnStartRace.onClick.RemoveListener(StartRaceAnimation);
        _btnEndRace.onClick.RemoveListener(EndRace);
        _btnSkip.onClick.RemoveListener(OnSkip);
    }

    //================================================================================
    // publicメソッド
    //================================================================================
    public void PrepareRace(RaceParameters raceParams, List<HorseData> contenders) {
        _raceParameters = raceParams;
        _btnSkip.gameObject.SetActive(false);
        _btnSkipShown = false;

        float startOffsetPx = raceParams.Distance * PIXELS_PER_METRE;
        _containerMovableBG.localPosition = new Vector2(startOffsetPx, 0);
        _containerGate.localPosition = new Vector2(startOffsetPx * -1, 0);

        var input = new RaceSimulationInput {
            HorseCount = contenders.Count,
            DistanceInMetres = raceParams.Distance
        };

        var simulator = new RaceSimulator(input);
        _raceResult = simulator.RunSimulation();

        string goalFramesLog = string.Join("f|", _raceResult.GoalTimesInFrames.Select(f => f.ToString()));
        Debug.Log($"Goal Frames: {goalFramesLog}f");

        _raceVisualizer.PrepareVisualization(_raceResult, _raceParameters);
    }

    public void StartRaceAnimation() {
        Debug.Log("Race animation started.");
        _raceVisualizer.StartRaceAnimation();
    }

    public void OnRaceAnimationFinished() {
        Debug.Log("Race animation finished.");
        HideSkipButton();
    }

    public void ShowSkipButton() {
        if (!_btnSkipShown) {
            _btnSkip.gameObject.SetActive(true);
            _btnSkipShown = true;
        }
    }

    public void HideSkipButton() {
        if (_btnSkipShown) {
            _btnSkip.gameObject.SetActive(false);
            _btnSkipShown = false;
        }
    }

    //================================================================================
    // privateメソッド (UIイベントハンドラ)
    //================================================================================
    private void OnSkip() {
        _raceVisualizer.SkipToFinalStretch();
        HideSkipButton();
    }

    private void EndRace() {
        LobbyManager.Instance.SwitchMode(LobbyManager.GameMode.CustomRace);
    }
}