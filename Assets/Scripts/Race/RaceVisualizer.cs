using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RaceVisualizer : MonoBehaviour {
    // --- 定数 ---
    private const float INITIAL_X = 0;
    private const float INITIAL_Y = 27f;
    private const float LANE_OFFSET_Y = -10f;
    private const int BASE_HORSE_SORT_ORDER = 11;
    private const int PIXELS_PER_METRE = 10;
    private const float SIMULATION_SCALE_FACTOR = 10.0f;
    private const int FRAME_PLAYBACK_SPEED = 10;

    private const int ZOOM_SECTION_UNITS = 50000;
    private const float ZOOM_MIN_SCALE = 0.75f;
    private const float ZOOM_MAX_SCALE = 1.5f;
    private const float ZOOM_OFFSET_MULTIPLIER = 150f;

    [Header("Component References")]
    [SerializeField] private RectTransform _containerRaceSet;
    [SerializeField] private RectTransform _containerMovableBG;
    
    [Header("Zoom Target")]
    [SerializeField] private RectTransform _viewRaceTransform;

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

        _containerRaceSet.localPosition = new Vector2(-100, _containerRaceSet.localPosition.y);

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

        // ★変更: 最初にズームを計算してscale値を取得
         float scale = UpdateRaceZoom(leadHorsePositionUnits);
        float zoomOffsetX = (ZOOM_MAX_SCALE - scale) * ZOOM_OFFSET_MULTIPLIER;
        float dynamicThresholdPx = 180f + (zoomOffsetX);

        int currentFollowTargetUnits = 0;
        float leadHorseAbsolutePx = leadHorsePositionUnits / SIMULATION_SCALE_FACTOR;

        if (leadHorseAbsolutePx >= dynamicThresholdPx) {
            int thresholdInUnits = (int)(dynamicThresholdPx * SIMULATION_SCALE_FACTOR);
            currentFollowTargetUnits = leadHorsePositionUnits - thresholdInUnits;
        }

if (_lockedFollowTargetUnits == null && _firstGoalFrame != -1 && _currentFrame >= _firstGoalFrame) {
    float startOffsetPx = _raceParameters.Distance * PIXELS_PER_METRE;
    currentFollowTargetUnits = (int)((startOffsetPx - 180f) * SIMULATION_SCALE_FACTOR);
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
        if (leadHorseAbsolutePx >= 180) {
            int thresholdInUnits = (int)(180 * SIMULATION_SCALE_FACTOR);
            followTargetUnits = leadHorsePositionUnits - thresholdInUnits;
        }

        UpdateRaceViewPosition(followTargetUnits);
        UpdateEachHorsePosition(finishFrameLog, followTargetUnits);
        _viewRaceTransform.localScale = new Vector3(1.5f, 1.5f, 1.0f);

    }

    private float UpdateRaceZoom(int leadHorsePositionUnits) {
        if (_viewRaceTransform == null || _raceParameters == null) return ZOOM_MAX_SCALE;

        float totalDistanceUnits = _raceParameters.Distance * 100;
        float remainingDistanceUnits = totalDistanceUnits - leadHorsePositionUnits;

        float scale = ZOOM_MAX_SCALE;
        bool inZoomSection = false;
        float progress = 0f;

        if (leadHorsePositionUnits < ZOOM_SECTION_UNITS) {
            progress = (float)leadHorsePositionUnits / ZOOM_SECTION_UNITS;
            inZoomSection = true;
        }
        else if (remainingDistanceUnits < ZOOM_SECTION_UNITS) {
            progress = 1.0f - (remainingDistanceUnits / ZOOM_SECTION_UNITS);
            inZoomSection = true;
        }

        if (inZoomSection) {
            float parabola = Mathf.Abs(progress - 0.5f) * 2.0f;
            scale = Mathf.Lerp(ZOOM_MIN_SCALE, ZOOM_MAX_SCALE, parabola);
        }
     
        _viewRaceTransform.localScale = new Vector3(scale, scale, 1.0f);
        return scale;
    }
}