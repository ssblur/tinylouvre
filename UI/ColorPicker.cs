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
    public ColorButton(Anchor anchor, Vector2 size) : base(anchor, size)
    {
    }

    public ColorButton(Anchor anchor, Vector2 size, string text = null, string tooltipText = null) : base(anchor, size, text, tooltipText)
    {
    }

    public ColorButton(Anchor anchor, Vector2 size, Paragraph.TextCallback textCallback = null, Paragraph.TextCallback tooltipTextCallback = null) : base(anchor, size, textCallback, tooltipTextCallback)
    {
    }
}