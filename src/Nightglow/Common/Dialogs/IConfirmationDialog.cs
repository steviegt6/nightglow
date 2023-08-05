using System;

namespace Nightglow.Common.Dialogs;

public interface IConfirmationDialog : IDisposable {
    public void Initialize(string title, string info, params DialogOption<IConfirmationDialog>[] opts);
    public void Close();
}
