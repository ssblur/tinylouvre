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
using TinyLouvre.UI.Components;

namespace TinyLouvre.UI;

public class PaintWindow : CoveringGroup
{
    private Panel Root;
    private ColorPicker ColorPicker;
    private PaintingArea Area;
    private ShareButton Share;
    private CopyButton Copy;
    private SaveButton Save;
    private ClearButton ClearButton;
    
    public PaintWindow(PersonLike person, ActionInfo info)
    {
        var window = this;
        
        Root = new Panel(Anchor.Center, new Vector2(200, 150));
        AddChild(Root);

        ColorPicker = new ColorPicker(Anchor.TopLeft, Vector2.One, true, true);
        Root.AddChild(ColorPicker);

        Area = new PaintingArea(Anchor.Center, new Vector2(80, 128));
        Root.AddChild(Area);
        
        var toolGroup = new Group(Anchor.BottomLeft, Vector2.One, true, true);
        Root.AddChild(toolGroup);

        ClearButton = new ClearButton(Anchor.AutoLeft, new Vector2(12, 12), _ => "");
        toolGroup.AddChild(ClearButton);

        var buttonWidth = 40;
        var shareGroup = new Group(Anchor.BottomRight, Vector2.One, true, true);
        Root.AddChild(shareGroup);

        if (!(OperatingSystem.IsLinux() && !LouvreUtil.XSelAvailable))
        {
            Copy = new CopyButton(Anchor.AutoLeft, new Vector2(buttonWidth, 12), _ => "Copy");
            shareGroup.AddChild(Copy);
        }

        var test = new Button(Anchor.AutoLeft, new Vector2(buttonWidth, 12), _ => "Test");
        test.OnPressed += _ =>
        {
            var root = GameImpl.Instance.UiSystem.Add(
                "TinyLouvreViewWindows",
                new PaintViewingWindow(PaintingArea.GetExport(), "By you", "https://blur.gay")
            );
            root.SetPauseGame();
        };
        shareGroup.AddChild(test);

        Share = new ShareButton(Anchor.AutoLeft, new Vector2(buttonWidth, 12), _ => "Share");
        shareGroup.AddChild(Share);
        
        Save = new SaveButton(
            Anchor.AutoLeft, 
            new Vector2(buttonWidth, 12), 
            _ => Localization.Get(LnCategory.Ui, "TinyLouvre.Finish", TinyLouvre.Options.PaintingCost)
        );
        Save.OnPressed += _ =>
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
        shareGroup.AddChild(Save);
    }
    
    
    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        base.Draw(time, batch, alpha, context);
    }
}