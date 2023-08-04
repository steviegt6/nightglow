using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gtk;
using Nightglow.Common;

namespace Nightglow.UI;

public class IconWindow : ApplicationWindow, IDisposable {
    private List<IDisposable> disposables;

    private static FileFilter Filter;
    private Button okButton;
    private Button removeIconButton;
    private Button defaultButton = null!;
    private Button selectedButton = null!;
    private string selectedIcon = null!;

    static IconWindow() {
        Filter = FileFilter.New();
        Filter.AddPixbufFormats();
    }

    // I love state
    private void SelectDefault() {
        if (defaultButton == null)
            return;

        defaultButton.Sensitive = false;
        selectedIcon = "Nightglow.Assets.Icons.Terraria.png";

        if (selectedButton != null)
            selectedButton.Sensitive = true;

        selectedButton = defaultButton;
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

        this.Application = (Application)application;
        this.Title = "Select Icon - Nightglow";
        this.SetDefaultSize(800, 800);
        this.SetTransientFor(parent);
        this.SetModal(true);

        var rootBox = new Box { Name = "IconWindow rootBox" };
        // For some reason, disposing of the root box causes a crash...
        /* disposables.Add(rootBox); */
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        var scrolled = new ScrolledWindow();
        disposables.Add(scrolled);
        var view = new GridView { Name = "IconWindow iconView" };
        disposables.Add(view);
        view.SetVexpand(true);
        var model = StringList.New(IconUtils.GetAllIcons().ToArray()); // This should be sorted, but gir core doesn't support Expressions...
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

            if (icon == "Nightglow.Assets.Icons.Terraria.png") {
                defaultButton = button;

                if (selected == null)
                    button.Sensitive = false;
            }

            var image = (Image?)box.GetFirstChild();
            if (image == null)
                return;

            // Eventually do embedded resources
            image.SetFromFile(IconUtils.GetPath(icon));

            var label = (Label?)box.GetLastChild();
            if (label == null)
                return;

            label.SetText(IconUtils.GetFakeName(icon));

            if (icon == selected) {
                button.Sensitive = false;
                selectedButton = button;
                selectedIcon = icon;
            }

            // FIXME: Seemingly gets called like 6 times
            onClicked = (sender, args) => {
                selectedIcon = icon;

                if (selectedButton != null)
                    selectedButton.Sensitive = true;

                sender.Sensitive = false;
                selectedButton = sender;

                if (okButton != null)
                    okButton.Sensitive = true;

                if (IconUtils.IsEmbedded(icon)) // I hope removeIconButton isn't somehow null here :)
                    removeIconButton!.Sensitive = false;
                else
                    removeIconButton!.Sensitive = true;
            };
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
        scrolled.SetChild(view);
        rootBox.Append(scrolled);

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

        removeIconButton = new Button { Name = "IconWindow addIconButton", Label = "Remove Icon" };
        if (selected != null && IconUtils.IsEmbedded(selected))
            removeIconButton.Sensitive = false;
        removeIconButton.OnClicked += (_, _) => {
            for (uint i = 0; i < model.GetNItems(); i++) {
                var item = model.GetString(i);
                if (item != selectedIcon)
                    continue;

                if (!IconUtils.IsEmbedded(item)) {
                    model.Remove(i);
                    File.Delete(IconUtils.GetPath(item));
                    SelectDefault();
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

        okButton = new Button { Name = "iconWindow okButton", Label = "Ok" };
        if (selected != null)
            okButton.Sensitive = true;
        okButton.OnClicked += (_, _) => {
            this.Close();
            this.Dispose();
            callback(selectedIcon);
        };
        rightBox.Append(okButton);

        var cancelButton = new Button { Name = "IconWindow cancelButton", Label = "Cancel" };
        disposables.Add(cancelButton);
        cancelButton.OnClicked += (_, _) => { this.Close(); this.Dispose(); };
        rightBox.Append(cancelButton);
        footer.Append(rightBox);
        rootBox.Append(footer);
    }

    public override void Dispose() {
        DisposableUtils.DisposeList(disposables);

        okButton.Dispose();
        removeIconButton.Dispose();

        base.Dispose();
    }
}
