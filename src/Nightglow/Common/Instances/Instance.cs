using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using VDFParser;
using VDFParser.Models;

namespace Nightglow.Common.Instances;

public abstract class Instance : IDisposable {
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

    public Process? Process { get; set; }

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

    public void AddToSteam() {
        var steamPath = Launcher.Platform.SteamPath();

        if (!Directory.Exists(steamPath)) {
            // TODO: handle gracefully
            throw new Exception("Steam not found");
        }

        var userdata = new DirectoryInfo(Path.Combine(steamPath, "userdata"));
        var user = userdata.GetDirectories()[0]; // TODO: support multiple users
        var shortcutsPath = Path.Combine(user.FullName, "config", "shortcuts.vdf");
        var shortcuts = VDFParser.VDFParser.Parse(shortcutsPath).ToList();
        shortcuts.Add(new VDFEntry {
            AllowDesktopConfig = 1,
            AllowOverlay = 1,
            AppName = Info.Name,
            Devkit = 0,
            DevkitGameID = "",
            DevkitOverrideAppID = 0,
            Exe = Environment.ProcessPath ?? throw new Exception("Process path not found"),
            FlatpakAppID = "",
            Icon = IconUtils.GetPath(Info.Icon),
            Index = shortcuts.Count,
            IsHidden = 0,
            LastPlayTime = 0,
            LaunchOptions = $"run \"{Info.Name}\"",
            OpenVR = 0,
            ShortcutPath = "",
            StartDir = Environment.CurrentDirectory,
            Tags = Array.Empty<string>(),
            appid = new Random().Next(int.MinValue, -1),
        });

        File.WriteAllBytes(shortcutsPath, VDFSerializer.Serialize(shortcuts.ToArray()));
    }

    public void Dispose() {
        Process?.Dispose();
    }
}
