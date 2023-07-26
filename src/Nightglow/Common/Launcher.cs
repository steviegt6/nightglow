using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Nightglow.Common.Instances;
using Nightglow.Common.Platform;
using static Nightglow.Common.Instances.Instance;

namespace Nightglow.Common;

public class Launcher {
    public static IPlatform Platform { get; }
    public static string InstancesPath => Path.Combine(Platform.DataPath(), "instances");
    public List<Instance> Instances { get; set; }

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
                throw new Exception($"Instance {directory} does not contain nightglow.json");
            else {
                var info = JsonConvert.DeserializeObject<InstanceInfo>(File.ReadAllText(infoFile));
                Instances.Add((Instance)info!.Type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(string), typeof(InstanceInfo) })!.Invoke(new object[] { directory, (object)info }));
            }
        }
    }

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
