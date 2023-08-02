using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Nightglow.Common.Instances;

[RequiresPreviewFeatures]
public class TAPIInstance : Instance, ICreateInstance {
    public TAPIInstance(string path, InstanceInfo info) : base(path, info) { }

    public static string DefaultIcon => "Nightglow.Assets.Icons.tAPI.png";

    public static string NetPath => throw new System.NotImplementedException();

    public override bool WindowsOnly => throw new System.NotImplementedException();

    public override string WindowsExecutable => throw new System.NotImplementedException();

    [RequiresPreviewFeatures]
    public static async Task<Instance> Create(string name, string? icon) {
        throw new System.NotImplementedException();
    }

}
