using System;
using fluXis.Graphics.Background;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace fluXis.Screens.Edit.Tabs.Shared.Points.Settings;

public partial class PointSettingsBezier : PointSettingsBase
{
    /// <summary>
    /// (X1, Y1, X2, Y2)
    /// </summary>
    public Bindable<CubicBezierEasingFunction> Bindable = new(new CubicBezierEasingFunction(0, 0, 1f, 1f));

    public BezierGraph Graph;

    public PointSettingsBezier(Bindable<CubicBezierEasingFunction> Bindable)
    {
        this.Bindable = Bindable;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        InternalChildren = new Drawable[]
        {
            Graph = new BezierGraph(Bindable)
        };
    }

    public static CubicBezierEasingFunction EasingToBezier(Easing easing)
    {
        double s1 = Interpolation.ApplyEasing(easing, 1.0 / 3.0);
        double s2 = Interpolation.ApplyEasing(easing, 2.0 / 3.0);
        
        double cy1 = (3.0 * s1) - (1.5 * s2) + (1.0 / 3.0);
        double cy2 = (3.0 * s2) - (1.5 * s1) - (5.0 / 6.0);

        double x1 = 0.33333;
        double x2 = 0.66667;

        x1 = Math.Clamp(x1, 0, 1);
        x2 = Math.Clamp(x2, 0, 1);
        
        cy1 = Math.Clamp(cy1, 0, 1);
        cy2 = Math.Clamp(cy2, 0, 1);

        return new CubicBezierEasingFunction(x1, cy1, x2, cy2);
    }

    public static Vector4 BezierToVector4(CubicBezierEasingFunction easing)
    {
        return new Vector4((float)easing.X1, (float)easing.Y1, (float)easing.X2, (float)easing.Y2);
    }
    
    public partial class BezierGraph : CompositeDrawable
    {
        private static readonly CubicBezierEasingFunction default_func = new(0, 0, 1f, 1f);
        
        private readonly Bindable<Vector2> p1 = new(new Vector2((float)default_func.X1, (float)default_func.Y1));
        private readonly Bindable<Vector2> p2 = new(new Vector2((float)default_func.X2, (float)default_func.Y2));
        private readonly SmoothPath path;
        private readonly Box line1, line2;
        private readonly Container graphContainer;

        public float GraphHeight => graphContainer.DrawHeight;

        public readonly Bindable<CubicBezierEasingFunction> EasingFunction = new();

        public BezierGraph(Bindable<CubicBezierEasingFunction> easingFunction)
        {
            EasingFunction.BindTo(easingFunction);
            
            RelativeSizeAxes = Axes.X;
            Height = 200;

            InternalChild = graphContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(1f),
                FillMode = FillMode.Fill,
                Children = new Drawable[]
                {
                    new GridBackground
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    path = new SmoothPath
                    {
                        PathRadius = 2f,
                        Anchor = Anchor.TopLeft,
                        Colour = Color4.White,
                    },
                    line1 = new Box
                    {
                        Height = 2,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.CentreLeft,
                        EdgeSmoothness = new Vector2(1),
                        Colour = Color4.White,
                        Alpha = 0.3f,
                    },
                    line2 = new Box
                    {
                        Height = 2,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.CentreRight,
                        EdgeSmoothness = new Vector2(1),
                        Colour = Color4.White,
                        Alpha = 0.3f,
                    },
                    new ControlPoint
                    {
                        Point = { BindTarget = p1 },
                    },
                    new ControlPoint
                    {
                        Point = { BindTarget = p2 },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (EasingFunction?.Value != null)
            {
                p1.Value = new Vector2((float)EasingFunction.Value.X1, (float)EasingFunction.Value.Y1);
                p2.Value = new Vector2((float)EasingFunction.Value.X2, (float)EasingFunction.Value.Y2);
            }

            EasingFunction.BindValueChanged(e =>
            {
                if (e.NewValue.IsNotNull())
                {
                    p1.Value = new Vector2((float)e.NewValue.X1, (float)e.NewValue.Y1);
                    p2.Value = new Vector2((float)e.NewValue.X2, (float)e.NewValue.Y2);
                }
            });

            p1.BindValueChanged(_ => Scheduler.AddOnce(easingChanged), true);
            p2.BindValueChanged(_ => Scheduler.AddOnce(easingChanged), true);
        }
        
        private void easingChanged()
        {
            path.ClearVertices();

            var easing = EasingFunction.Value = new CubicBezierEasingFunction(p1.Value.X, p1.Value.Y, p2.Value.X, p2.Value.Y);

            var width = graphContainer.DrawWidth;
            var height = graphContainer.DrawHeight;

            for (double d = 0; d < 1; d += 0.01)
            {
                double value = easing.ApplyEasing(d);
                path.AddVertex(new Vector2((float)d * width, (1 - (float)value) * height));
            }

            path.AddVertex(new Vector2(width, (1 - (float)easing.ApplyEasing(1)) * height));

            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);

            line1.Width = p1.Value.Length * width;
            line1.Rotation = -MathHelper.RadiansToDegrees(MathF.Atan2(p1.Value.Y, p1.Value.X));

            line2.Width = Vector2.Distance(p2.Value, Vector2.One) * width;
            line2.Rotation = -MathHelper.RadiansToDegrees(MathF.Atan2(1 - p2.Value.Y, 1 - p2.Value.X));
        }
    }

    private partial class ControlPoint : CompositeDrawable
    {
        public readonly Bindable<Vector2> Point = new();

        public ControlPoint()
        {
            RelativePositionAxes = Axes.Both;
            Size = new Vector2(20);
            Origin = Anchor.Centre;

            InternalChild = new Circle
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.White,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Point.BindValueChanged(p => Position = new Vector2(p.NewValue.X, 1 - p.NewValue.Y), true);
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            var position = Vector2.Divide(Parent!.ToLocalSpace(e.ScreenSpaceMousePosition), Parent.ChildSize);

            Point.Value = new Vector2(
                float.Round(float.Clamp(position.X, 0, 1), 2),
                float.Round(float.Clamp(1f - position.Y, 0, 1), 2)
            );
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.ScaleTo(1.35f, 50);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.ScaleTo(1f, 50);
        }
    }
}