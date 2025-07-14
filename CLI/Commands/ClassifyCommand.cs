using System.CommandLine;
using Microsoft.Extensions.Logging;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.CLI.Handlers;

namespace LLMSensitiveDataGoverance.CLI.Commands;

/// <summary>
/// Command for classifying grounding data with sensitivity labels
/// </summary>
public class ClassifyCommand
{
    private readonly ISensitivityLabelService _labelService;
    private readonly CommandHandler _commandHandler;
    private readonly OutputFormatter _outputFormatter;
    private readonly ILogger<ClassifyCommand> _logger;

    /// <summary>
    /// Initializes a new instance of the ClassifyCommand class
    /// </summary>
    /// <param name="labelService">Sensitivity label service</param>
    /// <param name="commandHandler">Command handler</param>
    /// <param name="outputFormatter">Output formatter</param>
    /// <param name="logger">Logger</param>
    public ClassifyCommand(
        ISensitivityLabelService labelService,
        CommandHandler commandHandler,
        OutputFormatter outputFormatter,
        ILogger<ClassifyCommand> logger)
    {
        _labelService = labelService ?? throw new ArgumentNullException(nameof(labelService));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _outputFormatter = outputFormatter ?? throw new ArgumentNullException(nameof(outputFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the classify command
    /// </summary>
    /// <returns>Command instance</returns>
    public Command GetCommand()
    {
        var fileOption = new Option<FileInfo>(
            new[] { "--file", "-f" },
            "Input file containing grounding data to classify")
        {
            IsRequired = true
        };

        var contentOption = new Option<string>(
            new[] { "--content", "-c" },
            "Direct content to classify (alternative to file input)");

        var sourceOption = new Option<string>(
            new[] { "--source", "-s" },
            "Source identifier for the grounding data")
        {
            IsRequired = true
        };

        var dataTypeOption = new Option<string>(
            new[] { "--data-type", "-t" },
            () => "text",
            "Type of data being classified (text, document, etc.)");

        var outputOption = new Option<string>(
            new[] { "--output", "-o" },
            "Output file for classification results");

        var formatOption = new Option<string>(
            new[] { "--format" },
            () => "table",
            "Output format (table, json, xml)");

        var verboseOption = new Option<bool>(
            new[] { "--verbose", "-v" },
            "Enable verbose output");

        var command = new Command("classify", "Classify grounding data with sensitivity labels")
        {
            fileOption,
            contentOption,
            sourceOption,
            dataTypeOption,
            outputOption,
            formatOption,
            verboseOption
        };

        command.SetHandler(async (file, content, source, dataType, output, format, verbose) =>
        {
            await ExecuteClassifyAsync(file, content, source, dataType, output, format, verbose);
        }, fileOption, contentOption, sourceOption, dataTypeOption, outputOption, formatOption, verboseOption);

        return command;
    }

    /// <summary>
    /// Executes the classify command
    /// </summary>
    /// <param name="file">Input file</param>
    /// <param name="content">Direct content</param>
    /// <param name="source">Source identifier</param>
    /// <param name="dataType">Data type</param>
    /// <param name="output">Output file</param>
    /// <param name="format">Output format</param>
    /// <param name="verbose">Verbose output</param>
    private async Task ExecuteClassifyAsync(
        FileInfo? file,
        string? content,
        string source,
        string dataType,
        string? output,
        string format,
        bool verbose)
    {
        try
        {
            _logger.LogInformation("Starting classification process");

            // Get content from file or direct input
            var inputContent = await GetInputContentAsync(file, content);
            if (string.IsNullOrEmpty(inputContent))
            {
                Console.WriteLine("Error: No content provided for classification");
                return;
            }

            // Create grounding data
            var groundingData = new GroundingData
            {
                Id = Guid.NewGuid().ToString(),
                Content = inputContent,
                Source = source,
                DataType = dataType,
                Metadata = new Dictionary<string, object>
                {
                    ["ProcessedAt"] = DateTime.UtcNow,
                    ["CLI"] = true
                },
                LastModified = DateTime.UtcNow
            };

            if (verbose)
            {
                Console.WriteLine($"Processing {dataType} data from {source}");
                Console.WriteLine($"Content length: {inputContent.Length} characters");
            }

            // Classify the data
            var result = await _labelService.ClassifyAsync(groundingData);

            // Format and output results
            await _outputFormatter.FormatClassificationResultAsync(result, format, output, verbose);

            _logger.LogInformation("Classification completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during classification");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets input content from file or direct input
    /// </summary>
    /// <param name="file">Input file</param>
    /// <param name="content">Direct content</param>
    /// <returns>Input content</returns>
    private async Task<string> GetInputContentAsync(FileInfo? file, string? content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            return content;
        }

        if (file != null && file.Exists)
        {
            return await File.ReadAllTextAsync(file.FullName);
        }

        throw new ArgumentException("Either file or content must be provided");
    }
}