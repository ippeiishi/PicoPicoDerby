// RaceLogic.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// レースシミュレーションの基本入力パラメータ。
/// </summary>
public class RaceSimulationInput
{
    /// <summary>
    /// 出走頭数。
    /// </summary>
    public int HorseCount { get; set; }

    /// <summary>
    /// レース距離（メートル単位）。
    /// </summary>
    public int DistanceInMetres { get; set; }
}

/// <summary>
/// 1フレームにおける、1頭の馬の状態。
/// </summary>
public class FrameHorseState
{
    /// <summary>
    /// レース開始からの総移動距離（ピクセル単位）。
    /// </summary>
    public float TotalDistancePx { get; set; }

    // 将来の拡張用:
    // public int PositionY { get; set; }
    // public BoostStateType BoostState { get; set; }
}

/// <summary>
/// レースシミュレーションの結果。全フレームの全馬の軌跡データを含む。
/// </summary>
public class RaceSimulationResult
{
    /// <summary>
    /// 全フレームの馬の状態を記録したリスト。
    /// 外側のリストがフレーム(インデックス=フレーム数-1)、内側のリストが馬(インデックス=馬ID)に対応する。
    /// 例: RaceLog[10][3] は、11フレーム目における4番目の馬の状態。
    /// </summary>
    public List<List<FrameHorseState>> RaceLog { get; private set; }

    /// <summary>
    /// 各馬のゴールタイム（フレーム数）。
    /// </summary>
    public List<int> GoalTimesInFrames { get; private set; }

    public RaceSimulationResult(int horseCount)
    {
        RaceLog = new List<List<FrameHorseState>>();
        GoalTimesInFrames = new List<int>(new int[horseCount]);
        for (int i = 0; i < horseCount; i++)
        {
            GoalTimesInFrames[i] = -1;
        }
    }
}

/// <summary>
/// 「ランダムな揺らぎを持つ基礎速度」のみを考慮した、最もシンプルなレースシミュレーター。
/// </summary>
public class SimpleRaceSimulator
{
    // --- 定数 ---
    private const int METRES_TO_PIXELS_RATE = 50;
    private const float TIME_ADJUSTMENT_FACTOR = 0.127f; // 旧コードの係数
    private const int BASE_SPEED_UNIT = 100;

    private readonly RaceSimulationInput _input;

    public SimpleRaceSimulator(RaceSimulationInput input)
    {
        _input = input;
    }

    public RaceSimulationResult RunSimulation()
    {
        int totalDistancePx = _input.DistanceInMetres * METRES_TO_PIXELS_RATE;
        var result = new RaceSimulationResult(_input.HorseCount);
        
        // 現在の各馬の状態を保持するリスト
        var currentStates = new List<FrameHorseState>();
        for (int i = 0; i < _input.HorseCount; i++)
        {
            currentStates.Add(new FrameHorseState { TotalDistancePx = 0 });
        }

        int finishedCount = 0;
        int currentFrame = 0;

        while (finishedCount < _input.HorseCount)
        {
            currentFrame++;
            
            // このフレームの馬の状態を記録するリストを準備
            var frameLog = new List<FrameHorseState>();

            for (int i = 0; i < _input.HorseCount; i++)
            {
                // ゴール済みの馬は位置を更新しない
                if (result.GoalTimesInFrames[i] == -1)
                {
                    float frameDistance = ((BASE_SPEED_UNIT * TIME_ADJUSTMENT_FACTOR) + Random.Range(0, 3)) * 10;
                    currentStates[i].TotalDistancePx += frameDistance;

                    // ゴール判定
                    if (currentStates[i].TotalDistancePx >= totalDistancePx)
                    {
                        result.GoalTimesInFrames[i] = currentFrame;
                        finishedCount++;
                    }
                }
                
                // 現在の状態をコピーしてこのフレームのログに追加
                frameLog.Add(new FrameHorseState { TotalDistancePx = currentStates[i].TotalDistancePx });
            }
            
            // このフレームの全馬の状態をレースログに追加
            result.RaceLog.Add(frameLog);
        }

        return result;
    }
}