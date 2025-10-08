using Microsoft.Xna.Framework;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;

namespace TinyLouvre.UI.Components;

class SaveButton : Button
{
    public SaveButton(Anchor anchor, Vector2 size) : base(anchor, size)
    {
    }

    public SaveButton(Anchor anchor, Vector2 size, string text = null, string tooltipText = null) : base(anchor, size, text, tooltipText)
    {
    }

    public SaveButton(Anchor anchor, Vector2 size, Paragraph.TextCallback textCallback = null, Paragraph.TextCallback tooltipTextCallback = null) : base(anchor, size, textCallback, tooltipTextCallback)
    {
    }
}

class ShareButton : Button
{
    public ShareButton(Anchor anchor, Vector2 size, Paragraph.TextCallback textCallback = null, Paragraph.TextCallback tooltipTextCallback = null) : base(anchor, size, textCallback, tooltipTextCallback)
    {
        OnPressed += element =>
        {
            MlemPlatform.Current.OpenLinkOrFile(PaintingArea.GetLink());
        };
    }
    
}