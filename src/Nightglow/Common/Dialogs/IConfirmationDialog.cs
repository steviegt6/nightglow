using System.Collections.Generic;

namespace Nightglow.Common.Dialogs;

public interface IConfirmationDialog {
    public void Initialize(string title, string info, IEnumerable<DialogOption> opts);
    public void Close();
}
