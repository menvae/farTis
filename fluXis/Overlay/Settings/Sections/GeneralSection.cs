using fluXis.Graphics.Sprites.Icons;
using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Localization;
using fluXis.Overlay.Settings.Sections.General;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace fluXis.Overlay.Settings.Sections;

public partial class GeneralSection : SettingsSection
{
    public override IconUsage Icon => FontAwesome6.Solid.Gear;
    public override LocalisableString Title => LocalizationStrings.Settings.General.Title;

    [BackgroundDependencyLoader]
    private void load()
    {
        AddRange(new Drawable[]
        {
            new GeneralLanguageSection(),
            Divider,
            new GeneralFoldersSection(),
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Margin = new MarginPadding { Top = 20 },
                Children = new Drawable[]
                {
                    new FluXisSpriteText
                    {
                        Text = "fluXis",
                        FontSize = 32,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.BottomCentre
                    },
                    new FluXisSpriteText
                    {
                        Text = FluXisGameBase.VersionString,
                        Colour = Theme.Text2,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.TopCentre
                    }
                }
            }
        });
    }
}
