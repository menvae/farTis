using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FFmpeg.AutoGen;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace fluXis.Utils;

public static class ImageUtils
{
    public static Colour4 GetAverageColour(Stream stream)
    {
        if (stream == null)
            return Colour4.Transparent;

        try
        {
            var image = Image.Load<Rgba32>(stream);
            image.Mutate(x => x.Quantize(new WuQuantizer(new QuantizerOptions { MaxColors = 10 })));

            var dict = new Dictionary<Rgba32, int>();

            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var pixel = image[x, y];

                    if (pixel.A == 0)
                        continue;

                    if (!dict.TryAdd(pixel, 1))
                        dict[pixel]++;
                }
            }

            var orderedByLight = dict.Select(x => x.Key).Select(x => new Colour4(x.R, x.G, x.B, 255)).OrderBy(x => x.ToHSL().Z).ToList();
            return orderedByLight.ElementAt(orderedByLight.Count / 2);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get average colour from image");
            return Colour4.Transparent;
        }
    }

    public static unsafe AVFrame* Rgba32ToAvFrame(Image<Rgba32> image)
    {
        var frame = ffmpeg.av_frame_alloc();
        frame->format = (int)AVPixelFormat.AV_PIX_FMT_RGBA;
        frame->width = image.Width;
        frame->height = image.Height;

        int bufferSize = ffmpeg.av_image_get_buffer_size(
            AVPixelFormat.AV_PIX_FMT_RGBA, frame->width, frame->height, 1);

        byte* buffer = (byte*)ffmpeg.av_malloc((ulong)bufferSize);

        var data = new byte_ptrArray4();
        var linesize = new int_array4();

        ffmpeg.av_image_fill_arrays(
            ref data, ref linesize,
            buffer,
            AVPixelFormat.AV_PIX_FMT_RGBA,
            frame->width, frame->height, 1);

        frame->data[0] = data[0];
        frame->linesize[0] = linesize[0];

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                fixed (void* rowPtr = row)
                {
                    System.Buffer.MemoryCopy(
                        rowPtr,
                        frame->data[0] + (y * frame->linesize[0]),
                        frame->linesize[0],
                        row.Length * sizeof(Rgba32));
                }
            }
        });

        return frame;
    }
}
