using System;
using Microsoft.Xna.Framework;
using TinyLife.Objects;
using TinyLife.World;

namespace TinyLouvre.Objects;

public class Easel : Furniture
{
    public Easel(Guid id, FurnitureType type, int[] colors, Map map, Vector2 pos, float floor) : base(id, type, colors, map, pos, floor)
    {
    }
}