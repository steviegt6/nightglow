using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nightglow.Common;

public static class IconUtils {
    // Icons are stored either as the full name of an embedded resource or the name of the icon's file. This prevents name collisions as well as preventing the icons from pointing to the wrong cache or data path if an instance changes OS.
    public static string GetPath(string icon) {
        if (IsEmbedded(icon))
            return Path.Combine(Launcher.Platform.CachePath(), icon);
        else
            return Path.Combine(Launcher.IconsPath, icon);
    }

    public static bool IsEmbedded(string icon) {
        return icon.StartsWith("Nightglow.Assets.Icons");
    }

    public static string GetFakeName(string icon) {
        return IsEmbedded(icon) ? icon.Replace("Nightglow.Assets.Icons.", "") : icon;
    }

    public static IEnumerable<string> GetAllIcons() {
        return LoadEmbeddedIcons()
            .Concat(Directory.GetFiles(Launcher.IconsPath)
                .Select(f => Path.GetFileName(f)))
            .OrderByFakeName();
    }

    public static IEnumerable<string> OrderByFakeName(this IEnumerable<string> icons) {
        return icons.OrderBy(i => GetFakeName(i));
    }

    public static List<string> LoadEmbeddedIcons() {
        var ret = new List<string>();
        var assembly = Assembly.GetAssembly(typeof(IconUtils))!;
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
