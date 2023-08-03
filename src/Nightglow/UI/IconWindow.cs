using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gtk;
using Nightglow.Common;

namespace Nightglow.UI;

public class IconWindow : ApplicationWindow {
    private List<IDisposable> disposables;

    private static FileFilter Filter;
    private string selectedIcon = null!;

    static IconWindow() {
        Filter = FileFilter.New();
        Filter.AddPixbufFormats();
    }

    // Very very very slow... seemingly the only option for sorting the list though, as the normal gtk sorter things are unimplemented by gir core
    // Yes, this defeats the purpose of a stringlist, which is meant to quickly add and remove items... because this is slow...
    private void SortStringList(StringList list) {
        var temp = new List<string>();

        while (list.GetNItems() > 0) {
            var item = list.GetString(0);
            if (item != null)
                temp.Add(item);

            list.Remove(0);
        }

        foreach (var item in temp.OrderByFakeName())
            list.Append(item);
    }

    public IconWindow(Gio.Application application, Window parent, string? selected, Action<string> callback) {
        disposables = new List<IDisposable>();
        this.OnCloseRequest += (_, _) => { this.Dispose(); return false; };

        this.Application = (Application)application;
        this.Title = "Select Icon - Nightglow";
        this.SetDefaultSize(800, 800);
        this.SetTransientFor(parent);
        this.SetModal(true);

        var rootBox = new Box { Name = "IconWindow rootBox" };
        disposables.Add(rootBox);
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        var view = new GridView { Name = "IconWindow iconView" };
        disposables.Add(view);
        view.SetVexpand(true);
        var model = StringList.New(IconHelper.GetAllIcons().ToArray()); // This should be sorted, but gir core doesn't support Expressions...
        disposables.Add(model);
        view.SetModel(NoSelection.New(model));
        GObject.SignalHandler<Button> onClicked = (_, _) => { };
        var factory = SignalListItemFactory.New();
        disposables.Add(factory);
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

            label.SetText(IconHelper.GetFakeName(icon));

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
        disposables.Add(footer);
        footer.SetOrientation(Orientation.Horizontal);

        var leftBox = new Box { Name = "IconWindow leftBox" };
        disposables.Add(leftBox);
        leftBox.SetOrientation(Orientation.Horizontal);

        var addIconButton = new Button { Name = "IconWindow addIconButton", Label = "Add Icon" };
        disposables.Add(addIconButton);
        addIconButton.OnClicked += (_, _) => {
            var fileDialog = FileChooserNative.New("Select an icon - Nightglow", this, FileChooserAction.Open, "Open", "Cancel");
            fileDialog.SetFilter(Filter);
            fileDialog.SetModal(true);
            fileDialog.Show();
            fileDialog.OnResponse += (dialog, args) => {
                fileDialog.Dispose();
                if (args.ResponseId != (int)ResponseType.Accept)
                    return;

                var iconFile = fileDialog.GetFile();
                var path = iconFile?.GetPath();
                if (path == null)
                    return;

                var name = Path.GetFileName(path);
                var newPath = Path.Combine(Launcher.IconsPath, name);
                if (File.Exists(newPath)) {
                    // TODO: Use confirmation dialog
                    return;
                }

                File.Copy(path, newPath);

                // Sort eventually?
                model.Append(name);
                SortStringList(model);
            };
        };
        leftBox.Append(addIconButton);

        var removeIconButton = new Button { Name = "IconWindow addIconButton", Label = "Remove Icon" };
        disposables.Add(removeIconButton);
        removeIconButton.OnClicked += (_, _) => {
            for (uint i = 0; i < model.GetNItems(); i++) {
                var item = model.GetString(i);
                if (item != selectedIcon)
                    continue;

                if (!IconHelper.IsEmbedded(item)) {
                    model.Remove(i);
                    File.Delete(IconHelper.GetPath(item));
                    selectedIcon = "Nightglow.Assets.Icons.Terraria.png";
                }
            }
        };
        leftBox.Append(removeIconButton);
        footer.Append(leftBox);

        var rightBox = new Box { Name = "IconWindow rightBox" };
        disposables.Add(rightBox);
        rightBox.SetOrientation(Orientation.Horizontal);
        rightBox.SetHexpand(true);
        rightBox.SetHalign(Align.End);
        var okButton = new Button { Name = "iconWindow okButton", Label = "Ok" };
        disposables.Add(okButton);
        okButton.OnClicked += (_, _) => {
            this.Close();
            callback(selectedIcon);
        };
        rightBox.Append(okButton);
        var cancelButton = new Button { Name = "IconWindow cancelButton", Label = "Cancel" };
        disposables.Add(cancelButton);
        cancelButton.OnClicked += (_, _) => { this.Close(); };
        rightBox.Append(cancelButton);
        footer.Append(rightBox);
        rootBox.Append(footer);
    }
}
