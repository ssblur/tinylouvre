using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Graphics;
using MLEM.Maths;
using TinyLife.Objects;
using TinyLife.World;

namespace TinyLouvre.Objects;

public class Easel : Furniture
{
    public Easel(Guid id, FurnitureType type, int[] colors, Map map, Vector2 pos, float floor) : base(id, type, colors, map, pos, floor)
    {
    }

    public override void Draw(GameTime time, object batch, Vector2 pos, float floor, Vector2 drawPos, Color? overrideColor,
        Direction2 rotation, int[] colors, float drawScale, bool pivot, ParentInfo parent, float depthOffset, List<StaticSpriteBatch.Item> items)
    {
        base.Draw(time, batch, pos, floor, drawPos, overrideColor, rotation, colors, drawScale, pivot, parent, depthOffset, items);
    }
}