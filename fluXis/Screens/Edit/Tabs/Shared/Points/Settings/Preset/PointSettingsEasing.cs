using System;
using System.Linq;
using fluXis.Graphics.Sprites.Icons;
using fluXis.Map.Structures.Bases;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;
using osuTK;

namespace fluXis.Screens.Edit.Tabs.Shared.Points.Settings.Preset;

public partial class PointSettingsEasing<T> : PointSettingsDropdown<Easing>
    where T : class, ITimedObject, IHasEasing
{
    private readonly Bindable<bool> bezierEnabled = new(false);
    private readonly Bindable<CubicBezierEasingFunction> bezierFunction;
    private PointSettingsBezier bezierSettings;
    private readonly EditorMap map;
    private readonly T obj;
    
    public PointSettingsEasing(EditorMap map, T obj)
    {
        this.map = map;
        this.obj = obj;
        
        Text = "Easing";
        TooltipText = "The easing function used to interpolate between scales.";
        Items = Enum.GetValues<Easing>().ToList();
        CurrentValue = obj.Easing;
        
        bezierFunction = new Bindable<CubicBezierEasingFunction>(
            obj.ControlPoints.HasValue
                ? new CubicBezierEasingFunction(obj.ControlPoints.Value.X, obj.ControlPoints.Value.Y, 
                                                 obj.ControlPoints.Value.Z, obj.ControlPoints.Value.W)
                : PointSettingsBezier.EasingToBezier(obj.Easing)
        );
        
        bezierEnabled.Value = obj.ControlPoints.HasValue;
        
        OnValueChanged = easingChanged;
        bezierEnabled.BindValueChanged(bezierToggled);
        bezierFunction.BindValueChanged(bezierChanged);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        TextContainer.Add(new BezierToggle(bezierEnabled));
        
        AddInternal(bezierSettings = new PointSettingsBezier(bezierFunction)
        {
            Alpha = bezierEnabled.Value ? 1f : 0f,
        });
    }

    private void easingChanged(Easing easing)
    {
        obj.Easing = easing;
        
        if (bezierEnabled.Value)
        {
            var bezier = PointSettingsBezier.EasingToBezier(easing);
            bezierFunction.Value = bezier;
            obj.ControlPoints = PointSettingsBezier.BezierToVector4(bezier);
        }
        else
        {
            obj.ControlPoints = null;
        }
        
        map.Update(obj);
    }

    private void bezierToggled(ValueChangedEvent<bool> e)
    {
        obj.ControlPoints = e.NewValue 
            ? PointSettingsBezier.BezierToVector4(bezierFunction.Value) 
            : null;
        
        if (bezierSettings != null)
        {
            bezierSettings.FadeTo(e.NewValue ? 1f : 0f, 200);
            bezierSettings.TransformTo(nameof(Margin), new MarginPadding { Top = e.NewValue ? DrawHeight + 16 : 0 }, 200, Easing.OutQuad);
            this.TransformTo(nameof(Padding), new MarginPadding { Bottom = e.NewValue ? bezierSettings.Graph.GraphHeight : 0 }, 200, Easing.OutQuad);
        }
        
        map.Update(obj);
    }

    private void bezierChanged(ValueChangedEvent<CubicBezierEasingFunction> e)
    {
        if (bezierEnabled.Value)
        {
            var defaultBezier = PointSettingsBezier.EasingToBezier(obj.Easing);
            bool isCustom = !Precision.AlmostEquals(e.NewValue.X1, defaultBezier.X1) ||
                           !Precision.AlmostEquals(e.NewValue.Y1, defaultBezier.Y1) ||
                           !Precision.AlmostEquals(e.NewValue.X2, defaultBezier.X2) ||
                           !Precision.AlmostEquals(e.NewValue.Y2, defaultBezier.Y2);
            
            Dropdown.UpdateLabel(isCustom ? "Custom" : obj.Easing.ToString());
            obj.ControlPoints = PointSettingsBezier.BezierToVector4(e.NewValue);
            map.Update(obj);
        }
    }

    private partial class BezierToggle : ClickableContainer
    {
        private const float disabled_opacity = 0.3f;
        private const float enabled_opacity = 1f;
        
        public BezierToggle(Bindable<bool> isEnabled)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreLeft;
            Margin = new MarginPadding { Left = 10 };
            
            Child = new FluXisSpriteIcon
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Icon = FontAwesome.Solid.ChartLine,
                Size = new Vector2(20)
            };
            
            Action = () => isEnabled.Value = !isEnabled.Value;
            
            isEnabled.BindValueChanged(e => this.FadeTo(e.NewValue ? enabled_opacity : disabled_opacity, 200), true);
        }
    }
}
