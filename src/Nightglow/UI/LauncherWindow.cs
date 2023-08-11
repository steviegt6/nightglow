using Gtk;
using Nightglow.Common;

namespace Nightglow.UI;

public class LauncherWindow : ApplicationWindow {
    public LauncherWindow(Application application) {
        this.Application = application;
        this.Title = "Nightglow";
        this.SetDefaultSize(800, 600);

        var rootBox = new Box { Name = "rootBox" };
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        var centerBox = new Box { Name = "centerBox" };
        var instancePane = new InstancePane(application, this);
        var instanceFlow = new FlowBox { Name = "instanceFlow" };

        var ribbonBox = new Box { Name = "ribbonBox" };
        ribbonBox.SetOrientation(Orientation.Horizontal);
        var addInstanceButton = new Button { Label = "Add Instance" };
        addInstanceButton.OnClicked += (_, _) => {
            var addInstanceWindow = new AddInstanceWindow(application, this, (instance) => {
                Program.Launcher.Instances.Add(instance);
                var uiInstance = new UIInstance(instance, instancePane);
                instanceFlow.Append(uiInstance);
                instancePane.SetInstance(uiInstance);
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
    }
}
