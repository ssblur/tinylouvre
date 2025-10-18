using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using ExtremelySimpleLogger;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Data.Content;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using Newtonsoft.Json;
using TinyLife;
using TinyLife.Actions;
using TinyLife.Emotions;
using TinyLife.Mods;
using TinyLife.Objects;
using TinyLife.Tools;
using TinyLife.Uis;
using TinyLife.Utilities;
using TinyLife.World;
using TinyLouvre.Actions;
using TinyLouvre.Objects;
using TinyLouvre.UI;
using Action = TinyLife.Actions.Action;
using Group = MLEM.Ui.Elements.Group;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TinyLouvre;

// ReSharper disable once ClassNeverInstantiated.Global
public class TinyLouvre : Mod {

    // the logger that we can use to log info about this mod
    public static Logger Logger { get; private set; }
    public static LouvreOptions Options { get; private set; }
    public static TinyLouvre Instance;

    // visual data about this mod
    public override string Name => "Tiny Louvre";
    public override string Description => "Share your creations with everyone!";
    public override TextureRegion Icon => UiTextures[new Point(0, 0)];
    public override string IssueTrackerUrl => "https://github.com/ssblur/tinylouvre/issues";
    public override string WebsiteUrl => "https://ssblur.com/mods/tinylouvre.html";
    public override string TestedVersionRange => "[0.47.8,0.48.3]";

    public static Dictionary<Point, TextureRegion> UiTextures;

    public override void Initialize(Logger logger, RawContentManager content, RuntimeTexturePacker texturePacker, ModInfo info)
    {
        Instance = this;
        Logger = logger;
        Options = info.LoadOptions(() => new LouvreOptions());
        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("UiTextures"), 8, 8), r => UiTextures = r, 1, true, true);
        
        
        if (!OperatingSystem.IsLinux()) return;
        Logger.Warn("Cannot confirm if xsel is available, disabling Clipboard functionality.");
        // This was throwing an exception on another process, which was not being caught.
        // try
        // {
        //     var xsel = new ProcessStartInfo
        //     {
        //         WindowStyle = ProcessWindowStyle.Hidden,
        //         FileName = "xsel",
        //         RedirectStandardOutput = true,
        //         RedirectStandardError = true,
        //         
        //     };
        //     var process = Process.Start(xsel);
        //     process?.WaitForExit();
        // }
        // catch
        // {
        //     Logger.Warn("xsel does not appear to be available, disabling clipboard functionality.");
        //     throw;
        // }
    }


    public FurnitureType Easel;
    public FurnitureType ArtPiece;
    public ActionType Finish;
    public override void AddGameContent(GameImpl game, ModInfo info) {
        Easel = FurnitureType.Register(new FurnitureType.TypeSettings("TinyLouvre.Easel", new Point(1, 1), ObjectCategory.Table, 150, ColorScheme.White, ColorScheme.White  , ColorScheme.White) {
            ConstructedType = typeof(Easel),
            Icon = Icon,
            Tab = FurnitureTool.Tab.Other,
        });

        var artPieceCategory = ObjectCategory.WallHanging | ObjectCategory.NonBuyable;
        ArtPiece = FurnitureType.Register(
            new FurnitureType.TypeSettings(
                "TinyLouvre.Painting", 
                new Point(1, 1), 
                artPieceCategory, 
                -1, 
                ColorScheme.White
            ) {
            ConstructedType = typeof(ArtPiece),
            Icon = Icon,
        });
        
        ActionType.Register(new ActionType.TypeSettings("TinyLouvre.Paint", ObjectCategory.Table, typeof(PaintAction)) {
            CanExecute = (actionInfo, _) => actionInfo.GetActionObject<Easel>() != null ? CanExecuteResult.Valid : CanExecuteResult.Hidden,
            Ai = {
                CanDoRandomly = false,
                SolvedNeeds = [],
                PassivePriority = p => 0
            },
            Texture = UiTextures[new Point(1, 0)]
        });
        
        Finish = ActionType.Register(new ActionType.TypeSettings("TinyLouvre.Finish", ObjectCategory.Table, typeof(FinishAction)) {
            CanExecute = (_, _) => CanExecuteResult.Hidden,
            Ai = {
                CanDoRandomly = false,
                SolvedNeeds = [],
                PassivePriority = p => 0
            },
            Texture = UiTextures[new Point(1, 0)],
        });
        
        ActionType.Register(new ActionType.TypeSettings("TinyLouvre.View", artPieceCategory, typeof(ViewAction)) {
            CanExecute = (actionInfo, _) => actionInfo.GetActionObject<ArtPiece>() != null ? CanExecuteResult.Valid : CanExecuteResult.Hidden,
            Ai = {
                CanDoRandomly = false,
                SolvedNeeds = [],
                PassivePriority = p => 0
            },
            Texture = UiTextures[new Point(0, 0)]
        });
        
        OnlineMode.Update();
    }

    public override IEnumerable<string> GetCustomFurnitureTextures(ModInfo info) { yield return "CustomFurniture"; }

    public override void PopulateOptions(Group group, ModInfo info)
    {
        var oldOnlineMode = Options.OnlineMode;
        group.AddChild(new Paragraph(
            Anchor.AutoLeft, 
            100, 
            _ => $"{Localization.Get(LnCategory.Ui, "TinyLouvre.Options.PaintingCost")} ${Options.PaintingCost}"
        ));
        group.AddChild(new Slider(Anchor.AutoLeft, new Vector2(100, 8), 5, 1000) {
            CurrentValue = Options.PaintingCost,
            OnValueChanged = (_, v) => Options.PaintingCost = (int) v
        });

        group.AddChild(new VerticalSpace(2));
        
        group.AddChild(new Checkbox(
            Anchor.AutoLeft, 
            new Vector2(100, 8), 
            Localization.Get(LnCategory.Ui, "TinyLouvre.Options.OnlineMode"), 
            Options.OnlineMode
        )
        {
            OnCheckStateChange = (_, value) => Options.OnlineMode = value 
        });
        
        group.AddChild(new VerticalSpace(2));

        group.AddChild(new Paragraph(Anchor.AutoLeft, 100, _ => "Online Mode ATProto Feed Link:"));
        group.AddChild(new TextField(Anchor.AutoLeft, new Vector2(100, 12), null, null, Options.OnlineModeAtprotoFeed)
        {
            OnTextChange = (_, text) => Options.OnlineModeAtprotoFeed = text,
        });
        
        group.OnRemovedFromUi += _ =>
        {
            info.SaveOptions(Options);
            
            // If online mode was just enabled, fetch new paintings.
            if(Options.OnlineMode != oldOnlineMode) OnlineMode.Update();
        };
    }
}

