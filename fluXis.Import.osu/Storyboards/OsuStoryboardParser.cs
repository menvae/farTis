using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Storyboards;
using fluXis.Utils;
using Newtonsoft.Json.Linq;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osuTK;

namespace fluXis.Import.osu.Storyboards;

public class OsuStoryboardParser
{
    private const float width = 640;
    private const float widescreen_width = 854;
    private const float x_adjust = (widescreen_width - width) / 2;

    private Storyboard storyboard;
    private StoryboardElement currentElement;
    private OsuStoryboardLoop currentLoop;
    private Dictionary<string, string> variables;

    public Storyboard Parse(string data)
    {
        var lines = data.Split("\n");
        storyboard = new Storyboard
        {
            Resolution = new Vector2(widescreen_width, 480)
        };
        variables = new Dictionary<string, string>();

        var idx = 0;

        foreach (var line in lines)
        {
            idx++;
            var trimmedLine = line.Trim();

            try
            {
                if (trimmedLine.StartsWith("//") || string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                if (trimmedLine.StartsWith("["))
                    continue;

                if (trimmedLine.TrimStart().StartsWith("$"))
                {
                    parseVariable(trimmedLine);
                    continue;
                }

                parseLine(line);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to parse storyboard line {idx}: {line}");
            }
        }

        expandAnimations();
        calculateElementTimings();
        sortElements();

        return storyboard;
    }

    #region Parse

    private void parseVariable(string line)
    {
        var parts = line.Split('=');
        if (parts.Length != 2)
            return;

        var varName = parts[0].Trim();
        var varValue = parts[1].Trim();

        variables[varName] = varValue;
    }

    private string replaceVariables(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        foreach (var keyVal in variables)
            value = value.Replace(keyVal.Key, keyVal.Value);

        return value;
    }

    private void parseLine(string line)
    {
        line = replaceVariables(line);
        var depth = getLineDepth(line);

        line = line[depth..];
        var split = line.Split(',');

        if (depth == 0)
            parseElement(split);
        else
            parseCommand(split, depth);
    }

    private static int getLineDepth(string line)
    {
        var depth = 0;
        foreach (char c in line)
        {
            if (c is ' ' or '_')
                depth++;
            else
                break;
        }
        return depth;
    }

    private void parseElement(string[] split)
    {
        currentLoop?.PushTo(currentElement.Animations);
        currentLoop = null;
        currentElement = null;

        switch (split[0])
        {
            case "Sprite":
                currentElement = createSpriteElement(split);
                break;

            case "Animation":
                currentElement = createAnimationElement(split);
                break;

            default:
                return;
        }

        storyboard.Elements.Add(currentElement);
    }

    private void parseCommand(string[] split, int depth)
    {
        var commandType = split[0];

        if (depth == 1 && currentLoop is not null)
        {
            currentLoop.PushTo(currentElement.Animations);
            currentLoop = null;
        }

        if (commandType == "T" || commandType == "P")
            return;

        if (commandType == "L")
        {
            parseLoop(split);
            return;
        }

        var animations = parseAnimationCommand(split, commandType);
        if (animations == null || animations.Length == 0)
            return;

        if (depth == 2 && currentLoop is not null)
            animations.ForEach(currentLoop.Animations.Add);
        else if (depth == 1)
            animations.ForEach(currentElement.Animations.Add);
    }

    private void parseLoop(string[] split)
    {
        var startTime = split[1].ToDoubleInvariant();
        var count = split[2].ToIntInvariant();

        currentLoop = new OsuStoryboardLoop
        {
            StartTime = startTime,
            LoopCount = count
        };
    }

    private static StoryboardAnimation[] parseAnimationCommand(string[] split, string commandType)
    {
        if (string.IsNullOrEmpty(split[3]))
            split[3] = split[2];

        var easing = (Easing)split[1].ToIntInvariant();
        var startTime = split[2].ToDoubleInvariant();
        var endTime = split[3].ToDoubleInvariant();
        var duration = endTime - startTime;

        return commandType switch
        {
            "F" => parseFade(split, easing, startTime, duration),
            "S" => parseScale(split, easing, startTime, duration),
            "V" => parseVectorScale(split, easing, startTime, duration),
            "R" => parseRotate(split, easing, startTime, duration),
            "M" => parseMove(split, easing, startTime, duration),
            "MX" => parseMoveX(split, easing, startTime, duration),
            "MY" => parseMoveY(split, easing, startTime, duration),
            "C" => parseColor(split, easing, startTime, duration),
            _ => null
        };
    }

    #endregion

    #region Event Types

    private static StoryboardAnimation[] parseFade(string[] split, Easing easing, double startTime, double duration)
    {
        var start = split[4].ToFloatInvariant();
        var end = split.Length > 5 ? split[5].ToFloatInvariant() : start;

        return new[]
        {
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.Fade,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = start.ToStringInvariant(),
                ValueEnd = end.ToStringInvariant()
            }
        };
    }

