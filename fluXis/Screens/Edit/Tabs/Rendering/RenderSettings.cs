using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fluXis.Mods;
using fluXis.Utils;

namespace fluXis.Screens.Edit.Tabs.Rendering;

public record struct RenderSettings()
{
    private static string defaultExportPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "fluXis");

    public (double, double) TimeRange = (0d, 1d); // relative

    public string VideoTitle = "Untitled";
    public string VideoExt = ".mp4";
    public string ExportPath = defaultExportPath;

    public int Height { get; set; } = 1080;
    public double AspectRatio { get; set; } = 16 / 9d;

    public int Fps { get; set; } = 60;
    public RenderFlags Flags { get; set; } = RenderFlags.All;

    public List<IMod> Mods { get; init; } = [];

    public void EnsureValid()
    {
        if (!ModUtils.HasMod<AutoPlayMod>(Mods)) Mods.Add(new AutoPlayMod());
        if (ModUtils.HasIncompatibleMods(Mods, out var incompatibleMods))
            Mods.RemoveAll(m => incompatibleMods.Contains(m));
        if (!PathUtils.IsValidDirectory(ExportPath)) Directory.CreateDirectory(ExportPath);
    }

    /// <returns>(Width, Height)</returns>
    public (int, int) GetDimensions() => ((int, int))(Height * AspectRatio, Height);
}

[Flags]
public enum RenderFlags
{
    OptimizeAfterRender = 0,
    UseHardwareAccel = 1,
    RenderGameplay = 2,
    RenderStoryboard = 4,
    HideLeaderboardInGameplay = 8,

    All = OptimizeAfterRender |
          UseHardwareAccel |
          RenderGameplay |
          RenderStoryboard |
          HideLeaderboardInGameplay
}

public enum CodecType { H264, Hevc, Av1, Vp9 }

public enum Preset { UltraFast, Fast, Medium, Slow, VerySlow }

public abstract record RateControl
{
    private RateControl() { }

    public sealed record Crf(int Quality) : RateControl;

    public sealed record Abr(int Bitrate) : RateControl;

    public sealed record Cbr(int Bitrate) : RateControl;

    public sealed record CappedCrf(int Quality, int MaxBitrate) : RateControl;
}
