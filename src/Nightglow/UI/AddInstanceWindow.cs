using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using Nightglow.Common.Dialogs;
using Nightglow.Common.Instances;

namespace Nightglow.UI;

public class AddInstanceWindow : ApplicationWindow {
    private Entry nameEntry;
    private Entry groupEntry;
    private Dictionary<string, Box> createInstanceBoxes;
    private Dictionary<string, MethodInfo> createInstanceMethods;
    private string visibleBox;
    private InstancePane pane;
    private FlowBox flow;

    public AddInstanceWindow(Gio.Application application, InstancePane pane, FlowBox flow) {
        this.pane = pane;
        this.flow = flow;

        var rootBox = new Box { Name = "rootBox" };
        rootBox.SetOrientation(Orientation.Vertical);
        this.SetChild(rootBox);

        var headerBox = new Box { Name = "headerBox" };
        headerBox.SetOrientation(Orientation.Horizontal);
        headerBox.SetValign(Align.Start);
        headerBox.SetVexpandSet(true);

        var iconBox = new Box { Name = "iconBox" };
        iconBox.SetOrientation(Orientation.Vertical);
        var iconButton = new Button { Name = "iconButton", Label = "Icon" };
        iconButton.SetVexpand(true);
        iconBox.Append(iconButton);
        headerBox.Append(iconBox);

        var textBox = new Box { Name = "textBox" };
        textBox.SetOrientation(Orientation.Vertical);
        var nameBox = new Box { Name = "nameBox" };
        nameBox.SetOrientation(Orientation.Horizontal);
        var nameLabel = new Label { Name = "nameLabel" };
        nameLabel.SetText("Name: ");
        nameBox.Append(nameLabel);
        nameEntry = new Entry { Name = "nameEntry" };
        nameEntry.SetHexpand(true);
        nameBox.Append(nameEntry);
        textBox.Append(nameBox);

        var groupBox = new Box();
        groupBox.SetOrientation(Orientation.Horizontal);
        var groupLabel = new Label();
        groupLabel.SetText("Group: ");
        groupBox.Append(groupLabel);
        groupEntry = new Entry();
        groupEntry.SetHexpand(true);
        groupBox.Append(groupEntry);
        textBox.Append(groupBox);

        headerBox.Append(textBox);
        rootBox.Append(headerBox);

        var selectInstanceBox = new Box();
        selectInstanceBox.SetOrientation(Orientation.Horizontal);
        selectInstanceBox.SetValign(Align.Fill);
        selectInstanceBox.SetVexpand(true);
        var selectInstanceColumn = new Box();
        selectInstanceColumn.SetOrientation(Orientation.Vertical);
        selectInstanceBox.Append(selectInstanceColumn);
        rootBox.Append(selectInstanceBox);

        var footerBox = new Box();
        footerBox.SetOrientation(Orientation.Horizontal);
        footerBox.SetHalign(Align.End);
        var okButton = new Button { Label = "Ok" };
        okButton.OnClicked += CreateSelectedInstance;
        footerBox.Append(okButton);
        var cancelButton = new Button { Label = "Cancel" };
        cancelButton.OnClicked += (_, _) => { this.Close(); };
        footerBox.Append(cancelButton);
        rootBox.Append(footerBox);

        createInstanceBoxes = new Dictionary<string, Box>();
        createInstanceMethods = new Dictionary<string, MethodInfo>();

        // This should instead get all of the types, sort them, then loop back through so it is in the correct order
        foreach (Type type in Assembly.GetCallingAssembly().GetTypes()) {
            if (type.GetInterface("ICreateInstance") != null) {
                var button = new Button { Label = type.Name };
                button.OnClicked += (Button sender, EventArgs args) => {
                    createInstanceBoxes[visibleBox!].Hide();
                    createInstanceBoxes[sender.Label!].Show();
                    visibleBox = sender.Label!;
                };

                selectInstanceColumn.Append(button);
                var box = new Box();
                box.SetOrientation(Orientation.Vertical);
                box.SetHexpand(true);
                createInstanceBoxes.Add(button.Label, box);
                var testFlow = new FlowBox();
                testFlow.Append(new FlowBoxChild { Name = "guhh" });
                box.Append(testFlow);
                selectInstanceBox.Append(box);
                box.Hide();

                createInstanceMethods.Add(button.Label, type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)!);
            }
        }

        visibleBox = createInstanceBoxes.Keys.First();
        createInstanceBoxes.Values.First().Show();

        this.Application = (Application)application;
        this.Title = "New Instance - Nightglow";
        this.SetDefaultSize(800, 600);
    }

    private void CreateSelectedInstance(Button sender, EventArgs args) {
        if (string.IsNullOrWhiteSpace(nameEntry.GetText())) {
            nameEntry.GrabFocus();
            return;
        }

        Task.Run(async () => {
            var instance = await (Task<Instance>)createInstanceMethods[visibleBox].Invoke(null, new object[] { nameEntry.GetText() })!;
            Program.Launcher.Instances.Add(instance);
            var uiInstance = new UIInstance(instance, pane);
            pane.SetInstance(uiInstance);
            flow.Append(uiInstance); // Eventually needs to be sorted

            this.Close();
        });
    }
}
