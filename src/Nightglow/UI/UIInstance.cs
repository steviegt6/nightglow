using System;
using GObject;
using Nightglow.Common.Instances;

namespace Nightglow.UI;

public class UIInstance : GObject.Object {
    public readonly Instance Instance;
    public UIInstance(Instance instance) : base(true, Array.Empty<ConstructArgument>()) {
        Instance = instance;
    }
}
