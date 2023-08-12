using System;
using System.Collections.Generic;
using Gtk;
using Nightglow.Common;

namespace Nightglow.UI;

public class LauncherWindow : ApplicationWindow {
    private UIInstance? selected;
    private Dictionary<Widget, EventController> controllerPerWidget;

    public LauncherWindow(Application application) {
        controllerPerWidget = new Dictionary<Widget, EventController>();

        this.Application = application;
        this.Title = "Nightglow";
        this.SetDefaultSize(800, 600);

        var rootBox = new Box { Name = "rootBox" };
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        var instancePane = new InstancePane(application, this);

        var scrolled = new ScrolledWindow();
        var view = new GridView { Name = "instanceView" };
        view.SetHalign(Align.Fill);
        view.SetVexpand(true);
        view.SetHexpand(true);
        var model = Gio.ListStore.New(UIInstance.GetGType());
        Program.Launcher.Instances.ForEach(i => model.Append(new UIInstance(i)));
        view.SetModel(NoSelection.New(model));
        var factory = SignalListItemFactory.New();
        factory.OnSetup += (factory, args) => {
            var item = (ListItem)args.Object;

            var button = new Button { Name = "LauncherWindow instance" };
            var box = new Box { Name = "instance rootBox" };
            box.SetOrientation(Orientation.Vertical);
            button.SetChild(box);

            var image = Image.New();
            image.SetSizeRequest(128, 128);
            box.Append(image);
            var entry = new Entry { Name = "instance entry" };
            box.Append(entry);

            item.SetChild(button);
        };
        factory.OnBind += (factory, args) => {
            var item = (ListItem)args.Object;
            var instance = ((UIInstance?)item?.GetItem())?.Instance;
            var button = (Button?)item?.GetChild();
            var box = (Box?)button?.GetChild();
            if (instance == null || button == null || box == null)
                return;

            var image = (Image?)box.GetFirstChild();
            if (image == null)
                return;

            image.SetFromFile(IconUtils.GetPath(instance.Info.Icon));

            var entry = (Entry?)box.GetLastChild();
            if (entry == null)
                return;

            entry.SetText(instance.Info.Name);
            // FIXME: This suddenly stopped getting called
            var keyController = EventControllerKey.New();
            keyController.OnKeyPressed += (controller, args) => {
                Console.WriteLine($"{args.Keycode} {args.Keyval} {args.State}");
                if (args.Keycode == 133) { // Enter key. I couldn't find an enum
                    instance.Info.Name = entry.GetText();
                    instance.Save();
                    entry.Sensitive = false;

                    instancePane.Refresh();
                }

                return true;
            };
            AddController(entry, keyController);

            // FIXME: This only gets called once
            var gestureClick = GestureClick.New();
            gestureClick.OnPressed += (controller, args) => {
                if (args.NPress == 1)
                    instancePane.SetInstance(instance, button);
                else if (args.NPress == 2)
                    instance.Launch();
            };
            AddController(button, gestureClick);
        };
        factory.OnUnbind += (factory, args) => {
            var item = (ListItem)args.Object;
            var button = (Button?)item?.GetChild();
            var box = (Box?)button?.GetChild();
            var image = (Image?)box?.GetFirstChild();
            var entry = (Entry?)box?.GetLastChild();

            RemoveController(button);
            image?.Clear();
            entry?.SetText("");
            RemoveController(entry);
        };
        factory.OnTeardown += (factory, args) => {
            var item = (ListItem)args.Object;
            var button = (Button?)item?.GetChild();
            var box = (Box?)button?.GetChild();
            var image = (Image?)box?.GetFirstChild();
            var entry = (Entry?)box?.GetLastChild();

            image?.Dispose();
            entry?.Dispose();
            box?.Dispose();
            button?.Dispose();
        };
        view.SetFactory(factory);
        scrolled.SetChild(view);

        var ribbonBox = new Box { Name = "ribbonBox" };
        ribbonBox.SetOrientation(Orientation.Horizontal);

        var addInstanceButton = new Button { Label = "Add Instance" };
        addInstanceButton.OnClicked += (_, _) => {
            var addInstanceWindow = new AddInstanceWindow(application, this, (instance) => {
                Program.Launcher.Instances.Add(instance);
                /* var uiInstance = new UIInstance(instance, instancePane); */
                /* /1* instanceFlow.Append(uiInstance); *1/ */
                /* instancePane.SetInstance(uiInstance); */
            });
            addInstanceWindow.Show();
        };
        ribbonBox.Append(addInstanceButton);

        var foldersButton = new Button { Name = "foldersButton", Label = "Folders" };
        ribbonBox.Append(foldersButton);
        ribbonBox.Append(new Button { Label = "Settings" });

        var helpButton = new Button { Label = "Help" };
        helpButton.OnClicked += (_, _) => { Launcher.Platform.OpenUrl("https://github.com/steviegt6/terraprisma"); };
        ribbonBox.Append(helpButton);
        rootBox.Append(ribbonBox);

        var centerBox = new Box { Name = "centerBox" };
        centerBox.SetOrientation(Orientation.Horizontal);
        centerBox.Append(scrolled);
        instancePane.SetParent(centerBox);
        rootBox.Append(centerBox);
    }

    private void AddController(Widget widget, EventController controller) {
        widget.AddController(controller);
        controllerPerWidget.Add(widget, controller);
    }

    private void RemoveController(Widget? widget) {
        if (widget == null)
            return;

        bool s = controllerPerWidget.TryGetValue(widget, out EventController? controller);

        if (s && controller != null) {
            widget.RemoveController(controller);
            controllerPerWidget.Remove(widget);
        }
    }
}
