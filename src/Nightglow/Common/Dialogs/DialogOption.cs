using System;

namespace Nightglow.Common.Dialogs;

public record struct DialogOption(string Label, string ToolTip, Action<IProgressDialog> OnAccept);
