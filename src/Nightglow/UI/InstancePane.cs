using System;
using Gtk;

namespace Nightglow.UI;

public class InstancePane {
    private UIInstance displayed = null!; // Unfortunately, this can't be set in the ctor for reasons
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
            displayed.Instance.Info.Icon = icon;
            displayed.Instance.Save();
        });
        rootBox.Append(iconButton);

        nameButton = new Button { Name = "nameButton" };
        nameButton.OnClicked += (_, _) => {
            displayed.NameEntry.GrabFocus();
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
            displayed.Instance.Launch();
            displayed.Instance.Process!.Exited += onExit;
            killButton.Sensitive = true;
            launchButton.Sensitive = false;
        };
        rootBox.Append(launchButton);

        killButton.OnClicked += (_, _) => {
            if (displayed.Instance.Process != null) {
                displayed.Instance.Process.Kill(true);
                displayed.Instance.Process.Dispose();
                // Null this out to prevent exceptions from trying to access properties on it later
                displayed.Instance.Process = null;
            }

            killButton.Sensitive = false;
            launchButton.Sensitive = true;
        };
        rootBox.Append(killButton);

        var addToSteamButton = new Button { Name = "addToSteamButton", Label = "Add to Steam" };
        addToSteamButton.OnClicked += (_, _) => {
            displayed.Instance.AddToSteam();
        };
        rootBox.Append(addToSteamButton);
    }

    public void SetParent(Box parent) {
        parent.Append(rootBox);
    }

    public void SetInstance(UIInstance uiInstance) {
        if (displayed == uiInstance)
            return;

        if (displayed != null && displayed.Instance.Process != null)
            displayed.Instance.Process.Exited -= onExit; // It is legal to remove an event handler that doesn't exist, so just always remove it

        if (uiInstance.Instance.Process != null && !uiInstance.Instance.Process.HasExited) {
            uiInstance.Instance.Process.Exited += onExit;
            killButton.Sensitive = true;
            launchButton.Sensitive = false;
        }
        else {
            killButton.Sensitive = false;
            launchButton.Sensitive = true;
        }
        displayed = uiInstance;
        nameButton.Label = uiInstance.Instance.Info.Name;

        iconButton.SetIcon(uiInstance.Instance.Info.Icon);
    }
}
