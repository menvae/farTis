using System;
using System.Buffers;
using System.Threading.Channels;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Timing;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Logging;

namespace fluXis.Graphics;

#nullable enable
public partial class DrawableCapture : Component
{
    public CaptureContainer Buffer { get; }
    public ManualFramedClock CaptureClock { get; }

    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private IRenderer renderer { get; set; } = null!;

    private readonly IFrameBasedClock originalClock;

    private bool isCapturing;
    private int captureFrameIndex;
    private int totalFrames;
    private double startMs;
    private double fps;

    private Channel<RawFrame>? frameChannel;

    private volatile bool isWaitingForExtraction;
    private bool isExtracting;

    public DrawableCapture(CaptureContainer buffer)
    {
        Buffer = buffer;
        originalClock = buffer.Clock;
        CaptureClock = new ManualFramedClock { IsRunning = false };

        Buffer.OnFrameRendered = onFrameRendered;
    }

    public ChannelReader<RawFrame>? StartCapture(double startMs, double endMs, double fps)
    {
        if (isCapturing) return null;

        this.startMs = startMs;
        this.fps = fps;

        totalFrames = (int)Math.Ceiling((endMs - startMs) / (1000d / fps));
        captureFrameIndex = 0;
        isWaitingForExtraction = false;
        isExtracting = false;

        frameChannel = Channel.CreateUnbounded<RawFrame>();

        CaptureClock.IsRunning = true;
        seekTo(startMs);
        Buffer.Clock = CaptureClock;

        isCapturing = true;
        Logger.Log($"Starting raw capture: {totalFrames} frames at {fps}fps...");

        return frameChannel.Reader;
    }

    private void seekTo(double timeMs)
    {
        double previousTime = CaptureClock.CurrentTime;
        CaptureClock.CurrentTime = timeMs;
        CaptureClock.ElapsedFrameTime = timeMs - previousTime;
    }

    protected override void Update()
    {
        base.Update();

        if (!isCapturing) return;

        if (isWaitingForExtraction) return;

        if (captureFrameIndex >= totalFrames)
        {
            stopCapture();
            return;
        }

        isWaitingForExtraction = true;
    }

    private void onFrameRendered(IFrameBuffer frameBuffer)
    {
        if (!isCapturing || !isWaitingForExtraction || isExtracting) return;

        isExtracting = true;
        double renderedTimeMs = startMs + captureFrameIndex * (1000d / fps);

        host.DrawThread.Scheduler.Add(() =>
        {
            var image = renderer.ExtractFrameBufferData(frameBuffer);

            if (image == null)
            {
                isExtracting = false;
                return;
            }

            int byteCount = image.Width * image.Height * 4;
            byte[] pooledBuffer = ArrayPool<byte>.Shared.Rent(byteCount);

            image.CopyPixelDataTo(pooledBuffer.AsSpan(0, byteCount));

            var rawFrame = new RawFrame(image.Width, image.Height, renderedTimeMs, pooledBuffer, byteCount);
            frameChannel?.Writer.TryWrite(rawFrame);

            image.Dispose();

            Scheduler.Add(() =>
            {
                isExtracting = false;
                isWaitingForExtraction = false;

                captureFrameIndex++;
                seekTo(startMs + captureFrameIndex * (1000d / fps));
            });
        });
    }

    private void stopCapture()
    {
        isCapturing = false;
        Buffer.Clock = originalClock;
        CaptureClock.IsRunning = false;

        frameChannel?.Writer.Complete();
        Logger.Log("Capture complete, waiting for encoder to finish...");
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        if (isCapturing) stopCapture();
        if (Buffer != null) Buffer.OnFrameRendered = null;
    }
}

public readonly struct RawFrame : IDisposable
{
    public int Width { get; }
    public int Height { get; }
    public double TimeMs { get; }

    public byte[] PixelData { get; }
    private readonly int byteCount;

    public RawFrame(int width, int height, double timeMs, byte[] pixelData, int byteCount)
    {
        Width = width;
        Height = height;
        TimeMs = timeMs;
        PixelData = pixelData;
        this.byteCount = byteCount;
    }

    public ReadOnlySpan<byte> Data => PixelData.AsSpan(0, byteCount);

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(PixelData);
    }
}
#nullable restore
