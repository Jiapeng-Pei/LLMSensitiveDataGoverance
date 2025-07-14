
using System.CommandLine;
using Microsoft.Extensions.Logging;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.CLI.Handlers;

namespace LLMSensitiveDataGoverance.CLI.Commands;

/// <summary>
/// Command for listing available sensitivity labels
/// </summary>
public class ListLabelsCommand
{
    private readonly ILabelRepository _labelRepository;
    private readonly OutputFormatter _outputFormatter;
    private readonly ILogger<ListLabelsCommand> _logger;

    /// <summary>
    /// Initializes a new instance of the ListLabelsCommand class
    /// </summary>
    /// <param name="labelRepository">Label repository</param>
    /// <param name="outputFormatter">Output formatter</param>
    /// <param name="logger">Logger</param>
    public ListLabelsCommand(
        ILabelRepository labelRepository,
        OutputFormatter outputFormatter,
        ILogger<ListLabelsCommand> logger)
    {
        _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
        _outputFormatter = outputFormatter ?? throw new ArgumentNullException(nameof(outputFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the list labels command
    /// </summary>
    /// <returns>Command instance</returns>
    public Command GetCommand()
    {
        var priorityOption = new Option<LabelPriority?>(
            new[] { "--priority", "-p" },
            "Filter by label priority");

        var activeOnlyOption = new Option<bool>(
            new[] { "--active-only", "-a" },
            "Show only active labels");

        var formatOption = new Option<string>(
            new[] { "--format" },
            () => "table",
            "Output format (table, json, xml)");

        var outputOption = new Option<string>(
            new[] { "--output", "-o" },
            "Output file for label list");

        var detailsOption = new Option<bool>(
            new[] { "--details", "-d" },
            "Show detailed information");

        var command = new Command("list", "List available sensitivity labels")
        {
            priorityOption,
            activeOnlyOption,
            formatOption,
            outputOption,
            detailsOption
        };

        command.SetHandler(async (priority, activeOnly, format, output, details) =>
        {
            await ExecuteListLabelsAsync(priority, activeOnly, format, output, details);
        }, priorityOption, activeOnlyOption, formatOption, outputOption, detailsOption);

        return command;
    }

    /// <summary>
    /// Executes the list labels command
    /// </summary>
    /// <param name="priority">Priority filter</param>
    /// <param name="activeOnly">Active only filter</param>
    /// <param name="format">Output format</param>
    /// <param name="output">Output file</param>
    /// <param name="details">Show details</param>
    private async Task ExecuteListLabelsAsync(
        LabelPriority? priority,
        bool activeOnly,
        string format,
        string? output,
        bool details)
    {
        try
        {
            _logger.LogInformation("Retrieving sensitivity labels");

            IEnumerable<SensitivityLabel> labels;

            if (priority.HasValue)
            {
                labels = await _labelRepository.GetByPriorityAsync(priority.Value);
            }
            else
            {
                labels = await _labelRepository.GetAllAsync();
            }

            if (activeOnly)
            {
                labels = labels.Where(l => l.IsActive);
            }

            var labelList = labels.OrderBy(l => l.Priority).ThenBy(l => l.Name).ToList();

            if (!labelList.Any())
            {
                Console.WriteLine("No sensitivity labels found matching the criteria");
                return;
            }

            await _outputFormatter.FormatLabelListAsync(labelList, format, output, details);

            _logger.LogInformation($"Listed {labelList.Count} sensitivity labels");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing labels");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}