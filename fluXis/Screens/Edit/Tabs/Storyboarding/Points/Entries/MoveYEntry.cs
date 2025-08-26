using System.Collections.Generic;
using System.Linq;
using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures.Bases;
using fluXis.Screens.Edit.Tabs.Shared.Points.List;
using fluXis.Screens.Edit.Tabs.Shared.Points.Settings;
using fluXis.Screens.Edit.Tabs.Shared.Points.Settings.Preset;
using fluXis.Screens.Edit.Tabs.Storyboarding.Timeline.Blueprints;
using fluXis.Storyboards;
using fluXis.Utils;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace fluXis.Screens.Edit.Tabs.Storyboarding.Points.Entries;

public partial class MoveYEntry : PointListEntry
{
    [Resolved]
    private TimelineBlueprintContainer blueprints { get; set; }

    protected override string Text => "Move Y";
    protected override Colour4 Color => Theme.Flash;

    private StoryboardAnimation animation => Object as StoryboardAnimation;

    public MoveYEntry(StoryboardAnimation obj)
        : base(obj)
    {
        WithTime = false;
    }

    public override ITimedObject CreateClone() => animation.JsonCopy();

    private ITimedObject selectedObject = null;
    
    private void changeSettings(StoryboardAnimation animation)
    {
        Map.Update(animation);
        Map.Storyboard.Update(selectedObject ??= blueprints.SelectionHandler.SelectedObjects?.First());
    }

    protected override Drawable[] CreateValueContent()
    {
        var startValue = string.IsNullOrEmpty(animation.ValueStart) ? "0" : animation.StartFloat.ToString("F1");
        var endValue = string.IsNullOrEmpty(animation.ValueEnd) ? "0" : animation.EndFloat.ToString("F1");
        
        return new Drawable[]
        {
            new FluXisSpriteText
            {
                Text = $"{Text} {startValue} → {endValue} {(int)animation.Duration}ms {animation.Easing}",
                Colour = Color,
                Alpha = 0.8f
            }
        };
    }

    protected override IEnumerable<Drawable> CreateSettings()
    {
        return base.CreateSettings().Concat(new Drawable[]
        {
            
            new PointSettingsTime(Map, animation)
            {
                Text = "Start Time",
                OnTextChanged = box =>
                {
                    if (box.Text.TryParseDoubleInvariant(out var result))
                    {
                        animation.StartTime = result;
                        changeSettings(animation);
                    }
                    else
                        box.NotifyError();
                },
            },
            new PointSettingsLength<StoryboardAnimation>(Map, animation, BeatLength) {
                Text = "Duration",
                OnTextChanged = box =>
                {
                    if (box.Text.TryParseFloatInvariant(out var result))
                        animation.Duration = result * BeatLength;
                    else
                        box.NotifyError();

                    changeSettings(animation);
                }
            },
            new PointSettingsTextBox
            {
                Text = "Start Y",
                DefaultText = animation.ValueStart ?? "0",
                OnTextChanged = box =>
                {
                    if (string.IsNullOrWhiteSpace(box.Text))
                    {
                        animation.ValueStart = "0";
                        changeSettings(animation);
                        return;
                    }

                    if (box.Text.TryParseFloatInvariant(out var result))
                    {
                        animation.ValueStart = result.ToStringInvariant();
                        changeSettings(animation);
                    }
                    else
                        box.NotifyError();
                }
            },
            new PointSettingsTextBox
            {
                Text = "End Y",
                DefaultText = animation.ValueEnd ?? "0",
                OnTextChanged = box =>
                {
                    if (string.IsNullOrWhiteSpace(box.Text))
                    {
                        animation.ValueEnd = "0";
                        changeSettings(animation);
                        return;
                    }

                    if (box.Text.TryParseFloatInvariant(out var result))
                    {
                        animation.ValueEnd = result.ToStringInvariant();
                        changeSettings(animation);
                    }
                    else
                        box.NotifyError();
                }
            },
            new PointSettingsEasing<StoryboardAnimation>(Map, animation)
            {
                OnValueChanged = newValue => {
                    animation.Easing = newValue;
                    changeSettings(animation);
                }
            },
        });
    }
}