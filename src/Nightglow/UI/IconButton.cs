using System;
using System.IO;
using Gtk;
using Nightglow.Common;

namespace Nightglow.UI;

public class IconButton : Button {
    private readonly int _size;

    public IconButton(Application application, Window parent, string? icon, int size, Action<string> callback) {
        if (icon != null)
            SetIcon(icon);
        _size = size;
        OnClicked += (_, _) => {
            var iconWindow = new IconWindow(application, parent, null, (icon) => {
                SetIcon(icon);
                callback(icon);
            });
            iconWindow.Show();
        };
    }

    public void SetIcon(string icon) {
        var image = this.GetChild();
        image?.Dispose();

        image = Image.NewFromFile(Path.Combine(Launcher.IconsPath, icon));
        image.SetSizeRequest(_size, _size);
        this.SetChild(image); // I don't know if I even need to reset the Child but it can't hurt...
    }
}
