using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Mono.Cecil;
using MonoMod.Cil;
using Nightglow.Common.Instances;

namespace Nightglow.Common.Platform;

public class Windows : IPlatform {
    public string NetFxPath => @"C:\Windows\Microsoft.NET\Framework\v4.0.30319";

    public string NetCorePath => throw new NotImplementedException(".NET Core path resolution not yet implemented on Linux.");

    public string CachePath() {
        return Path.Combine(DataPath(), "cache");
    }

    public string DataPath() {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nightglow");
    }

    public void DataPathIL(ModuleDefinition md, ILCursor c, string mscorlibPath) {
        new Linux().DataPathIL(md, c, mscorlibPath);
    }

    public void OpenUrl(string url) {
        Process.Start(new ProcessStartInfo(url) {
            UseShellExecute = true,
        });
    }

    public void OpenPath(string path) {
        Process.Start("explorer.exe", path);
    }

    public Task ConfigureInstance(Instance instance) {
        return Task.CompletedTask;
    }

    public Process Launch(Instance instance) {
        var proc = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = Path.Combine(Path.Combine(instance.InstancePath, "game"), instance.WindowsExecutable),
                Arguments = "",
                WorkingDirectory = Path.Combine(instance.InstancePath, "game"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };

        proc.OutputDataReceived += (_, args) => {
            Console.WriteLine(args.Data);
        };

        proc.ErrorDataReceived += (_, args) => {
            Console.WriteLine(args.Data);
        };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        return proc;
    }
}
