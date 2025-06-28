using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RaceVisualizer : MonoBehaviour {
    // --- 定数 ---
    private const float INITIAL_X = 0;
    private const float INITIAL_Y = 30f;
    private const float LANE_OFFSET_Y = -12f;
    private const int BASE_HORSE_SORT_ORDER = 11;
    private const int PIXELS_PER_METRE = 10;
    private const float SIMULATION_SCALE_FACTOR = 10.0f;
    private const int FRAME_PLAYBACK_SPEED = 2; // ★追加: ログの再生速度（飛ばし係数）

    [Header("Component References")]
    [SerializeField] private RectTransform _containerRaceSet;
    [SerializeField] private RectTransform _containerMovableBG;

    private List<RectTransform> _horseTransforms = new List<RectTransform>();
    private List<Canvas> _horseCanvases = new List<Canvas>();

    private RaceSimulationResult _result;
    private RaceParameters _raceParameters;
    private int _currentFrame = 0;
    private bool _isPlaying = false;

    private int _firstGoalFrame = -1;
    private int? _lockedFollowTargetUnits = null;

    private void Awake() {
        _horseTransforms.Clear();
        _horseCanvases.Clear();
        
        foreach (Transform child in _containerRaceSet) {
            if (child.name.StartsWith("Horse_")) {
                _horseTransforms.Add(child.GetComponent<RectTransform>());
                _horseCanvases.Add(child.GetComponent<Canvas>());
            }
        }
    }

    public void PrepareVisualization(RaceSimulationResult result, RaceParameters raceParams) {
        _result = result;
        _raceParameters = raceParams;
        _currentFrame = 0;
        _isPlaying = false;

        _firstGoalFrame = -1;
        _lockedFollowTargetUnits = null;

        if (result != null && result.GoalTimesInFrames.Any(t => t > 0)) {
            _firstGoalFrame = result.GoalTimesInFrames.Where(t => t > 0).Min();
        }

        _containerRaceSet.localPosition = new Vector2(-150, 190);

        if (result == null || result.RaceLog.Count == 0) {
            foreach (var horseTransform in _horseTransforms) {
                horseTransform.gameObject.SetActive(false);
            }
            return;
        }

        int horseCount = result.RaceLog[0].Count;

        for (int i = 0; i < _horseTransforms.Count; i++) {
            bool isActive = i < horseCount;
            _horseTransforms[i].gameObject.SetActive(isActive);

            if (isActive) {
                _horseTransforms[i].anchoredPosition = new Vector2(INITIAL_X, INITIAL_Y + (LANE_OFFSET_Y * i));
                _horseCanvases[i].sortingOrder = BASE_HORSE_SORT_ORDER + (i * 2);
            }
        }
        
        UpdateHorsePositions();
    }

    public void StartRaceAnimation() {
        _isPlaying = true;
    }

    private void Update() {
        if (!_isPlaying) return;

        // ★変更: 定義した再生速度でフレームを進める
        _currentFrame += FRAME_PLAYBACK_SPEED;

        if (_currentFrame >= _result.RaceLog.Count) {
            _currentFrame = _result.RaceLog.Count - 1;
            _isPlaying = false;

            SnapToFinishLinePhoto();
            
            RaceStageManager.Instance.OnRaceAnimationFinished();
            return;
        }

        UpdateHorsePositions();
    }

    private void UpdateHorsePositions() {
        if (_result == null || _currentFrame >= _result.RaceLog.Count) return;

        var currentFrameLog = _result.RaceLog[_currentFrame];
        
        int leadHorsePositionUnits = 0;
        if (currentFrameLog.Count > 0) {
            leadHorsePositionUnits = currentFrameLog.Max(state => state.PositionX);
        }

        int currentFollowTargetUnits = 0;
        float leadHorseAbsolutePx = leadHorsePositionUnits / SIMULATION_SCALE_FACTOR;
        if (leadHorseAbsolutePx >= 270) {
            int thresholdInUnits = (int)(270 * SIMULATION_SCALE_FACTOR);
            currentFollowTargetUnits = leadHorsePositionUnits - thresholdInUnits;
        }

        if (_lockedFollowTargetUnits == null && _firstGoalFrame != -1 && _currentFrame >= _firstGoalFrame) {
            _lockedFollowTargetUnits = currentFollowTargetUnits;
        }

        int finalFollowTargetUnits = _lockedFollowTargetUnits ?? currentFollowTargetUnits;

        UpdateRaceViewPosition(finalFollowTargetUnits);
        UpdateEachHorsePosition(currentFrameLog, finalFollowTargetUnits);
    }

    private void UpdateRaceViewPosition(int leadHorsePositionX) {
        float startOffsetPx = _raceParameters.Distance * PIXELS_PER_METRE;
        float newCourseX = startOffsetPx - (leadHorsePositionX / SIMULATION_SCALE_FACTOR);
        _containerMovableBG.localPosition = new Vector2(newCourseX, _containerMovableBG.localPosition.y);
    }

    private void UpdateEachHorsePosition(List<FrameHorseState> currentFrameLog, int leadHorsePositionX) {
        for (int i = 0; i < currentFrameLog.Count; i++) {
            float localPosX = currentFrameLog[i].PositionX / SIMULATION_SCALE_FACTOR;
            float screenPosX = localPosX - (leadHorsePositionX / SIMULATION_SCALE_FACTOR);
            
            float localPosY = _horseTransforms[i].anchoredPosition.y;
            int sortingOrder = _horseCanvases[i].sortingOrder;

            _horseTransforms[i].anchoredPosition = new Vector2(screenPosX, localPosY);
            _horseCanvases[i].sortingOrder = sortingOrder;
        }
    }

    private void SnapToFinishLinePhoto() {
        if (_result == null || _firstGoalFrame == -1 || _firstGoalFrame >= _result.RaceLog.Count) {
            return;
        }

        var finishFrameLog = _result.RaceLog[_firstGoalFrame];

        int leadHorsePositionUnits = finishFrameLog.Max(state => state.PositionX);
        int followTargetUnits = 0;
        float leadHorseAbsolutePx = leadHorsePositionUnits / SIMULATION_SCALE_FACTOR;
        if (leadHorseAbsolutePx >= 270) {
            int thresholdInUnits = (int)(270 * SIMULATION_SCALE_FACTOR);
            followTargetUnits = leadHorsePositionUnits - thresholdInUnits;
        }

        UpdateRaceViewPosition(followTargetUnits);
        UpdateEachHorsePosition(finishFrameLog, followTargetUnits);
    }
}