using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
using Nightglow.Common.Instances;

namespace Nightglow.Common.Platform;

public class Linux : IPlatform {
    private string winePath => Path.Combine(DataPath(), "wine");

    private void SetWineEnv(string path) {
        Environment.SetEnvironmentVariable("WINEPREFIX", winePath);
        Environment.SetEnvironmentVariable("WINEARCH", "win64");
    }

    public async Task ConfigureInstance(Instance instance) {
        if (instance.WindowsOnly) {
            if (Directory.Exists(winePath))
                return;

            SetWineEnv(Path.Combine(instance.InstancePath, "wine"));

            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "winetricks",
                    Arguments = "-q --force xna40", // hardcoded for now, but a given instance could provide it's own needed libs
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            proc.OutputDataReceived += (sender, args) => { Console.WriteLine(args.Data); };
            proc.ErrorDataReceived += (sender, args) => { Console.WriteLine("err: " + args.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            // Do some kind of interface for providing the output and result of this command!
            /* while (!proc.StandardOutput.EndOfStream) { */
            /*     Console.WriteLine(await proc.StandardOutput.ReadLineAsync()); */
            /* } */

            await proc.WaitForExitAsync();
            Console.WriteLine("Configured instance");
        }
        // is there anything else to even configure?
    }

    public string DataPath() {
        var xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

        if (!string.IsNullOrEmpty(xdg))
            return Path.Combine(xdg, "nightglow");
        else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local", "share", "nightglow");
    }

    public Process Launch(Instance instance) {
        SetWineEnv(Path.Combine(instance.InstancePath, "wine"));

        var proc = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = instance.WindowsOnly ? "wine" : instance.LinuxExecutable,
                Arguments = instance.WindowsOnly ? instance.WindowsExecutable : "",
                WorkingDirectory = Path.Combine(instance.InstancePath, "game"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        proc.OutputDataReceived += (sender, args) => { Console.WriteLine(args.Data); };
        proc.ErrorDataReceived += (sender, args) => { Console.WriteLine("err: " + args.Data); };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        return proc;
    }

    public string MakeWinePath(string path) {
        return Path.Combine("Z:", path);
    }

    public void OpenUrl(string url) {
        Process.Start("xdg-open", url);
    }

    public void DataPathIL(ModuleDefinition md, ILCursor c, string mscorlibPath) {
        var mscorlib = ModuleDefinition.ReadModule(mscorlibPath);
        c.Emit(OpCodes.Call, md.ImportReference(mscorlib.GetTypes().First(t => t.FullName == "System.Environment").GetMethods().First(m => m.Name == "get_CurrentDirectory")));
        c.Emit(OpCodes.Call, md.ImportReference(mscorlib.GetTypes().First(t => t.FullName == "System.IO.Directory").GetMethods().First(m => m.Name == "GetParent")));
        c.Emit(OpCodes.Callvirt, md.ImportReference(mscorlib.GetTypes().First(t => t.FullName == "System.IO.FileSystemInfo").GetMethods().First(m => m.Name == "get_FullName")));
    }
}
