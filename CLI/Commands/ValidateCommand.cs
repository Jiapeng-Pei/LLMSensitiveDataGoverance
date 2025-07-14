
using System.CommandLine;
using Microsoft.Extensions.Logging;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.CLI.Handlers;

namespace LLMSensitiveDataGoverance.CLI.Commands;

/// <summary>
/// Command for validating sensitivity labels and configuration
/// </summary>
public class ValidateCommand
{
    private readonly ILabelValidator _labelValidator;
    private readonly ILabelRepository _labelRepository;
    private readonly OutputFormatter _outputFormatter;
    private readonly ILogger<ValidateCommand> _logger;

    /// <summary>
    /// Initializes a new instance of the ValidateCommand class
    /// </summary>
    /// <param name="labelValidator">Label validator</param>
    /// <param name="labelRepository">Label repository</param>
    /// <param name="outputFormatter">Output formatter</param>
    /// <param name="logger">Logger</param>
    public ValidateCommand(
        ILabelValidator labelValidator,
        ILabelRepository labelRepository,
        OutputFormatter outputFormatter,
        ILogger<ValidateCommand> logger)
    {
        _labelValidator = labelValidator ?? throw new ArgumentNullException(nameof(labelValidator));
        _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
        _outputFormatter = outputFormatter ?? throw new ArgumentNullException(nameof(outputFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the validate command
    /// </summary>
    /// <returns>Command instance</returns>
    public Command GetCommand()
    {
        var labelIdOption = new Option<string>(
            new[] { "--label-id", "-l" },
            "Validate specific label by ID");

        var labelNameOption = new Option<string>(
            new[] { "--label-name", "-n" },
            "Validate specific label by name");

        var allOption = new Option<bool>(
            new[] { "--all", "-a" },
            "Validate all labels");

        var formatOption = new Option<string>(
            new[] { "--format" },
            () => "table",
            "Output format (table, json, xml)");

        var outputOption = new Option<string>(
            new[] { "--output", "-o" },
            "Output file for validation results");

        var detailedOption = new Option<bool>(
            new[] { "--detailed", "-d" },
            "Show detailed validation results");

        var command = new Command("validate", "Validate sensitivity labels and configuration")
        {
            labelIdOption,
            labelNameOption,
            allOption,
            formatOption,
            outputOption,
            detailedOption
        };

        command.SetHandler(async (labelId, labelName, all, format, output, detailed) =>
        {
            await ExecuteValidateAsync(labelId, labelName, all, format, output, detailed);
        }, labelIdOption, labelNameOption, allOption, formatOption, outputOption, detailedOption);

        return command;
    }

    /// <summary>
    /// Executes the validate command
    /// </summary>
    /// <param name="labelId">Label ID to validate</param>
    /// <param name="labelName">Label name to validate</param>
    /// <param name="all">Validate all labels</param>
    /// <param name="format">Output format</param>
    /// <param name="output">Output file</param>
    /// <param name="detailed">Show detailed results</param>
    private async Task ExecuteValidateAsync(
        string? labelId,
        string? labelName,
        bool all,
        string format,
        string? output,
        bool detailed)
    {
        try
        {
            _logger.LogInformation("Starting validation process");

            var validationResults = new List<ValidationResult>();

            if (all)
            {
                var allLabels = await _labelRepository.GetAllAsync();
                foreach (var label in allLabels)
                {
                    var result = await _labelValidator.ValidateDetailedAsync(label);
                    validationResults.Add(result);
                }
            }
            else if (!string.IsNullOrEmpty(labelId))
            {
                var label = await _labelRepository.GetByIdAsync(labelId);
                if (label == null)
                {
                    Console.WriteLine($"Label with ID '{labelId}' not found");
                    return;
                }

                var result = await _labelValidator.ValidateDetailedAsync(label);
                validationResults.Add(result);
            }
            else if (!string.IsNullOrEmpty(labelName))
            {
                var label = await _labelRepository.GetByNameAsync(labelName);
                if (label == null)
                {
                    Console.WriteLine($"Label with name '{labelName}' not found");
                    return;
                }

                var result = await _labelValidator.ValidateDetailedAsync(label);
                validationResults.Add(result);
            }
            else
            {
                Console.WriteLine("Error: Must specify --label-id, --label-name, or --all");
                return;
            }

            await _outputFormatter.FormatValidationResultsAsync(validationResults, format, output, detailed);

            var validCount = validationResults.Count(r => r.IsValid);
            var invalidCount = validationResults.Count - validCount;

            Console.WriteLine($"\nValidation Summary: {validCount} valid, {invalidCount} invalid");

            _logger.LogInformation($"Validation completed: {validCount} valid, {invalidCount} invalid");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during validation");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}