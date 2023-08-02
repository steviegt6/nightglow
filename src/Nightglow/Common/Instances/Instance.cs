using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Nightglow.Common.Instances;

public abstract class Instance {
    public class InstanceInfo {
        public string Name { get; set; }
        public Type Type { get; set; }
        public string Icon { get; set; }

        public InstanceInfo(string name, Type type, string icon) {
            Name = name;
            Type = type;
            Icon = icon;
        }
    }

    public string InstancePath { get; set; }
    private string jsonPath => Path.Combine(InstancePath, "nightglow.json");
    public abstract bool WindowsOnly { get; }
    // There is definitely a better way to do this
    public abstract string WindowsExecutable { get; }
    public virtual string MacExecutable => null!;
    public virtual string LinuxExecutable => null!;
    public Process? Process { get; private set; }

    public InstanceInfo Info { get; set; }

    public Instance(string path, InstanceInfo info) {
        InstancePath = path;
        Info = info;
    }

    public void Save() {
        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(Info));
    }

    protected static string DeterminePath(string name) {
        var path = Path.Combine(Launcher.InstancesPath, string.Concat(name.Split(Path.GetInvalidPathChars())));

        if (Directory.Exists(path)) {
            for (int i = 1; ; i++) {
                var discriminated = path + '-' + i;
                if (!Directory.Exists(discriminated))
                    return discriminated;
            }
        }
        else {
            return path;
        }
    }

    public void Launch() {
        Process = Launcher.Platform.Launch(this);
    }
}
