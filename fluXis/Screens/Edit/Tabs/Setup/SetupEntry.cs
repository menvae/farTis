using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Graphics.UserInterface.Interaction;
using fluXis.Utils.Extensions;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

namespace fluXis.Screens.Edit.Tabs.Setup;

public partial class SetupEntry : CompositeDrawable, IHasTooltip
{
    public LocalisableString TooltipText { get; set; }
    public ColourInfo BackgroundColor { get; init; } = Theme.Background3;

    protected virtual float ContentSpacing => 4;
    protected virtual bool ShowHoverFlash => false;

    private FluXisSpriteText titleSprite;
    private HoverLayer hover;
    private FlashLayer flash;

    private string title { get; }

    public SetupEntry(string title)
    {
        this.title = title;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        Height = 60;
        CornerRadius = 10;
        Masking = true;
        BorderColour = BackgroundColor;
        BorderThickness = 3;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = BackgroundColor
            },
            hover = new HoverLayer(),
            flash = new FlashLayer(),
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(ContentSpacing),
                Children = new[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new[]
                        {
                            titleSprite = new FluXisSpriteText
                            {
                                Text = title,
                                WebFontSize = 16,
                                Colour = Theme.Text2
                            },
                            CreateRightTitle().With(d =>
                            {
                                d.Anchor = Anchor.TopRight;
                                d.Origin = Anchor.TopRight;
                            })
                        }
                    },
                    CreateContent()
                }
            }
        };
    }

    protected virtual Drawable CreateContent() => Empty();
    protected virtual Drawable CreateRightTitle() => Empty();

    protected void StartHighlight()
    {
        BorderThickness = 3;
        this.BorderColorTo(Theme.Highlight, 50);
        titleSprite.FadeColour(Theme.Highlight, 50);
    }

    protected void StopHighlight()
    {
        this.BorderColorTo(BackgroundColor, 50);
        titleSprite.FadeColour(Theme.Text.Opacity(.8f), 50);
    }

    protected override bool OnHover(HoverEvent e)
    {
        if (!ShowHoverFlash)
            return false;

        hover.Show();
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e) => hover.Hide();

    protected override bool OnClick(ClickEvent e)
    {
        if (!ShowHoverFlash)
            return false;

        flash.Show();
        return true;
    }
}
