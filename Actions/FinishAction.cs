using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TinyLife;
using TinyLife.Actions;
using TinyLife.Objects;
using TinyLife.Uis;
using TinyLife.Utilities;
using TinyLouvre.Objects;
using TinyLouvre.UI;
using Action = TinyLife.Actions.Action;

namespace TinyLouvre.Actions;

public class FinishAction(ActionType type, ActionInfo info) : MultiAction(type, info)
{
    protected override IEnumerable<Action> CreateFirstActions()
    {
        yield return ActionType.GoHere.Construct<Action>(Info.ToFreeActionSpot() ?? Info);
    }

    protected override void AndThenUpdate(GameTime time, TimeSpan passedInGame, float speedMultiplier)
    {
        base.AndThenUpdate(time, passedInGame, speedMultiplier);
        
        Person.CurrentPose = Pose.WorkingStanding;
    }

    protected override void AndThenOnCompleted(CompletionType type)
    {
        base.AndThenOnCompleted(type);
        var artPiece = new ArtPiece(Guid.NewGuid(), TinyLouvre.Instance.ArtPiece, [0], Person.Map, Vector2.One, 0);
        artPiece.SetPaintingData(PaintingArea.GetExport(), Person.FullName, "");
        PaintingArea.ClearCanvas();
            
        Person.Household.AddToFurnitureStorage(artPiece);
        Notifications.Add(Notifications.MailIcon, Localization.Get(LnCategory.Ui, "TinyLouvre.Finished"));
    }

    protected override CompletionType AndThenIsCompleted()
    {
        return CompleteIfTimeUp(TimeSpan.FromMinutes(15));
    }

    protected override void AndThenInitialize()
    {
        base.AndThenInitialize();
        Person.Money -= TinyLouvre.Options.PaintingCost;
    }
}