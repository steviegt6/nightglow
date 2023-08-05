using System;
using System.Collections.Generic;
using Gtk;
using Nightglow.Common;
using Nightglow.Common.Dialogs;

namespace Nightglow.UI;

public class UIConfirmationDialog : IConfirmationDialog {
    private List<IDisposable> disposables;

    private readonly Application _application;
    private readonly Window _parent;
    private Window dialog = null!;
    private Label label = null!;

    public UIConfirmationDialog(Application application, Window parent) {
        disposables = new List<IDisposable>();
        _application = application;
        _parent = parent;
    }

    public void Initialize(string title, string text, params DialogOption<IConfirmationDialog>[] opts) {
        dialog = new Window { Title = title, Application = _application, Modal = true };

        var rootBox = new Box { Name = "ConfirmationDialog rootBox" };
        disposables.Add(rootBox);
        rootBox.SetOrientation(Orientation.Vertical);
        dialog.SetChild(rootBox);

        label = new Label { Name = "ConfirmationDialog label" };
        label.SetText(text);
        label.SetVexpand(true);
        rootBox.Append(label);

        var footerBox = new Box { Name = "ConfirmationDialog footerBox" };
        disposables.Add(footerBox);
        footerBox.SetOrientation(Orientation.Horizontal);
        footerBox.SetHalign(Align.End);
        rootBox.Append(footerBox);

        foreach (DialogOption<IConfirmationDialog> opt in opts) {
            var button = new Button { Label = opt.Label };
            disposables.Add(button);
            button.TooltipText = opt.ToolTip;
            if (opt.OnAccept != null)
                button.OnClicked += (_, _) => opt.OnAccept(this);
            footerBox.Append(button);
        }

        dialog.SetDefaultSize(300, 100);
        dialog.SetTransientFor(_parent);
        dialog.Show();
    }

    public void Close() {
        dialog.Close();
    }

    public void Dispose() {
        DisposableUtils.DisposeList(disposables);

        dialog.Dispose();
        label.Dispose();
    }
}
