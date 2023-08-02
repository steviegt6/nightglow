using System;
using System.IO;
using System.Linq;
using Gtk;
using Nightglow.Common;

namespace Nightglow.UI;

public class IconWindow : ApplicationWindow {
    private string selectedIcon = null!;

    public IconWindow(Gio.Application application, Window parent, string? selected, Action<string> callback) {
        this.Application = (Application)application;
        this.Title = "Select Icon - Nightglow";
        this.SetDefaultSize(800, 800);
        this.SetTransientFor(parent);
        this.SetModal(true);

        var rootBox = new Box { Name = "IconWindow rootBox" };
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        var view = new GridView { Name = "IconWindow iconView" };
        view.SetVexpand(true);
        var model = StringList.New(IconHelper.GetAllIcons().ToArray()); // This should be sorted, but gir core doesn't support Expressions...
        view.SetModel(NoSelection.New(model));
        GObject.SignalHandler<Button> onClicked = (_, _) => { };
        var factory = SignalListItemFactory.New();
        factory.OnSetup += (factory, args) => {
            var item = (ListItem)args.Object;

            var button = new Button { Name = "IconWindow button" };
            var box = new Box();
            box.SetOrientation(Orientation.Vertical);
            button.SetChild(box);
            var image = Image.New();
            image.SetSizeRequest(128, 128);
            box.Append(image);
            var label = new Label { Name = "IconWindow label" };
            box.Append(label);
            item.SetChild(button);
        };
        factory.OnBind += (factory, args) => {
            var item = (ListItem)args.Object;
            var button = (Button?)item?.GetChild();
            var box = (Box?)button?.GetChild();
            var icon = ((StringObject?)item?.GetItem())?.String;
            if (icon == null || button == null || box == null)
                return;

            var image = (Image?)box.GetFirstChild();
            if (image == null)
                return;

            // Eventually do embedded resources
            image.SetFromFile(IconHelper.GetPath(icon));

            var label = (Label?)box.GetLastChild();
            if (label == null)
                return;

            label.SetText(icon.StartsWith("Nightglow.Assets.Icons") ? icon.Replace("Nightglow.Assets.Icons.", "") : icon);

            // FIXME: Seemingly gets called like 6 times
            onClicked = (_, _) => { selectedIcon = icon; button.GrabFocus(); };
            button.OnClicked += onClicked;
        };
        factory.OnUnbind += (factory, args) => {
            var item = (ListItem)args.Object;
            var button = (Button?)item?.GetChild();
            var box = (Box?)button?.GetChild();
            var image = (Image?)box?.GetFirstChild();
            var label = (Label?)box?.GetLastChild();

            if (button != null)
                button.OnClicked -= onClicked;

            if (image != null)
                image.Clear();

            if (label != null)
                label.SetText("");
        };
        factory.OnTeardown += (factory, args) => {
            var item = (ListItem)args.Object;
            var button = (Button?)item?.GetChild();
            var box = (Box?)button?.GetChild();
            var image = (Image?)box?.GetFirstChild();
            var label = (Label?)box?.GetLastChild();

            if (box != null)
                box.Dispose();

            if (image != null)
                image.Dispose();

            if (button != null)
                button.Dispose();

            if (label != null)
                label.Dispose();
        };
        view.SetFactory(factory);
        rootBox.Append(view);

        var footer = new Box { Name = "IconWindow footer" };
        footer.SetOrientation(Orientation.Horizontal);

        var leftBox = new Box { Name = "IconWindow leftBox" };
        leftBox.SetOrientation(Orientation.Horizontal);
        var addIconButton = new Button { Name = "IconWindow addIconButton", Label = "Add Icon" };
        addIconButton.OnClicked += (_, _) => {
            var filter = FileFilter.New();
            filter.AddPixbufFormats();
            var fileDialog = FileChooserNative.New("Select an icon - Nightglow", this, FileChooserAction.Open, "Open", "Cancel");
            fileDialog.SetFilter(filter);
            fileDialog.SetModal(true);
            fileDialog.Show();
            fileDialog.OnResponse += (dialog, args) => {
                if (args.ResponseId != (int)ResponseType.Accept)
                    return;

                var iconFile = fileDialog.GetFile();
                var path = iconFile?.GetPath();
                if (path == null)
                    return;

                var name = Path.GetFileName(path);
                var newPath = Path.Combine(Launcher.IconsPath, name);
                File.Copy(path, newPath);

                // Sort eventually?
                model.Append(name);
            };
        };
        leftBox.Append(addIconButton);
        footer.Append(leftBox);

        var rightBox = new Box { Name = "IconWindow rightBox" };
        rightBox.SetOrientation(Orientation.Horizontal);
        rightBox.SetHexpand(true);
        rightBox.SetHalign(Align.End);
        var okButton = new Button { Name = "iconWindow okButton", Label = "Ok" };
        okButton.OnClicked += (_, _) => {
            this.Close();
            callback(selectedIcon);
        };
        rightBox.Append(okButton);
        var cancelButton = new Button { Name = "IconWindow cancelButton", Label = "Cancel" };
        cancelButton.OnClicked += (_, _) => { this.Close(); };
        rightBox.Append(cancelButton);
        footer.Append(rightBox);
        rootBox.Append(footer);
    }
}
