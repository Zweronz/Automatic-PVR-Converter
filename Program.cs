using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Linq;
using System.IO;
using ImageMagick;

class Programmation
{
    static void Main()
    {
        Console.WriteLine("Flip Images (y/n)");
        bool flip = Console.ReadLine() == "y";

        Console.WriteLine("Gamma Correction (y/n)");
        bool linearize = Console.ReadLine() == "y";

        Console.WriteLine("File extension (leave empty for .pvr)");
        string? extension = Console.ReadLine();
        string extensionFile = extension == "" || extension == null ? ".pvr" : extension;

        string toolPath = "pvrcli/PVRTexToolCLI.exe";
        string outputDirectory = "convertedpvrs";

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        string[] inputFiles = Directory.GetFiles("pvrs", "*" + extensionFile);

        if (inputFiles.Length == 0)
            Console.WriteLine("please put pvr files into the pvrs folder for conversion");

        Console.WriteLine("Starting conversion of " + inputFiles.Length + " pvr files");

        foreach (string inputFile in inputFiles)
        {
            string inputFileName = Path.GetFileName(inputFile);
            string outputFileName = Path.ChangeExtension(inputFileName, ".png");
            string outputPath = Path.Combine(outputDirectory, outputFileName);
            string arguments = $"-f r8g8b8a8 -i \"{inputFile}\" -d \"{outputPath}\"";

            ExecuteCommand(toolPath, arguments);
        }

        string[] outputFiles = Directory.GetFiles(outputDirectory, "*.png");
        foreach (string outputFile in outputFiles)
        {
            if (linearize)
                LinearizeImage(outputFile, outputFile);

            if (flip)
                FlipImage(outputFile);
        }

        string[] stupidFiles = (from file in Directory.GetFiles("pvrs", "*.pvr") where Path.GetFileNameWithoutExtension(file).EndsWith("_Out") select file).ToArray();
        foreach (string stupidFile in stupidFiles)
        {
            Console.WriteLine("Deleting unnecessary generated file (" + Path.GetFileName(stupidFile) + ").");
            File.Delete(stupidFile);
        }

        Console.WriteLine("Finished. Press any key to exit");
        Console.ReadKey();
    }

    static void LinearizeImage(string input, string output)
    {
        Console.WriteLine("Linearizing image: " + Path.GetFileName(input));

        using (MagickImage image = new MagickImage(input))
        {
            image.Level(0, Quantum.Max, 0.4545, Channels.RGB);
            image.Write(output);
        }
    }

    static void ExecuteCommand(string toolPath, string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = toolPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }
    }
    static void FlipImage(string imagePath)
    {
        Console.WriteLine("Flipping image: " + Path.GetFileName(imagePath));

        using (var image = new Bitmap(imagePath))
        {
            image.RotateFlip(RotateFlipType.Rotate180FlipX);
            image.Save(imagePath);
        }
    }
}