using fluXis.Map.Structures.Bases;
using fluXis.Utils;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osuTK;
using SixLabors.ImageSharp;

namespace fluXis.Storyboards;

public class StoryboardAnimation : IDeepCloneable<StoryboardAnimation>, IMapEvent, IHasDuration, IHasEasing
{
    /// <summary>
    /// The start time of the animation.
    /// </summary>
    [JsonProperty("start")]
    public double StartTime { get; set; }
    
    public double Time // IMapEvent implementation
    {
        get => StartTime;
        set => StartTime = value;
    }

    [JsonIgnore]
    public double EndTime => StartTime + Duration;

    /// <summary>
    /// The duration of the animation.
    /// </summary>
    [JsonProperty("duration")]
    public double Duration { get; set; }

    /// <summary>
    /// The easing of the animation.
    /// </summary>
    [JsonProperty("easing")]
    public Easing Easing { get; set; }

    /// <summary>
    /// The type of the animation.
    /// </summary>
    [JsonProperty("type")]
    public StoryboardAnimationType Type { get; set; }

    /// <summary>
    /// The start value of the animation.
    /// </summary>
    [JsonProperty("start-value")]
    public string ValueStart { get; set; }

    /// <summary>
    /// The end value of the animation.
    /// </summary>
    [JsonProperty("end-value")]
    public string ValueEnd { get; set; }

    [JsonIgnore]
    public float StartFloat => string.IsNullOrEmpty(ValueStart) ? 0f : ValueStart.ToFloatInvariant();

    [JsonIgnore]
    public float EndFloat => string.IsNullOrEmpty(ValueEnd) ? 0f : ValueEnd.ToFloatInvariant();

    [JsonIgnore]
    public Vector2 StartVector
    {
        get
        {
            if (string.IsNullOrEmpty(ValueStart)) 
                return Vector2.Zero;
                
            var xy = ValueStart.Split(',');
            if (xy.Length < 2) 
                return Vector2.Zero;
                
            return new Vector2(xy[0].ToFloatInvariant(), xy[1].ToFloatInvariant());
        }
    }

    [JsonIgnore]
    public Vector2 EndVector
    {
        get
        {
            if (string.IsNullOrEmpty(ValueEnd)) 
                return Vector2.Zero;
                
            var xy = ValueEnd.Split(',');
            if (xy.Length < 2) 
                return Vector2.Zero;
                
            return new Vector2(xy[0].ToFloatInvariant(), xy[1].ToFloatInvariant());
        }
    }

    public StoryboardAnimation DeepClone() => new()
    {
        StartTime = StartTime,
        Duration = Duration,
        Easing = Easing,
        Type = Type,
        ValueStart = ValueStart,
        ValueEnd = ValueEnd
    };
}

public enum StoryboardAnimationType
{
    MoveX = 0,
    MoveY = 1,
    Scale = 2,
    ScaleVector = 3,
    Width = 4,
    Height = 5,
    Rotate = 6,
    Fade = 7,
    Color = 8
}