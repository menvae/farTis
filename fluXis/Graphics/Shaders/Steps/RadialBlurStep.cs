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

    private float centerX { get; set; }
    private float centerY { get; set; }

    public override void UpdateParameters(IFrameBuffer current) => ParameterBuffer.Data = ParameterBuffer.Data with
    {
        TexSize = current.Size,
        Sigma = (Strength / 4f) + 0.5f,
        Position = new Vector2((centerX + 1f) / 2f, (-centerY + 1f) / 2f)
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct BlurParameters
    {
        public UniformVector2 TexSize;
        public UniformFloat Sigma;
        public UniformPadding4 pad1;
        public UniformVector2 Position;
        public UniformPadding8 pad2;
    }
}
