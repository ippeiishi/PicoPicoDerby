using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RaceVisualizer : MonoBehaviour {
    // --- 定数 ---
    private const float INITIAL_X = 0;
    private const float INITIAL_Y = 30f;
    private const float LANE_OFFSET_Y = -12f;
    private const int FRAME_INCREMENT = 3;
    private const int BASE_HORSE_SORT_ORDER = 11;
    private const int PIXELS_PER_METRE = 10;

    [Header("Component References")]
    [SerializeField] private RectTransform _containerCourse;
    [SerializeField] private RectTransform _containerHorses;
    
    private List<RectTransform> _horseTransforms = new List<RectTransform>();
    private List<Canvas> _horseCanvases = new List<Canvas>();

    private RaceSimulationResult _result;
    private RaceParameters _raceParameters;
    private int _currentFrame = 0;
    private bool _isPlaying = false;

    private void Awake() {
        _horseTransforms.Clear();
        _horseCanvases.Clear();
        foreach (Transform child in _containerHorses) {
            _horseTransforms.Add(child.GetComponent<RectTransform>());
            _horseCanvases.Add(child.GetComponent<Canvas>());
        }
    }

    public void PrepareVisualization(RaceSimulationResult result, RaceParameters raceParams) {
        _result = result;
        _raceParameters = raceParams;
        _currentFrame = 0;
        _isPlaying = false;

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

        _currentFrame += FRAME_INCREMENT;

        if (_currentFrame >= _result.RaceLog.Count) {
            _currentFrame = _result.RaceLog.Count - 1;
            _isPlaying = false;
            RaceStageManager.Instance.OnRaceAnimationFinished();
        }

        UpdateHorsePositions();
    }

    private void UpdateHorsePositions() {
        if (_result == null || _currentFrame >= _result.RaceLog.Count) return;

        var currentFrameLog = _result.RaceLog[_currentFrame];
        
        float leadHorsePositionX = 0;
        if (currentFrameLog.Count > 0) {
            leadHorsePositionX = currentFrameLog.Max(state => state.PositionX);
        }

        UpdateCoursePosition(leadHorsePositionX);
        UpdateEachHorsePosition(currentFrameLog, leadHorsePositionX);
    }

    private void UpdateCoursePosition(float leadHorsePositionX) {
        float startOffsetPx = _raceParameters.Distance * PIXELS_PER_METRE;
        float newCourseX = startOffsetPx - leadHorsePositionX;
        _containerCourse.localPosition = new Vector2(newCourseX, _containerCourse.localPosition.y);
    }

    private void UpdateEachHorsePosition(List<FrameHorseState> currentFrameLog, float leadHorsePositionX) {
        for (int i = 0; i < currentFrameLog.Count; i++) {
            float screenPosX = currentFrameLog[i].PositionX - leadHorsePositionX;
            float screenPosY = _horseTransforms[i].anchoredPosition.y;
            int sortingOrder = _horseCanvases[i].sortingOrder;

            _horseTransforms[i].anchoredPosition = new Vector2(screenPosX, screenPosY);
            _horseCanvases[i].sortingOrder = sortingOrder;
        }
    }
}