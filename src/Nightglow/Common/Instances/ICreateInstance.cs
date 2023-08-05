using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Nightglow.Common.Instances;

[RequiresPreviewFeatures]
public interface ICreateInstance {
    public static abstract string DefaultIcon { get; }
    public static abstract Task<Instance> Create(string name, string? icon);
}
