using System;
using System.Diagnostics;
using Gtk;

namespace Nightglow.UI.Launcher;

public class LauncherWindow : ApplicationWindow {
    private Box rootBox;
    private Box ribbonBox;
    private FlowBox instanceFlow;

    public LauncherWindow(Gio.Application application) {
        this.Application = (Application)application;

        rootBox = new Box();
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        ribbonBox = new Box();
        ribbonBox.SetOrientation(Orientation.Horizontal);
        ribbonBox.Append(new Button { Label = "Add Instance" });
        ribbonBox.Append(new Button { Label = "Folders" });
        ribbonBox.Append(new Button { Label = "Settings" });

        var helpButton = new Button { Label = "Help" };
        helpButton.OnClicked += (Button sender, EventArgs args) => { Process.Start("xdg-open", "https://github.com/steviegt6/terraprisma"); };
        ribbonBox.Append(helpButton);

        rootBox.Append(ribbonBox);

        instanceFlow = new FlowBox();
        instanceFlow.SetValign(Gtk.Align.Fill);
        instanceFlow.SetVexpand(true);
        instanceFlow.Append(new FlowBoxChild { Name = "guhhhh" });
        rootBox.Append(instanceFlow);

        this.Title = "Nightglow";
        this.SetDefaultSize(800, 600);
    }
}
