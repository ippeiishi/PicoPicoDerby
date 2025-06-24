using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RaceVisualizer : MonoBehaviour {
    [Header("Object References")]
    [SerializeField] private GameObject _horsePrefab;
    [SerializeField] private Transform _horsesParent;
    [SerializeField] private Transform _goalPostTransform; // ★ゴール板のTransform

    [Header("Visual Settings")]
    [SerializeField] private float _laneHeight = 0.5f;
    [SerializeField] private float _pixelsPerUnit = 100f;

    [Header("Camera Control")]
    [SerializeField] private Camera _raceCamera;
    [SerializeField] private float _fixedOrthographicSize = 5f;

    [Header("Playback Settings")]
    [SerializeField] private float _framesPerSecond = 30.0f;

    private List<GameObject> _horseObjects = new List<GameObject>();
    private RaceSimulationResult _result;
    private int _currentFrame = 0;
    private bool _isPlaying = false;
    private float _frameTimer = 0f;

    public void PrepareVisualization(RaceSimulationResult result, RaceParameters raceParams) {
        _result = result;
        _currentFrame = 0;
        _isPlaying = false;
        _frameTimer = 0f;

        int horseCount = result.RaceLog[0].Count;

        // ゴール板の位置を設定し、カメラを初期位置へ
        SetupScene(raceParams.Distance);

        // 馬の生成
        foreach (var horse in _horseObjects) { Destroy(horse); }
        _horseObjects.Clear();

        for (int i = 0; i < horseCount; i++) {
            Vector3 startPos = new Vector3(0, (_laneHeight * (horseCount - 1) / 2) - (_laneHeight * i), -1);
            GameObject horse = Instantiate(_horsePrefab, startPos, Quaternion.identity, _horsesParent);
            _horseObjects.Add(horse);
        }

        UpdateHorsePositions();
    }

    public void StartRaceAnimation() {
        _isPlaying = true;
    }

    private void UpdateHorsePositions() {
        if (_result == null || _currentFrame >= _result.RaceLog.Count) return;

        for (int i = 0; i < _horseObjects.Count; i++) {
            float posX = _result.RaceLog[_currentFrame][i].TotalDistancePx / _pixelsPerUnit;
            Vector3 currentPos = _horseObjects[i].transform.position;
            _horseObjects[i].transform.position = new Vector3(posX, currentPos.y, currentPos.z);
        }
    }

    void Update() {
        if (!_isPlaying) return;

        _frameTimer += Time.deltaTime;
        float timePerFrame = 1.0f / _framesPerSecond;

        while (_frameTimer >= timePerFrame) {
            _frameTimer -= timePerFrame;

            if (_currentFrame >= _result.RaceLog.Count - 1) {
                _isPlaying = false;
                UpdateHorsePositions();
                RaceSceneController.Instance.OnRaceAnimationFinished();
                return;
            }
            _currentFrame++;
        }
        
        UpdateHorsePositions();
    }

    void LateUpdate() {
        if (!_isPlaying) return;
        if (_raceCamera == null || _result == null || _result.RaceLog.Count == 0) return;

        float leadHorsePx = _result.RaceLog[_currentFrame].Max(horseState => horseState.TotalDistancePx);
        float leadHorsePosX = leadHorsePx / _pixelsPerUnit;

        _raceCamera.transform.position = new Vector3(leadHorsePosX, 0, -10);
    }

    private void SetupScene(int distanceInMetres) {
        // 1. ゴール板の位置を設定
        if (_goalPostTransform != null) {
            // シミュレーション上のゴール位置を計算 (1m = 50px)
            float goalDistancePx = distanceInMetres * 50;
            // ワールド座標に変換
            float goalPosX = goalDistancePx / _pixelsPerUnit;
            _goalPostTransform.position = new Vector3(goalPosX, _goalPostTransform.position.y, _goalPostTransform.position.z);
        }

        // 2. カメラのサイズと初期位置を設定
        if (_raceCamera != null) {
            _raceCamera.orthographicSize = _fixedOrthographicSize;
            // 初期位置はスタート地点(X=0)を映す
            _raceCamera.transform.position = new Vector3(0, 0, -10);
        }
    }
}