public class LouvreOptions
{
    public bool OnlineMode = false;
    public bool ShowOnlineModePrompt = true; // TODO: show online prompt if true, then disable and save
    public int PaintingCost = 50;

    // An ATProto / BlueSky feed link. Will be access unauthenticated to search for valid paintings in posts.
    // Only does anything if Online Mode is enabled.
    public string OnlineModeAtprotoFeed =
        "https://public.api.bsky.app/xrpc/app.bsky.feed.getAuthorFeed?actor=did:plc:o3fftrm6ifwqrg2ecefine2h";
}

public record Painting(byte[,] Canvas, int[] Colors)
{
    public const int SIZE_X = 10;
    public const int SIZE_Y = 14;
    public string Export()
    {
        var array = new List<byte>();
        foreach (var c in Colors)
        {
            array.Add((byte) (c & 255));
            array.Add((byte) (c >> 8 & 255));
            array.Add((byte) (c >> 16 & 255));
        }

        foreach (var b in LinearCanvas().Chunk(4))
        {
            byte c = 0;
            for (var i = 0; i < b.Length; i++)
            {
                c |= (byte) (b[i] << (i * 2));
            }
            array.Add(c);
        }
        return Convert.ToBase64String(array.ToArray());
    }
    
    public Texture2D[] FurnitureTextures()
    {
        var device = GameImpl.Instance.GraphicsDevice;
        var canvasColors = new[] {
            Color.FromNonPremultiplied(LouvreUtil.IntToVector4(Colors[0])),
            Color.FromNonPremultiplied(LouvreUtil.IntToVector4(Colors[1])),
            Color.FromNonPremultiplied(LouvreUtil.IntToVector4(Colors[2])),
            Color.FromNonPremultiplied(LouvreUtil.IntToVector4(Colors[3]))
        };

        var canvas = LinearCanvas().Select(b => canvasColors[b]).ToArray();
        
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
    
    public void SetCanvasTexture(Texture2D canvas)
    {
        var canvasColors = new[] {
            Color.FromNonPremultiplied(LouvreUtil.IntToVector4(Colors[0])),
            Color.FromNonPremultiplied(LouvreUtil.IntToVector4(Colors[1])),
            Color.FromNonPremultiplied(LouvreUtil.IntToVector4(Colors[2])),
            Color.FromNonPremultiplied(LouvreUtil.IntToVector4(Colors[3]))
        };

        var r = LinearCanvas().Select(b => canvasColors[b]).ToArray();
        canvas.SetData(r);
    }

    private byte[] LinearCanvas()
    {
        var r = new byte[SIZE_X * SIZE_Y];
        for (var y = 0; y < SIZE_Y; y++)
        {
            var o = y * SIZE_X;
            for (var x = 0; x < SIZE_X; x++)
            {
                r[o + x] = Canvas[x, y];
            }
        }

        return r;
    }
}

public class LouvreUtil
{
    public static bool XSelAvailable = false;
    
