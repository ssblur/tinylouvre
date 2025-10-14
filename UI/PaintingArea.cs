using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;

namespace TinyLouvre.UI;

public class PaintingArea : Element
{
    private static Painting _painting;
    private static Texture2D _canvasTexture;
    private static byte _activeColor = 1;
    public static int[] Colors = [0xffffff, 0x000000, 0x992600, 0x222a99];
    
    public PaintingArea(Anchor anchor, Vector2 size) : base(anchor, size)
    {
        
        // _painting ??= LouvreUtil.ImportPainting("////AAAAAAAA/wAAAAAAAACAIAAAAAgAAqoKAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
        // Colors = _painting.Colors;
        _painting ??= new Painting(new byte[Painting.SIZE_X, Painting.SIZE_Y], Colors);

        _canvasTexture ??= new Texture2D(GameImpl.Instance.GraphicsDevice, Painting.SIZE_X, Painting.SIZE_Y);
        _painting.SetCanvasTexture(_canvasTexture);

        OnPressed += element =>
        {
            var diff = element.Controls.Input.MousePosition.ToVector2() - element.DisplayArea.Location;
            var scale = diff / element.DisplayArea.Size;
            var pos = scale * new Vector2(Painting.SIZE_X, Painting.SIZE_Y);
            pos.Floor();
            _painting.Canvas[(int) pos.X, (int) pos.Y] = _activeColor;
            UpdateCanvas();
        };
    }

    public override bool CanBePressed => true;

    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        base.Draw(time, batch, alpha, context);
        batch.Draw(_canvasTexture, DisplayArea, Color.White);
    }

    public static void SetColor(byte color)
    {
        _activeColor = color;
    }

    public static void UpdateCanvas()
    {
        _painting.SetCanvasTexture(_canvasTexture);
    }

    public static void ClearCanvas()
    {
        for(var x = 0; x < Painting.SIZE_X; x++) 
            for(var y = 0; y < Painting.SIZE_Y; y++)
                _painting.Canvas[x, y] = 0;
        _painting.SetCanvasTexture(_canvasTexture);
    }

    private const string Message = "I made a masterpiece in #TinyLouvre! ";
    private const string Link = "https://bsky.app/intent/compose?text=";

    public static string GetLink()
    {
        return $"{Link}{Uri.EscapeDataString(Message)}tlv.{_painting.Export()}.";
    }

    public static string GetExport()
    {
        return _painting.Export();
    }
}