#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using fluXis.Utils.Attributes;
using Newtonsoft.Json;

namespace fluXis.Map.Structures.Bases;

public interface IHasMutableProperties
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> cache = new();

    [JsonIgnore]
    IReadOnlyDictionary<string, PropertyInfo> MutableProperties =>
        cache.GetOrAdd(GetType(), build);

    private static IReadOnlyDictionary<string, PropertyInfo> build(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(p => (Property: p, Attr: p.GetCustomAttribute<StoryboardChangeablePropertyAttribute>()))
            .Where(x => x.Attr is not null && x.Property.CanWrite)
            .ToDictionary(x => x.Attr!.Key, x => x.Property);
}
