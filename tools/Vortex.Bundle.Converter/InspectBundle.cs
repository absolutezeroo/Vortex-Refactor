using System.IO;

using Vortex.Bundle.Data;
using Vortex.Bundle.IO;

namespace Vortex.Bundle.Converter;

public static class BundleInspector
{
    public static int Inspect(string path)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"File not found: {path}");
            return 1;
        }

        using FileStream stream = File.OpenRead(path);
        VortexBundleReader reader = new();
        VortexBundleData data = reader.Read(stream);

        Console.WriteLine($"File: {Path.GetFileName(path)} ({stream.Length:N0} bytes)");
        Console.WriteLine($"Version: {data.Version}, Flags: 0x{data.Flags:X4}");
        Console.WriteLine($"Strings: {data.StringTable?.Count ?? 0}");
        Console.WriteLine($"Assets:  {data.Assets?.Length ?? 0}");
        Console.WriteLine($"Aliases: {data.Aliases?.Length ?? 0}");
        Console.WriteLine($"Frames:  {data.SpritesheetMeta?.Frames.Length ?? 0}");
        Console.WriteLine();

        if (data is { Assets.Length: > 0, StringTable: not null })
        {
            int withOffset = data.Assets.Count(a => a.OffsetX != 0 || a.OffsetY != 0);
            Console.WriteLine($"Assets with non-zero offset: {withOffset}/{data.Assets.Length}");
            Console.WriteLine();

            Console.WriteLine("First 15 assets:");
            foreach (AssetEntry a in data.Assets.Take(15))
            {
                string name = data.StringTable.Resolve(a.NameIndex) ?? "?";
                Console.WriteLine($"  {name,-40} offset=({a.OffsetX,4},{a.OffsetY,4}) flags=0x{a.Flags:X2}");
            }
        }

        if (data is { Aliases.Length: > 0, StringTable: not null })
        {
            Console.WriteLine();
            Console.WriteLine($"First 10 aliases:");
            foreach (AliasEntry a in data.Aliases.Take(10))
            {
                string name = data.StringTable.Resolve(a.NameIndex) ?? "?";
                string link = data.StringTable.Resolve(a.LinkIndex) ?? "?";
                Console.WriteLine($"  {name,-40} → {link} flags=0x{a.Flags:X2}");
            }
        }

        return 0;
    }
}
