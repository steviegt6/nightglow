using System;
using System.Collections.Generic;

namespace Nightglow.Common;

public static class DisposableUtils {
    public static void DisposeList(List<IDisposable> disposables) {
        foreach (var disposable in disposables)
            disposable?.Dispose();
    }
}
