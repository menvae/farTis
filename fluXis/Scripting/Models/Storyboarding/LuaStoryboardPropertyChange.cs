using fluXis.Storyboards;
using NLua;

namespace fluXis.Scripting.Models.Storyboarding;

public class LuaStoryboardPropertyChange : ILuaModel
{
    [LuaMember(Name = "time")]
    public double Time { get; set; }

    [LuaMember(Name = "prop")]
    public string PropertyKey { get; set; }

    [LuaMember(Name = "value")]
    public string Value { get; set; }

    public StoryboardPropertyChange Build() => new()
    {
        Time = Time,
        PropertyKey = PropertyKey,
        Value = Value,
    };
}
