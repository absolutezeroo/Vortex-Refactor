using Vortex.Bundle.Converter;
using Vortex.Bundle.Converter.Convert;

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

string? inputPath = null;
string? outputPath = null;
string? batchDir = null;
string? outputDir = null;
string? inspectPath = null;
bool usePng = false;

// Parse arguments
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--input" or "-i" when i + 1 < args.Length:
            inputPath = args[++i];
            break;
        case "--output" or "-o" when i + 1 < args.Length:
            outputPath = args[++i];
            break;
        case "--batch-dir" or "-b" when i + 1 < args.Length:
            batchDir = args[++i];
            break;
        case "--output-dir" when i + 1 < args.Length:
            outputDir = args[++i];
            break;
        case "--inspect" when i + 1 < args.Length:
            inspectPath = args[++i];
            break;
        case "--png":
            usePng = true;
            break;
        case "--help" or "-h":
            PrintUsage();
            return 0;
    }
}

// Inspect mode
if (inspectPath != null)
{
    return BundleInspector.Inspect(inspectPath);
}

ConvertOptions options = new()
{
    UseWebP = !usePng,
};
ConvertPipeline pipeline = new();

if (batchDir != null)
{
    // Batch mode: convert all .swf files in directory
    if (!Directory.Exists(batchDir))
    {
        Console.Error.WriteLine($"Error: directory not found: {batchDir}");
        return 1;
    }

    outputDir ??= Path.Combine(batchDir, "output");
    Directory.CreateDirectory(outputDir);

    string[] swfFiles = Directory.GetFiles(batchDir, "*.swf");
    Console.WriteLine($"Found {swfFiles.Length} SWF files in {batchDir}");

    int success = 0, failed = 0;
    foreach (string swfFile in swfFiles)
    {
        string name = Path.GetFileNameWithoutExtension(swfFile);
        string outFile = Path.Combine(outputDir, name + ".vortex");

        ConvertResult result = ConvertPipeline.Convert(swfFile, outFile, options);
        if (result.Success)
        {
            Console.WriteLine($"  OK  {name} → {result.OutputSize:N0} bytes " +
                              $"({result.ImageCount} images, {result.FrameCount} frames, {result.StringCount} strings)");
            success++;
        }
        else
        {
            Console.Error.WriteLine($"  FAIL {name}: {result.Error}");
            failed++;
        }
    }

    Console.WriteLine($"\nDone: {success} succeeded, {failed} failed.");
    return failed > 0 ? 1 : 0;
}
else if (inputPath != null)
{
    // Single file mode
    if (!File.Exists(inputPath))
    {
        Console.Error.WriteLine($"Error: file not found: {inputPath}");
        return 1;
    }

    outputPath ??= Path.ChangeExtension(inputPath, ".vortex");

    Console.WriteLine($"Converting {Path.GetFileName(inputPath)} → {Path.GetFileName(outputPath)}");

    ConvertResult result = ConvertPipeline.Convert(inputPath, outputPath, options);
    if (result.Success)
    {
        Console.WriteLine($"  Document class: {result.DocumentClass}");
        Console.WriteLine($"  Images:         {result.ImageCount}");
        Console.WriteLine($"  XML sections:   {string.Join(", ", result.XmlDocuments)}");
        Console.WriteLine($"  Atlas:          {result.AtlasWidth}x{result.AtlasHeight} ({result.FrameCount} frames)");
        Console.WriteLine($"  Strings:        {result.StringCount}");
        Console.WriteLine($"  Output:         {result.OutputSize:N0} bytes");
        return 0;
    }
    else
    {
        Console.Error.WriteLine($"Error: {result.Error}");
        return 1;
    }
}
else
{
    Console.Error.WriteLine("Error: specify --input or --batch-dir.");
    PrintUsage();
    return 1;
}

static void PrintUsage()
{
    Console.WriteLine("Vortex Bundle Converter — SWF to .vortex asset pipeline");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  Single file:");
    Console.WriteLine("    dotnet run -- --input <file.swf> [--output <file.vortex>] [--png]");
    Console.WriteLine();
    Console.WriteLine("  Batch directory:");
    Console.WriteLine("    dotnet run -- --batch-dir <dir> [--output-dir <dir>] [--png]");
    Console.WriteLine();
    Console.WriteLine("  Inspect bundle:");
    Console.WriteLine("    dotnet run -- --inspect <file.vortex>");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --input, -i     Input SWF file path");
    Console.WriteLine("  --output, -o    Output .vortex file path (default: same name)");
    Console.WriteLine("  --batch-dir, -b Convert all .swf files in directory");
    Console.WriteLine("  --output-dir    Output directory for batch mode");
    Console.WriteLine("  --inspect       Inspect a .vortex bundle (show assets, offsets, aliases)");
    Console.WriteLine("  --png           Use PNG instead of WebP for spritesheet");
    Console.WriteLine("  --help, -h      Show this help");
}
