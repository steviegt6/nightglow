﻿using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
using MonoMod.Utils;
using Nightglow.Common.Dialogs;

namespace Nightglow.Common.Instances;

[RequiresPreviewFeatures]
public class TConfigInstance : Instance, ICreateInstance {
    public override bool WindowsOnly => true;
    public override string WindowsExecutable => "tConfig.exe";
    public static string DefaultIcon => "Nightglow.Assets.Icons.tConfig.png";

    public TConfigInstance(string path, InstanceInfo info) : base(path, info) { }

    [RequiresPreviewFeatures]
    public static async Task<Instance> Create(string name, string? icon) {
        var path = DeterminePath(name);
        Directory.CreateDirectory(path);

        using var dialog = Program.Launcher.NewProgressDialog("Creating instance " + name, "Creating instance " + name, "", new DialogOption<IProgressDialog>("Cancel", null, null)); // TODO: Actually cancel lol

        var archivePath = Path.Combine(path, "tConfig.zip");
        await Downloader.Download("https://ppeb.me/nightglow/tConfig.zip", archivePath, (tB, tBR, perc) => {
            dialog.SetText($"Downloading tConfig.zip: {tBR} bytes/{tB} bytes");
            dialog.SetFraction(perc ?? 0);
            return false; // TODO: Should be some kind of CancellationToken thing
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

        return instance;
    }

    private static void ModifyAssembly(string path) {
        var asmPath = Path.Combine(path, "game", "tConfig.exe");
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.Combine(path, "game"));
        resolver.AddSearchDirectory(Path.Combine(path, Launcher.Platform.NetFxPath));
        var md = ModuleDefinition.ReadModule(asmPath, new ReaderParameters { AssemblyResolver = resolver });
        var main = md.GetType("Terraria.Main");
        var mainCctor = main.GetStaticConstructor();
        var il = new ILContext(mainCctor);
        var c = new ILCursor(il);

        c.GotoNext(MoveType.Before, x => x.MatchStsfld(main.FindField("SavePath")!));
        c.Emit(OpCodes.Pop);
        Launcher.Platform.DataPathIL(md, c, Path.Combine(Launcher.Platform.NetFxPath, "mscorlib.dll"));

        // tConfig makes its paths by string.join-ing an array, just set the tConfig part empty so it doesn't make any subdirectories
        for (int i = 0; i < 2; i++) {
            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdstr("tConfig")
            ))
                throw new Exception("Unable to patch tConfig.exe");
            else {
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldstr, "");
            }
        }

        var steam = md.GetType("Terraria.Steam");
        var kill = steam.FindMethod("Kill")!;
        kill.Body.Instructions.Clear();
        kill.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        var ms = new MemoryStream();
        md.Write(ms);
        md.Dispose();

        File.Move(asmPath, asmPath + ".bak");

        using var fs = new FileStream(asmPath, FileMode.Create);
        ms.Position = 0;
        ms.CopyTo(fs);
    }
}
