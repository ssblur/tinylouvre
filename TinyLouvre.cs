using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
using TinyLife.Mods;
using TinyLife.Objects;
using TinyLife.Tools;
using TinyLife.Utilities;
using TinyLife.World;
using TinyLouvre.Actions;
using TinyLouvre.Objects;
using TinyLouvre.UI;
using Group = MLEM.Ui.Elements.Group;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TinyLouvre;

// ReSharper disable once ClassNeverInstantiated.Global
public class TinyLouvre : Mod {

    // the logger that we can use to log info about this mod
    public static Logger Logger { get; private set; }
    public static LouvreOptions Options { get; private set; }
    public static TinyLouvre Instance;
    public static ModInfo Info;

    // visual data about this mod
    public override string Name => "Tiny Louvre";
    public override string Description => "Share your creations with everyone!";
    public override TextureRegion Icon => UiTextures[new Point(0, 0)];
    public override string IssueTrackerUrl => "https://github.com/ssblur/tinylouvre/issues";
    public override string WebsiteUrl => "https://ssblur.com/mods/tinylouvre.html";
    public override string TestedVersionRange => "[0.47.8,0.48.3]";

    public static Dictionary<Point, TextureRegion> UiTextures;
    
    public Texture2D MapIcon;

    public override void Initialize(Logger logger, RawContentManager content, RuntimeTexturePacker texturePacker, ModInfo info)
    {
        Instance = this;
        Logger = logger;
        Info = info;
        Options = info.LoadOptions(() => new LouvreOptions());
        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("UiTextures"), 8, 8), r => UiTextures = r, 1, true, true);
        MapIcon = content.Load<Texture2D>("MapIcon");
        
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
    public FurnitureType MuseumArtPiece;
    public ActionType Finish;
    public override void AddGameContent(GameImpl game, ModInfo info) {
        Map.OnGetStaticMaps += list =>
        {
            list.Add(("TinyLouvre", Localization.Get(LnCategory.Ui, "TinyLouvre.MapDescription"), MapIcon, false));
        };
        Map.OnLoadStaticMap += (MapInfo mapInfo, Newtonsoft.Json.JsonSerializer serializer, ref Map map) =>
        {
            if(mapInfo.Name == "TinyLouvre") 
                map ??= info.Content.LoadJson<Map>($"Maps/TinyLouvre", null, serializer);
        };

        game.OnSwitchGameMode += e =>
        {
            if(Options.OnlineMode) OnlineMode.Update();
            else Museum.Load();
            
            if (!Options.ShowOnlineModePrompt || e.Mode != GameImpl.GameMode.MainMenu) return;
            
            var root = GameImpl.Instance.UiSystem.Add(
                "TinyLouvreOnlineConfirmWindows",
                new ConfirmWindow()
            );
            root.SetPauseGame();
        };
        
        Easel = FurnitureType.Register(new FurnitureType.TypeSettings("TinyLouvre.Easel", new Point(1, 1), ObjectCategory.Table, 150, ColorScheme.White, ColorScheme.White  , ColorScheme.White) {
            ConstructedType = typeof(Easel),
            Icon = Icon,
            Tab = FurnitureTool.Tab.Other,
        });

        var artPieceCategory = ObjectCategory.WallHanging | ObjectCategory.NonBuyable | ObjectCategory.ArtPiece;
        ArtPiece = FurnitureType.Register(
            new FurnitureType.TypeSettings(
                "TinyLouvre.Painting", 
                new Point(1, 1), 
                artPieceCategory, 
                0, 
                ColorScheme.White
            ) {
            ConstructedType = typeof(ArtPiece),
            Icon = Icon,
        });

        MuseumArtPiece = FurnitureType.Register(
            new FurnitureType.TypeSettings(
                "TinyLouvre.MuseumPainting", 
                new Point(1, 1), 
                artPieceCategory, 
                0, 
                ColorScheme.White
            ) {
                ConstructedType = typeof(MuseumArtPiece),
                Icon = Icon,
                BuyableVariations = () =>
                    Enumerable.Range(0, Museum.Paintings.Length).Select<int, Action<Furniture>>(i =>
                    {
                        return furniture =>
                        {
                            if (furniture is MuseumArtPiece art) art.MuseumIndex = i;
                        };
                    }).ToArray(),
                GetDisplayName = ((_, furniture) =>
                {
                    var i = 0;
                    if (furniture is MuseumArtPiece art) i = art.MuseumIndex;
                    return Localization.Get(LnCategory.BuildMode, "TinyLouvre.MuseumPainting", i);
                })
            });
        
        ActionType.Register(new ActionType.TypeSettings("TinyLouvre.Paint", ObjectCategory.Table, typeof(PaintAction)) {
            CanExecute = (actionInfo, _) => actionInfo.GetActionObject<Easel>() != null ? CanExecuteResult.Valid : CanExecuteResult.Hidden,
            Ai = {
                CanDoRandomly = false,
                SolvedNeeds = [],
                PassivePriority = _ => 0
            },
            Texture = UiTextures[new Point(1, 0)]
        });
        
        Finish = ActionType.Register(new ActionType.TypeSettings("TinyLouvre.Finish", ObjectCategory.Table, typeof(FinishAction)) {
            CanExecute = (_, _) => CanExecuteResult.Hidden,
            Ai = {
                CanDoRandomly = false,
                SolvedNeeds = [],
                PassivePriority = _ => 0
            },
            Texture = UiTextures[new Point(1, 0)],
        });
        
        ActionType.Register(new ActionType.TypeSettings("TinyLouvre.View", artPieceCategory, typeof(ViewAction)) {
            CanExecute = (actionInfo, _) => 
                (actionInfo.GetActionObject<ArtPiece>() != null) || (actionInfo.GetActionObject<MuseumArtPiece>() != null)
                    ? CanExecuteResult.Valid : CanExecuteResult.Hidden,
            Ai = {
                CanDoRandomly = false,
                SolvedNeeds = [],
                PassivePriority = _ => 0
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
    public bool OnlineMode;
    public bool ShowOnlineModePrompt = true;
    public int PaintingCost = 50;

    // An ATProto / BlueSky feed link. Will be access unauthenticated to search for valid paintings in posts.
    // Only does anything if Online Mode is enabled.
    public string OnlineModeAtprotoFeed =
        "https://public.api.bsky.app/xrpc/app.bsky.feed.getAuthorFeed?actor=did:plc:o3fftrm6ifwqrg2ecefine2h";
}

public record Painting(byte[,] Canvas, int[] Colors)
{
    // ReSharper disable once InconsistentNaming
    public const int SIZE_X = 10;
    // ReSharper disable once InconsistentNaming
    public const int SIZE_Y = 14;
    public string Export()
    {
        var array = new byte[47];
        var index = 0;
        foreach (var c in Colors)
        {
            array[index] = (byte) (c & 255);
            index++;
            array[index] = (byte) (c >> 8 & 255);
            index++;
            array[index] = (byte) (c >> 16 & 255);
            index++;
        }

        foreach (var b in LinearCanvas().Chunk(4))
        {
            byte c = 0;
            for (var i = 0; i < b.Length; i++)
            {
                c |= (byte) (b[i] << (i * 2));
            }
            array[index] = c;
            index++;
        }
        return Convert.ToBase64String(array);
    }

    private Texture2D[] _furnitureTextures;
    public Texture2D[] FurnitureTextures()
    {
        _furnitureTextures ??= new Texture2D[2];
        if (_furnitureTextures[0] != null && _furnitureTextures[1] != null) return _furnitureTextures;
        
        var ho = 4;
        var device = GameImpl.Instance.GraphicsDevice;
        _furnitureTextures[0] ??= new Texture2D(device, SIZE_X, (SIZE_Y + ho));
        _furnitureTextures[1] ??= new Texture2D(device, SIZE_X, (SIZE_Y + ho));

        var temp = new Texture2D(device, SIZE_X, SIZE_Y);
        SetCanvasTexture(temp);
        var data = new Color[SIZE_X * SIZE_Y];
        temp.GetData(data);
        
        var bg = Color.Transparent;
        
        // up (0) should be offset downwards and work up
        var store = new Color[SIZE_X * (SIZE_Y + ho)];
        for (var y = 0; y < (SIZE_Y + ho); y++)
        {
            for (var x = 0; x < SIZE_X; x++)
            {
                var oy = y + (ho - (x / 2)) - ho;
                if (oy is >= 0 and < SIZE_Y)
                {
                    store[y * SIZE_X + x] = data[oy * SIZE_X + x];
                }
                else
                {
                    store[y * SIZE_X + x] = bg;
                }
            }
        }
        _furnitureTextures[0].SetData(store);
        
        // right (1) should work down
        for (var y = 0; y < (SIZE_Y + ho); y++)
        {
            for (var x = 0; x < SIZE_X; x++)
            {
                var oy = y + (x / 2) - ho;
                if (oy is >= 0 and < SIZE_Y)
                {
                    store[y * SIZE_X + x] = data[oy * SIZE_X + x];
                }
                else
                {
                    store[y * SIZE_X + x] = bg;
                }
            }
        }
        _furnitureTextures[1].SetData(store);

        return _furnitureTextures;
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

// ReSharper disable once NotAccessedPositionalProperty.Global
public record PaintingWithMetadata(Painting Painting, string Author, string Handle, string Link);

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
public static partial class OnlineMode
{
    public static PaintingWithMetadata[] RecentPaintings = [];
    
    private record Record(string createdAt, string text);
    private record Author(string handle, string displayName);
    private record Post(Author author, Record record, string uri);
    private record FeedItem(Post post);
    private record Feed(FeedItem[] feed);

    static string atToBsky(string uri)
    {
        var regex = new Regex("^at:\\/\\/([^/]+)\\/[^/]+\\/([^/]+)$");
        var match = regex.Match(uri);
        return match.Groups.Count < 3 ? uri : $"https://bsky.app/profile/{match.Groups[1]}/post/{match.Groups[2]}";
    }

    public static async void Update()
    {
        try
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
            var pattern = TLVRegex();
            foreach (var item in feed.feed)
            {
                try
                {
                    var post = item.post.record.text;
                    var groups = pattern.Match(post).Groups;
                    if (groups.Count < 2) continue;
                    var painting = LouvreUtil.ImportPainting(groups[1].Value.Replace(' ', '+'));

                    var authorName = item.post.author.displayName;
                    var authorHandle = item.post.author.handle;
                    var uri = item.post.uri;
                    

                    recentPaintings.Add(new PaintingWithMetadata(painting, authorName, authorHandle, atToBsky(uri)));
                }
                catch (Exception e)
                {
                    TinyLouvre.Logger.Warn("There was an error processing a post.");
                    TinyLouvre.Logger.Warn(e);
                }
            }

            RecentPaintings = recentPaintings.ToArray();
            Museum.Load();
        }
        catch (Exception e)
        {
            TinyLouvre.Logger.Error(e);
        }
    }

    [GeneratedRegex(@"tlv\.(.*?)\.")]
    private static partial Regex TLVRegex();
}

public record EncodedPaintingWithMetadata(string Painting, string Author, string Link);
public class Museum
{
    public static PaintingWithMetadata[] Paintings = new PaintingWithMetadata[20];
    public static EncodedPaintingWithMetadata[] DefaultPaintings = [
        new("ufr5Dlh/AR4uBQUKAAAAAAAAAAAAAAAAoAIAFYBPRQVQBQAAAAAAwDM/AAAAAAA=", "René Magritte", "https://en.wikipedia.org/wiki/The_Treachery_of_Images"),
        new("dopnAAAYKCgtkLvdAAAAVADQHwD9B9B/APUH0H8B/iWob4KqKqqqqqqq2qdq/6k=", "Leonardo da Vinci", "https://en.wikipedia.org/wiki/Mona_Lisa"),
        new("JQoCAzZijL/6ym9mAAAAAAAAUADAVwD/BahOgCoEqEIAaABQBUBVAVQVUFUBVRU=", "Johannes Vermeer", "https://www.britannica.com/topic/Girl-with-a-Pearl-Earring-by-Vermeer"),
        new("HWqXAQbW8PD7ByMOAAAAAAAADwCwAAAXAFQBUBWA9QP4nwAVAFQBAFUAgAoAAAA=", "Hishikawa Moronobu", "https://www.tnm.jp/modules/r_collection/index.php?controller=dtl&colid=A60&lang=en"),
        new("FTJoAQbW8PD7AAABAAAAMADA/wC8CsCqALAKAKoAqQrVKlCtFtVaUeklpZZaaX0=", "Tōshūsai Sharaku", "https://www.metmuseum.org/art/collection/search/37358"),
    ];

    public static void Load()
    {
        var index = 0;
        if (TinyLouvre.Options.OnlineMode)
        {
            foreach (var painting in OnlineMode.RecentPaintings)
            {
                Paintings[index] = painting;
                index++;

                if (index >= Paintings.Length) return;
            }
        }
        
        if (index >= Paintings.Length) return;
        
        for (var i = index; i < Paintings.Length; i++)
        {
            var painting = DefaultPaintings[i % DefaultPaintings.Length];
            Paintings[i] = new(
                LouvreUtil.ImportPainting(painting.Painting),
                painting.Author,
                "",
                painting.Link
            );
        }
    }
}