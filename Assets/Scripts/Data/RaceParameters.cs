using System;
using Newtonsoft.Json;

public enum TrackType { Turf, Dirt }
public enum TrackDirection { Right, Left }
public enum WeatherType { Sunny, Cloudy, Rain, Storm, Snow }
public enum TrackCondition { Firm, Good, Yielding, Soft }

[Serializable]
public class RaceParameters {
    [JsonProperty("track_type")]
    public TrackType TrackType { get; set; }

    [JsonProperty("direction")]
    public TrackDirection Direction { get; set; }

    [JsonProperty("distance")]
    public int Distance { get; set; }

    [JsonProperty("weather")]
    public WeatherType Weather { get; set; }

    [JsonProperty("track_condition")]
    public TrackCondition TrackCondition { get; set; }

    public RaceParameters() {
        TrackType = TrackType.Turf;
        Direction = TrackDirection.Right;
        Distance = 1200;
        Weather = WeatherType.Sunny;
        TrackCondition = TrackCondition.Firm;
    }
}