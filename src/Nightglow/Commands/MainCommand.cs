using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Nightglow.Common;
using Nightglow.UI;

namespace Nightglow.Commands;

/// <summary>
///     The main (default) command, which launches into the Nightglow GUI.
/// </summary>
[Command]
public sealed class MainCommand : BaseCommand {
    protected override ValueTask ExecuteAsync(IConsole console) {
        Program.Launcher = new UILauncher();
        var application = Gtk.Application.New("dev.tomat.terraprisma.nightglow", Gio.ApplicationFlags.FlagsNone);
        application.OnActivate += (sender, args) => {
            var window = new LauncherWindow((Gtk.Application)sender);
            window.Show();
            ((UILauncher)Program.Launcher).SetApplicationAndWindow(application, window);
            Launcher.Context = SynchronizationContext.Current ?? throw new Exception("Unable to get GlibMainLoop"); // This exception should never be hit
        };
        application.RunWithSynchronizationContext();
        return default;
    }
}
