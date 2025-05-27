using System.Text;
using Microsoft.ML.Tokenizers;

namespace SourceCodeConcatenator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("File Concatenator with Token Counter");
        Console.WriteLine("------------------------------------");

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

        // Find all files
        var csFiles = Directory.GetFiles(repositoryPath, "*.cs", SearchOption.AllDirectories);
        var jsFiles = Directory.GetFiles(repositoryPath, "*.js", SearchOption.AllDirectories);
        var cssFiles = Directory.GetFiles(repositoryPath, "*.css", SearchOption.AllDirectories);
        var htmlFiles = Directory.GetFiles(repositoryPath, "*.cshtml", SearchOption.AllDirectories);

        Console.WriteLine($"Found {csFiles.Length} .cs files in the repository.");

        bool foundAny = false;
        if (csFiles.Length > 0) foundAny = true;
        if (jsFiles.Length > 0) foundAny = true;
        if (htmlFiles.Length > 0) foundAny = true;
        if (cssFiles.Length > 0) foundAny = true;

        if (!foundAny)
        {
            throw new Exception("No files to concatenate!");
        }

        // Initialize tokenizer
        var tokenizer = InitializeTokenizer();

        // Process each file type and count tokens
        var totalTokens = 0;
        totalTokens += ConcatenateFilesWithTokenCount(csFiles, repositoryPath, csOutputFileName, tokenizer, "C#");
        totalTokens += ConcatenateFilesWithTokenCount(jsFiles, repositoryPath, jsOutputFileName, tokenizer, "JavaScript");
        totalTokens += ConcatenateFilesWithTokenCount(cssFiles, repositoryPath, cssOutputFileName, tokenizer, "CSS");
        totalTokens += ConcatenateFilesWithTokenCount(htmlFiles, repositoryPath, htmlOutputFileName, tokenizer, "Razor/HTML");

        Console.WriteLine();
        Console.WriteLine($"Total tokens across all generated files: {totalTokens:N0}");

        // Provide context about token limits
        PrintTokenLimitContext(totalTokens);
    }

    static Tokenizer InitializeTokenizer()
    {
        try
        {
            // Using cl100k_base encoding (GPT-4/ChatGPT tokenizer)
            // This is the most commonly used tokenizer for modern LLMs
            return TiktokenTokenizer.CreateForModel("gpt-4");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not initialize GPT-4 tokenizer ({ex.Message})");
            Console.WriteLine("Falling back to GPT-3.5 tokenizer...");

            try
            {
                return TiktokenTokenizer.CreateForModel("gpt-3.5-turbo");
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"Warning: Could not initialize tiktoken tokenizer ({ex2.Message})");
                Console.WriteLine("Falling back to basic tokenizer...");

                // Fallback to a basic tokenizer if tiktoken fails
                return TiktokenTokenizer.CreateForEncoding("cl100k_base");
            }
        }
    }

    static int ConcatenateFilesWithTokenCount(string[] files, string repositoryPath, string outputFileName, Tokenizer tokenizer, string fileType)
    {
        if (files.Length == 0)
        {
            Console.WriteLine($"No {fileType} files found - skipping {outputFileName}");
            return 0;
        }

        // Create the combined file
        var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);
        var contentBuilder = new StringBuilder();

        // Add header comment
        contentBuilder.AppendLine("// Generated on: " + DateTime.Now.ToString());
        contentBuilder.AppendLine();

        // Process each file
        foreach (string filePath in files)
        {
            string relativePath = GetRelativePath(filePath, repositoryPath);

            // Write start delimiter
            contentBuilder.AppendLine($"// Content start for {relativePath}");

            // Copy the file content
            string content = File.ReadAllText(filePath);
            contentBuilder.AppendLine(content);

            // If the file doesn't end with a newline, add one
            if (content.Length > 0 && !content.EndsWith(Environment.NewLine))
            {
                contentBuilder.AppendLine();
            }

            // Write end delimiter
            contentBuilder.AppendLine($"// Content end for {relativePath}");
            contentBuilder.AppendLine();
        }

        // Write the combined content to file
        var finalContent = contentBuilder.ToString();
        File.WriteAllText(outputFilePath, finalContent, Encoding.UTF8);

        // Count tokens
        var tokenCount = CountTokens(finalContent, tokenizer);

        Console.WriteLine($"Successfully combined {files.Length} {fileType} files into '{outputFileName}'");
        Console.WriteLine($"  File size: {new FileInfo(outputFilePath).Length:N0} bytes");
        Console.WriteLine($"  Token count: {tokenCount:N0} tokens");

        return tokenCount;
    }

    static int CountTokens(string text, Tokenizer tokenizer)
    {
        try
        {
            var encoded = tokenizer.EncodeToIds(text);
            return encoded.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not count tokens ({ex.Message})");
            // Fallback: rough estimation (1 token ≈ 4 characters for English text)
            return text.Length / 4;
        }
    }

    static void PrintTokenLimitContext(int totalTokens)
    {
        Console.WriteLine();
        Console.WriteLine("Token Limit Context:");
        Console.WriteLine("-------------------");

        var limits = new[]
        {
            ("GPT-4 Turbo", 128000),
            ("GPT-4", 8192),
            ("GPT-3.5 Turbo", 16385),
            ("Claude 3.5 Sonnet", 200000),
            ("Claude 3 Haiku", 200000)
        };

        foreach (var (model, limit) in limits)
        {
            var percentage = (double)totalTokens / limit * 100;
            var status = percentage <= 100 ? "✓" : "✗";
            var color = percentage <= 80 ? "within" : percentage <= 100 ? "near" : "exceeds";

            Console.WriteLine($"  {status} {model}: {totalTokens:N0}/{limit:N0} tokens ({percentage:F1}%) - {color} limit");
        }

        if (totalTokens > 8192)
        {
            Console.WriteLine();
            Console.WriteLine("💡 Tip: If you need to fit within smaller context windows, consider:");
            Console.WriteLine("   - Processing files separately by type");
            Console.WriteLine("   - Excluding certain file types or directories");
            Console.WriteLine("   - Using models with larger context windows");
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