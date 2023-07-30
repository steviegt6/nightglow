using System.Runtime.Versioning;
using System.Threading.Tasks;
using Nightglow.Common.Dialogs;

namespace Nightglow.Common.Instances;

[RequiresPreviewFeatures]
public class TAPIInstance : Instance, ICreateInstance {
    public TAPIInstance(string path, InstanceInfo info) : base(path, info) { }

    public static string NetPath => throw new System.NotImplementedException();

    public override bool WindowsOnly => throw new System.NotImplementedException();

    public override string WindowsExecutable => throw new System.NotImplementedException();

    [RequiresPreviewFeatures]
    public static async Task<Instance> Create(string name) {
        throw new System.NotImplementedException();
    }

}
