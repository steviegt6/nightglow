using Gtk;
using Nightglow.Common.Instances;

namespace Nightglow.UI;

public class UIInstance : FlowBoxChild {
    public Instance Instance { get; }
    public Entry NameEntry { get; }

    public UIInstance(Instance instance, InstancePane pane) {
        this.Instance = instance;
        this.Name = instance.Info.Name;

        var rootBox = new Box { Name = "UIInstance rootBox" };
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        var button = new Button { Name = instance.Info.Name + "-Button", Label = instance.Info.Name };
        button.OnClicked += (_, _) => {
            pane.SetInstance(this);
        };
        // gesture don't work
        /* var gestureClick = GestureClick.New(); */
        /* gestureClick.SetButton(0); */
        /* gestureClick.OnPressed += (GestureClick sender, GestureClick.PressedSignalArgs args) => { */
        /*     if (args.NPress == 1) */
        /*         pane.SetInstance(this); */
        /*     else if (args.NPress == 2) */
        /*         Instance.Launch() */
        /* }; */
        /* gestureClick.OnReleased += (_, _) => { Console.WriteLine("released"); }; */
        /* button.AddController(gestureClick); */
        rootBox.Append(button);

        NameEntry = new Entry { Name = "UIInstance nameEntry", CanFocus = false, FocusOnClick = false };
        NameEntry.SetText(instance.Info.Name);
        rootBox.Append(NameEntry);
    }
}
