using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Ui.Elements;
using TinyLife.Uis;
using TinyLouvre.UI.Components;

namespace TinyLouvre.UI;

public class PaintWindow : CoveringGroup
{
    private Panel Root;
    private ColorButton[] Buttons;
    private PaintingArea Area;
    private ShareButton Share;
    private SaveButton Save;
    
    public PaintWindow()
    {
        Root = new Panel(MLEM.Ui.Anchor.Center, new Vector2(200, 150));
        AddChild(Root);

        var buttonGroup = new Group(MLEM.Ui.Anchor.CenterLeft, Vector2.One, true, true);
        Root.AddChild(buttonGroup);
        
        Buttons = new ColorButton[4];
        for (var i = 0; i < Buttons.Length; i++)
        {
            Buttons[i] = new ColorButton(MLEM.Ui.Anchor.AutoLeft, new Vector2(8, 8));
            buttonGroup.AddChild(Buttons[i], i);
        }

        Area = new PaintingArea(MLEM.Ui.Anchor.Center, new Vector2(80, 128));
        Root.AddChild(Area);
        
        var shareGroup = new Group(MLEM.Ui.Anchor.BottomRight, Vector2.One, true, true);
        Root.AddChild(shareGroup);

        Share = new ShareButton(MLEM.Ui.Anchor.AutoLeft, new Vector2(32, 12), _ => "Share");
        shareGroup.AddChild(Share);
        
        Save = new SaveButton(MLEM.Ui.Anchor.AutoLeft, new Vector2(32, 12), _ => "Save");
        shareGroup.AddChild(Save);
    }
    
    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        base.Draw(time, batch, alpha, context);
    }
}