    public static Painting ImportPainting(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        var canvas = new byte[10,14];
        var colors = new int[4];

        for (var i = 0; i < 4; i++)
        {
            var o = i * 3;
            colors[i] = bytes[o] + (bytes[o + 1] << 8) + (bytes[o + 2] << 16);
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

    public static Vector4 IntToVector4(int c)
    {
        return new Vector4((c >> 16 & 255) / 255f, (c >> 8 & 255) / 255f, (c & 255) / 255f, 1f);
    }

    public static int ColorToInt(Color c)
    {
        return c.B + (c.G << 8) + (c.R << 16);
    }

    public static string GarbageDataForFun()
    {
        var random = new Random();
        var bytes = new byte[47];
        random.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}

public record PaintingWithMetadata(Painting painting, string author, string handle, string link);

public class OnlineMode
{
    public static PaintingWithMetadata[] RecentPaintings = [];

    record Record(string createdAt, string text);
    record Author(string handle, string displayName);
    record Post(Author author, Record record, string uri);

    record FeedItem(Post post);
    record Feed(FeedItem[] feed);

    public static async void Update()
    {
        if (!TinyLouvre.Options.OnlineMode)
        {
            TinyLouvre.Logger.Info("Online Mode disabled, not fetching new paintings.");
            return;
        }

        var client = new HttpClient(new HttpClientHandler
            { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
        var request = await client.GetAsync(TinyLouvre.Options.OnlineModeAtprotoFeed);
        if (!request.IsSuccessStatusCode)
        {
            TinyLouvre.Logger.Warn("There was an error while fetching new paintings:");
            TinyLouvre.Logger.Warn($"{request.StatusCode} {request.ReasonPhrase}");
            return;
        }

        var feed = JsonSerializer.Deserialize<Feed>(await request.Content.ReadAsStringAsync());
        if(feed == null)
        {
            TinyLouvre.Logger.Warn("There was an error while fetching new paintings:");
            TinyLouvre.Logger.Warn("Feed is invalid!");
            return;
        }

        var recentPaintings = new List<PaintingWithMetadata>();
        var pattern = new Regex(@"tlv\.(.*?)\.");
        foreach (var item in feed.feed)
        {
            try
            {
                var post = item.post.record.text;
                var groups = pattern.Match(post).Groups;
                if (groups.Count < 2) continue;
                var painting = LouvreUtil.ImportPainting(groups[1].Value);

                var authorName = item.post.author.displayName;
                var authorHandle = item.post.author.handle;
                var uri = item.post.uri;

                recentPaintings.Add(new PaintingWithMetadata(painting, authorName, authorHandle, uri));
            }
            catch (Exception e)
            {
                TinyLouvre.Logger.Warn("There was an error processing a post.");
                TinyLouvre.Logger.Warn(e);
            }
        }

        RecentPaintings = recentPaintings.ToArray();
    }
}