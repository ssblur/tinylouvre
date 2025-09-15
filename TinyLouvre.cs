using System;
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
