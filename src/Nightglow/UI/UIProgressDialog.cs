using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using Nightglow.Common.Dialogs;

namespace Nightglow.UI;

public class UIProgressDialog : IProgressDialog {
    private readonly Application _application;
    private readonly Window _parent;
    private Window dialog = null!;
    private Label headerLabel = null!;
    private Label label = null!;
    private ProgressBar bar = null!;

    public UIProgressDialog(Application application, Window parent) { // This is stupid
        _application = application;
        _parent = parent;
    }

    public void Initialize(string title, string header, string text, IEnumerable<DialogOption> opts) {
        dialog = new Window { Title = title, Application = _application, Modal = true };

        var rootBox = new Box { Name = "ProgressDialog rootBox" };
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
        footerBox.SetOrientation(Orientation.Horizontal);
        footerBox.SetHalign(Align.End);
        rootBox.Append(footerBox);

        foreach (DialogOption opt in opts) {
            var button = new Button { Label = opt.Label };
            button.OnClicked += (_, _) => opt.OnAccept(this);
            footerBox.Append(button);
        }

        dialog.SetDefaultSize(300, 100);
        dialog.SetTransientFor(_parent);
        dialog.Show();
    }

    public void SetFraction(double fraction) {
        bar.SetFraction(fraction);
    }

    public void SetHeader(string header) {
        headerLabel.SetText(header);
    }

    public string GetHeader() {
        return headerLabel.GetText();
    }

    public void SetText(string text) {
        lock (this) {
            label.SetText(text);
        }
    }

    public string GetText() {
        return label.GetText();
    }

    public void Pulse() {
        lock (this) {
            bar.Pulse();
        }
    }

    public void SetPulseStep(double fraction) {
        bar.SetPulseStep(fraction);
    }

    public void PulseWhile(int ms, Func<bool> condition) {
        Console.WriteLine("creating pulsewhile: " + Thread.CurrentThread.ManagedThreadId);
        Task.Run(() => {
            while (condition()) {
                bar?.Pulse();
                Thread.Sleep(ms);
            }
        });
    }

    public void Close() {
        dialog.Close();
    }
}
