using ImageMagick;

var inputPath = @"C:\Temp\Images";  // Set the path to the folder containing the HEIC files
var outputPath = inputPath;  // Use the same path for output files or change it if necessary

var files = Directory.GetFiles(inputPath, "*.HEIC", SearchOption.AllDirectories);
foreach (var file in files)
{
    try
    {
        using (var image = new MagickImage(file))
        {
            var outputFile = Path.ChangeExtension(file, ".jpg");
            image.Write(outputFile);
            Console.WriteLine($"Converted: {outputFile}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing {file}: {ex.Message}");
    }
}