using System;

namespace Nightglow.Common.Dialogs;

public record struct DialogOption<T>(string Label, string? ToolTip, Action<T>? OnAccept);
