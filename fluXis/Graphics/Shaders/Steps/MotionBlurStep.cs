using System;
using System.Runtime.InteropServices;
using fluXis.Map.Structures.Events;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Utils;
using osuTK;

namespace fluXis.Graphics.Shaders.Steps;

public class MotionBlurStep : ShaderStep<MotionBlurStep.BlurParameters>
{
    protected override string FragmentShader => "Blur";
    public override ShaderType Type => ShaderType.MotionBlur;

    public override bool ShouldRender => Strength > 0;

    private Vector2 blurDirection
    {
        get
        {
            float rad = MathHelper.DegreesToRadians(Strength2);
            return new Vector2(MathF.Cos(rad), MathF.Sin(rad));
        }
    }

    private const float max_sigma = 40f;

    public override void UpdateParameters(IFrameBuffer current) => ParameterBuffer.Data = ParameterBuffer.Data with
    {
        TexSize = current.Size,
        Direction = blurDirection,
        Radius = Blur.KernelSize(Strength * max_sigma),
        Sigma = Strength * max_sigma
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct BlurParameters
    {
        public UniformVector2 TexSize;
        public UniformInt Radius;
        public UniformFloat Sigma;
        public UniformVector2 Direction;
        private readonly UniformPadding8 pad1;
    }
}
