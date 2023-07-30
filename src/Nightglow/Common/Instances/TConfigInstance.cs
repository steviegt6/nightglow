using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
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
    public static string NetPath => Path.Combine(Launcher.Platform.DataPath(), "wine", "drive_c", "windows", "Microsoft.NET", "Framework", "v4.0.30319");

    public TConfigInstance(string path, InstanceInfo info) : base(path, info) { }

    [RequiresPreviewFeatures]
    public static async Task<Instance> Create(IProgressDialog dialog, string name) {
        var path = DeterminePath(name);
        Directory.CreateDirectory(path);

        var archivePath = Path.Combine(path, "tConfig.zip");
        var dlTask = Download(archivePath);
        dlTask.Wait();
        ZipFile.ExtractToDirectory(archivePath, path);
        File.Delete(archivePath);

        var gamePath = Path.Combine(path, "game");
        Directory.Move(Path.Combine(path, "tConfig 0.38"), gamePath);

        TConfigInstance instance = new TConfigInstance(path, new InstanceInfo(name, typeof(TConfigInstance)));
        instance.Save();

        Console.WriteLine("Create: " + Thread.CurrentThread.ManagedThreadId);

        await Launcher.Platform.ConfigureInstance(dialog, instance);

        ModifyAssembly(path);

        return instance;
    }

    private static async Task Download(string archivePath) {
        var client = new HttpClient();
        var resultStream = await client.GetAsync("https://ppeb.me/nightglow/tConfig.zip").Result.Content.ReadAsStreamAsync();
        using var fileStream = File.Create(archivePath);
        resultStream.CopyTo(fileStream);
    }

    private static void ModifyAssembly(string path) {
        var asmPath = Path.Combine(path, "game", "tConfig.exe");
        var resolver = new DefaultAssemblyResolver();
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
            Launcher.Platform.DataPathIL(md, c, Path.Combine(path, Path.Combine(path, NetPath, "mscorlib.dll")));
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

        // md.Write errors if you don't write it to a temporary path first
        var tempAsm = Path.Combine(path, "tConfig.exe-1");
        md.Write(tempAsm);
        File.Delete(asmPath);
        File.Move(tempAsm, asmPath);
    }
}
