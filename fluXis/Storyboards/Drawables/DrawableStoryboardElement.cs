using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Map.Structures.Bases;
using fluXis.Utils.Extensions;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;

namespace fluXis.Storyboards.Drawables;

public partial class DrawableStoryboardElement : CompositeDrawable, IHasMutableProperties
{
    protected virtual bool AllowBorder => false;
    public StoryboardElement Element { get; }

    public override bool RemoveCompletedTransforms => false;

    public DrawableStoryboardElement(StoryboardElement element)
    {
        Element = element;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Element.Anchor;
        Origin = Element.Origin;
        X = Element.StartX;
        Y = Element.StartY;
        Width = Element.Width;
        Height = Element.Height;
        Colour = Colour4.FromRGBA(Element.Color);
        AlwaysPresent = true;

        Blending = Element.Blending ? BlendingParameters.GetDefaultParameters(Element.BlendingMode) : BlendingParameters.Mixture;

        var anims = Element.Animations.OrderBy(x => x.StartTime).ToList();
        var propChanges = Element.PropertyChanges.OrderBy(x => x.Time).ToList();

        buildAnimations(anims);
        buildPropertyChanges(propChanges);
    }

    private void buildPropertyChanges(List<StoryboardPropertyChange> propChanges)
    {
        var mutableProps = ((IHasMutableProperties)this).MutableProperties;

        foreach (var propertyChange in propChanges)
        {
            if (!mutableProps.TryGetValue(propertyChange.PropertyKey, out var prop)) continue;

            try
            {
                var convertedProp = StoryboardElement.ConvertProperty(propertyChange.Value, prop.PropertyType);
                if (convertedProp == null) continue;

                using (BeginAbsoluteSequence(Element.StartTime + propertyChange.Time))
                {
                    TransformableExtensions.TransformTo((dynamic)this, prop.Name, (dynamic)convertedProp, 0);
                }
            }
            catch (Exception e)
            {
                // TODO:
                // There are some unhandled exceptions that shouldn't happen when moving/rescaling element.
                // look into later...
                Logger.Log($"Failed to apply property transform: {e}");
            }
        }
    }

    private void buildAnimations(List<StoryboardAnimation> anims)
    {
        foreach (var animation in anims)
        {
            using (BeginAbsoluteSequence(Element.StartTime + animation.StartTime))
            {
                var duration = Math.Max(animation.Duration, 0);
                var useStart = animation.UseStartValue;

                switch (animation.Type)
                {
                    case StoryboardAnimationType.MoveX:
                        if (useStart) this.MoveToX(animation.StartFloat);
                        this.MoveToX(animation.EndFloat, duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.MoveY:
                        if (useStart) this.MoveToY(animation.StartFloat);
                        this.MoveToY(animation.EndFloat, duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.Scale:
                        if (useStart) this.ScaleTo(animation.StartFloat);
                        this.ScaleTo(animation.EndFloat, duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.ScaleVector:
                        if (useStart) this.ScaleTo(animation.StartVector);
                        this.ScaleTo(animation.EndVector, duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.Width:
                        if (useStart) this.ResizeWidthTo(animation.StartFloat);
                        this.ResizeWidthTo(animation.EndFloat, duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.Height:
                        if (useStart) this.ResizeHeightTo(animation.StartFloat);
                        this.ResizeHeightTo(animation.EndFloat, duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.Rotate:
                        if (useStart) this.RotateTo(animation.StartFloat);
                        this.RotateTo(animation.EndFloat, duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.Fade:
                        if (useStart) this.FadeTo(animation.StartFloat);
                        this.FadeTo(animation.EndFloat, duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.Color:
                        if (useStart) this.FadeColour(Colour4.FromHex(animation.StartValue));
                        this.FadeColour(Colour4.FromHex(animation.ValueEnd), duration, animation.Easing);
                        break;

                    case StoryboardAnimationType.Border when AllowBorder:
                        if (useStart) this.BorderTo(animation.StartFloat);
                        this.BorderTo(animation.EndFloat, duration, animation.Easing);
                        break;
                }
            }
        }
    }
}
