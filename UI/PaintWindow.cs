using System;
using System.Data;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;
using TinyLife.Actions;
using TinyLife.Objects;
using TinyLife.Uis;
using TinyLife.Utilities;
using TinyLouvre.Actions;

namespace TinyLouvre.UI;

public class PaintWindow : CoveringGroup
{
    public PaintWindow(PersonLike person, ActionInfo info)
    {
        var window = this;
        
        var root = new Panel(Anchor.Center, new Vector2(200, 150));
        // ReSharper disable once VirtualMemberCallInConstructor
        AddChild(root);

        var colorPicker = new ColorPicker(Anchor.TopLeft, Vector2.One, true, true);
        root.AddChild(colorPicker);

        var area1 = new PaintingArea(Anchor.Center, new Vector2(80, 128));
        root.AddChild(area1);
        
        var toolGroup = new Group(Anchor.BottomLeft, Vector2.One, true, true);
        root.AddChild(toolGroup);

        var clearButton = new ClearButton(Anchor.AutoLeft, new Vector2(12, 12), _ => "");
        toolGroup.AddChild(clearButton);

        var buttonWidth = 40;
        var shareGroup = new Group(Anchor.BottomRight, Vector2.One, true, true);
        root.AddChild(shareGroup);

        if (!(OperatingSystem.IsLinux() && !LouvreUtil.XSelAvailable))
        {
            var copy = new CopyButton(Anchor.AutoLeft, new Vector2(buttonWidth, 12), _ => "Copy");
            shareGroup.AddChild(copy);
        }

        // var test = new Button(Anchor.AutoLeft, new Vector2(buttonWidth, 12), _ => "Test");
        // test.OnPressed += _ =>
        // {
        //     var root = GameImpl.Instance.UiSystem.Add(
        //         "TinyLouvreViewWindows",
        //         new PaintViewingWindow(PaintingArea.GetExport(), "By you", "https://blur.gay")
        //     );
        //     root.SetPauseGame();
        // };
        // shareGroup.AddChild(test);

        var share = new ShareButton(Anchor.AutoLeft, new Vector2(buttonWidth, 12), _ => "Share");
        shareGroup.AddChild(share);
        
        var save = new SaveButton(
            Anchor.AutoLeft, 
            new Vector2(buttonWidth, 12), 
            _ => Localization.Get(LnCategory.Ui, "TinyLouvre.Finish", TinyLouvre.Options.PaintingCost)
        );
        save.OnPressed += _ =>
        {
            person.EnqueueAction<FinishAction>(
                TinyLouvre.Instance.Finish,
                ActionInfo.FromActionInfo(person, info), 
                false,
                true,
                true
            );
            window.Close();
        };
        shareGroup.AddChild(save);
    }
}