using fluXis.Map.Structures.Bases;
using Newtonsoft.Json;

namespace fluXis.Storyboards;

public record StoryboardPropertyChange : ITimedObject
{
    /// <summary>
    /// The time when the property changes.
    /// </summary>
    [JsonProperty("time")]
    public double Time { get; set; }

    /// <summary>
    /// The property that should change.
    /// </summary>
    [JsonProperty("prop")]
    public string PropertyKey { get; set; }

    /// <summary>
    /// The new value of the property.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonIgnore]
    int ITimedObject.Lane { get; set; }

    [JsonIgnore]
    string ITimedObject.Group { get; set; }
}
