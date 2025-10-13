using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;
using TinyLife.Uis;

namespace TinyLouvre.UI;

public class ColorPickerWindow : CoveringGroup
{
    private readonly SaturationValueField _saturationValueField;
    private readonly HueSlider _hueSlider;
    private readonly byte _color;
    public ColorPickerWindow(byte color)
    {
        var picker = this;
        var colorValue = Color.FromNonPremultiplied(LouvreUtil.IntToVector4(PaintingArea.Colors[_color]));
        _color = color;
        
        var root = new Panel(Anchor.Center, new Vector2(200, 150));
        AddChild(root);

        var (hue, saturation, value) = colorValue.ToHsv();
        
        var group = new Group(Anchor.CenterRight, new Vector2(40, 130), false, false);
        root.AddChild(group);

        group.AddChild(new Paragraph(Anchor.AutoLeft, 40, "Old:"));
        group.AddChild(new Panel(Anchor.AutoLeft, new Vector2(20, 20))
        {
            DrawColor = colorValue
        });
        
        group.AddChild(new Paragraph(Anchor.AutoLeft, 40, "New:"));
        var newColor = new Panel(Anchor.AutoLeft, new Vector2(20, 20))
        {
            DrawColor = colorValue
        };
        group.AddChild(newColor);

        var button = new Button(Anchor.BottomLeft, new Vector2(40, 12), _ => "Done");
        group.AddChild(button);
        button.OnPressed += _ => picker.Close();
        
        group = new Group(Anchor.CenterLeft, new Vector2(140, 130), false, false);
        root.AddChild(group);
        
        _saturationValueField = new SaturationValueField(Anchor.CenterLeft, new Vector2(130, 130));
        _saturationValueField.SetHue((int) (hue * 255));
        _saturationValueField.OnSaturationValueSet += (element, s, v) =>
        {
            saturation = s;
            value = v;

            PaintingArea.Colors[_color] = LouvreUtil.ColorToInt(ColorHelper.FromHsv(hue / 255f, saturation / 255f, value / 255f));;
            
            PaintingArea.UpdateCanvas();
            newColor.DrawColor = ColorHelper.FromHsv(hue / 255f, saturation / 255f, value / 255f);
        };
        _saturationValueField.Saturation = (int) (saturation * 255);
        _saturationValueField.Value = (int) (value * 255);
        group.AddChild(_saturationValueField);

        _hueSlider = new HueSlider(Anchor.CenterRight, new Vector2(8, 130));
        _hueSlider.OnHueSet += (element, i) =>
        {
            hue = i;
            _saturationValueField.SetHue(i);
            PaintingArea.UpdateCanvas();
            newColor.DrawColor = ColorHelper.FromHsv(hue / 255f, saturation / 255f, value / 255f);
        };
        _hueSlider.Value = (int) (hue * 255);
        group.AddChild(_hueSlider);
    }
}

internal class SaturationValueField : Element
{
    private static Texture2D _field;
    private static float _crosshairScale = 2.0f;
    private readonly TextureRegion _crosshair = TinyLouvre.UiTextures[new Point(4, 0)];
    public int Saturation;
    public int Value;
    public SaturationValueField(Anchor anchor, Vector2 size) : base(anchor, size)
    {
        if (_field == null)
        {
            _field = new Texture2D(GameImpl.Instance.GraphicsDevice, 256, 256);
            var buffer = new Color[65536];
            for (var x = 0; x < 256; x++)
                for (var y = 0; y < 256; y++)
                    buffer[(y << 8) + x] = ColorHelper.FromHsv(1f, x / 255f, y / 255f);
            _field.SetData(buffer);
        }
        OnPressed += element =>
        {
            var diff = element.Controls.Input.MousePosition.ToVector2() - element.DisplayArea.Location;
            var scale = diff / element.DisplayArea.Size;
            Saturation = (int) Math.Floor(scale.X * 255);
            Value = (int) Math.Floor(scale.Y * 255);
            OnSaturationValueSet?.Invoke(this, Saturation, Value);
        };
    }
    
    public delegate void UpdateDelegate(SaturationValueField element, int saturation, int value);
    public event UpdateDelegate OnSaturationValueSet;
    public override bool CanBePressed => true;    
    
    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        base.Draw(time, batch, alpha, context);
        batch.Draw(_field, DisplayArea, Color.White);
        
        var crosshairY = (Value / 255f) * DisplayArea.Height - (5f * _crosshairScale);
        var crosshairX = (Saturation / 255f) * DisplayArea.Height - (5f * _crosshairScale);
        var crosshairRect = new RectangleF(DisplayArea.X + crosshairX, DisplayArea.Y + crosshairY, 16 * _crosshairScale, 16 * _crosshairScale);
        batch.Draw(_crosshair, crosshairRect, Color.White);
    }

    private Color[] _buffer = new Color[65536];
    public void SetHue(int hue)
    {
        for (var x = 0; x < 256; x++)
        for (var y = 0; y < 256; y++)
            _buffer[(y << 8) + x] = ColorHelper.FromHsv(hue / 255f, x / 255f, y / 255f);
        _field.SetData(_buffer);
    }
}

internal class HueSlider : Element
{
    private static Texture2D _hueSlider;
    private static float _sliderScale = 2.0f;
    private readonly TextureRegion _sliderBar = TinyLouvre.UiTextures[new Point(5, 0)];
    public int Value;
    public HueSlider(Anchor anchor, Vector2 size) : base(anchor, size)
    {
        if (_hueSlider == null)
        {
            _hueSlider = new Texture2D(GameImpl.Instance.GraphicsDevice, 1, 256);
            var buffer = new Color[256];
            for (var i = 0; i < 256; i++)
            {
                buffer[i] = ColorHelper.FromHsv(i / 255f, 1f, 1f);
            }
            _hueSlider.SetData(buffer);
        }

        OnPressed += element =>
        {
            var diff = element.Controls.Input.MousePosition.ToVector2() - element.DisplayArea.Location;
            var scale = diff / element.DisplayArea.Size;
            Value = (int) Math.Floor(scale.Y * 255);
            OnHueSet?.Invoke(this, Value);
        };
    }

    public delegate void UpdateDelegate(HueSlider element, int hue);
    public event UpdateDelegate OnHueSet;
    public override bool CanBePressed => true;
    
    public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context)
    {
        base.Draw(time, batch, alpha, context);
        batch.Draw(_hueSlider, DisplayArea, Color.White);

        var sliderY = (Value / 255f) * DisplayArea.Height - (2 * _sliderScale);
        var sliderRect = new RectangleF(DisplayArea.X, DisplayArea.Y + sliderY, DisplayArea.Width, 16 * _sliderScale);
        batch.Draw(_sliderBar, sliderRect, Color.White);
    }
}