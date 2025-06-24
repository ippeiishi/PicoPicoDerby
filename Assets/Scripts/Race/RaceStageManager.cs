using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RaceStageManager : MonoBehaviour {
    public static RaceStageManager Instance { get; private set; }

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

        _buttonStartRace.onClick.AddListener(StartRaceAnimation);
        _buttonEndRace.onClick.AddListener(EndRace);
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
    }

    private void StartRaceAnimation() {
        Debug.Log("Race animation started.");
        // ボタンの表示状態は変更しない
    }

    public void OnRaceAnimationFinished() {
        Debug.Log("Race animation finished.");
        // ボタンの表示状態は変更しない
    }

    private void EndRace() {
        LobbyManager.Instance.SwitchMode(LobbyManager.GameMode.CustomRace);
    }
}