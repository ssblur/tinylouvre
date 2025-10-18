using System;
using System.Collections.Generic;
using TinyLife;
using TinyLife.Actions;
using TinyLife.Utilities;
using TinyLouvre.Objects;
using TinyLouvre.UI;
using Action = TinyLife.Actions.Action;

namespace TinyLouvre.Actions;

public class ViewAction(ActionType type, ActionInfo info) : MultiAction(type, info)
{
    protected override IEnumerable<Action> CreateFirstActions()
    {
        yield return ActionType.GoHere.Construct<Action>(Info.ToFreeActionSpot() ?? Info);
    }

    protected override CompletionType AndThenIsCompleted()
    {
        return CompleteIfTimeUp(TimeSpan.FromMinutes(1));
    }
    
    protected override void AndThenInitialize()
    {
        base.AndThenInitialize();
        var painting = Info.GetActionObject<ArtPiece>();
        if (painting == null) return;
        
        var root = GameImpl.Instance.UiSystem.Add(
            "TinyLouvreViewWindows", 
            new PaintViewingWindow(painting.EncodedPainting ?? LouvreUtil.GarbageDataForFun(), painting.Author ?? "MISSINGNO", painting.Link ?? "")
        );
        root.SetPauseGame();
    }
}