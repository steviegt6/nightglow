using System;
using Gtk;
using Nightglow.Common;

namespace Nightglow.UI;

public class IconButton : Button, IDisposable {
    private readonly int _size;
    private string _icon = null!;

    public IconButton(Gtk.Application application, Window parent, string? icon, int size, Action<string> callback) {
        _size = size;
        SetIcon(icon ?? "Nightglow.Assets.Icons.Terraria.png");
        OnClicked += (_, _) => {
            var iconWindow = new IconWindow(application, parent, _icon, (icon) => {
                SetIcon(icon);
                callback(icon);
            });
            iconWindow.Show();
        };
    }

    public void SetIcon(string icon) {
        var image = this.GetChild();
        image?.Dispose();

        _icon = icon;
        image = Image.NewFromFile(IconUtils.GetPath(icon));
        image.SetSizeRequest(_size, _size);
        this.SetChild(image); // I don't know if I even need to reset the Child but it can't hurt...
    }

    public override void Dispose() {
        this.GetChild()?.Dispose();
        base.Dispose();
    }
}
