using System;
using System.Collections.Generic;
using System.Threading;
using Gtk;
using Nightglow.Common;
using Nightglow.Common.Dialogs;

namespace Nightglow.UI;

public class UILauncher : Launcher {
    public Gtk.Application Application = null!;
    public Window MainWindow = null!; // This could probably just be combined into one class with UILauncher...

    // This is kind of a hack. I shouldn't need to do this. Someone find a better way to do this.
    // The reason why I can't just pass it in via a constructor is because I need to construct it before creating
    // the appliction in MainCommand.ExecuteAsync because Program.Launcher is used in application. Calling
    // Application.Run within the ctor would just stop the ctor from ever finishing, meaning the static
    // Program.Launcher would never be set. This is probably a symptom of bad design!
    public void SetApplicationAndWindow(Application application, Window window) {
        Application = application;
        MainWindow = window;
    }

    public override IConfirmationDialog NewConfirmationDialog(string title, string text, IEnumerable<DialogOption> opts) {
        // This will eventually need to be modeled after NewProgressDialog
        var dialog = new UIConfirmationDialog(Application, MainWindow);
        dialog.Initialize(title, text, opts);
        return (IConfirmationDialog)dialog;
    }

    public override IProgressDialog NewProgressDialog(string title, string header, string text, IEnumerable<DialogOption> opts) {
        IProgressDialog? dialog = null;

        if (Environment.CurrentManagedThreadId != 1) {
            ExecuteInMainContext(() => {
                dialog = new UIProgressDialog(Application, MainWindow);
                dialog.Initialize(title, header, text, opts);
            });

            if (dialog == null) {
                throw new Exception();
            }
        }
        else {
            dialog = new UIProgressDialog(Application, MainWindow);
            dialog.Initialize(title, header, text, opts);
        }

        return dialog;
    }
}
