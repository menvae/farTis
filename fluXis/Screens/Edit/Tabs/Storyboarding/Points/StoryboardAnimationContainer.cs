using System.Collections.Generic;
using fluXis.Screens.Edit.Tabs.Shared.Points.List;
using fluXis.Screens.Edit.Tabs.Storyboarding.Timeline.Blueprints;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Screens.Edit.Tabs.Storyboarding.Points;

public partial class StoryboardAnimationContainer : Container
{
    [Resolved]
    protected EditorMap Map { get; private set; }

    [Resolved]
    private TimelineBlueprintContainer blueprints { get; set; }

    private bool showingSettings;

    private PointsList pointsList;
    public ClickableContainer SettingsWrapper;
    public FillFlowContainer SettingsFlow;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        Height = 400;

        InternalChildren = new Drawable[]
        {
            pointsList = CreatePointsList().With(l =>
            {

                l.Alpha = 0;
                l.ShowSettings = showPointSettings;
                l.RequestClose = close;
            }),
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        blueprints.SelectionHandler.SelectedObjects.BindCollectionChanged((el, n) =>
        {
            bool shouldClose = false;

            if (n?.NewItems is null)
                shouldClose |= true;
            else if (n.NewItems.Count != 1)
                shouldClose |= true;

            if (shouldClose) close();
        }, true);

        pointsList.Show();
        pointsList.Header.Text = "Animations";
        pointsList.Header.WebFontSize = 16;
        pointsList.Header.Height = 36;
        pointsList.Padding = new MarginPadding(0);
    }

    protected PointsList CreatePointsList() => new StoryboardAnimationPointsList();

    private void showPointSettings(IEnumerable<Drawable> drawables)
    {
        if (showingSettings) return;

        showingSettings = true;

        SettingsFlow.Clear();
        SettingsFlow.AddRange(drawables);

        pointsList.FadeOut(200).ScaleTo(.9f, 400, Easing.OutQuint);
        SettingsWrapper.ScaleTo(1.1f).FadeInFromZero(200).ScaleTo(1, 400, Easing.OutQuint);
    }

    private void close()
    {
        if (!showingSettings)
        {
            return;
        }

        showingSettings = false;

        SettingsWrapper.FadeOut(200).ScaleTo(1.2f, 400, Easing.OutQuint);
        pointsList.FadeIn(200).ScaleTo(1, 400, Easing.OutQuint);
    }
}
