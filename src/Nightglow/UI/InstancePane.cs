using Gtk;

namespace Nightglow.UI;

public class InstancePane {
    private UIInstance displayed = null!; // Unfortunately, this can't be set in the ctor for reasons
    private Button iconButton; // Temporary, can replace with an actual type eventually, also used in AddInstanceWindow
    private Button nameButton;
    private Box rootBox;

    public InstancePane() {
        rootBox = new Box { Name = "InstancePane rootBox" };
        rootBox.SetOrientation(Orientation.Vertical);

        iconButton = new Button();
        rootBox.Append(iconButton);

        nameButton = new Button { Name = "nameButton" };
        nameButton.OnClicked += (_, _) => {
            displayed.NameEntry.GrabFocus();
        };
        rootBox.Append(nameButton);


        var killButton = new Button { Name = "killButton", Label = "Kill", CanTarget = false };
        var launchButton = new Button { Name = "launchButton", Label = "Launch" };
        launchButton.OnClicked += (_, _) => {
            displayed.Instance.Launch();
            killButton.CanTarget = true;
        };
        rootBox.Append(launchButton);

        killButton.OnClicked += (_, _) => {
            if (displayed.Instance.Process != null) {
                displayed.Instance.Process.Kill(true);
                displayed.Instance.Process.Dispose();
            }

            killButton.CanTarget = false;
        };
        rootBox.Append(killButton);
    }

    public void SetParent(Box parent) {
        parent.Append(rootBox);
    }

    public void SetInstance(UIInstance uiInstance) {
        if (displayed == uiInstance)
            return;

        displayed = uiInstance;
        nameButton.Label = uiInstance.Instance.Info.Name;
    }
}
