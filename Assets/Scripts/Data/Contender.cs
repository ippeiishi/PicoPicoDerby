using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class Contender {
    [JsonProperty("source_uid")]
    public string SourceUid { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("final_speed")]
    public int FinalSpeed { get; set; }

    [JsonProperty("final_stamina")]
    public int FinalStamina { get; set; }

    [JsonProperty("final_guts")]
    public int FinalGuts { get; set; }

    [JsonProperty("final_mentality")]
    public int FinalMentality { get; set; }

    [JsonProperty("tactic")]
    public int Tactic { get; set; }

    [JsonProperty("weight")]
    public int Weight { get; set; }

    [JsonProperty("ability_ids")]
    public List<int> AbilityIds { get; set; }

    public Contender() {
        AbilityIds = new List<int>();
    }
}