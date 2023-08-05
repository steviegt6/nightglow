using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using Nightglow.Common;
using Nightglow.Common.Dialogs;

namespace Nightglow.UI;

public class UIProgressDialog : IProgressDialog {
    private List<IDisposable> disposables;

    private readonly Application _application;
    private readonly Window _parent;
    private Window dialog = null!;
    private Label headerLabel = null!;
    private Label label = null!;
    private ProgressBar? bar;

    public UIProgressDialog(Application application, Window parent) { // This is stupid
        disposables = new List<IDisposable>();
        _application = application;
        _parent = parent;
    }

    public void Initialize(string title, string header, string text, params DialogOption<IProgressDialog>[] opts) {
        dialog = new Window { Title = title, Application = _application, Modal = true };

        var rootBox = new Box { Name = "ProgressDialog rootBox" };
        disposables.Add(rootBox);
        rootBox.SetOrientation(Orientation.Vertical);
        dialog.SetChild(rootBox);

        headerLabel = new Label { Name = "ProgressDialog header" };
        headerLabel.SetText(header);
        rootBox.Append(headerLabel);

        bar = new ProgressBar { Name = "ProgressDialog bar" };
        rootBox.Append(bar);

        label = new Label { Name = "ProgressDialog label" };
        label.SetText(text);
        label.SetVexpand(true);
        rootBox.Append(label);

        var footerBox = new Box { Name = "ProgressDialog footerBox" };
        disposables.Add(footerBox);
        footerBox.SetOrientation(Orientation.Horizontal);
        footerBox.SetHalign(Align.End);
        rootBox.Append(footerBox);

        foreach (DialogOption<IProgressDialog> opt in opts) {
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

    public void SetFraction(double fraction) {
        bar?.SetFraction(fraction);
    }

    public void SetHeader(string header) {
        headerLabel.SetText(header);
    }

    public string GetHeader() {
        return headerLabel.GetText();
    }

    public void SetText(string text) {
        label.SetText(text);
    }

    public string GetText() {
        return label.GetText();
    }

    public void Pulse() {
        bar?.Pulse();
    }

    public void SetPulseStep(double fraction) {
        bar?.SetPulseStep(fraction);
    }

    public void PulseWhile(int ms, Func<bool> condition) {
        // TODO: Implement cancellation token or something so that this will stop running when PulseWhile is called again or the class is closed or disposed
        Task.Run(() => {
            while (condition() && bar != null) {
                bar?.Pulse();
                Thread.Sleep(ms);
            }
        });
    }

    public void Close() {
        dialog.Close();
    }

    public void Dispose() {
        Close();

        DisposableUtils.DisposeList(disposables);

        dialog.Dispose();
        headerLabel.Dispose();
        label.Dispose();
        bar?.Dispose();
        bar = null; // Prevent the task from accessing it again
    }
}
