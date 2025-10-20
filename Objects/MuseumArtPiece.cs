using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
using MLEM.Textures;
using TinyLife;
using TinyLife.Objects;
using TinyLife.Skills;
using TinyLife.World;

namespace TinyLouvre.Objects;

public class MuseumArtPiece(Guid id, FurnitureType type, int[] colors, Map map, Vector2 pos, float floor)
    : Furniture(id, type, colors, map, pos, floor)
{
    [DataMember]
    public int MuseumIndex;
    
    private Painting _painting;
    private Texture2D[] _textures;
    private TextureRegion[] _textureRegions;

    public void SetMuseumIndex(int index)
    {
        MuseumIndex = index;
    }

    public override void Draw(GameTime time, object batch, Vector2 pos, float floor, Vector2 drawPos, Color? overrideColor,
        Direction2 rotation, int[] colors, float drawScale, bool pivot, ParentInfo parent, float depthOffset, List<StaticSpriteBatch.Item> items)
    {
        base.Draw(time, batch, pos, floor, drawPos, overrideColor, rotation, colors, drawScale, pivot, parent, depthOffset, items);

        if (MuseumIndex >= Museum.Paintings.Length || MuseumIndex < 0) return;
        _painting ??= Museum.Paintings[MuseumIndex]?.Painting;
        if (_painting == null) return;
        _textures ??= _painting.FurnitureTextures();
        _textureRegions ??= _textures.Select(t => new TextureRegion(t)).ToArray();
        
        var tex = _textureRegions[0];
        var ox = -3;
        const int oy = 29;
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (rotation)
        {
            case Direction2.Down: 
                break;
            case Direction2.Right:
                ox = 13;
                tex = _textureRegions[1]; 
                break;
            default: return;
        }

        DrawColumns(
            Map,
            pos,
            floor,
            batch,
            new Vector2(drawPos.X - ox, drawPos.Y - oy),
            tex,
            Color.White,
            new Vector2(drawScale),
            TinyLouvre.Instance.MuseumArtPiece.GetSize(rotation),
            items,
            false,
            parent,
            depthOffset + 0.01f,
            SpriteEffects.None
        );
    }
}