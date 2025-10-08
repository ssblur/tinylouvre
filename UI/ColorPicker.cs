using Microsoft.Xna.Framework;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace TinyLouvre.UI;

public class ColorPicker : Group
{
    public ColorPicker(Anchor anchor, Vector2 size, bool setHeightBasedOnChildren = true) : base(anchor, size, setHeightBasedOnChildren)
    {
    }

    public ColorPicker(Anchor anchor, Vector2 size, bool setWidthBasedOnChildren, bool setHeightBasedOnChildren) : base(anchor, size, setWidthBasedOnChildren, setHeightBasedOnChildren)
    {
    }
}

class ColorButton : Button
{
    public ColorButton(Anchor anchor, Vector2 size, byte color) : base(anchor, size)
    {
        OnPressed += element =>
        {
            PaintingArea.SetColor(color);
        };
    }
}