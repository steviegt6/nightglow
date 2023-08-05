using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Nightglow.CLI;

namespace Nightglow.Commands;

[Command("run")]
public class RunCommand : BaseCommand {
    [CommandParameter(0, Description = "The name of the instance to run.")]
    public string? InstanceName { get; set; }

    protected override ValueTask ExecuteAsync(IConsole console) {
        if (InstanceName is null)
            throw new InvalidOperationException("Cannot run an instance without a name.");

        var launcher = Program.Launcher = new CliLauncher();
        var instance = launcher.Instances.FirstOrDefault(x => x.Info.Name == InstanceName);
        if (instance is null)
            throw new InvalidOperationException($"Instance {InstanceName} does not exist.");
        
        instance.Launch();
        return default;
    }
}
