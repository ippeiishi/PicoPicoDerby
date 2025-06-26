using System.Collections.Generic;
using UnityEngine;

// --- データ構造 ---

/// <summary>
/// レースシミュレーションの基本入力パラメータ。
/// </summary>
public class RaceSimulationInput {
    public int HorseCount { get; set; }
    public int DistanceInMetres { get; set; }
}

/// <summary>
/// 1フレームにおける、1頭の馬の状態。
/// </summary>
public class FrameHorseState {
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public int SortingOrder { get; set; }
}

/// <summary>
/// レースシミュレーションの結果。全フレームの全馬の軌跡データを含む。
/// </summary>
public class RaceSimulationResult {
    public List<List<FrameHorseState>> RaceLog { get; private set; }
    public List<int> GoalTimesInFrames { get; private set; }

    public RaceSimulationResult(int horseCount) {
        RaceLog = new List<List<FrameHorseState>>();
        GoalTimesInFrames = new List<int>(new int[horseCount]);
        for (int i = 0; i < horseCount; i++) {
            GoalTimesInFrames[i] = -1;
        }
    }
}


// --- シミュレーター ---

/// <summary>
/// 旧ピコダビのロジック核心部を継承したレースシミュレーター。
/// 全フレームの全馬の位置と深度を事前に計算し、ログとして出力する。
/// </summary>
public class RaceSimulator {
    // --- 定数 ---
    private const float BASE_SPEED_PX_PER_FRAME = 0.2f;
    private const int PIXELS_PER_METRE = 10;

    private readonly RaceSimulationInput _input;

    public RaceSimulator(RaceSimulationInput input) {
        _input = input;
    }

    public RaceSimulationResult RunSimulation() {
        int totalDistancePx = _input.DistanceInMetres * PIXELS_PER_METRE;
        var result = new RaceSimulationResult(_input.HorseCount);
        
        var currentStates = new List<FrameHorseState>();
        for (int i = 0; i < _input.HorseCount; i++) {
            currentStates.Add(new FrameHorseState { 
                PositionX = 0,
                PositionY = 0,
                SortingOrder = 0
            });
        }

        var initialFrameLog = new List<FrameHorseState>();
        foreach (var state in currentStates) {
            initialFrameLog.Add(new FrameHorseState { 
                PositionX = state.PositionX,
                PositionY = state.PositionY,
                SortingOrder = state.SortingOrder
            });
        }
        result.RaceLog.Add(initialFrameLog);

        int finishedCount = 0;
        int currentFrame = 0;

        while (finishedCount < _input.HorseCount) {
            currentFrame++;
            
            var frameLog = new List<FrameHorseState>();

            for (int i = 0; i < _input.HorseCount; i++) {
                if (result.GoalTimesInFrames[i] == -1) {
                    float frameDistance = BASE_SPEED_PX_PER_FRAME + (Random.Range(0, 4) * 0.1f);
                    currentStates[i].PositionX += frameDistance;

                    if (currentStates[i].PositionX >= totalDistancePx) {
                        // ゴールしたフレームでは、座標をゴールラインに完全に一致させる
                        currentStates[i].PositionX = totalDistancePx;
                        
                        result.GoalTimesInFrames[i] = currentFrame;
                        finishedCount++;
                    }
                }
                
                frameLog.Add(new FrameHorseState {
                    PositionX = currentStates[i].PositionX,
                    PositionY = currentStates[i].PositionY,
                    SortingOrder = currentStates[i].SortingOrder
                });
            }
            
            result.RaceLog.Add(frameLog);
        }
        return result;
    }
}