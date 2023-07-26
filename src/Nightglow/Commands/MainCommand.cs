using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Nightglow.Commands;

/// <summary>
///     The main (default) command, which launches into the Nightglow GUI.
/// </summary>
[Command]
public sealed class MainCommand : BaseCommand {
    protected override ValueTask ExecuteAsync(IConsole console) {
        var application = Gtk.Application.New("dev.tomat.terraprisma.nightglow", Gio.ApplicationFlags.FlagsNone);
        application.OnActivate += (sender, args) => {
            var window = new UI.LauncherWindow(sender);
            window.Show();
        };
        application.Run();
        return default;
    }
}
