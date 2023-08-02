using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Nightglow.Common;

namespace Nightglow.UI;

public static class IconHelper {
    // Icons are stored either as the full name of an embedded resource or the name of the icon's file. This prevents name collisions as well as preventing the icons from pointing to the wrong cache or data path if an instance changes OS.
    public static string GetPath(string icon) {
        if (icon.StartsWith("Nightglow.Assets.Icons"))
            return Path.Combine(Launcher.Platform.CachePath(), icon);
        else
            return Path.Combine(Launcher.IconsPath, icon);
    }

    public static IEnumerable<string> GetAllIcons() {
        return LoadEmbeddedIcons()
            .Concat(Directory.GetFiles(Launcher.IconsPath)
                .Select(f => Path.GetFileName(f)))
            .OrderBy(i => i);
    }

    public static List<string> LoadEmbeddedIcons() {
        var ret = new List<string>();
        var assembly = Assembly.GetAssembly(typeof(IconHelper))!;
        foreach (var resource in assembly.GetManifestResourceNames()) {
            if (!resource.StartsWith("Nightglow.Assets.Icons"))
                continue;

            var path = Path.Combine(Launcher.Platform.CachePath(), resource);
            if (File.Exists(path)) {
                ret.Add(resource);
                continue;
            }

            using var resourceStream = assembly.GetManifestResourceStream(resource);
            if (resourceStream == null)
                continue;

            using var fileStream = File.OpenWrite(path);
            resourceStream.CopyTo(fileStream);

            ret.Add(resource);
        }

        return ret;
    }
}
