using System;
using Microsoft.Xna.Framework;
using MLEM.Maths;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TextCopy;
using TinyLife;

namespace TinyLouvre.UI;

class SaveButton(
    Anchor anchor,
    Vector2 size,
    Paragraph.TextCallback textCallback = null,
    Paragraph.TextCallback tooltipTextCallback = null)
    : Button(anchor, size, textCallback, tooltipTextCallback);

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
        // ReSharper disable once VirtualMemberCallInConstructor
        AddChild(image);
        
        OnPressed += element =>
        {
            PaintingArea.ClearCanvas();
        };
    }
    
}

class BuyButton(Anchor anchor, Vector2 size) : Button(anchor, size,
    _ => Localization.Get(LnCategory.Ui, "TinyLouvre.Buy", TinyLouvre.Options.PaintingCost));
    
class WhiteButton: Button
{
    public WhiteButton(Anchor anchor, Vector2 size) : base(anchor, size)
    {
        var region = TinyLouvre.UiTextures[new Point(0, 1)];
        Texture = new NinePatch(region, MLEM.Maths.Padding.Empty);
    }
}

class WhitePanel : Panel
{
    public WhitePanel(Anchor anchor, Vector2 size, bool setHeightBasedOnChildren = false, bool scrollOverflow = false, bool autoHideScrollbar = true) : base(anchor, size, setHeightBasedOnChildren, scrollOverflow, autoHideScrollbar)
    {
        var region = TinyLouvre.UiTextures[new Point(0, 1)];
        Texture = new NinePatch(region, MLEM.Maths.Padding.Empty);
    }
}