using System;
using System.Threading;
using Gtk;
using Nightglow.Common;
using Nightglow.Common.Dialogs;

namespace Nightglow.UI;

public class LauncherWindow : ApplicationWindow {
    public LauncherWindow(Application application) {
        var rootBox = new Box { Name = "rootBox" };
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        var centerBox = new Box { Name = "centerBox" };
        var instancePane = new InstancePane();
        var instanceFlow = new FlowBox { Name = "instanceFlow" };

        var ribbonBox = new Box { Name = "ribbonBox" };
        ribbonBox.SetOrientation(Orientation.Horizontal);
        var addInstanceButton = new Button { Label = "Add Instance" };
        addInstanceButton.OnClicked += (Button _, EventArgs _) => {
            var addInstanceWindow = new AddInstanceWindow(application, instancePane, instanceFlow);
            addInstanceWindow.SetTransientFor(this);
            addInstanceWindow.Show();
        };
        ribbonBox.Append(addInstanceButton);
        var foldersButton = new Button { Name = "foldersButton", Label = "Folders" };
        ribbonBox.Append(foldersButton);
        ribbonBox.Append(new Button { Label = "Settings" });

        var helpButton = new Button { Label = "Help" };
        helpButton.OnClicked += (_, _) => { Launcher.Platform.OpenUrl("https://github.com/steviegt6/terraprisma"); };
        ribbonBox.Append(helpButton);

        Console.WriteLine("making guh: " + Thread.CurrentThread.ManagedThreadId);

        var guh = new Button { Label = "guh" };
        guh.OnClicked += (_, _) => {
            Console.WriteLine("guh on clicked: " + Thread.CurrentThread.ManagedThreadId);

            var copt = new DialogOption("cancel", "cancel guh", (sender) => { Console.WriteLine("guh"); });
            var d = Program.Launcher.NewProgressDialog("guhdialog", "guhheader", "guhtext", new DialogOption[] { copt });
            d.PulseWhile(100, () => {
                Console.WriteLine("guh pulsewhile: " + Thread.CurrentThread.ManagedThreadId);
                return true; });
        };
        ribbonBox.Append(guh);

        rootBox.Append(ribbonBox);

        centerBox.SetOrientation(Orientation.Horizontal);

        centerBox.Append(instanceFlow);

        instanceFlow.SetValign(Gtk.Align.Fill);
        instanceFlow.SetVexpand(true);
        instanceFlow.SetHexpand(true);
        foreach (var instance in Program.Launcher.Instances) {
            instanceFlow.Append(new UIInstance(instance, instancePane)); // Needs to be sorted eventually
        }
        instancePane.SetParent(centerBox);

        var uiInstance = instanceFlow.GetChildAtIndex(0);
        if (uiInstance != null)
            instancePane.SetInstance((UIInstance)uiInstance);
        rootBox.Append(centerBox);

        this.Application = application;
        this.Title = "Nightglow";
        this.SetDefaultSize(800, 600);
    }
}
