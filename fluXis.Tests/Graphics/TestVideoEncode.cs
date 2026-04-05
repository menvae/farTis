using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using fluXis.Graphics;
using fluXis.Graphics.Background;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Utils;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Framework.Logging;
using osuTK;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace fluXis.Tests.Graphics;

public partial class TestVideoEncode : FluXisTestScene
{
    private DrawableCapture capture = null!;
    private VideoEncoder videoEncoder = null!;
    private string outputPath = null!;

    private const int videoWidth = 1920;
    private const int videoHeight = 1080;
    private const int fps = 60;

    [BackgroundDependencyLoader]
    private void load()
    {
        CreateClock();

        var backgrounds = new GlobalBackground();
        TestDependencies.CacheAs(backgrounds);

        var captureContainer = new CaptureContainer
        {
            RelativeSizeAxes = Axes.Both,
            Child = new SpinningBoxes()
        };

        capture = new DrawableCapture(captureContainer);

        Children = new Drawable[]
        {
            backgrounds,
            captureContainer,
            capture
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        outputPath = Path.Combine(Path.GetTempPath(), "fluXis_capture_test.mp4").Replace('\\', '/');

        videoEncoder = new VideoEncoder(outputPath, videoWidth, videoHeight, fps);
        videoEncoder.StartEncoding(AVCodecID.AV_CODEC_ID_H264);

        if (videoEncoder.IsFaulted)
        {
            Logger.Log("VideoEncoder failed to start, Aborting.", level: LogLevel.Error);
            return;
        }

        var frameReader = capture.StartCapture(startMs: 0, endMs: 5000, fps: fps);

        if (frameReader == null)
        {
            Logger.Log("Capture already in progress.", level: LogLevel.Error);
            return;
        }

        Task.Run(() => encodeFramesAsync(frameReader));
    }

    private async Task encodeFramesAsync(ChannelReader<RawFrame> frameReader)
    {
        Logger.Log("Encode started.");

        try
        {
            await foreach (var rawFrame in frameReader.ReadAllAsync())
            {
                using (rawFrame)
                {
                    unsafe
                    {
                        using var image = Image.LoadPixelData<Rgba32>(
                            rawFrame.Data, rawFrame.Width, rawFrame.Height);

                        AVFrame* avFrame = ImageUtils.Rgba32ToAvFrame(image);

                        if (avFrame == null)
                        {
                            Logger.Log($"Rgba32ToAvFrame returned null at {rawFrame.TimeMs:F0}ms, skipping.",
                                level: LogLevel.Important);
                            continue;
                        }

                        videoEncoder.SendFrame(avFrame);
                        FFmpeg.AutoGen.ffmpeg.av_frame_free(&avFrame);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log($"Frame pipe threw: {e}", level: LogLevel.Error);
        }

        videoEncoder.Dispose();

        Logger.Log(videoEncoder.IsFaulted
            ? $"Encode failed"
            : $"Encode complete: {outputPath}");
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        if (videoEncoder is { IsRunning: true })
            videoEncoder.Dispose();
    }

    private partial class SpinningBoxes : Container
    {
        public SpinningBoxes()
        {
            RelativeSizeAxes = Axes.Both;
            FillAspectRatio = 1;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new Box { RelativePositionAxes = Axes.Both, Size = new Vector2(200), Anchor = Anchor.Centre, X = -0.15f, Colour = Theme.Aqua },
                new Box { RelativePositionAxes = Axes.Both, Size = new Vector2(200), Anchor = Anchor.Centre, X = 0.15f, Colour = Theme.Red }
            };
        }

        protected override void Update()
        {
            base.Update();
            Rotation += (float)Time.Elapsed / 5;
        }
    }
}
