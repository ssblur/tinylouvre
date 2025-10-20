using Microsoft.Xna.Framework;
using MLEM.Formatting;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;
using TinyLife.Uis;

namespace TinyLouvre.UI;

public class ConfirmWindow : CoveringGroup
{
    public ConfirmWindow()
    {
        var root = new Panel(Anchor.Center, new Vector2(140, 90));
        AddChild(root);

        for (var i = 1; i <= 4; i++)
        {
            var key = $"TinyLouvre.OnlineModeDisclaimer.{i}";
            var paragraph = new Paragraph(Anchor.AutoLeft,
                120,
                _ => Localization.Get(LnCategory.Ui, key),
                TextAlignment.Left,
                true
            );
            root.AddChild(paragraph);
        }

        root.AddChild(new VerticalSpace(10));
        
        var group = new Group(Anchor.AutoLeft, new Vector2(root.Size.X - 10, 12));

        var confirm = new Button(Anchor.CenterLeft, new Vector2(50, 12), _ => Localization.Get(LnCategory.Ui, "TinyLouvre.Enable"));
        confirm.OnPressed += _ =>
        {
            TinyLouvre.Options.OnlineMode = true;
            TinyLouvre.Options.ShowOnlineModePrompt = false;
            TinyLouvre.Info.SaveOptions(TinyLouvre.Options);
            OnlineMode.Update();
            Close();
        };
        group.AddChild(confirm);
        
        var deny = new Button(Anchor.CenterRight, new Vector2(50, 12), _ => Localization.Get(LnCategory.Ui, "TinyLouvre.Disable"));
        deny.OnPressed += _ =>
        {
            TinyLouvre.Options.OnlineMode = false;
            TinyLouvre.Options.ShowOnlineModePrompt = false;
            TinyLouvre.Info.SaveOptions(TinyLouvre.Options);
            Museum.Load();
            Close();
        };
        group.AddChild(deny);

        root.AddChild(group);
    }
}