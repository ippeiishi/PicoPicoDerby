using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RaceVisualizer))]
public class RaceSceneController : MonoBehaviour {
    public static RaceSceneController Instance { get; private set; }

    private RaceVisualizer _raceVisualizer;

    [Header("Race UI")]
    [SerializeField] private Button _buttonStartRace;
    [SerializeField] private Button _buttonEndRace;

    private RaceSimulationResult _raceResult;
    private RaceParameters _raceParameters;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        _raceVisualizer = GetComponent<RaceVisualizer>();

        _buttonStartRace.onClick.AddListener(StartRaceAnimation);
        _buttonEndRace.onClick.AddListener(EndRace);
    }

    void OnEnable() {
        _buttonStartRace.gameObject.SetActive(true);
        _buttonEndRace.gameObject.SetActive(false);
    }

    private void OnDestroy() {
        _buttonStartRace.onClick.RemoveListener(StartRaceAnimation);
        _buttonEndRace.onClick.RemoveListener(EndRace);
    }

    public void PrepareRace(RaceParameters raceParams, List<HorseData> contenders) {
        _raceParameters = raceParams;

        var input = new RaceSimulationInput {
            HorseCount = contenders.Count,
            DistanceInMetres = raceParams.Distance
        };
        var simulator = new SimpleRaceSimulator(input);
        _raceResult = simulator.RunSimulation();
        Debug.Log("Race simulation complete. Ready to visualize.");

        _raceVisualizer.PrepareVisualization(_raceResult, _raceParameters);
    }

    private void StartRaceAnimation() {
        _buttonStartRace.gameObject.SetActive(false);
        _raceVisualizer.StartRaceAnimation();
    }

    public void OnRaceAnimationFinished() {
        Debug.Log("Race animation finished.");
        _buttonEndRace.gameObject.SetActive(true);
    }

    private void EndRace() {
        LobbyManager.Instance.SwitchMode(LobbyManager.GameMode.CustomRace);
    }
}