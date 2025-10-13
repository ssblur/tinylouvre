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
using TinyLife.Uis;
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
    
    public PaintWindow()
    {
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
        
        var shareGroup = new Group(Anchor.BottomRight, Vector2.One, true, true);
        Root.AddChild(shareGroup);

        if (!(OperatingSystem.IsLinux() && !LouvreUtil.XSelAvailable))
        {
            Copy = new CopyButton(Anchor.AutoLeft, new Vector2(32, 12), _ => "Copy");
            shareGroup.AddChild(Copy);
        }

        Share = new ShareButton(Anchor.AutoLeft, new Vector2(32, 12), _ => "Share");
        shareGroup.AddChild(Share);
        
        Save = new SaveButton(Anchor.AutoLeft, new Vector2(32, 12), _ => "Finish");
        shareGroup.AddChild(Save);
    }
    
    
    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        base.Draw(time, batch, alpha, context);
    }
}