using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Nightglow.Common.Instances;

[RequiresPreviewFeatures]
public interface ICreateInstance {
    static abstract string DefaultIcon { get; }

    static abstract Task<Instance> Create(string name, string? icon);
}
