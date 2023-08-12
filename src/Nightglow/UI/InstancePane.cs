using System;
using Gtk;
using Nightglow.Common;
using Nightglow.Common.Instances;

namespace Nightglow.UI;

public class InstancePane {
    private Instance displayed = null!; // Unfortunately, this can't be set in the ctor for reasons
    private Button displayedButton = null!;
    private Image? displayedImage => (Image?)displayedButton?.GetChild()?.GetFirstChild();
    private Entry? displayedEntry => (Entry?)displayedButton?.GetChild()?.GetLastChild();
    private IconButton iconButton; // Temporary, can replace with an actual type eventually, also used in AddInstanceWindow
    private Button nameButton;
    private Button killButton;
    private Button launchButton;
    private Box rootBox;
    EventHandler onExit;

    public InstancePane(Application application, Window parent) {
        rootBox = new Box { Name = "InstancePane rootBox" };
        rootBox.SetOrientation(Orientation.Vertical);

        iconButton = new IconButton(application, parent, null, 64, (icon) => {
            displayed.Info.Icon = icon;
            displayed.Save();
            displayedImage?.SetFromFile(IconUtils.GetPath(icon));
        });
        rootBox.Append(iconButton);

        nameButton = new Button { Name = "nameButton" };
        nameButton.OnClicked += (_, _) => {
            displayedEntry?.GrabFocus();
        };
        rootBox.Append(nameButton);

        killButton = new Button { Name = "killButton", Label = "Kill" };
        launchButton = new Button { Name = "launchButton", Label = "Launch" };

        killButton.Sensitive = false;
        onExit = (_, _) => {
            killButton.Sensitive = false;
            launchButton.Sensitive = true;
        };

        launchButton.OnClicked += (_, _) => {
            displayed.Launch();
            displayed.Process!.Exited += onExit;
            killButton.Sensitive = true;
            launchButton.Sensitive = false;
        };
        rootBox.Append(launchButton);

        killButton.OnClicked += (_, _) => {
            if (displayed.Process != null) {
                displayed.Process.Kill(true);
                displayed.Process.Dispose();
                // Null this out to prevent exceptions from trying to access properties on it later
                displayed.Process = null;
            }

            killButton.Sensitive = false;
            launchButton.Sensitive = true;
        };
        rootBox.Append(killButton);

        var addToSteamButton = new Button { Name = "addToSteamButton", Label = "Add to Steam" };
        addToSteamButton.OnClicked += (_, _) => {
            displayed.AddToSteam();
        };
        rootBox.Append(addToSteamButton);
    }

    public void SetParent(Box parent) {
        parent.Append(rootBox);
    }

    public void SetInstance(Instance instance, Button button) {
        if (displayed == instance)
            return;

        if (displayed != null && displayed.Process != null)
            displayed.Process.Exited -= onExit; // It is legal to remove an event handler that doesn't exist, so just always remove it

        if (instance.Process != null && !instance.Process.HasExited) {
            instance.Process.Exited += onExit;
            killButton.Sensitive = true;
            launchButton.Sensitive = false;
        }
        else {
            killButton.Sensitive = false;
            launchButton.Sensitive = true;
        }

        displayed = instance;
        displayedButton = button;
        nameButton.Label = instance.Info.Name;
        iconButton.SetIcon(instance.Info.Icon);
    }

    /// <summary>
    /// Force the window to reflect changes to the instance it's currently displaying
    /// </summary>
    public void Refresh() {
        nameButton.Label = displayed.Info.Name;
        iconButton.SetIcon(displayed.Info.Icon);
    }
}
