using System.Collections.Generic;
using Gtk;
using Nightglow.Common.Dialogs;

namespace Nightglow.UI;

public class UIConfirmationDialog : IConfirmationDialog {
    public UIConfirmationDialog(Application application, Window parent) {

    }

    public void Initialize(string title, string tooltip, IEnumerable<DialogOption> opts) {
    }

    public void Close() {
        throw new System.NotImplementedException();
    }

    public void Dispose() {
        throw new System.NotImplementedException();
    }
}
