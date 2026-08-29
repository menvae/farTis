#nullable enable
using System;

namespace fluXis.Utils.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class StoryboardChangeablePropertyAttribute : Attribute
{
    public string Key { get; }

    public StoryboardChangeablePropertyAttribute(string key)
    {
        Key = key;
    }
}
