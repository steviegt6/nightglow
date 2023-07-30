using System.Threading.Tasks;
using CliFx;
using Nightglow.Common;

namespace Nightglow;

internal static class Program {
    public static Launcher Launcher = null!; // Needs to be set at the beginning of the command based on the UI type

    internal static async Task<int> Main(string[] args) {
        return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
    }
}