    private static StoryboardAnimation[] parseScale(string[] split, Easing easing, double startTime, double duration)
    {
        var start = split[4].ToFloatInvariant();
        var end = split.Length > 5 ? split[5].ToFloatInvariant() : start;

        return new[]
        {
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.Scale,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = start.ToStringInvariant(),
                ValueEnd = end.ToStringInvariant()
            }
        };
    }

    private static StoryboardAnimation[] parseVectorScale(string[] split, Easing easing, double startTime, double duration)
    {
        var startX = split[4].ToFloatInvariant();
        var startY = split[5].ToFloatInvariant();
        var endX = split.Length > 6 ? split[6].ToFloatInvariant() : startX;
        var endY = split.Length > 7 ? split[7].ToFloatInvariant() : startY;

        return new[]
        {
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.ScaleVector,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = $"{startX},{startY}",
                ValueEnd = $"{endX},{endY}"
            }
        };
    }

    private static StoryboardAnimation[] parseRotate(string[] split, Easing easing, double startTime, double duration)
    {
        var start = split[4].ToFloatInvariant();
        var end = split.Length > 5 ? split[5].ToFloatInvariant() : start;

        var startDeg = float.RadiansToDegrees(start);
        var endDeg = float.RadiansToDegrees(end);

        return new[]
        {
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.Rotate,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = startDeg.ToStringInvariant(),
                ValueEnd = endDeg.ToStringInvariant()
            }
        };
    }

    private static StoryboardAnimation[] parseMove(string[] split, Easing easing, double startTime, double duration)
    {
        var startX = split[4].ToFloatInvariant() + x_adjust;
        var startY = split[5].ToFloatInvariant();
        var endX = split.Length > 6 ? split[6].ToFloatInvariant() + x_adjust : startX;
        var endY = split.Length > 7 ? split[7].ToFloatInvariant() : startY;

        return new[]
        {
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.MoveX,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = startX.ToStringInvariant(),
                ValueEnd = endX.ToStringInvariant()
            },
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.MoveY,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = startY.ToStringInvariant(),
                ValueEnd = endY.ToStringInvariant()
            }
        };
    }

    private static StoryboardAnimation[] parseMoveX(string[] split, Easing easing, double startTime, double duration)
    {
        var start = split[4].ToFloatInvariant() + x_adjust;
        var end = split.Length > 5 ? split[5].ToFloatInvariant() + x_adjust : start;

        return new[]
        {
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.MoveX,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = start.ToStringInvariant(),
                ValueEnd = end.ToStringInvariant()
            }
        };
    }

    private static StoryboardAnimation[] parseMoveY(string[] split, Easing easing, double startTime, double duration)
    {
        var start = split[4].ToFloatInvariant();
        var end = split.Length > 5 ? split[5].ToFloatInvariant() : start;

        return new[]
        {
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.MoveY,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = start.ToStringInvariant(),
                ValueEnd = end.ToStringInvariant()
            }
        };
    }

    private static StoryboardAnimation[] parseColor(string[] split, Easing easing, double startTime, double duration)
    {
        var startRed = split[4].ToFloatInvariant();
        var startGreen = split[5].ToFloatInvariant();
        var startBlue = split[6].ToFloatInvariant();
        var endRed = split.Length > 7 ? split[7].ToFloatInvariant() : startRed;
        var endGreen = split.Length > 8 ? split[8].ToFloatInvariant() : startGreen;
        var endBlue = split.Length > 9 ? split[9].ToFloatInvariant() : startBlue;

        var hexStart = $"#{(int)startRed:X2}{(int)startGreen:X2}{(int)startBlue:X2}";
        var hexEnd = $"#{(int)endRed:X2}{(int)endGreen:X2}{(int)endBlue:X2}";

        return new[]
        {
            new StoryboardAnimation
            {
                Type = StoryboardAnimationType.Color,
                StartTime = startTime,
                Duration = duration,
                Easing = easing,
                ValueStart = hexStart,
                ValueEnd = hexEnd
            }
        };
    }

    #endregion

    #region Elements

    private static StoryboardElement createSpriteElement(string[] split)
    {
        var layer = parseLayer(split[1]);
        var origin = parseOrigin(split[2]);
        var path = cleanFilename(split[3]);
        var x = split[4].ToFloatInvariant() + x_adjust;
        var y = split[5].ToFloatInvariant();

        return new StoryboardElement
        {
            Type = StoryboardElementType.Sprite,
            Layer = layer,
            Anchor = Anchor.TopLeft,
            Origin = origin,
            StartX = x,
            StartY = y,
            Parameters = new Dictionary<string, JToken>
            {
                { "file", path }
            }
        };
    }

    private static StoryboardElement createAnimationElement(string[] split)
    {
        var layer = parseLayer(split[1]);
        var origin = parseOrigin(split[2]);
        var path = cleanFilename(split[3]);
        var x = split[4].ToFloatInvariant() + x_adjust;
        var y = split[5].ToFloatInvariant();
        var frameCount = split[6].ToIntInvariant();
        var frameDelay = split[7].ToDoubleInvariant();
        var loopType = split.Length > 8 ? split[8].Trim() : "LoopForever";

        var pathExt = path.Split('.').Last();
        var basePath = path.Replace($".{pathExt}", "");

        return new StoryboardElement
        {
            Type = StoryboardElementType.Sprite,
            Layer = layer,
            Anchor = Anchor.TopLeft,
            Origin = origin,
            StartX = x,
            StartY = y,
            Parameters = new Dictionary<string, JToken>
            {
                { "file", $"{basePath}0.{pathExt}" },
                { "frameCount", frameCount },
                { "frameDelay", frameDelay },
                { "loopType", loopType },
                { "basePath", basePath },
                { "extension", pathExt }
            }
        };
    }
    
    #endregion

    #region Animation Element

    private void expandAnimations()
    {
        var processedElements = new List<StoryboardElement>();

        foreach (var element in storyboard.Elements)
        {
            if (element.Parameters.ContainsKey("frameCount"))
                processedElements.AddRange(expandAnimationElement(element));
            else
                processedElements.Add(element);
        }

        storyboard.Elements = processedElements;
    }

    private static List<StoryboardElement> expandAnimationElement(StoryboardElement element)
    {
        var frameCount = element.Parameters["frameCount"].ToObject<int>();
        var frameDelay = element.Parameters["frameDelay"].ToObject<double>();
        var loopType = element.Parameters["loopType"].ToObject<string>();
        var basePath = element.Parameters["basePath"].ToObject<string>();
        var extension = element.Parameters["extension"].ToObject<string>();

        var (startTime, endTime) = calculateElementTimeRange(element);
        var cycleDuration = frameCount * frameDelay;
        var totalDuration = endTime - startTime;
        var loopCount = calculateLoopCount(loopType, totalDuration, cycleDuration);

        var loop = new OsuStoryboardLoop
        {
            StartTime = startTime,
            LoopCount = loopCount
        };

        var frameElements = new List<StoryboardElement>();

        for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
        {
            var frameElement = createFrameElement(element, basePath, extension, frameIdx);

            var frameShowTime = frameIdx * frameDelay;
            var frameHideTime = frameShowTime + frameDelay;

            loop.Animations.Add(new StoryboardAnimation
            {
                Type = StoryboardAnimationType.Fade,
                StartTime = frameShowTime,
                Duration = 0,
                Easing = Easing.None,
                ValueStart = "1",
                ValueEnd = "1"
            });

            loop.Animations.Add(new StoryboardAnimation
            {
                Type = StoryboardAnimationType.Fade,
                StartTime = frameHideTime,
                Duration = 0,
                Easing = Easing.None,
                ValueStart = "0",
                ValueEnd = "0"
            });

            loop.PushTo(frameElement.Animations);
            frameElements.Add(frameElement);
        }

        return frameElements;
    }

    private static (double startTime, double endTime) calculateElementTimeRange(StoryboardElement element)
    {
        var startTime = double.MaxValue;
        var endTime = double.MinValue;

        foreach (var animation in element.Animations)
        {
            startTime = Math.Min(startTime, animation.StartTime);
            endTime = Math.Max(endTime, animation.EndTime);
        }

        if (startTime == double.MaxValue)
        {
            startTime = 0;
            endTime = element.Parameters["frameCount"].ToObject<int>() *
                      element.Parameters["frameDelay"].ToObject<double>();
        }

        return (startTime, endTime);
    }

    private static int calculateLoopCount(string loopType, double totalDuration, double cycleDuration)
    {
        if (loopType == "LoopOnce")
            return 1;

        if (totalDuration > 0 && cycleDuration > 0)
            return (int)Math.Ceiling(totalDuration / cycleDuration);

        return 10; // We of course can't have infnite iterations
    }

    private static StoryboardElement createFrameElement(StoryboardElement source, string basePath, string extension, int frameIdx)
    {
        var frameElement = new StoryboardElement
        {
            Type = source.Type,
            Layer = source.Layer,
            Anchor = source.Anchor,
            Origin = source.Origin,
            StartX = source.StartX,
            StartY = source.StartY,
            ZIndex = source.ZIndex,
            Parameters = new Dictionary<string, JToken>
            {
                { "file", $"{basePath}{frameIdx}.{extension}" }
            }
        };

        foreach (var anim in source.Animations)
        {
            frameElement.Animations.Add(new StoryboardAnimation
            {
                Type = anim.Type,
                StartTime = anim.StartTime,
                Duration = anim.Duration,
                Easing = anim.Easing,
                ValueStart = anim.ValueStart,
                ValueEnd = anim.ValueEnd
            });
        }

        return frameElement;
    }

    #endregion

    #region Misc

    private void calculateElementTimings()
    {
        foreach (var element in storyboard.Elements)
        {
            var startTime = double.MaxValue;
            var endTime = double.MinValue;

            foreach (var animation in element.Animations)
            {
                startTime = Math.Min(startTime, animation.StartTime);
                endTime = Math.Max(endTime, animation.EndTime);
            }

            element.Animations.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

            if (element.Animations.Count <= 0)
            {
                Logger.Log($"Element {element.Type} ({element.Parameters["file"]}) does not have any animations!");
                continue;
            }

            element.StartTime = startTime;
            element.EndTime = endTime;
        }
    }

    private void sortElements()
    {
        storyboard.Elements.Sort((a, b) =>
        {
            var layerCompare = a.Layer.CompareTo(b.Layer);
            if (layerCompare != 0)
                return layerCompare;

            return a.StartTime.CompareTo(b.StartTime);
        });

        for (int i = 0; i < storyboard.Elements.Count; i++)
            storyboard.Elements[i].ZIndex = i;
    }

    private static StoryboardLayer parseLayer(string val)
    {
        return val switch
        {
            "Background" or "0" => StoryboardLayer.Background,
            "Fail" or "1" => StoryboardLayer.Foreground,
            "Pass" or "2" => StoryboardLayer.Foreground,
            "Foreground" or "3" => StoryboardLayer.Foreground,
            "Overlay" or "4" => StoryboardLayer.Overlay,
            _ => StoryboardLayer.Background
        };
    }

    private static Anchor parseOrigin(string val)
    {
        return val switch
        {
            "TopLeft" => Anchor.TopLeft,
            "TopCentre" => Anchor.TopCentre,
            "TopRight" => Anchor.TopRight,
            "CentreLeft" => Anchor.CentreLeft,
            "Centre" => Anchor.Centre,
            "CentreRight" => Anchor.CentreRight,
            "BottomLeft" => Anchor.BottomLeft,
            "BottomCentre" => Anchor.BottomCentre,
            "BottomRight" => Anchor.BottomRight,
            _ => Anchor.TopLeft
        };
    }

    private static string cleanFilename(string filename)
    {
        return filename.Replace(@"\\", @"\")
                       .Trim('"')
                       .ToStandardisedPath();
    }
}

#endregion
