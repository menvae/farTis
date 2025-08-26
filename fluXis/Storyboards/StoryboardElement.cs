using System;
using System.Collections.Generic;
using fluXis.Map.Structures.Bases;
using fluXis.Utils.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using static fluXis.Screens.Edit.EditorMap;

namespace fluXis.Storyboards;

public class StoryboardElement : ITimedObject, IChangeNotifier
{
    /// <summary>
    /// The type of the element.
    /// </summary>
    [JsonProperty("type")]
    public StoryboardElementType Type { get; set; }

    /// <summary>
    /// The layer of the element.
    /// </summary>
    [JsonProperty("layer")]
    public StoryboardLayer Layer { get; set; } = StoryboardLayer.Background;

    /// <summary>
    /// The depth of the element.
    /// </summary>
    [JsonProperty("z-index")]
    public int ZIndex { get; set; }

    /// <summary>
    /// The start time of the element.
    /// Spawns the element at this time.
    /// </summary>
    [JsonProperty("start")]
    public double StartTime { get; set; }

    /// <summary>
    /// The end time of the element.
    /// Despawns the element at this time.
    /// </summary>
    [JsonProperty("end")]
    public double EndTime { get; set; }

    [JsonProperty("anchor")]
    public Anchor Anchor { get; set; } = Anchor.TopLeft;

    [JsonProperty("origin")]
    public Anchor Origin { get; set; } = Anchor.TopLeft;

    [JsonProperty("x")]
    public float StartX { get; set; }

    [JsonProperty("y")]
    public float StartY { get; set; }

    [JsonProperty("blend")]
    public bool Blending { get; set; }

    [JsonProperty("width")]
    public float Width { get; set; }

    [JsonProperty("height")]
    public float Height { get; set; }

    [JsonProperty("color")]
    public uint Color { get; set; } = 0xFFFFFFFF;

    [JsonProperty("parameters")]
    public Dictionary<string, JToken> Parameters { get; set; } = new();

    [JsonProperty("animations")]
    public List<StoryboardAnimation> Animations { get; set; } = new();

    [JsonIgnore]
    double ITimedObject.Time { get => StartTime; set => StartTime = value; }

    public event Action<StoryboardAnimation> AnimationAdded;
    public event Action<StoryboardAnimation> AnimationRemoved;
    public event Action<StoryboardAnimation> AnimationUpdated;

    public event Action<ITimedObject> OnAdd;
    public event Action<ITimedObject> OnRemove;
    public event Action<ITimedObject> OnUpdate;

    public T GetParameter<T>(string key, T fallback)
    {
        if (!Parameters.TryGetValue(key, out var token))
            return fallback;

        try
        {
            return token.ToObject<T>() ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    public void Add(ITimedObject obj)
    {
        var animation = (StoryboardAnimation)obj;
        Animations.Add(animation);
        AnimationAdded?.Invoke(animation);
        OnAdd?.Invoke(animation);
    }

    public void ApplyOffset(float offset) => Animations.ForEach(x =>
    {
        x.StartTime += offset;
    });

    public bool Matches(Type type) => typeof(StoryboardAnimation) == type;

    public void Remove(ITimedObject obj)
    {
        var animation = (StoryboardAnimation)obj;

        if (OnRemove.IsNull() || AnimationRemoved.IsNull()) {}

        Animations.Remove(animation);
        AnimationRemoved?.Invoke(animation);
        OnRemove?.Invoke(animation);
    }

    public void Update(ITimedObject obj)
    {
        var animation = (StoryboardAnimation)obj;

        AnimationUpdated?.Invoke(animation);
        OnUpdate?.Invoke(obj);
    }
}

public enum StoryboardElementType
{
    [Icon(0xf0c8)]
    Box = 0,

    [Icon(0xf03e)]
    Sprite = 1,

    [Icon(0xf031)]
    Text = 2,

    [Icon(0xf70e)]
    Script = 3
}

public enum StoryboardLayer
{
    Background = 0,
    Foreground = 1,
    Overlay = 2
}
