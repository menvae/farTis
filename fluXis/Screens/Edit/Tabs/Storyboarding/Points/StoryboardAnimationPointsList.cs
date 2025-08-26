using System.Collections.Generic;
using System.Linq;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures.Bases;
using fluXis.Screens.Edit.Tabs.Shared.Points.List;
using fluXis.Screens.Edit.Tabs.Storyboarding.Points.Entries;
using fluXis.Screens.Edit.Tabs.Storyboarding.Timeline.Blueprints;
using fluXis.Storyboards;
using osu.Framework.Allocation;

namespace fluXis.Screens.Edit.Tabs.Storyboarding.Points;

public partial class StoryboardAnimationPointsList : PointsList
{
    [Resolved]
    private TimelineBlueprintContainer blueprints { get; set; }

    protected override void RegisterEvents()
    {
        var element = blueprints.SelectionHandler.SelectedObjects.First();
        element.AnimationAdded += AddPoint;
        element.AnimationRemoved += RemovePoint;
        element.AnimationUpdated += UpdatePoint;
        element.Animations.ForEach(AddPoint);
    }

    protected override PointListEntry CreateEntryFor(ITimedObject obj) => obj switch
    {
        StoryboardAnimation anim when anim.Type == StoryboardAnimationType.MoveX => new MoveXEntry(anim),
        StoryboardAnimation anim when anim.Type == StoryboardAnimationType.MoveY => new MoveYEntry(anim),
        _ => null
    };

    protected override IEnumerable<DropdownEntry> CreateDropdownEntries()
    {
        var entries = new List<DropdownEntry>
        {
            new("Move X", Theme.ScrollMultiply, () => Create(new StoryboardAnimation { Type = StoryboardAnimationType.MoveX }), x => x is StoryboardAnimation { Type: StoryboardAnimationType.MoveX }),
            new("Move Y", Theme.Flash, () => Create(new StoryboardAnimation { Type = StoryboardAnimationType.MoveY }), x => x is StoryboardAnimation { Type: StoryboardAnimationType.MoveY }),
        };

        return entries;
    }
}