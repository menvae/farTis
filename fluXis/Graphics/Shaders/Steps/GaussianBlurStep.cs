using System.Runtime.InteropServices;
using fluXis.Map.Structures.Events;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Utils;
using osuTK;

namespace fluXis.Graphics.Shaders.Steps;

public class GaussianBlurStep : ShaderStep<GaussianBlurStep.BlurParameters>
{
    protected override string FragmentShader => "GaussianBlur";
    public override ShaderType Type => ShaderType.GaussianBlur;

    private int kernelRadius;
    private float sigma;
    private Vector2 direction;

    private const float max_blur = 32f;

    public override void UpdateParameters(IFrameBuffer current) => ParameterBuffer.Data = ParameterBuffer.Data with
    {
        TexSize = current.Size,
        Radius = kernelRadius,
        Sigma = sigma,
        Direction = direction
    };

    private IFrameBuffer bufferX;
    private IFrameBuffer bufferY;

    private void drawPass(IRenderer renderer, IFrameBuffer src, IFrameBuffer dst, Vector2 dir)
    {
        direction = dir;
        UpdateParameters(src);
        Shader.BindUniformBlock($"m_{nameof(BlurParameters)}", ParameterBuffer);
        dst.Bind();
        Shader.Bind();
        DrawFrameBuffer(renderer, src);
        Shader.Unbind();
        dst.Unbind();
    }

    public override void DrawBuffer(IRenderer renderer, IFrameBuffer current, IFrameBuffer target)
    {
        sigma = max_blur * Strength;
        kernelRadius = Blur.KernelSize(sigma);
        DrawColor = Colour4.White;

        bufferX ??= renderer.CreateFrameBuffer();
        bufferX.Size = current.Size;
        bufferY ??= renderer.CreateFrameBuffer();
        bufferY.Size = current.Size;

        target.Unbind();
        drawPass(renderer, current, bufferX, Vector2.UnitX);
        drawPass(renderer, bufferX, bufferY, Vector2.UnitY);

        target.Bind();
        Shader.Bind();
        DrawFrameBuffer(renderer, bufferY);
        Shader.Unbind();
    }

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
