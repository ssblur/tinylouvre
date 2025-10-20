using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using TinyLife;
using TinyLife.Utilities;

namespace TinyLouvre.UI;

public class ColorPicker : Group
{
    public ColorPicker(Anchor anchor, Vector2 size, bool setWidthBasedOnChildren, bool setHeightBasedOnChildren) : base(anchor, size, setWidthBasedOnChildren, setHeightBasedOnChildren)
    {
        for (var i = 0; i < 4; i++)
        {
            var group = new Group(Anchor.AutoLeft, new Vector2(26, 12), false);
            group.AddChild(new ColorButton(Anchor.CenterLeft, new Vector2(12, 12), (byte) i));
            group.AddChild(new EditColorButton(Anchor.CenterRight, new Vector2(12, 12), (byte) i));
            AddChild(group);
        }
    }
}

internal class ColorButton : WhiteButton
{
    private readonly byte _color;
    public ColorButton(Anchor anchor, Vector2 size, byte color) : base(anchor, size)
    {
        _color = color;
        OnPressed += _ =>
        {
            PaintingArea.SetColor(color);
        };
    }

    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        NormalColor = Color.FromNonPremultiplied(LouvreUtil.IntToVector4(PaintingArea.Colors[_color]));
        base.Draw(time, batch, alpha, context);
    }
}

internal class EditColorButton : Button
{
    public EditColorButton(Anchor anchor, Vector2 size, byte color) : base(anchor, size)
    {
        var image = new Image(Anchor.Center, new Vector2(14, 14), TinyLouvre.UiTextures[new Point(2, 0)], true) {
            Padding = new Padding(3, 3)
        };
        AddChild(image);
        
        OnPressed += _ =>
        {
            // open color picker set to this color
            var root = GameImpl.Instance.UiSystem.Add("TinyLouvreColorPickerWindow", new ColorPickerWindow(color));
            root.SetPauseGame();
        };
    }
}