using System;
using System.Runtime.InteropServices;
using fluXis.Map.Structures.Events;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Utils;
using osuTK;

namespace fluXis.Graphics.Shaders.Steps;

public class RadialBlurStep : ShaderStep<RadialBlurStep.BlurParameters>
{
    protected override string FragmentShader => "RadialBlur";
    public override ShaderType Type => ShaderType.RadialBlur;

    public override bool ShouldRender => !Precision.AlmostEquals(Strength, 0);

    public override void UpdateParameters(IFrameBuffer current)
    {
        float sigma = (Strength / 4f) + 0.5f;
        float angleStep = ((sigma - 0.5f) * 4.0f) / 128f;
        int samples = Math.Min(128, (int)MathF.Ceiling(2f * MathF.PI / MathF.Abs(angleStep)));

        ParameterBuffer.Data = ParameterBuffer.Data with
        {
            TexSize = current.Size,
            Sigma = sigma,
            Samples = samples,
            Position = new Vector2((Strength2 + 1f) / 2f, (-Strength3 + 1f) / 2f),
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct BlurParameters
    {
        public UniformVector2 TexSize;
        public UniformFloat Sigma;
        public UniformInt Samples;
        public UniformVector2 Position;
        public UniformPadding8 pad2;
    }
}
