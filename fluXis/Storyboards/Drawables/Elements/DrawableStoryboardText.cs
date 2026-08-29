using System;
using fluXis.Graphics.Sprites.Text;
using fluXis.Utils.Attributes;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;

namespace fluXis.Storyboards.Drawables.Elements;

public partial class DrawableStoryboardText : DrawableStoryboardElement
{
    [StoryboardChangeableProperty("text")]
    private LocalisableString innerText
    {
        get => spriteText.Text;
        set => spriteText.Text = value;
    }

    private FluXisSpriteText spriteText = new();

    public DrawableStoryboardText(StoryboardElement element)
        : base(element)
    {
        if (element.Type != StoryboardElementType.Text)
            throw new ArgumentException("Element provided is not a text", nameof(element));
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AutoSizeAxes = Axes.Both;

        var text = Element.GetParameter("text", string.Empty);
        var size = Element.GetParameter("size", 20f);
        var renderBoundsOnly = Element.GetParameter("renderBoundsOnly", false);

        InternalChild = spriteText = new FluXisSpriteText
        {
            Text = text,
            FontSize = size,
            RenderBoundsOnly = renderBoundsOnly
        };
    }
}
