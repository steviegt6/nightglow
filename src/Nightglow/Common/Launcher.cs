using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nightglow.Common.Dialogs;
using Nightglow.Common.Instances;
using Nightglow.Common.Platform;
using static Nightglow.Common.Instances.Instance;

namespace Nightglow.Common;

public abstract class Launcher {
    public static IPlatform Platform { get; }
    public static string InstancesPath => Path.Combine(Platform.DataPath(), "instances");
    public static string IconsPath => Path.Combine(Platform.DataPath(), "icons");
    public List<Instance> Instances { get; set; }
    public static SynchronizationContext Context = null!; // If this wasn't set in MainCommand it already threw

    static Launcher() {
        Platform = GetPlatform();
    }

    public Launcher() {
        Directory.CreateDirectory(Platform.DataPath());
        Directory.CreateDirectory(InstancesPath);

        Instances = new List<Instance>();

        foreach (string directory in Directory.GetDirectories(InstancesPath)) {
            var infoFile = Path.Combine(directory, "nightglow.json");

            if (!File.Exists(infoFile))
                /* throw new Exception($"Instance {directory} does not contain nightglow.json"); */
                ; // TODO: Some kind of actual logging for broken instances
            else {
                var info = JsonConvert.DeserializeObject<InstanceInfo>(File.ReadAllText(infoFile));
                if (info == null)
                    continue;

                var instance = Activator.CreateInstance(info!.Type, new object[] { directory, (object)info });
                if (instance == null)
                    continue;
                Instances.Add((Instance)instance);
            }
        }
    }

    /// <summary>
    /// Executes a delegate in the main GTK context.
    /// Blocks until the action has completed.
    /// </summary>
    /// <param name="action">the delegate to be executed</param>
    public static void ExecuteInMainContext(Action action) {
        using ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

        if (Context != null)
            Context.Post(_ => {
                action();
                resetEvent.Set();
            }, null);
        else
            Task.Factory.StartNew(() => {
                action();
                resetEvent.Set();
            });

        resetEvent.Wait();
    }

    /// <summary>
    /// Executes a delegate in the main GTK context.
    /// Does not block.
    /// </summary>
    /// <param name="action">the delegate to be executed</param>
    public static void ExecuteInMainContextNonBlocking(Action action) {
        if (Context != null)
            Context.Post(_ => action(), null);
        else
            Task.Factory.StartNew(action);
    }

    public abstract IConfirmationDialog NewConfirmationDialog(string title, string text, IEnumerable<DialogOption> opts);

    public abstract IProgressDialog NewProgressDialog(string title, string header, string text, IEnumerable<DialogOption> opts);

    private static IPlatform GetPlatform() {
        if (OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Unknown or unsupported platform!");
        else if (OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Unknown or unsupported platform!");
        else if (OperatingSystem.IsLinux())
            return new Linux();

        throw new PlatformNotSupportedException("Unknown or unsupported platform!");
    }
}
