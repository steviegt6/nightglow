using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
using Nightglow.Common.Dialogs;

namespace Nightglow.Common.Instances;

[RequiresPreviewFeatures]
public class TConfigInstance : Instance, ICreateInstance {
    public override bool WindowsOnly => true;
    public override string WindowsExecutable => "tConfig.exe";
    public static string DefaultIcon => "Nightglow.Assets.Icons.tConfig.png";
    public static string NetPath => Path.Combine(Launcher.Platform.DataPath(), "wine", "drive_c", "windows", "Microsoft.NET", "Framework", "v4.0.30319");

    public TConfigInstance(string path, InstanceInfo info) : base(path, info) { }

    [RequiresPreviewFeatures]
    public static async Task<Instance> Create(string name, string? icon) {
        var path = DeterminePath(name);
        Directory.CreateDirectory(path);

        var dialog = Program.Launcher.NewProgressDialog("Creating instance " + name, "Creating instance " + name, "", new DialogOption[] { });

        var archivePath = Path.Combine(path, "tConfig.zip");
        await Downloader.Download("https://ppeb.me/nightglow/tConfig.zip", archivePath, (tB, tBR, perc) => {
            dialog.SetText($"Downloading tConfig.zip: {tBR} bytes/{tB} bytes");
            dialog.SetFraction(perc ?? 0);
            return false; // Should be some kind of CancellationToken thing
        });

        dialog.PulseWhile(100, () => { return true; });
        dialog.SetText("Extracting archive tConfig.zip");

        ZipFile.ExtractToDirectory(archivePath, path);
        File.Delete(archivePath);

        var gamePath = Path.Combine(path, "game");
        Directory.Move(Path.Combine(path, "tConfig 0.38"), gamePath);

        TConfigInstance instance = new TConfigInstance(path, new InstanceInfo(name, typeof(TConfigInstance), icon ?? DefaultIcon));
        instance.Save();

        dialog.SetText("Configuring instance");
        await Launcher.Platform.ConfigureInstance(instance);

        dialog.SetText("Patching assembly tConfig.exe");
        ModifyAssembly(path);
        dialog.Close();

        return instance;
    }

    private static void ModifyAssembly(string path) {
        var asmPath = Path.Combine(path, "game", "tConfig.exe");
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.Combine(path, "game"));
        resolver.AddSearchDirectory(Path.Combine(path, NetPath));
        using var md = ModuleDefinition.ReadModule(asmPath, new ReaderParameters { AssemblyResolver = resolver });
        var main = md.GetType("Terraria.Main");
        var mainCctor = main.GetStaticConstructor();
        var il = new ILContext(mainCctor);
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.Before,
            i => i.MatchLdcI4(5),
            i => i.MatchNewarr<object>(),
            i => i.MatchStloc(1),
            i => i.MatchLdloc(1),
            i => i.MatchLdcI4(0)
        ))
            throw new Exception("Unable to patch tConfig.exe");
        else {
            c.RemoveRange(28);
            Launcher.Platform.DataPathIL(md, c, Path.Combine(NetPath, "mscorlib.dll"));
        }

        // tConfig makes its paths by string.join-ing an array, just set the tConfig part empty so it doesn't make any subdirectories
        for (int i = 0; i < 2; i++) {
            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdstr("tConfig")
            ))
                throw new Exception("Unable to patch tConfig.exe");
            else {
                c.Remove();
                c.Emit(OpCodes.Ldstr, "");
            }
        }

        File.Move(asmPath, asmPath + ".bak");
        md.Write(asmPath);
    }
}
