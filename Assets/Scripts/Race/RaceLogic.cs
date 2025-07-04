using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceSimulationInput
{
    public int HorseCount { get; set; }
    public int DistanceInMetres { get; set; }
    public List<HorseData> Horses { get; set; } // ←追加
}

public class FrameHorseState {
    public int PositionX { get; set; }
    public float PositionY { get; set; }
    public int SortingOrder { get; set; }
}

public class RaceSimulationResult {
    public List<List<FrameHorseState>> RaceLog { get; private set; }
    public List<int> GoalTimesInFrames { get; private set; }
    public List<int> FinishMarginsInUnits { get; private set; }

    public RaceSimulationResult(int horseCount) {
        RaceLog = new List<List<FrameHorseState>>();
        GoalTimesInFrames = new List<int>(new int[horseCount]);
        FinishMarginsInUnits = new List<int>(new int[horseCount]);
        for (int i = 0; i < horseCount; i++) {
            GoalTimesInFrames[i] = -1;
        }
    }
}
public class RaceSimulator {
    private const int UNITS_PER_METRE = 100;
    private const int FPS = 30;
    private const int SIMULATION_EXTENSION_UNITS = 2000; // ゴール後、画面外に消えるまでシミュレーションを延長する距離
    // デバッグ用の固定スピード
    public int base_sp = 24;
    private const int SPEED_RANDOM_RANGE = 6;

    private readonly RaceSimulationInput _input;

    public RaceSimulator(RaceSimulationInput input) {
        _input = input;
    }
public static int CalculateBoostCount(int st) {
    int raw = (int)(st / 20f) + 3;
    return Mathf.Min(raw, 10); // 最大値は10
}

private int MetresToPixels(int metres){
    const int PIXELS_PER_METRE = 10;
    return metres * PIXELS_PER_METRE;
}



    public RaceSimulationResult RunSimulation()
    {
        int totalDistanceUnits = _input.DistanceInMetres * UNITS_PER_METRE;
        int finalDistanceUnits = totalDistanceUnits + SIMULATION_EXTENSION_UNITS;
        var result = new RaceSimulationResult(_input.HorseCount);

        var currentStates = new List<FrameHorseState>();
        for (int i = 0; i < _input.HorseCount; i++)
        {
            currentStates.Add(new FrameHorseState { PositionX = 0, PositionY = 0, SortingOrder = 0 });
        }

        result.RaceLog.Add(new List<FrameHorseState>(currentStates.Select(s => new FrameHorseState { PositionX = s.PositionX, PositionY = s.PositionY, SortingOrder = s.SortingOrder })));

        int currentFrame = 0;
        int slowestHorsePosition = 0;

        while (slowestHorsePosition < finalDistanceUnits)
        {
            currentFrame++;

            var frameLog = new List<FrameHorseState>();
            slowestHorsePosition = int.MaxValue;

            for (int i = 0; i < _input.HorseCount; i++)
            {
                //int frameDistance = base_sp + Random.Range(0, SPEED_RANDOM_RANGE + 1);
                int frameDistance = base_sp;
                currentStates[i].PositionX += frameDistance;

                if (result.GoalTimesInFrames[i] == -1 && currentStates[i].PositionX >= totalDistanceUnits)
                {
                    currentStates[i].PositionX = totalDistanceUnits;
                    result.GoalTimesInFrames[i] = currentFrame;
                }

                frameLog.Add(new FrameHorseState
                {
                    PositionX = currentStates[i].PositionX,
                    PositionY = currentStates[i].PositionY,
                    SortingOrder = currentStates[i].SortingOrder
                });

                if (currentStates[i].PositionX < slowestHorsePosition)
                {
                    slowestHorsePosition = currentStates[i].PositionX;
                }
            }

            result.RaceLog.Add(frameLog);
        }

        AnalyzeAndLogResults(result, totalDistanceUnits);

        return result;
    }

    private void AnalyzeAndLogResults(RaceSimulationResult result, int totalDistanceUnits) {
        var sortedHorseIndices = Enumerable.Range(0, _input.HorseCount)
            .Where(i => result.GoalTimesInFrames[i] != -1)
            .OrderBy(i => result.GoalTimesInFrames[i])
            .ThenBy(i => i)
            .ToList();

        for (int i = 0; i < sortedHorseIndices.Count; i++) {
            int horseIndex = sortedHorseIndices[i];
            int goalFrame = result.GoalTimesInFrames[horseIndex];
            var goalFrameLog = result.RaceLog[goalFrame];

            var followingHorses = sortedHorseIndices.Skip(i + 1);
            int maxPositionOfFollowers = 0;
            bool followerExists = false;

            foreach (var followerIndex in followingHorses) {
                int followerPosition = goalFrameLog[followerIndex].PositionX;
                if (followerPosition > maxPositionOfFollowers) {
                    maxPositionOfFollowers = followerPosition;
                    followerExists = true;
                }
            }

            if (followerExists) {
                result.FinishMarginsInUnits[horseIndex] = totalDistanceUnits - maxPositionOfFollowers;
            } else {
                result.FinishMarginsInUnits[horseIndex] = 0;
            }
        }


    }
}