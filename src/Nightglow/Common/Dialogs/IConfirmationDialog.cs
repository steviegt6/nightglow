using System;

namespace Nightglow.Common.Dialogs;

public interface IConfirmationDialog : IDisposable {
    void Initialize(string title, string info, params DialogOption<IConfirmationDialog>[] opts);

    void Close();
}
