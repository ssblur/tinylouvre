using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;

namespace TinyLouvre.UI;

public class PaintingArea : Panel
{
    private static byte[,] Paint;
    
    public PaintingArea(Anchor anchor, Vector2 size) : base(anchor, size, false, false)
    {
        Paint ??= new byte[10, 16];

        OnPressed += element =>
        {
            // this is the element's position 
            // element.DisplayArea.Location;
            // mouse position
            // element.Controls.Input.MousePosition;
        };
    }

    public override bool CanBePressed { get => true; }

    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        base.Draw(time, batch, alpha, context);
        
    }
}