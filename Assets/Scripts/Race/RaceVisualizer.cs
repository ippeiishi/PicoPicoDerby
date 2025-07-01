using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class RaceVisualizer : MonoBehaviour {
    //================================================================================
    // 定数
    //================================================================================
    private const float INITIAL_X = 0;
    private const float INITIAL_Y = 27f;
    private const float LANE_OFFSET_Y = -10f;
    private const int BASE_HORSE_SORT_ORDER = 11;
    private const int PIXELS_PER_METRE = 10;
    private const float SIMULATION_SCALE_FACTOR = 10.0f;
    private const int FRAME_PLAYBACK_SPEED = 5;
    private const int UNITS_PER_METRE = 100;
    private const int FINAL_STRETCH_METRES = 500;
    private const int FINAL_STRETCH_UNITS = FINAL_STRETCH_METRES * UNITS_PER_METRE;

    // --- Zoom ---
    private const int ZOOM_SECTION_UNITS = 50000;
    private const float ZOOM_MIN_SCALE = 0.75f;
    private const float ZOOM_MAX_SCALE = 1.5f;
    private const float ZOOM_OFFSET_MULTIPLIER = 150f;

    // --- Animation ---
    private const int RAIL_ANIM_CYCLE_UNITS = 260;
    private const int RAIL_ANIM_FRAME_COUNT = 4;

    // --- Curve ---
    private const int COURSE_SECTION_UNITS = 50000;
    private const float MAX_CURVE_DISPLACEMENT = 150f;
    private const float Y_OFFSET_COMPENSATION_RATIO = 0.25f;
    private const float FOREGROUND_RAIL_HALF_WIDTH_PX = 400f;
    private const int HURLON_POLE_INTERVAL_UNITS = 20000;
    private const float HURLON_POLE_Y_OFFSET = 35f;
    private const string INNER_RAIL_NAME = "Img_Inner_Rail";

    // --- Minimap ---
    private const int SUBCOURSE_SECTION_PX = 5000;
    private const float POINTER_X_OFFSET_PX = 240f;
    private const float LEFT_CURVE_X_PX = -260f;
    private const float RIGHT_CURVE_X_PX = 260f;

    //================================================================================
    // シリアライズされたフィールド
    //================================================================================
    [Header("Component References")]
    [SerializeField] private RectTransform _containerRaceSet;
    [SerializeField] private RectTransform _containerMovableBG;

    [Header("Zoom Target")]
    [SerializeField] private RectTransform _viewRaceTransform;

    [Header("Animation References")]
    [SerializeField] private Image _imgOuterRail;
    [SerializeField] private Image _imgInnerRail;
    [SerializeField] private List<Sprite> _spritesOuterRail;
    [SerializeField] private List<Sprite> _spritesInnerRail;

    [Header("Curve References")]
    [SerializeField] private List<CUIImage2> _curvedImages;

    [Header("Sub-Course (Minimap)")]
    [SerializeField] private RectTransform _subPointer;
    [SerializeField] private TextMeshProUGUI _txtSubCourseInfo;

    //================================================================================
    // イベント
    //================================================================================
    public event System.Action OnCameraStartedMoving;

    //================================================================================
    // プライベートフィールド
    //================================================================================
    private RectTransform _imgInnerRailRect;
    private List<RectTransform> _horseTransforms = new List<RectTransform>();
    private List<Canvas> _horseCanvases = new List<Canvas>();
    private RaceSimulationResult _result;
    private RaceParameters _raceParameters;
    private int _currentFrame = 0;
    private bool _isPlaying = false;
    private int _firstGoalFrame = -1;
    private int? _lockedFollowTargetUnits = null;
    private int? _lockedRailAnimPositionUnits = null;
    private bool[] _isCurveSection = { false, true, false, true, false, true, false, true, false, true };
    private List<Vector3[]> _initialCurvePointsList = new List<Vector3[]>();
    private List<Vector2> _initialAnchoredPositions = new List<Vector2>();
    private bool _hasCameraStartedMoving = false;
    private bool _hasSkipped = false;
    private int RL => (_raceParameters.Direction == TrackDirection.Left) ? 1 : -1;

    //================================================================================
    // Unityライフサイクルメソッド
    //================================================================================
    private void Awake() {
        _horseTransforms.Clear();
        _horseCanvases.Clear();

        foreach (Transform child in _containerRaceSet) {
            if (child.name.StartsWith("Horse_")) {
                _horseTransforms.Add(child.GetComponent<RectTransform>());
                _horseCanvases.Add(child.GetComponent<Canvas>());
            }
        }

        _initialCurvePointsList.Clear();
        _initialAnchoredPositions.Clear();
        foreach (var c_image in _curvedImages) {
            if (c_image != null) {
                if (c_image.RefCurves.Length == 2) {
                    _initialCurvePointsList.Add((Vector3[])c_image.RefCurves[0].ControlPoints.Clone());
                    _initialCurvePointsList.Add((Vector3[])c_image.RefCurves[1].ControlPoints.Clone());
                }
                _initialAnchoredPositions.Add(c_image.GetComponent<RectTransform>().anchoredPosition);
            }
        }

        if (_imgInnerRail != null) {
            _imgInnerRailRect = _imgInnerRail.GetComponent<RectTransform>();
        }
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

    //================================================================================
    // publicメソッド
    //================================================================================
    public void PrepareVisualization(RaceSimulationResult result, RaceParameters raceParams) {
        _result = result;
        _raceParameters = raceParams;
        _currentFrame = 0;
        _isPlaying = false;
        _firstGoalFrame = -1;
        _lockedFollowTargetUnits = null;
        _lockedRailAnimPositionUnits = null;
        _hasCameraStartedMoving = false;
        _hasSkipped = false;

        _isCurveSection = new bool[] { false, true, false, true, false, true, false, true, false, true };
        int distanceM = _raceParameters.Distance;

        if (distanceM == 1000) {
            _isCurveSection[1] = false;
        } else if (distanceM >= 1500 && distanceM <= 2000) {
            _isCurveSection[3] = false;
        } else if (distanceM >= 2500 && distanceM <= 3000) {
            _isCurveSection[5] = false;
        }

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

        ResetAllCurves();
        UpdateHorsePositions();
    }

    public void StartRaceAnimation() {
        _isPlaying = true;
    }

    public void SkipToFinalStretch() {
        if (_hasSkipped || !_isPlaying || _result == null) return;

        _currentFrame = FindFinalStretchFrame();
        _lockedFollowTargetUnits = null;
        _lockedRailAnimPositionUnits = null;
        UpdateHorsePositions();
        _hasSkipped = true;
    }

    //================================================================================
    // privateメソッド (レース更新処理)
    //================================================================================
    private void UpdateHorsePositions() {
        if (_result == null || _currentFrame >= _result.RaceLog.Count) return;

        var currentFrameLog = _result.RaceLog[_currentFrame];
        int leadHorsePositionUnits = 0;
        if (currentFrameLog.Count > 0) {
            leadHorsePositionUnits = currentFrameLog.Max(state => state.PositionX);
        }

        float scale = UpdateRaceZoom(leadHorsePositionUnits);
        float zoomOffsetX = (ZOOM_MAX_SCALE - scale) * ZOOM_OFFSET_MULTIPLIER;
        float dynamicThresholdPx = 180f + (zoomOffsetX);
        int currentFollowTargetUnits = 0;
        float leadHorseAbsolutePx = leadHorsePositionUnits / SIMULATION_SCALE_FACTOR;

        if (leadHorseAbsolutePx >= dynamicThresholdPx) {
            int thresholdInUnits = (int)(dynamicThresholdPx * SIMULATION_SCALE_FACTOR);
            currentFollowTargetUnits = leadHorsePositionUnits - thresholdInUnits;
            if (!_hasCameraStartedMoving) {
                OnCameraStartedMoving?.Invoke();
                _hasCameraStartedMoving = true;
            }
        }

        if (_lockedFollowTargetUnits == null && _firstGoalFrame != -1 && _currentFrame >= _firstGoalFrame) {
            float startOffsetPx = _raceParameters.Distance * PIXELS_PER_METRE;
            currentFollowTargetUnits = (int)((startOffsetPx - 180f) * SIMULATION_SCALE_FACTOR);
            _lockedFollowTargetUnits = currentFollowTargetUnits;
        }

        int finalFollowTargetUnits = _lockedFollowTargetUnits ?? currentFollowTargetUnits;
        UpdateRaceViewPosition(finalFollowTargetUnits);
        UpdateEachHorsePosition(currentFrameLog, finalFollowTargetUnits);
        UpdateRailAnimation(leadHorsePositionUnits, leadHorseAbsolutePx, dynamicThresholdPx);
        UpdateCurveEffect(leadHorsePositionUnits);
        UpdateMovableBGCurve(finalFollowTargetUnits);
        int remainingUnits = (_raceParameters.Distance * UNITS_PER_METRE) - leadHorsePositionUnits;
        UpdateSubCoursePointer(remainingUnits);
        if (remainingUnits <= FINAL_STRETCH_UNITS && RaceStageManager.Instance != null){
         RaceStageManager.Instance.HideSkipButton();
        }
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

    private float UpdateRaceZoom(int leadHorsePositionUnits) {
        if (_viewRaceTransform == null || _raceParameters == null) return ZOOM_MAX_SCALE;

        float directionMultiplier = (_raceParameters.Direction == TrackDirection.Right) ? -1.0f : 1.0f;
        float totalDistanceUnits = _raceParameters.Distance * UNITS_PER_METRE;
        float remainingDistanceUnits = totalDistanceUnits - leadHorsePositionUnits;
        float scale = ZOOM_MAX_SCALE;
        bool inZoomSection = false;
        float progress = 0f;

        if (leadHorsePositionUnits < ZOOM_SECTION_UNITS) {
            progress = (float)leadHorsePositionUnits / ZOOM_SECTION_UNITS;
            inZoomSection = true;
        } else if (remainingDistanceUnits < ZOOM_SECTION_UNITS) {
            progress = 1.0f - (remainingDistanceUnits / ZOOM_SECTION_UNITS);
            inZoomSection = true;
        }

        if (inZoomSection) {
            float parabola = Mathf.Abs(progress - 0.5f) * 2.0f;
            scale = Mathf.Lerp(ZOOM_MIN_SCALE, ZOOM_MAX_SCALE, parabola);
        }

        _viewRaceTransform.localScale = new Vector3(scale * directionMultiplier, scale, 1.0f);
        FindObjectOfType<CourseBuilder>()?.AdjustPoleTextDirection(directionMultiplier);
        return scale;
    }

    //================================================================================
    // privateメソッド (視覚効果)
    //================================================================================
    private void UpdateRailAnimation(int leadHorsePositionUnits, float leadHorseAbsolutePx, float dynamicThresholdPx) {
        if (_spritesOuterRail.Count == 0 || _spritesInnerRail.Count == 0) return;
        if (_lockedRailAnimPositionUnits != null) return;
        if (leadHorseAbsolutePx < dynamicThresholdPx) return;

        if (_firstGoalFrame != -1 && _currentFrame >= _firstGoalFrame) {
            _lockedRailAnimPositionUnits = leadHorsePositionUnits;
        }

        int positionForAnim = _lockedRailAnimPositionUnits ?? leadHorsePositionUnits;
        int unitsPerFrame = RAIL_ANIM_CYCLE_UNITS / RAIL_ANIM_FRAME_COUNT;
        int animationIndex = (positionForAnim / unitsPerFrame) % RAIL_ANIM_FRAME_COUNT;
        _imgOuterRail.sprite = _spritesOuterRail[animationIndex];
        _imgInnerRail.sprite = _spritesInnerRail[animationIndex];
    }

    private void UpdateCurveEffect(int leadHorsePositionUnits) {
        if (_curvedImages.Count == 0 || _raceParameters == null) return;

        int totalDistanceUnits = _raceParameters.Distance * UNITS_PER_METRE;
        int remainingUnits = totalDistanceUnits - leadHorsePositionUnits;
        if (remainingUnits < 0) remainingUnits = 0;

        int sectionIndex = remainingUnits / COURSE_SECTION_UNITS;
        if (sectionIndex >= _isCurveSection.Length || !_isCurveSection[sectionIndex]) {
            ResetAllCurves();
            return;
        }

        float progressRatio = (float)(remainingUnits % COURSE_SECTION_UNITS) / COURSE_SECTION_UNITS;
        float parabola = 1.0f - Mathf.Pow((progressRatio - 0.5f) * 2.0f, 2);
        float currentDisplacement = MAX_CURVE_DISPLACEMENT * parabola;

        for (int i = 0; i < _curvedImages.Count; i++) {
            var c_image = _curvedImages[i];
            if (c_image == null) continue;

            var rectTransform = c_image.GetComponent<RectTransform>();
            var initialPos = _initialAnchoredPositions[i];
            float offsetY = currentDisplacement * Y_OFFSET_COMPENSATION_RATIO;
            rectTransform.anchoredPosition = new Vector2(initialPos.x, initialPos.y - offsetY);

            var bottomCurve = c_image.RefCurves[0];
            var topCurve = c_image.RefCurves[1];
            var initialBottomPoints = _initialCurvePointsList[i * 2];
            var initialTopPoints = _initialCurvePointsList[i * 2 + 1];

            bottomCurve.ControlPoints[0].y = initialBottomPoints[0].y + currentDisplacement;
            bottomCurve.ControlPoints[3].y = initialBottomPoints[3].y + currentDisplacement;
            topCurve.ControlPoints[0].y = initialTopPoints[0].y + currentDisplacement;
            topCurve.ControlPoints[3].y = initialTopPoints[3].y + currentDisplacement;
            c_image.Refresh();
        }
    }

    private void UpdateMovableBGCurve(int cameraFollowTargetUnits) {
        if (_raceParameters == null) return;

        float roundedPos = Mathf.Round(cameraFollowTargetUnits / (float)HURLON_POLE_INTERVAL_UNITS) * HURLON_POLE_INTERVAL_UNITS;
        int nearestPoleAbsoluteUnits = (int)roundedPos;
        float relativePosUnits = nearestPoleAbsoluteUnits - cameraFollowTargetUnits;
        float relativePosPx = relativePosUnits / SIMULATION_SCALE_FACTOR;
        float correctedRelativePosPx = relativePosPx + _containerRaceSet.localPosition.x;

        if (Mathf.Abs(correctedRelativePosPx) > FOREGROUND_RAIL_HALF_WIDTH_PX) {
            return;
        }

        var innerRailImage = _curvedImages.FirstOrDefault(ci => ci != null && ci.name == INNER_RAIL_NAME);
        if (innerRailImage == null) return;

        var bottomCurve = innerRailImage.RefCurves[0];
        float railBottomY = SampleCurveYPosition(correctedRelativePosPx, _imgInnerRailRect, bottomCurve);
        _containerMovableBG.anchoredPosition = new Vector2(_containerMovableBG.anchoredPosition.x, railBottomY - HURLON_POLE_Y_OFFSET);
    }

    private void ResetAllCurves() {
        if (_curvedImages.Count == 0 || _initialCurvePointsList.Count == 0) return;

        for (int i = 0; i < _curvedImages.Count; i++) {
            var c_image = _curvedImages[i];
            if (c_image != null) {
                c_image.GetComponent<RectTransform>().anchoredPosition = _initialAnchoredPositions[i];
                var bottomPoints = _initialCurvePointsList[i * 2];
                var topPoints = _initialCurvePointsList[i * 2 + 1];
                for (int j = 0; j < c_image.RefCurves[0].ControlPoints.Length; j++) {
                    c_image.RefCurves[0].ControlPoints[j] = bottomPoints[j];
                    c_image.RefCurves[1].ControlPoints[j] = topPoints[j];
                }
                c_image.Refresh();
            }
        }
    }

    private float SampleCurveYPosition(float localX, RectTransform railRect, CUIBezierCurve bottomCurve) {
        const float HALF_WIDTH = 400f;
        float t = (localX + HALF_WIDTH) / (HALF_WIDTH * 2f);
        Vector3 p0 = bottomCurve.ControlPoints[0];
        Vector3 p1 = bottomCurve.ControlPoints[1];
        Vector3 p2 = bottomCurve.ControlPoints[2];
        Vector3 p3 = bottomCurve.ControlPoints[3];
        float omt = 1f - t;
        float localY = (omt * omt * omt * p0.y) + (3f * omt * omt * t * p1.y) + (3f * omt * t * t * p2.y) + (t * t * t * p3.y);
        return railRect.anchoredPosition.y + localY + (railRect.pivot.y * railRect.rect.height);
    }

    //================================================================================
    // privateメソッド (ミニマップ)
    //================================================================================
    private void UpdateSubCoursePointer(int remainingUnits) {
        if (_subPointer == null || _raceParameters == null) return;

        float remainingPx = remainingUnits / SIMULATION_SCALE_FACTOR;
        if (remainingPx < 0) remainingPx = 0;

        if (_raceParameters.Distance <= 2000 && Mathf.Approximately(remainingPx % SUBCOURSE_SECTION_PX, 0f)) {
            remainingPx -= 0.1f;
        }

        int distanceM = _raceParameters.Distance;
        int sectionIndex = (int)(remainingPx / SUBCOURSE_SECTION_PX);
        int r = 500 - (int)((int)remainingPx % SUBCOURSE_SECTION_PX * 0.1f);

        if (sectionIndex == 5 && distanceM >= 2500 && distanceM <= 3000) {
            _subPointer.anchoredPosition = new Vector2(r - 1000 + POINTER_X_OFFSET_PX, 0);
            _subPointer.localEulerAngles = Vector3.zero;
            return;
        }
        if (sectionIndex == 3 && distanceM >= 1500 && distanceM <= 2000) {
            _subPointer.anchoredPosition = new Vector2(POINTER_X_OFFSET_PX + 500 - r, 0);
            _subPointer.localEulerAngles = new Vector3(0, 0, 180);
            return;
        }
        if (sectionIndex == 1 && distanceM == 1000) {
            _subPointer.anchoredPosition = new Vector2(r - 1000 + POINTER_X_OFFSET_PX, 0);
            _subPointer.localEulerAngles = Vector3.zero;
            return;
        }

        switch (sectionIndex) {
            case 0:
            case 4:
            case 8:
                _subPointer.anchoredPosition = new Vector2(r - 500 + POINTER_X_OFFSET_PX, 0);
                _subPointer.localEulerAngles = Vector3.zero;
                break;
            case 1:
            case 5:
            case 9:
                _subPointer.anchoredPosition = new Vector2(LEFT_CURVE_X_PX, 0);
                _subPointer.localEulerAngles = new Vector3(0, 0, (int)(r * 0.36f) * RL + 180);
                break;
            case 2:
            case 6:
            case 10:
                _subPointer.anchoredPosition = new Vector2(POINTER_X_OFFSET_PX - r, 0);
                _subPointer.localEulerAngles = new Vector3(0, 0, 180);
                break;
            default:
                _subPointer.anchoredPosition = new Vector2(RIGHT_CURVE_X_PX, 0);
                _subPointer.localEulerAngles = new Vector3(0, 0, (int)(r * 0.36f) * RL);
                break;
        }

        if (_txtSubCourseInfo != null && _result != null && _currentFrame < _result.RaceLog.Count) {
            int goalFrame = _result.GoalTimesInFrames[0];
            if (goalFrame != -1 && _currentFrame > goalFrame) return;

            int remainingMetres;
            if (remainingUnits <= 0) {
                remainingMetres = 0;
            } else {
                remainingMetres = Mathf.CeilToInt(remainingUnits / (float)UNITS_PER_METRE);
            }

            float elapsedSeconds = goalFrame != -1 && _currentFrame > goalFrame ? goalFrame / 60f : _currentFrame / 60f;
            int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
            float seconds = elapsedSeconds % 60f;
            _txtSubCourseInfo.text = $"残り {remainingMetres}m\nタイム {minutes}:{seconds:00.0}";
        }
    }

    //================================================================================
    // privateメソッド (その他)
    //================================================================================
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
        float directionMultiplier = (_raceParameters.Direction == TrackDirection.Right) ? -1.0f : 1.0f;
        _viewRaceTransform.localScale = new Vector3(1.5f * directionMultiplier, 1.5f, 1.0f);
    }

    private int FindFinalStretchFrame() {
        if (_result == null) return 0;

        int totalUnits = _raceParameters.Distance * UNITS_PER_METRE;
        for (int f = 0; f < _result.RaceLog.Count; f++) {
            int leadUnits = _result.RaceLog[f].Max(s => s.PositionX);
            int remaining = totalUnits - leadUnits;
            if (remaining <= FINAL_STRETCH_UNITS) {
                return f;
            }
        }
        return _result.RaceLog.Count - 1;
    }
}