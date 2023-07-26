using System.Runtime.Versioning;

namespace Nightglow.Common.Instances;

[RequiresPreviewFeatures]
public interface ICreateInstance {
    public static abstract string NetPath { get; }
    public static abstract Instance Create(string name);
}
