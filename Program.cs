using ImageMagick;

string directoryPath = string.Empty;
string outputFilePath = string.Empty;

int batchSize = 100;

if (args.Length <= 0)
{
    Console.WriteLine("Error: Directory path is missing. Please provide the directory path using -p option.");


    return;
} 
else
{
    for (int i = 0; i < args.Length; i += 2)
    {
        string option = args[i];
        string value = args[i + 1];

        switch (option)
        {
            case "-p":
                directoryPath = value;
                break;
            case "-t":
                if (!int.TryParse(value, out batchSize) || batchSize <= 0)
                {
                    Console.WriteLine("Invalid batch size.");
                    return;
                }
                break;
            case "-o":
                outputFilePath = value;
                break;
            default:
                Console.WriteLine($"Invalid option: {option}");
                return;
        }
    }

    string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".heic" };

    List<string> filesToProcess = new List<string>(Directory.GetFiles(directoryPath));
    int filesProcessed = 0;
    int totalFiles = filesToProcess.Count;

    Console.WriteLine($"Total files to process: {totalFiles}");

    // Create batches of files
    int numThreads = Environment.ProcessorCount;
    int filesPerThread = (int)Math.Ceiling((double)totalFiles / numThreads);

    Console.WriteLine($"Number of threads: {numThreads}");

    Task[] tasks = new Task[numThreads];


    for (int i = 0; i < numThreads; i++)
    {
        int start = i * filesPerThread;
        int end = Math.Min(start + filesPerThread, totalFiles);

        tasks[i] = Task.Run(() =>
        {
            for (int j = start; j < end; j++)
            {
                string filePath = filesToProcess[j];
                if (IsImageFile(filePath, imageExtensions, outputFilePath))
                {
                    DateTime originalDateTime = GetOriginalDateTime(filePath);
                    if (originalDateTime != DateTime.MinValue)
                    {
                        DateTime currentCreationTime = File.GetCreationTime(filePath);
                        
                        if (currentCreationTime != originalDateTime)
                        {
                            File.SetCreationTime(filePath, originalDateTime);
                        }
                    }
                    else
                    {
                        Console.WriteLine($" - Failed to read EXIF data from {filePath}");
                        ExportToUnprocessedFilesList(filePath, outputFilePath);
                    }
                }

                // Aggiornamento dell'avanzamento
                lock (Console.Out)
                {
                    filesProcessed++;
                    double progress = (double)filesProcessed / totalFiles * 100;
                    Console.CursorLeft = 0;
                    Console.Write($"Progress: {progress:F2}%");
                }
            }
        });
    }

    Task.WaitAll(tasks);

    Console.WriteLine("\nDone!");
} 


// Check if the file is a Photo
static bool IsImageFile(string filePath, string[] imageExtensions, string outputFilePath)
{
    string extension = Path.GetExtension(filePath).ToLower();

    foreach (string imageExtension in imageExtensions)
    {
        if (extension == imageExtension)
        {
            return true;
        }
    }

    ExportToUnprocessedFilesList(filePath, outputFilePath);

    return false;
}

// Get the EXIF Tag "OriginalDateTime"
static DateTime GetOriginalDateTime(string filePath)
{
    using (MagickImage image = new MagickImage(filePath))
    {
        if (image.GetExifProfile() != null)
        {
            DateTime originalDateTime;

            if (DateTime.TryParseExact(image.GetAttribute("exif:DateTimeOriginal"), "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out originalDateTime))
            {
                return originalDateTime;
            }
        }
    }
    return DateTime.MinValue;
}

// Export the unprocessed pictures in the output file
static void ExportToUnprocessedFilesList(string filePath, string outputFilePath)
{
    // The file is not a Photo, exporting its path in the output.txt file
    if (outputFilePath == string.Empty)
    {
        outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), "output.txt");
    }
    
    File.AppendAllText(outputFilePath, filePath + Environment.NewLine);   
}