using HomeLabManager.Common.Data.Git.Server;
using Material.Icons;

namespace HomeLabManager.Common.Utils;

/// <summary>
/// Simple utility class for mapping server kinds to icons and labels.
/// </summary>
public static class ServerKindMapper
{
    /// <summary>
    /// Map a server kind to its corresponding material icon.
    /// </summary>
    public static MaterialIconKind? GetIconKind(ServerKind? kind) => kind switch
    { 
        ServerKind.Unspecified => MaterialIconKind.QuestionMark,
        ServerKind.Windows => MaterialIconKind.DesktopWindows,
        ServerKind.StandardLinux => MaterialIconKind.Linux,
        ServerKind.TrueNasScale => MaterialIconKind.Nas,
        null => MaterialIconKind.QuestionMark,
        _ => null,
    };

    /// <summary>
    /// Map a server kind to its standard label.
    /// </summary>
    public static string GetLabel(ServerKind? kind) => kind switch
    {
        ServerKind.Unspecified => "Unspecified",
        ServerKind.Windows => "Windows",
        ServerKind.StandardLinux => "Generic Linux",
        ServerKind.TrueNasScale => "TrueNAS Scale",
        null => "Unspecified",
        _ => null,
    };
}
