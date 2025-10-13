using System;
using System.Collections.Generic;
using TinyLife;
using TinyLife.Actions;
using TinyLife.Utilities;
using TinyLouvre.UI;
using Action = TinyLife.Actions.Action;

namespace TinyLouvre.Actions;

public class FinishAction(ActionType type, ActionInfo info) : MultiAction(type, info)
{
    protected override IEnumerable<Action> CreateFirstActions()
    {
        yield return ActionType.GoHere.Construct<Action>(Info.ToFreeActionSpot() ?? Info);
    }

    protected override CompletionType AndThenIsCompleted()
    {
        return CompleteIfTimeUp(TimeSpan.FromMinutes(15));
    }

    protected override void AndThenInitialize()
    {
        base.AndThenInitialize();
        Person.Money -= 50;
    }
}