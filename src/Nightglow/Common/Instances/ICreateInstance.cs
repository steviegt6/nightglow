using System.Runtime.Versioning;
using System.Threading.Tasks;
using Nightglow.Common.Dialogs;

namespace Nightglow.Common.Instances;

[RequiresPreviewFeatures]
public interface ICreateInstance {
    public static abstract string NetPath { get; }
    public static abstract Task<Instance> Create(IProgressDialog dialog, string name);
}
