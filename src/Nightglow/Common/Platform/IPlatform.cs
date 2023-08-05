using System.Diagnostics;
using System.Threading.Tasks;
using Mono.Cecil;
using MonoMod.Cil;
using Nightglow.Common.Instances;

namespace Nightglow.Common.Platform;

public interface IPlatform {
    string NetFxPath { get; }

    string NetCorePath { get; }

    public string CachePath();
    public string DataPath();
    public void DataPathIL(ModuleDefinition md, ILCursor c, string mscorlibPath);
    public void OpenUrl(string url);
    public void OpenPath(string path);
    public Task ConfigureInstance(Instance instance);
    public Process Launch(Instance instance);
}
