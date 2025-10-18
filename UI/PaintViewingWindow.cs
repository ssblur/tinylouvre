using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;
using TinyLife.Uis;

namespace TinyLouvre.UI;

public class PaintViewingWindow : CoveringGroup
{
    private readonly Texture2D _texture;
    private readonly Painting _painting;
    private readonly string _author;
    private readonly string _link;
    public PaintViewingWindow(string encodedPainting, string author, string link)
    {
        _painting = LouvreUtil.ImportPainting(encodedPainting);
        _texture = new Texture2D(GameImpl.Instance.GraphicsDevice, Painting.SIZE_X, Painting.SIZE_Y);
        _painting.SetCanvasTexture(_texture);

        _author = author;
        _link = link;
        
        var root = new Panel(Anchor.Center, new Vector2(120, 170));
        AddChild(root);

        var image = new ArtPanel(Anchor.TopCenter, new Vector2(100, 140), _texture);
        root.AddChild(image);

        var authorGroup = new Group(Anchor.BottomCenter, new Vector2(100, 20));
        root.AddChild(authorGroup);

        var authorLabel = new Paragraph(Anchor.CenterLeft, 120, _ => _author);
        authorGroup.AddChild(authorLabel);

        if (_link?.Length > 0)
        {
            var linkButton = new Button(Anchor.CenterRight, new Vector2(12, 12), _ => "O");
            linkButton.OnPressed += _ => MlemPlatform.Current.OpenLinkOrFile(_link);
            authorGroup.AddChild(linkButton);
        }
    }
}

internal class ArtPanel(Anchor anchor, Vector2 size, Texture2D texture) : Element(anchor, size)
{
    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        base.Draw(time, batch, alpha, context);
        batch.Draw(texture, DisplayArea, Color.White);
    }
}