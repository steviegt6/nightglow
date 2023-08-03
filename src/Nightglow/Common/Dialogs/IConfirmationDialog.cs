using System;
using System.Collections.Generic;

namespace Nightglow.Common.Dialogs;

public interface IConfirmationDialog : IDisposable {
    public void Initialize(string title, string info, IEnumerable<DialogOption> opts);
    public void Close();
}
