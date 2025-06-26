using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RaceStageManager : MonoBehaviour {
    public static RaceStageManager Instance { get; private set; }

    [Header("Component References")]
    [SerializeField] private RaceVisualizer _raceVisualizer;
    
    [Header("Scene References")]
    [SerializeField] private RectTransform _containerCourse;
    [SerializeField] private RectTransform _containerGate;

    [Header("UI References")]
    [SerializeField] private Button _buttonStartRace;
    [SerializeField] private Button _buttonEndRace;

    private RaceSimulationResult _raceResult;
    private RaceParameters _raceParameters;

    private const int PIXELS_PER_METRE = 10;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        _buttonStartRace.onClick.AddListener(StartRaceAnimation);
        _buttonEndRace.onClick.AddListener(EndRace);
    }

    private void OnDestroy() {
        _buttonStartRace.onClick.RemoveListener(StartRaceAnimation);
        _buttonEndRace.onClick.RemoveListener(EndRace);
    }

    public void PrepareRace(RaceParameters raceParams, List<HorseData> contenders) {
        _raceParameters = raceParams;

        float startOffsetPx = raceParams.Distance * PIXELS_PER_METRE;
        _containerCourse.localPosition = new Vector2(startOffsetPx, _containerCourse.localPosition.y);
        _containerGate.localPosition = new Vector2(startOffsetPx * -1, _containerGate.localPosition.y);

        var input = new RaceSimulationInput {
            HorseCount = contenders.Count,
            DistanceInMetres = raceParams.Distance
        };
        
        var simulator = new RaceSimulator(input);
        _raceResult = simulator.RunSimulation();
        Debug.Log("Race simulation complete. Ready to visualize.");

        _raceVisualizer.PrepareVisualization(_raceResult, _raceParameters);
    }

    private void StartRaceAnimation() {
        Debug.Log("Race animation started.");
        _raceVisualizer.StartRaceAnimation();
    }

    public void OnRaceAnimationFinished() {
        Debug.Log("Race animation finished.");
    }

    private void EndRace() {
        LobbyManager.Instance.SwitchMode(LobbyManager.GameMode.CustomRace);
    }
}