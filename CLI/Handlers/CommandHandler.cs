
using Microsoft.Extensions.Logging;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Exceptions;

namespace LLMSensitiveDataGoverance.CLI.Handlers;

/// <summary>
/// Central handler for CLI command processing
/// </summary>
public class CommandHandler
{
    private readonly ISensitivityLabelService _labelService;
    private readonly ILabelRepository _labelRepository;
    private readonly ILogger<CommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CommandHandler class
    /// </summary>
    /// <param name="labelService">Sensitivity label service</param>
    /// <param name="labelRepository">Label repository</param>
    /// <param name="logger">Logger</param>
    public CommandHandler(
        ISensitivityLabelService labelService,
        ILabelRepository labelRepository,
        ILogger<CommandHandler> logger)
    {
        _labelService = labelService ?? throw new ArgumentNullException(nameof(labelService));
        _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles command execution with error handling and logging
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> HandleCommandAsync(Func<Task> operation, string operationName)
    {
        try
        {
            _logger.LogInformation($"Starting {operationName}");
            await operation();
            _logger.LogInformation($"Completed {operationName} successfully");
            return true;
        }
        catch (SensitivityLabelException ex)
        {
            _logger.LogError(ex, $"Label error during {operationName}");
            Console.WriteLine($"Label Error: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.LabelId))
            {
                Console.WriteLine($"Label ID: {ex.LabelId}");
            }
            return false;
        }
        catch (InvalidLabelException ex)
        {
            _logger.LogError(ex, $"Invalid label during {operationName}");
            Console.WriteLine($"Invalid Label: {ex.Message}");
            if (ex.ValidationResult != null)
            {
                Console.WriteLine($"Validation Details: {ex.ValidationResult.ErrorMessage}");
            }
            return false;
        }
        catch (EncryptionException ex)
        {
            _logger.LogError(ex, $"Encryption error during {operationName}");
            Console.WriteLine($"Encryption Error: {ex.Message}");
            Console.WriteLine($"Operation: {ex.Operation}");
            return false;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, $"File not found during {operationName}");
            Console.WriteLine($"File Error: {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, $"Access denied during {operationName}");
            Console.WriteLine($"Access Error: {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"Invalid argument during {operationName}");
            Console.WriteLine($"Argument Error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error during {operationName}");
            Console.WriteLine($"Unexpected Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Handles command execution with return value
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <returns>Result of the operation or default value on error</returns>
    public async Task<T?> HandleCommandAsync<T>(Func<Task<T>> operation, string operationName)
    {
        try
        {
            _logger.LogInformation($"Starting {operationName}");
            var result = await operation();
            _logger.LogInformation($"Completed {operationName} successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during {operationName}");
            Console.WriteLine($"Error: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Validates user input for commands
    /// </summary>
    /// <param name="input">Input to validate</param>
    /// <param name="parameterName">Parameter name for error messages</param>
    /// <param name="allowEmpty">Whether empty values are allowed</param>
    /// <returns>True if valid</returns>
    public bool ValidateInput(string? input, string parameterName, bool allowEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            if (!allowEmpty)
            {
                Console.WriteLine($"Error: {parameterName} cannot be empty");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates file path input
    /// </summary>
    /// <param name="filePath">File path to validate</param>
    /// <param name="mustExist">Whether the file must exist</param>
    /// <returns>True if valid</returns>
    public bool ValidateFilePath(string? filePath, bool mustExist = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine("Error: File path cannot be empty");
            return false;
        }

        if (mustExist && !File.Exists(filePath))
        {
            Console.WriteLine($"Error: File '{filePath}' does not exist");
            return false;
        }

        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Cannot access directory for '{filePath}': {ex.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets confirmation from user
    /// </summary>
    /// <param name="message">Confirmation message</param>
    /// <returns>True if confirmed</returns>
    public bool GetUserConfirmation(string message)
    {
        Console.Write($"{message} [y/N]: ");
        var response = Console.ReadLine();
        return !string.IsNullOrEmpty(response) && response.Equals("y", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Shows progress for long-running operations
    /// </summary>
    /// <param name="current">Current progress</param>
    /// <param name="total">Total items</param>
    /// <param name="operation">Operation description</param>
    public void ShowProgress(int current, int total, string operation)
    {
        var percentage = (double)current / total * 100;
        var progressBar = new string('█', (int)(percentage / 5));
        var emptyBar = new string('░', 20 - progressBar.Length);
        
        Console.Write($"\r{operation}: [{progressBar}{emptyBar}] {percentage:F1}% ({current}/{total})");
        
        if (current == total)
        {
            Console.WriteLine();
        }
    }
}
