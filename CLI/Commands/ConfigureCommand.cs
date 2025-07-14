
using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.CLI.Handlers;

namespace LLMSensitiveDataGoverance.CLI.Commands;

/// <summary>
/// Command for configuring sensitivity labels and settings
/// </summary>
public class ConfigureCommand
{
    private readonly ILabelRepository _labelRepository;
    private readonly ILabelValidator _labelValidator;
    private readonly OutputFormatter _outputFormatter;
    private readonly ILogger<ConfigureCommand> _logger;

    /// <summary>
    /// Initializes a new instance of the ConfigureCommand class
    /// </summary>
    /// <param name="labelRepository">Label repository</param>
    /// <param name="labelValidator">Label validator</param>
    /// <param name="outputFormatter">Output formatter</param>
    /// <param name="logger">Logger</param>
    public ConfigureCommand(
        ILabelRepository labelRepository,
        ILabelValidator labelValidator,
        OutputFormatter outputFormatter,
        ILogger<ConfigureCommand> logger)
    {
        _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
        _labelValidator = labelValidator ?? throw new ArgumentNullException(nameof(labelValidator));
        _outputFormatter = outputFormatter ?? throw new ArgumentNullException(nameof(outputFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the configure command
    /// </summary>
    /// <returns>Command instance</returns>
    public Command GetCommand()
    {
        var command = new Command("configure", "Configure sensitivity labels and settings");

        // Add label subcommand
        var addLabelCommand = CreateAddLabelCommand();
        command.AddCommand(addLabelCommand);

        // Update label subcommand
        var updateLabelCommand = CreateUpdateLabelCommand();
        command.AddCommand(updateLabelCommand);

        // Delete label subcommand
        var deleteLabelCommand = CreateDeleteLabelCommand();
        command.AddCommand(deleteLabelCommand);

        // Import labels subcommand
        var importCommand = CreateImportCommand();
        command.AddCommand(importCommand);

        // Export labels subcommand
        var exportCommand = CreateExportCommand();
        command.AddCommand(exportCommand);

        return command;
    }

    /// <summary>
    /// Creates the add label command
    /// </summary>
    /// <returns>Add label command</returns>
    private Command CreateAddLabelCommand()
    {
        var nameOption = new Option<string>(
            new[] { "--name", "-n" },
            "Label name")
        {
            IsRequired = true
        };

        var descriptionOption = new Option<string>(
            new[] { "--description", "-d" },
            "Label description")
        {
            IsRequired = true
        };

        var priorityOption = new Option<LabelPriority>(
            new[] { "--priority", "-p" },
            "Label priority")
        {
            IsRequired = true
        };

        var encryptionOption = new Option<bool>(
            new[] { "--encryption" },
            "Require encryption");

        var preventExtractionOption = new Option<bool>(
            new[] { "--prevent-extraction" },
            "Prevent data extraction");

        var preventCopyPasteOption = new Option<bool>(
            new[] { "--prevent-copy-paste" },
            "Prevent copy/paste operations");

        var preventGroundingOption = new Option<bool>(
            new[] { "--prevent-grounding" },
            "Prevent use in grounding");

        var command = new Command("add", "Add a new sensitivity label")
        {
            nameOption,
            descriptionOption,
            priorityOption,
            encryptionOption,
            preventExtractionOption,
            preventCopyPasteOption,
            preventGroundingOption
        };

        command.SetHandler(async (name, description, priority, encryption, preventExtraction, preventCopyPaste, preventGrounding) =>
        {
            await ExecuteAddLabelAsync(name, description, priority, encryption, preventExtraction, preventCopyPaste, preventGrounding);
        }, nameOption, descriptionOption, priorityOption, encryptionOption, preventExtractionOption, preventCopyPasteOption, preventGroundingOption);

        return command;
    }

    /// <summary>
    /// Creates the update label command
    /// </summary>
    /// <returns>Update label command</returns>
    private Command CreateUpdateLabelCommand()
    {
        var idOption = new Option<string>(
            new[] { "--id" },
            "Label ID to update")
        {
            IsRequired = true
        };

        var nameOption = new Option<string>(
            new[] { "--name", "-n" },
            "New label name");

        var descriptionOption = new Option<string>(
            new[] { "--description", "-d" },
            "New label description");

        var priorityOption = new Option<LabelPriority?>(
            new[] { "--priority", "-p" },
            "New label priority");

        var activeOption = new Option<bool?>(
            new[] { "--active" },
            "Set label active status");

        var command = new Command("update", "Update an existing sensitivity label")
        {
            idOption,
            nameOption,
            descriptionOption,
            priorityOption,
            activeOption
        };

        command.SetHandler(async (id, name, description, priority, active) =>
        {
            await ExecuteUpdateLabelAsync(id, name, description, priority, active);
        }, idOption, nameOption, descriptionOption, priorityOption, activeOption);

        return command;
    }

    /// <summary>
    /// Creates the delete label command
    /// </summary>
    /// <returns>Delete label command</returns>
    private Command CreateDeleteLabelCommand()
    {
        var idOption = new Option<string>(
            new[] { "--id" },
            "Label ID to delete")
        {
            IsRequired = true
        };

        var forceOption = new Option<bool>(
            new[] { "--force", "-f" },
            "Force deletion without confirmation");

        var command = new Command("delete", "Delete a sensitivity label")
        {
            idOption,
            forceOption
        };

        command.SetHandler(async (id, force) =>
        {
            await ExecuteDeleteLabelAsync(id, force);
        }, idOption, forceOption);

        return command;
    }

    /// <summary>
    /// Creates the import command
    /// </summary>
    /// <returns>Import command</returns>
    private Command CreateImportCommand()
    {
        var fileOption = new Option<FileInfo>(
            new[] { "--file", "-f" },
            "JSON file containing labels to import")
        {
            IsRequired = true
        };

        var overwriteOption = new Option<bool>(
            new[] { "--overwrite" },
            "Overwrite existing labels");

        var command = new Command("import", "Import labels from JSON file")
        {
            fileOption,
            overwriteOption
        };

        command.SetHandler(async (file, overwrite) =>
        {
            await ExecuteImportAsync(file, overwrite);
        }, fileOption, overwriteOption);

        return command;
    }

    /// <summary>
    /// Creates the export command
    /// </summary>
    /// <returns>Export command</returns>
    private Command CreateExportCommand()
    {
        var fileOption = new Option<FileInfo>(
            new[] { "--file", "-f" },
            "Output file for exported labels")
        {
            IsRequired = true
        };

        var command = new Command("export", "Export labels to JSON file")
        {
            fileOption
        };

        command.SetHandler(async (file) =>
        {
            await ExecuteExportAsync(file);
        }, fileOption);

        return command;
    }

    /// <summary>
    /// Executes the add label command
    /// </summary>
    private async Task ExecuteAddLabelAsync(
        string name,
        string description,
        LabelPriority priority,
        bool encryption,
        bool preventExtraction,
        bool preventCopyPaste,
        bool preventGrounding)
    {
        try
        {
            var label = new SensitivityLabel
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Priority = priority,
                Protection = new ProtectionSettings
                {
                    RequireEncryption = encryption,
                    PreventExtraction = preventExtraction,
                    PreventCopyPaste = preventCopyPaste,
                    PreventGrounding = preventGrounding,
                    AllowedUsers = new List<string>(),
                    AllowedGroups = new List<string>()
                },
                CustomProperties = new Dictionary<string, string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var validationResult = await _labelValidator.ValidateDetailedAsync(label);
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Invalid label configuration: {validationResult.ErrorMessage}");
                return;
            }

            var createdLabel = await _labelRepository.CreateAsync(label);
            Console.WriteLine($"Successfully created label '{createdLabel.Name}' with ID: {createdLabel.Id}");

            _logger.LogInformation($"Created new label: {createdLabel.Name} ({createdLabel.Id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating label");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes the update label command
    /// </summary>
    private async Task ExecuteUpdateLabelAsync(
        string id,
        string? name,
        string? description,
        LabelPriority? priority,
        bool? active)
    {
        try
        {
            var label = await _labelRepository.GetByIdAsync(id);
            if (label == null)
            {
                Console.WriteLine($"Label with ID '{id}' not found");
                return;
            }

            // Update properties
            if (!string.IsNullOrEmpty(name))
                label.Name = name;
            if (!string.IsNullOrEmpty(description))
                label.Description = description;
            if (priority.HasValue)
                label.Priority = priority.Value;
            if (active.HasValue)
                label.IsActive = active.Value;

            label.UpdatedAt = DateTime.UtcNow;

            var validationResult = await _labelValidator.ValidateDetailedAsync(label);
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Invalid label configuration: {validationResult.ErrorMessage}");
                return;
            }

            var updatedLabel = await _labelRepository.UpdateAsync(label);
            Console.WriteLine($"Successfully updated label '{updatedLabel.Name}'");

            _logger.LogInformation($"Updated label: {updatedLabel.Name} ({updatedLabel.Id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating label");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes the delete label command
    /// </summary>
    private async Task ExecuteDeleteLabelAsync(string id, bool force)
    {
        try
        {
            var label = await _labelRepository.GetByIdAsync(id);
            if (label == null)
            {
                Console.WriteLine($"Label with ID '{id}' not found");
                return;
            }

            if (!force)
            {
                Console.Write($"Are you sure you want to delete label '{label.Name}' ({label.Id})? [y/N]: ");
                var confirmation = Console.ReadLine();
                if (string.IsNullOrEmpty(confirmation) || !confirmation.Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Delete operation cancelled");
                    return;
                }
            }

            var success = await _labelRepository.DeleteAsync(id);
            if (success)
            {
                Console.WriteLine($"Successfully deleted label '{label.Name}'");
                _logger.LogInformation($"Deleted label: {label.Name} ({label.Id})");
            }
            else
            {
                Console.WriteLine("Failed to delete label");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting label");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes the import command
    /// </summary>
    private async Task ExecuteImportAsync(FileInfo file, bool overwrite)
    {
        try
        {
            if (!file.Exists)
            {
                Console.WriteLine($"File '{file.FullName}' not found");
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(file.FullName);
            var labels = JsonSerializer.Deserialize<List<SensitivityLabel>>(jsonContent);

            if (labels == null || !labels.Any())
            {
                Console.WriteLine("No valid labels found in the import file");
                return;
            }

            var importedCount = 0;
            var skippedCount = 0;
            var errorCount = 0;

            foreach (var label in labels)
            {
                try
                {
                    // Validate label
                    var validationResult = await _labelValidator.ValidateDetailedAsync(label);
                    if (!validationResult.IsValid)
                    {
                        Console.WriteLine($"Skipping invalid label '{label.Name}': {validationResult.ErrorMessage}");
                        errorCount++;
                        continue;
                    }

                    // Check if label exists
                    var existingLabel = await _labelRepository.GetByIdAsync(label.Id);
                    if (existingLabel != null)
                    {
                        if (overwrite)
                        {
                            label.UpdatedAt = DateTime.UtcNow;
                            await _labelRepository.UpdateAsync(label);
                            importedCount++;
                        }
                        else
                        {
                            Console.WriteLine($"Skipping existing label '{label.Name}' (use --overwrite to replace)");
                            skippedCount++;
                        }
                    }
                    else
                    {
                        label.CreatedAt = DateTime.UtcNow;
                        label.UpdatedAt = DateTime.UtcNow;
                        await _labelRepository.CreateAsync(label);
                        importedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing label '{label.Name}': {ex.Message}");
                    errorCount++;
                }
            }

            Console.WriteLine($"Import completed: {importedCount} imported, {skippedCount} skipped, {errorCount} errors");
            _logger.LogInformation($"Import completed: {importedCount} imported, {skippedCount} skipped, {errorCount} errors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during import");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes the export command
    /// </summary>
    private async Task ExecuteExportAsync(FileInfo file)
    {
        try
        {
            var labels = await _labelRepository.GetAllAsync();
            var labelList = labels.ToList();

            if (!labelList.Any())
            {
                Console.WriteLine("No labels to export");
                return;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonContent = JsonSerializer.Serialize(labelList, jsonOptions);
            await File.WriteAllTextAsync(file.FullName, jsonContent);

            Console.WriteLine($"Successfully exported {labelList.Count} labels to '{file.FullName}'");
            _logger.LogInformation($"Exported {labelList.Count} labels to {file.FullName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during export");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}