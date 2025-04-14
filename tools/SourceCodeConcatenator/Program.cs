using System.Text;

namespace SourceCodeConcatenator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("C# File Concatenator");
        Console.WriteLine("--------------------");

        var repositoryPath = "";
        var outputFileName = "";

        // Get repository path
        if (args.Length > 0)
        {
            repositoryPath = args[0];
        }

        if (string.IsNullOrWhiteSpace(repositoryPath))
        {
            Console.Write("Enter the repository path: ");
            repositoryPath = Console.ReadLine().Trim();
        }

        // Validate repository path
        if (!Directory.Exists(repositoryPath))
        {
            Console.WriteLine($"Error: Directory '{repositoryPath}' does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(outputFileName))
        {
            Console.Write("Enter the output file path (default: combined.cs in current directory): ");
            outputFileName = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(outputFileName))
            {
                outputFileName = "combined.cs";
            }
        }

        try
        {

            // Find all .cs files
            string[] csFiles = Directory.GetFiles(repositoryPath, "*.cs", SearchOption.AllDirectories);

            Console.WriteLine($"Found {csFiles.Length} .cs files in the repository.");

            if (csFiles.Length == 0)
            {
                Console.WriteLine("No .cs files found. Exiting...");
                return;
            }

            // Create the combined file
            var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);
            using (StreamWriter outputFile = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                // Add header comment
                outputFile.WriteLine("// Combined C# files from repository: " + repositoryPath);
                outputFile.WriteLine("// Generated on: " + DateTime.Now.ToString());
                outputFile.WriteLine();

                // Process each file
                foreach (string filePath in csFiles)
                {
                    string relativePath = GetRelativePath(filePath, repositoryPath);

                    // Write start delimiter
                    outputFile.WriteLine($"// Content start for {relativePath}");

                    // Copy the file content
                    string content = File.ReadAllText(filePath);
                    outputFile.WriteLine(content);

                    // If the file doesn't end with a newline, add one
                    if (content.Length > 0 && !content.EndsWith(Environment.NewLine))
                    {
                        outputFile.WriteLine();
                    }

                    // Write end delimiter
                    outputFile.WriteLine($"// Content end for {relativePath}");
                    outputFile.WriteLine();
                }
            }

            Console.WriteLine($"Successfully combined {csFiles.Length} files into '{outputFileName}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the relative path of a file with respect to a base directory
    /// </summary>
    static string GetRelativePath(string filePath, string basePath)
    {
        // Ensure paths use the same directory separator and have trailing slashes
        basePath = Path.GetFullPath(basePath);
        filePath = Path.GetFullPath(filePath);

        if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            basePath += Path.DirectorySeparatorChar;
        }

        // If the file is not in the base path, return the full path
        if (!filePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            return filePath;
        }

        // Return the relative path
        return filePath.Substring(basePath.Length);
    }
}
