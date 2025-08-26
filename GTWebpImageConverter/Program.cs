using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

// Define the directory path to scan for .webp files
string directoryPath = @"D:\Temp";

// Check if directory exists
if (!Directory.Exists(directoryPath))
{
    Console.WriteLine("Directory does not exist.");
    return;
}

// Get all .webp files in the directory
string[] files = Directory.GetFiles(directoryPath, "*.webp");
foreach (string file in files)
{
    ConvertWebpToJpg(file);
}

Console.WriteLine("Conversion completed.");

static void ConvertWebpToJpg(string filePath)
{
    try
    {
        // Load the .webp image
        using (Image image = Image.Load(filePath))
        {
            // Set the new file path with .jpg extension
            string newFilePath = Path.ChangeExtension(filePath, ".jpg");

            // Save the image in JPG format
            image.SaveAsJpeg(newFilePath, new JpegEncoder());
        }

        Console.WriteLine($"Converted {Path.GetFileName(filePath)} to JPG.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error converting {Path.GetFileName(filePath)}: {ex.Message}");
    }
}