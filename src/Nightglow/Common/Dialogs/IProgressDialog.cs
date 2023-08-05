using System;

namespace Nightglow.Common.Dialogs;

public interface IProgressDialog : IDisposable {
    public void Initialize(string title, string header, string text, params DialogOption<IProgressDialog>[] opts);
    public void SetFraction(double fraction);
    public void SetHeader(string header);
    public string GetHeader();
    public void SetText(string text);
    public string GetText();
    public void Pulse();
    public void SetPulseStep(double fraction);
    /// <summary>
    ///     Pulses the progress bar every (<paramref name="ms"/>) while (<paramref name="condition"/>) returns true
    /// </summary>
    public void PulseWhile(int ms, Func<bool> condition);
    public void Close();
}
