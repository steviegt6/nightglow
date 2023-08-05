using Nightglow.Common;
using Nightglow.Common.Dialogs;

namespace Nightglow.CLI; 

public class CliLauncher : Launcher {
    public override IConfirmationDialog NewConfirmationDialog(string title, string text, params DialogOption<IConfirmationDialog>[] opts) {
        throw new System.NotImplementedException();
    }

    public override IProgressDialog NewProgressDialog(string title, string header, string text, params DialogOption<IProgressDialog>[] opts) {
        throw new System.NotImplementedException();
    }
}
