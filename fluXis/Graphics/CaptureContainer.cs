using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osuTK;
using osuTK.Graphics;

namespace fluXis.Graphics;

#nullable enable
public partial class CaptureContainer : Container, IBufferedDrawable
{
    private readonly BufferedDrawNodeSharedData sharedData = new(new[] { RenderBufferFormat.D16 }, pixelSnapping: true, clipToRootNode: true);
    private IShader textureShader = null!;

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders)
    {
        textureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
    }

    IShader ITexturedShaderDrawable.TextureShader => textureShader;
    Color4 IBufferedDrawable.BackgroundColour => new Color4(0, 0, 0, 0);
    DrawColourInfo? IBufferedDrawable.FrameBufferDrawColour => new DrawColourInfo(Color4.White);
    Vector2 IBufferedDrawable.FrameBufferScale => Vector2.One;

    public Action<IFrameBuffer>? OnFrameRendered;

    protected override bool RequiresChildrenUpdate => true;

    protected override DrawNode CreateDrawNode() => new CaptureDrawNode(this, sharedData);

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        sharedData.Dispose();
    }

    private class CaptureDrawNode : BufferedDrawNode
    {
        private Action<IFrameBuffer>? onRendered;

        public CaptureDrawNode(CaptureContainer source, BufferedDrawNodeSharedData sharedData)
            : base(source, new CompositeDrawableDrawNode(source), sharedData)
        {
        }

        public override void ApplyState()
        {
            base.ApplyState();
            onRendered = ((CaptureContainer)Source).OnFrameRendered;
        }

        protected override void DrawContents(IRenderer renderer)
        {
            base.DrawContents(renderer);

            onRendered?.Invoke(SharedData.MainBuffer);
        }
    }
}
#nullable restore
