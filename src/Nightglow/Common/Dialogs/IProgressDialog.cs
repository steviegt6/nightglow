using System;

namespace Nightglow.Common.Dialogs;

public interface IProgressDialog : IDisposable {
    void Initialize(string title, string header, string text, params DialogOption<IProgressDialog>[] opts);

    void SetFraction(double fraction);

    void SetHeader(string header);

    string GetHeader();

    void SetText(string text);

    string GetText();

    void Pulse();

    void SetPulseStep(double fraction);

    /// <summary>
    ///     Pulses the progress bar every (<paramref name="ms"/>) while (<paramref name="condition"/>) returns true
    /// </summary>
    void PulseWhile(int ms, Func<bool> condition);

    void Close();
}
