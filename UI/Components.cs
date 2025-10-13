using System;
using Microsoft.Xna.Framework;
using MLEM.Maths;
using MLEM.Misc;
using MLEM.Startup;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TextCopy;
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

class CopyButton : Button
{
    public CopyButton(Anchor anchor, Vector2 size, Paragraph.TextCallback textCallback = null, Paragraph.TextCallback tooltipTextCallback = null) : base(anchor, size, textCallback, tooltipTextCallback)
    {
        OnPressed += element =>
        {
            try
            {
                ClipboardService.SetTextAsync(PaintingArea.GetExport());
            }
            catch (Exception e)
            {
                TinyLouvre.Logger.Warn("Error copying to clipboard!");
                TinyLouvre.Logger.Warn(e);
                throw;
            }
        };
    }
    
}

class ClearButton : Button
{
    public ClearButton(Anchor anchor, Vector2 size, Paragraph.TextCallback textCallback = null, Paragraph.TextCallback tooltipTextCallback = null) : base(anchor, size, textCallback, tooltipTextCallback)
    {
        var image = new Image(Anchor.Center, new Vector2(14, 14), TinyLouvre.UiTextures[new Point(3, 0)], true) {
            Padding = new Padding(3, 3)
        };
        AddChild(image);
        
        OnPressed += element =>
        {
            PaintingArea.ClearCanvas();
        };
    }
    
}