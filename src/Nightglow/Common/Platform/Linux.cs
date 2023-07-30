using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
using Nightglow.Common.Dialogs;
using Nightglow.Common.Instances;

namespace Nightglow.Common.Platform;

public class Linux : IPlatform {
    private string winePath => Path.Combine(DataPath(), "wine");

    public string DataPath() {
        var xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

        if (!string.IsNullOrEmpty(xdg))
            return Path.Combine(xdg, "nightglow");
        else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local", "share", "nightglow");
    }

    public void DataPathIL(ModuleDefinition md, ILCursor c, string mscorlibPath) {
        var mscorlib = ModuleDefinition.ReadModule(mscorlibPath);
        c.Emit(OpCodes.Call, md.ImportReference(mscorlib.GetTypes().First(t => t.FullName == "System.Environment").GetMethods().First(m => m.Name == "get_CurrentDirectory")));
        c.Emit(OpCodes.Call, md.ImportReference(mscorlib.GetTypes().First(t => t.FullName == "System.IO.Directory").GetMethods().First(m => m.Name == "GetParent")));
        c.Emit(OpCodes.Callvirt, md.ImportReference(mscorlib.GetTypes().First(t => t.FullName == "System.IO.FileSystemInfo").GetMethods().First(m => m.Name == "get_FullName")));
    }

    public void OpenUrl(string url) {
        Process.Start("xdg-open", url);
    }

    public void OpenPath(string path) {
        Process.Start("xdg-open", path);
    }

    private void SetWineEnv(string path) {
        Environment.SetEnvironmentVariable("WINEPREFIX", winePath);
        Environment.SetEnvironmentVariable("WINEARCH", "win64");
    }

    public async Task ConfigureInstance(IProgressDialog dialog, Instance instance) {
        Console.WriteLine("Configure instance: " + Thread.CurrentThread.ManagedThreadId);
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

            /* var cancelOpt = new DialogOption("Cancel", "Kills the winetricks installer, removing leftover files", (sender) => { */
            /*     sender.SetHeader("Undoing winetricks setup"); */
            /*     sender.SetText("Killing winetricks"); */
            /*     if (!proc.HasExited) */
            /*         proc.Kill(true); */

            /*     sender.SetText("Deleting unfinished wine installation at " + winePath); */
            /*     Directory.Delete(winePath, true); */

            /*     sender.Close(); */
            /* }); */


            void DataRecieved(object sender, DataReceivedEventArgs args) {
                Console.WriteLine("data recieved: " + Thread.CurrentThread.ManagedThreadId);
                if (!string.IsNullOrEmpty(args.Data))
                    dialog.SetText(args.Data);
            }

            proc.OutputDataReceived += DataRecieved;
            proc.ErrorDataReceived += DataRecieved;

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            dialog.PulseWhile(100, () => {
                Console.WriteLine("dialog pulsewhile: " + Thread.CurrentThread.ManagedThreadId);
                return !proc.HasExited;
            });

            await proc.WaitForExitAsync();
            dialog.Close();
        }
        // is there anything else to even configure?
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
}
