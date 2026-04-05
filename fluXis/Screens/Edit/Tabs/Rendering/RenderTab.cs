// using System;
// using System.ComponentModel;
// using fluXis.Audio;
// using fluXis.Database.Maps;
// using fluXis.Graphics.Containers;
// using fluXis.Graphics.Sprites.Icons;
// using fluXis.Storyboards;
// using fluXis.Storyboards.Drawables;
// using fluXis.Utils;
// using osu.Framework.Allocation;
// using osu.Framework.Graphics.Sprites;
// using osu.Framework.Logging;
// using osu.Framework.Platform;
// using osu.Framework.Timing;
// using SixLabors.ImageSharp;
//
// namespace fluXis.Screens.Edit.Tabs.Rendering;
//
// public partial class RenderTab : EditorTab
// {
//     [Resolved]
//     private GameHost host { get; set; }
//
//     public override IconUsage Icon => FontAwesome6.Solid.PaintBrush;
//     public override string TabName => "Storyboard";
//
//     [Resolved]
//     private EditorClock clock { get; set; }
//
//     [Resolved]
//     private EditorMap map { get; set; }
//
//     [Resolved]
//     private Editor editor { get; set; }
//
//     private DependencyContainer dependencies;
//
//     private float scrollAccumulation;
//     private AspectRatioContainer aspect;
//     private IdleTracker idleTracker;
//     private Container loading;
//
//     private double start;
//     private ManualFramedClock clock;
//
//     public StoryboardRenderer(RealmMap map)
//     {
//         this.map = map;
//     }
//
//     [BackgroundDependencyLoader]
//     private void load(GlobalClock gc)
//     {
//         gc.Stop();
//
//         start = Clock.CurrentTime;
//
//         track = map.GetTrack();
//         track.Start();
//         track.Volume.Value = 0f;
//
//         var info = map.GetMapInfo()!;
//         var sb = info.CreateDrawableStoryboard()!;
//         LoadComponent(sb);
//
//         clock = new ManualFramedClock();
//
//         var layers = Enum.GetValues<StoryboardLayer>();
//
//         foreach (var layer in layers)
//         {
//             var wrapper = new DrawableStoryboardLayer(clock, sb, layer);
//             AddInternal(wrapper);
//         }
//     }
//
//     protected override void LoadComplete()
//     {
//         base.LoadComplete();
//         SchedulerAfterChildren.AddDelayed(updateFrame, 2000);
//     }
//
//     private long count = 1;
//
//     private void updateFrame()
//     {
//         var current = Clock.CurrentTime;
//         var elapsed = current - start;
//         var progress = clock.CurrentTime / end;
//
//         var estimatedTotal = elapsed * (1 / progress);
//
//         var remaining = estimatedTotal - elapsed;
//
//         var estimatedFormatted = TimeSpan.FromMilliseconds(estimatedTotal).ToString(@"hh\:mm\:ss");
//         var elapsedFormatted = TimeSpan.FromMilliseconds(elapsed).ToString(@"hh\:mm\:ss");
//         var remainingFormatted = TimeSpan.FromMilliseconds(remaining).ToString(@"hh\:mm\:ss");
//
//         Logger.Log($"[{count}] {TimeUtils.Format(clock.CurrentTime)}/{TimeUtils.Format(end)} ({elapsedFormatted}) - {progress:P2} - Estimated Total: {estimatedFormatted} - Remaining: {remainingFormatted}", LoggingTarget.Runtime,
//             LogLevel.Debug);
//
//         var image = host.TakeScreenshotAsync().Result;
//         var path = host.Storage.GetFullPath($"render/frame_{count++}.png", true);
//         image.SaveAsPng(path);
//
//         if (clock.CurrentTime >= end)
//         {
//             this.Exit();
//             return;
//         }
//
//         const double ms_per_frame = 1000 / 60d;
//         clock.CurrentTime += ms_per_frame;
//
//         ScheduleAfterChildren(updateFrame);
//     }
// }
