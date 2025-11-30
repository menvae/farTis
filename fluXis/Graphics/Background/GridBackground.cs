using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using Vector2 = System.Numerics.Vector2;

namespace fluXis.Graphics.Background;

public partial class GridBackground : CompositeDrawable
{
    private float gridSize = 25f;
    private Colour4 gridColor = Colour4.White.Opacity(0.2f);
    private float thickness = 2f;
    private GridDrawable gridDrawable;

    public float GridSize
    {
        get => gridSize;
        set
        {
            if (gridSize == value) return;
            gridSize = value;
            gridDrawable?.Invalidate(Invalidation.DrawNode);
        }
    }

    public Colour4 GridColor
    {
        get => gridColor;
        set
        {
            if (gridColor == value) return;
            gridColor = value;
            gridDrawable?.Invalidate(Invalidation.DrawNode);
        }
    }

    public float Thickness
    {
        get => thickness;
        set
        {
            if (thickness == value) return;
            thickness = value;
            gridDrawable?.Invalidate(Invalidation.DrawNode);
        }
    }

    public float DefaultOpacity { get; set; } = 0.7f;

    public float HoverOpacity { get; set; } = 0.35f;

    protected override void LoadComplete()
    {
        base.LoadComplete();
        RelativeSizeAxes = Axes.Both;

        AddInternal(gridDrawable = new GridDrawable(this)
        {
            RelativeSizeAxes = Axes.Both,
            Alpha = DefaultOpacity
        });
    }

    protected override bool OnHover(HoverEvent e)
    {
        gridDrawable?.FadeTo(HoverOpacity, 300, Easing.OutQuint);
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        gridDrawable?.FadeTo(DefaultOpacity, 300, Easing.OutQuint);
        base.OnHoverLost(e);
    }

    private partial class GridDrawable : Drawable
    {
        private readonly GridBackground parent;

        public GridDrawable(GridBackground parent)
        {
            this.parent = parent;
        }

        protected override DrawNode CreateDrawNode() => new GridBackgroundDrawNode(this);

        private class GridBackgroundDrawNode : DrawNode
        {
            protected new GridDrawable Source => (GridDrawable)base.Source;

            private float gridSize;
            private Colour4 gridColor;
            private float thickness;
            private Vector2 drawSize;

            public GridBackgroundDrawNode(GridDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                var parent = Source.parent;
                gridSize = parent.gridSize;
                gridColor = parent.gridColor;
                thickness = parent.thickness;
                drawSize = new Vector2(Source.DrawWidth, Source.DrawHeight);
            }

            private void drawLine(IRenderer renderer, Texture texture, Vector2 start, Vector2 end, Colour4 color)
            {
                var direction = Vector2.Normalize(end - start);
                var perpendicular = new Vector2(-direction.Y, direction.X) * (thickness / 2f);

                var p1 = start - perpendicular;
                var p2 = start + perpendicular;
                var p3 = end - perpendicular;
                var p4 = end + perpendicular;

                var quad = new Quad(
                    Vector2Extensions.Transform(new osuTK.Vector2(p1.X, p1.Y), DrawInfo.Matrix),
                    Vector2Extensions.Transform(new osuTK.Vector2(p2.X, p2.Y), DrawInfo.Matrix),
                    Vector2Extensions.Transform(new osuTK.Vector2(p3.X, p3.Y), DrawInfo.Matrix),
                    Vector2Extensions.Transform(new osuTK.Vector2(p4.X, p4.Y), DrawInfo.Matrix)
                );

                renderer.DrawQuad(texture, quad, color);
            }

            protected override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                if (gridSize <= 0) return;

                var texture = renderer.WhitePixel;
                var color = gridColor;

                for (float x = 0; x <= drawSize.X; x += gridSize)
                    drawLine(renderer, texture, new Vector2(x, 0), new Vector2(x, drawSize.Y), color);

                for (float y = 0; y <= drawSize.Y; y += gridSize)
                    drawLine(renderer, texture, new Vector2(0, y), new Vector2(drawSize.X, y), color);
            }
        }
    }
}