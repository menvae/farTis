using Newtonsoft.Json;
using osu.Framework.Graphics;
using osuTK;

namespace fluXis.Map.Structures.Bases;

public interface IHasEasing
{
    Easing Easing { get; set; }
    
    /// <summary>
    /// (X1, Y1, X2, Y2)
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    Vector4? ControlPoints 
    {
        get => null;
        set { }
    }
}