using System.Text;

namespace SourceCodeConcatenator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("File Concatenator");
        Console.WriteLine("--------------------");

        var repositoryPath = "";
        var csOutputFileName = "cs-files.txt";
        var jsOutputFileName = "js-files.txt";
        var cssOutputFileName = "css-files.txt";
        var htmlOutputFileName = "cshtml-files.txt";

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

        Console.WriteLine("Output files will be available in directory of the process.");

        // Find all .cs files
        var csFiles = Directory.GetFiles(repositoryPath, "*.cs", SearchOption.AllDirectories);
        var jsFiles = Directory.GetFiles(repositoryPath, "*.js", SearchOption.AllDirectories);
        var cssFiles = Directory.GetFiles(repositoryPath, "*.css", SearchOption.AllDirectories);
        var htmlFiles = Directory.GetFiles(repositoryPath, "*.cshtml", SearchOption.AllDirectories);

        Console.WriteLine($"Found {csFiles.Length} .cs files in the repository.");

        bool foundAny = false;
        if (csFiles.Length > 0)
        {
            foundAny = true;
        }

        if (jsFiles.Length > 0)
        {
            foundAny = true;
        }

        if (htmlFiles.Length > 0)
        {
            foundAny = true;
        }

        if (cssFiles.Length > 0)
        {
            foundAny = true;
        }

        if (!foundAny)
        {
            throw new Exception("No files to concatanate!");
        }

        ConcatanateFiles(csFiles, repositoryPath, csOutputFileName);
        ConcatanateFiles(jsFiles, repositoryPath, jsOutputFileName);
        ConcatanateFiles(cssFiles, repositoryPath, cssOutputFileName);
        ConcatanateFiles(htmlFiles, repositoryPath, htmlOutputFileName);
    }

    static void ConcatanateFiles(string[] files, string repositoryPath, string outputFileName)
    {
        // Create the combined file
        var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);
        using (StreamWriter outputFile = new StreamWriter(outputFilePath, false, Encoding.UTF8))
        {
            // Add header comment
            var outputFileExtension = Path.GetExtension(outputFilePath);
            outputFile.WriteLine("// Generated on: " + DateTime.Now.ToString());
            outputFile.WriteLine();

            // Process each file
            foreach (string filePath in files)
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

        Console.WriteLine($"Successfully combined {files.Length} files into '{outputFileName}'");
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
