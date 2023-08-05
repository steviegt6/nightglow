using System.Diagnostics;
using System.Threading.Tasks;
using Mono.Cecil;
using MonoMod.Cil;
using Nightglow.Common.Instances;

namespace Nightglow.Common.Platform;

public interface IPlatform {
    string NetFxPath { get; }

    string NetCorePath { get; }

    string CachePath();

    string DataPath();

    string SteamPath();

    void DataPathIL(ModuleDefinition md, ILCursor c, string mscorlibPath);

    void OpenUrl(string url);

    void OpenPath(string path);

    Task ConfigureInstance(Instance instance);

    Process Launch(Instance instance);
}
