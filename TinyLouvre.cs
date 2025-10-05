using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ExtremelySimpleLogger;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Data.Content;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;
using TinyLife.Actions;
using TinyLife.Emotions;
using TinyLife.Mods;
using TinyLife.Objects;
using TinyLife.Utilities;
using TinyLife.World;
using TinyLouvre.Actions;
using TinyLouvre.Objects;
using Action = TinyLife.Actions.Action;

namespace TinyLouvre;

public class TinyLouvre : Mod {

    // the logger that we can use to log info about this mod
    public static Logger Logger { get; private set; }
    public static LouvreOptions Options { get; private set; }

    // visual data about this mod
    public override string Name => "Tiny Louvre";
    public override string Description => "Share your creations with everyone!";
    public override TextureRegion Icon => uiTextures[new Point(0, 0)];
    public override string IssueTrackerUrl => "https://github.com/ssblur/tinylouvre/issues";
    public override string WebsiteUrl => "https://ssblur.com/mods/tinylouvre.html";
    public override string TestedVersionRange => "[0.47.8,0.47.11]";

    private Dictionary<Point, TextureRegion> uiTextures;

    public override void Initialize(Logger logger, RawContentManager content, RuntimeTexturePacker texturePacker, ModInfo info) {
        Logger = logger;
        Options = info.LoadOptions(() => new LouvreOptions());
        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("UiTextures"), 8, 8), r => uiTextures = r, 1, true, true);
    }

    public override void AddGameContent(GameImpl game, ModInfo info) {
        FurnitureType.Register(new FurnitureType.TypeSettings("TinyLouvre.Easel", new Point(1, 1), ObjectCategory.Table, 150, ColorScheme.SimpleWood) {
            ConstructedType = typeof(Easel),
            Icon = Icon,
            ObjectSpots = ObjectSpot.TableSpots(new Point(1, 1)).ToArray()
        });
        ActionType.Register(new ActionType.TypeSettings("TinyLouvre.Paint", ObjectCategory.Table, typeof(PaintAction)) {
            CanExecute = (actionInfo, _) => actionInfo.GetActionObject<Easel>() != null ? CanExecuteResult.Valid : CanExecuteResult.Hidden,
            Ai = {
                CanDoRandomly = false,
                SolvedNeeds = [NeedType.Entertainment],
                PassivePriority = p => 0
            },
            Texture = uiTextures[new Point(1, 0)]
        });
    }

    public override IEnumerable<string> GetCustomFurnitureTextures(ModInfo info) { yield return "CustomFurniture"; }

    public override void PopulateOptions(Group group, ModInfo info) {
        // group.AddChild(new Paragraph(Anchor.AutoLeft, 1, _ => $"{Localization.Get(LnCategory.Ui, "ExampleMod.DarkShirtSpeedOption")}: {TinyLouvreMod.Options.DarkShirtSpeedIncrease}"));
        group.OnRemovedFromUi += _ => info.SaveOptions(Options);
    }
}

public class LouvreOptions {

}

public record Painting(byte[,] Canvas, int[] Colors)
{
    public string ExportPainting()
    {
        var array = new List<byte>();
        foreach (var c in Colors)
        {
            array.Add((byte) (c & 255));
            array.Add((byte) (c >> 8 & 255));
            array.Add((byte) (c >> 16 & 255));
        }

        foreach (var b in Canvas.Cast<byte>().Chunk(4))
        {
            byte c = 0;
            for (var i = 0; i < b.Length; i++)
            {
                c &= (byte) (b[i] << (i * 2));
            }
            array.Add(c);
        }
        return Convert.ToBase64String(array.ToArray());
    }
}

public class LouvreUtil
{
    public static Painting ImportPainting(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        var canvas = new byte[10,14];
        var colors = new int[4];

        for (var i = 0; i < 4; i++)
        {
            var o = i * 3;
            colors[i] = bytes[o] + (bytes[o + 1] << 8) + (bytes[o + 1] << 16);
        }

        for (var i = 12; i < bytes.Length; i++)
        {
            var o = (i - 12) * 4;
            for (var j = 0; j < 4; j++)
            {
                var x = (o + j) % 10;
                var y = (int) Math.Floor((o + j) / 10.0);
                canvas[x, y] = (byte) ((bytes[i] >> (j * 2)) % 4);
            }
        }
        
        return new Painting(canvas, colors);
    }

    public static string ExportPainting(Painting painting)
    {
        return painting.ExportPainting();
    }

    public static void SetCanvasTexture(Painting painting, Texture2D canvas)
    {
        var c = painting.Colors[0];
        var canvasColors = new Color[4];
        canvasColors[0] = Color.FromNonPremultiplied(c & 255, c >> 8 & 255, c >> 16 & 255, 255);
        c = painting.Colors[0];
        canvasColors[1] = Color.FromNonPremultiplied(c & 255, c >> 8 & 255, c >> 16 & 255, 255);
        c = painting.Colors[0];
        canvasColors[2] = Color.FromNonPremultiplied(c & 255, c >> 8 & 255, c >> 16 & 255, 255);
        c = painting.Colors[0];
        canvasColors[3] = Color.FromNonPremultiplied(c & 255, c >> 8 & 255, c >> 16 & 255, 255);
        
        canvas.SetData((from b in painting.Canvas.Cast<byte>() select canvasColors[b]).ToArray());
    }

    public static Texture2D[] FurnitureTextures(Painting painting, GraphicsDevice device)
    {
        var c = painting.Colors[0];
        var canvasColors = new Color[4];
        canvasColors[0] = Color.FromNonPremultiplied(c & 255, c >> 8 & 255, c >> 16 & 255, 255);
        c = painting.Colors[0];
        canvasColors[1] = Color.FromNonPremultiplied(c & 255, c >> 8 & 255, c >> 16 & 255, 255);
        c = painting.Colors[0];
        canvasColors[2] = Color.FromNonPremultiplied(c & 255, c >> 8 & 255, c >> 16 & 255, 255);
        c = painting.Colors[0];
        canvasColors[3] = Color.FromNonPremultiplied(c & 255, c >> 8 & 255, c >> 16 & 255, 255);

        var output = (from b in painting.Canvas.Cast<byte>() select canvasColors[b]).ToArray();
        
        var down = new Texture2D(device, 16, 16);
        var left = new Texture2D(device, 16, 16);
        var right = new Texture2D(device, 16, 16);
        
        // TODO: place canvas on textures
        
        return [
            down,
            left,
            right
        ];
    }
}