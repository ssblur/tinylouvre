using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
using TinyLife.Objects;
using TinyLife.World;

namespace TinyLouvre.Objects;

public class ArtPiece(Guid id, FurnitureType type, int[] colors, Map map, Vector2 pos, float floor)
    : Furniture(id, type, colors, map, pos, floor)
{
    [DataMember]
    public string EncodedPainting;
    [DataMember]
    public string Author;
    [DataMember]
    public string Link;
    
    private Painting _painting;
    private Texture2D[] _textures;

    public void SetPaintingData(string painting, string author, string link)
    {
        EncodedPainting = painting;
        Author = author;
        Link = link;
    }

    public override void Draw(GameTime time, object batch, Vector2 pos, float floor, Vector2 drawPos, Color? overrideColor,
        Direction2 rotation, int[] colors, float drawScale, bool pivot, ParentInfo parent, float depthOffset, List<StaticSpriteBatch.Item> items)
    {
        base.Draw(time, batch, pos, floor, drawPos, overrideColor, rotation, colors, drawScale, pivot, parent, depthOffset, items);

        if (EncodedPainting == null) return;
        _painting ??= LouvreUtil.ImportPainting(EncodedPainting);
        _textures ??= _painting.FurnitureTextures();

        var tex = _textures[0];
        switch (rotation)
        {
            case Direction2.Up: return;
            case Direction2.Left: tex = _textures[1]; break;
            case Direction2.Right: tex = _textures[2]; break;
            default: return;
        }
        // TODO draw texture
    }
}