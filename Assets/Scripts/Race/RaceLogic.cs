using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceSimulationInput {
    public int HorseCount { get; set; }
    public int DistanceInMetres { get; set; }
    public List<HorseData> Horses { get; set; }
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
    private const int UNITS_PER_METRE = 500;
    private const int SIMULATION_EXTENSION_UNITS = 10000;
    public int base_sp = 100;

    private readonly RaceSimulationInput _input;

    public RaceSimulator(RaceSimulationInput input) {
        _input = input;
    }

    public RaceSimulationResult RunSimulation() {
        int totalDistanceUnits = _input.DistanceInMetres * UNITS_PER_METRE;
        int finalDistanceUnits = totalDistanceUnits + SIMULATION_EXTENSION_UNITS;
        var result = new RaceSimulationResult(_input.HorseCount);

        var currentStates = new List<FrameHorseState>();
        var totalPositions = new int[_input.HorseCount];

        for (int i = 0; i < _input.HorseCount; i++) {
            currentStates.Add(new FrameHorseState { PositionX = 0, PositionY = 0, SortingOrder = 0 });
        }

        result.RaceLog.Add(currentStates.Select(s => new FrameHorseState {
            PositionX = s.PositionX, PositionY = s.PositionY, SortingOrder = s.SortingOrder
        }).ToList());

        int currentFrame = 0;
        int slowestHorsePosition = 0;

        while (slowestHorsePosition < finalDistanceUnits) {
            currentFrame++;
            var frameLog = new List<FrameHorseState>();
            slowestHorsePosition = int.MaxValue;

            for (int i = 0; i < _input.HorseCount; i++) {
                currentStates[i].PositionX += base_sp;
                totalPositions[i] += base_sp;

                if (result.GoalTimesInFrames[i] == -1 && currentStates[i].PositionX >= totalDistanceUnits) {
                    currentStates[i].PositionX = totalDistanceUnits;
                    result.GoalTimesInFrames[i] = currentFrame;
                }

                frameLog.Add(new FrameHorseState {
                    PositionX = currentStates[i].PositionX,
                    PositionY = currentStates[i].PositionY,
                    SortingOrder = currentStates[i].SortingOrder
                });

                if (currentStates[i].PositionX < slowestHorsePosition) {
                    slowestHorsePosition = currentStates[i].PositionX;
                }
            }

            result.RaceLog.Add(frameLog);
        }

        // ビジュアライズ用スケール変換
        const float VISUALIZE_SCALE = 1f / 5f;
        foreach (var frame in result.RaceLog) {
            foreach (var state in frame) {
                state.PositionX = Mathf.FloorToInt(state.PositionX * VISUALIZE_SCALE);
                state.PositionY *= VISUALIZE_SCALE;
            }
        }

        return result;
    }

    private int MetresToPixels(int metres) {
        const int PIXELS_PER_METRE = 500;
        return metres * PIXELS_PER_METRE;
    }
}
