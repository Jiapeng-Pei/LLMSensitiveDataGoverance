using System.Text;
using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Logging;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.CLI.Handlers;

/// <summary>
/// Formats output for CLI commands in various formats
/// </summary>
public class OutputFormatter
{
    private readonly ILogger<OutputFormatter> _logger;

    /// <summary>
    /// Initializes a new instance of the OutputFormatter class
    /// </summary>
    /// <param name="logger">Logger</param>
    public OutputFormatter(ILogger<OutputFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Formats classification results
    /// </summary>
    /// <param name="result">Classification result</param>
    /// <param name="format">Output format</param>
    /// <param name="outputFile">Output file path</param>
    /// <param name="verbose">Verbose output</param>
    public async Task FormatClassificationResultAsync(
        SensitivityLabelResponse result,
        string format,
        string? outputFile,
        bool verbose)
    {
        var output = format.ToLowerInvariant() switch
        {
            "json" => FormatClassificationAsJson(result, verbose),
            "xml" => FormatClassificationAsXml(result, verbose),
            _ => FormatClassificationAsTable(result, verbose)
        };

        await WriteOutputAsync(output, outputFile);
    }

    /// <summary>
    /// Formats label list
    /// </summary>
    /// <param name="labels">List of labels</param>
    /// <param name="format">Output format</param>
    /// <param name="outputFile">Output file path</param>
    /// <param name="showDetails">Show detailed information</param>
    public async Task FormatLabelListAsync(
        IEnumerable<SensitivityLabel> labels,
        string format,
        string? outputFile,
        bool showDetails)
    {
        var output = format.ToLowerInvariant() switch
        {
            "json" => FormatLabelsAsJson(labels, showDetails),
            "xml" => FormatLabelsAsXml(labels, showDetails),
            _ => FormatLabelsAsTable(labels, showDetails)
        };

        await WriteOutputAsync(output, outputFile);
    }

    /// <summary>
    /// Formats validation results
    /// </summary>
    /// <param name="results">Validation results</param>
    /// <param name="format">Output format</param>
    /// <param name="outputFile">Output file path</param>
    /// <param name="detailed">Show detailed results</param>
    public async Task FormatValidationResultsAsync(
        IEnumerable<ValidationResult> results,
        string format,
        string? outputFile,
        bool detailed)
    {
        var output = format.ToLowerInvariant() switch
        {
            "json" => FormatValidationAsJson(results, detailed),
            "xml" => FormatValidationAsXml(results, detailed),
            _ => FormatValidationAsTable(results, detailed)
        };

        await WriteOutputAsync(output, outputFile);
    }

    /// <summary>
    /// Formats classification result as table
    /// </summary>
    private string FormatClassificationAsTable(SensitivityLabelResponse result, bool verbose)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Classification Result");
        sb.AppendLine("===================");
        sb.AppendLine();

        if (result.Label != null)
        {
            sb.AppendLine($"Label Name:        {result.Label.Name}");
            sb.AppendLine($"Label ID:          {result.Label.Id}");
            sb.AppendLine($"Priority:          {result.Label.Priority}");
            sb.AppendLine($"Description:       {result.Label.Description}");
            sb.AppendLine($"Is Active:         {result.Label.IsActive}");
            sb.AppendLine();

            sb.AppendLine("Protection Settings:");
            sb.AppendLine($"  Encryption:      {result.Label.Protection.RequireEncryption}");
            sb.AppendLine($"  Prevent Extract: {result.Label.Protection.PreventExtraction}");
            sb.AppendLine($"  Prevent Copy:    {result.Label.Protection.PreventCopyPaste}");
            sb.AppendLine($"  Prevent Ground:  {result.Label.Protection.PreventGrounding}");
            sb.AppendLine();

            sb.AppendLine("Response Settings:");
            sb.AppendLine($"  Should Display:  {result.ShouldDisplay}");
            sb.AppendLine($"  Allow Copy:      {result.AllowCopyPaste}");
            sb.AppendLine($"  Allow Grounding: {result.AllowGrounding}");
        }
        else
        {
            sb.AppendLine("No sensitivity label assigned");
        }

        if (verbose && !string.IsNullOrEmpty(result.Content))
        {
            sb.AppendLine();
            sb.AppendLine("Content Preview:");
            sb.AppendLine(result.Content.Length > 200 ? 
                result.Content.Substring(0, 200) + "..." : 
                result.Content);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats labels as table
    /// </summary>
    private string FormatLabelsAsTable(IEnumerable<SensitivityLabel> labels, bool showDetails)
    {
        var sb = new StringBuilder();
        var labelList = labels.ToList();

        if (!showDetails)
        {
            sb.AppendLine("Sensitivity Labels");
            sb.AppendLine("==================");
            sb.AppendLine();
            sb.AppendLine($"{"Name",-25} {"Priority",-15} {"Active",-8} {"Description",-40}");
            sb.AppendLine(new string('-', 88));

            foreach (var label in labelList)
            {
                var description = label.Description.Length > 37 ? 
                    label.Description.Substring(0, 37) + "..." : 
                    label.Description;
                    
                sb.AppendLine($"{label.Name,-25} {label.Priority,-15} {label.IsActive,-8} {description,-40}");
            }
        }
        else
        {
            foreach (var label in labelList)
            {
                sb.AppendLine($"Label: {label.Name}");
                sb.AppendLine(new string('=', label.Name.Length + 7));
                sb.AppendLine($"ID:          {label.Id}");
                sb.AppendLine($"Priority:    {label.Priority}");
                sb.AppendLine($"Description: {label.Description}");
                sb.AppendLine($"Active:      {label.IsActive}");
                sb.AppendLine($"Created:     {label.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Updated:     {label.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();

                sb.AppendLine("Protection Settings:");
                sb.AppendLine($"  Encryption:      {label.Protection.RequireEncryption}");
                sb.AppendLine($"  Prevent Extract: {label.Protection.PreventExtraction}");
                sb.AppendLine($"  Prevent Copy:    {label.Protection.PreventCopyPaste}");
                sb.AppendLine($"  Prevent Ground:  {label.Protection.PreventGrounding}");

                if (label.Protection.AllowedUsers.Any())
                {
                    sb.AppendLine($"  Allowed Users:   {string.Join(", ", label.Protection.AllowedUsers)}");
                }

                if (label.Protection.AllowedGroups.Any())
                {
                    sb.AppendLine($"  Allowed Groups:  {string.Join(", ", label.Protection.AllowedGroups)}");
                }

                if (label.CustomProperties.Any())
                {
                    sb.AppendLine("Custom Properties:");
                    foreach (var prop in label.CustomProperties)
                    {
                        sb.AppendLine($"  {prop.Key}: {prop.Value}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine(new string('-', 50));
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats validation results as table
    /// </summary>
    private string FormatValidationAsTable(IEnumerable<ValidationResult> results, bool detailed)
    {
        var sb = new StringBuilder();
        var resultList = results.ToList();

        sb.AppendLine("Validation Results");
        sb.AppendLine("==================");
        sb.AppendLine();

        if (!detailed)
        {
            sb.AppendLine($"{"Label",-25} {"Status",-10} {"Message",-50}");
            sb.AppendLine(new string('-', 85));

            foreach (var result in resultList)
            {
                var status = result.IsValid ? "✓ Valid" : "✗ Invalid";
                var message = result.ErrorMessage?.Length > 47 ? 
                    result.ErrorMessage.Substring(0, 47) + "..." : 
                    result.ErrorMessage ?? "";
                    
                sb.AppendLine($"{result.LabelName,-25} {status,-10} {message,-50}");
            }
        }
        else
        {
            foreach (var result in resultList)
            {
                sb.AppendLine($"Label: {result.LabelName}");
                sb.AppendLine($"Status: {(result.IsValid ? "✓ Valid" : "✗ Invalid")}");
                
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    sb.AppendLine($"Message: {result.ErrorMessage}");
                }

                if (result.ValidationDetails.Any())
                {
                    sb.AppendLine("Details:");
                    foreach (var detail in result.ValidationDetails)
                    {
                        sb.AppendLine($"  - {detail}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine(new string('-', 40));
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats classification result as JSON
    /// </summary>
    private string FormatClassificationAsJson(SensitivityLabelResponse result, bool verbose)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (!verbose)
        {
            var summary = new
            {
                label = result.Label?.Name,
                priority = result.Label?.Priority.ToString(),
                shouldDisplay = result.ShouldDisplay,
                allowCopyPaste = result.AllowCopyPaste,
                allowGrounding = result.AllowGrounding
            };
            return JsonSerializer.Serialize(summary, options);
        }

        return JsonSerializer.Serialize(result, options);
    }

    /// <summary>
    /// Formats labels as JSON
    /// </summary>
    private string FormatLabelsAsJson(IEnumerable<SensitivityLabel> labels, bool showDetails)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (!showDetails)
        {
            var summary = labels.Select(l => new
            {
                id = l.Id,
                name = l.Name,
                priority = l.Priority.ToString(),
                isActive = l.IsActive,
                description = l.Description
            });
            return JsonSerializer.Serialize(summary, options);
        }

        return JsonSerializer.Serialize(labels, options);
    }

    /// <summary>
    /// Formats validation results as JSON
    /// </summary>
    private string FormatValidationAsJson(IEnumerable<ValidationResult> results, bool detailed)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(results, options);
    }

    /// <summary>
    /// Formats classification result as XML
    /// </summary>
    private string FormatClassificationAsXml(SensitivityLabelResponse result, bool verbose)
    {
        var doc = new XmlDocument();
        var root = doc.CreateElement("ClassificationResult");
        doc.AppendChild(root);

        if (result.Label != null)
        {
            var labelElement = doc.CreateElement("Label");
            labelElement.SetAttribute("name", result.Label.Name);
            labelElement.SetAttribute("id", result.Label.Id);
            labelElement.SetAttribute("priority", result.Label.Priority.ToString());
            labelElement.SetAttribute("active", result.Label.IsActive.ToString());
            
            var descElement = doc.CreateElement("Description");
            descElement.InnerText = result.Label.Description;
            labelElement.AppendChild(descElement);

            root.AppendChild(labelElement);
        }

        var responseElement = doc.CreateElement("ResponseSettings");
        responseElement.SetAttribute("shouldDisplay", result.ShouldDisplay.ToString());
        responseElement.SetAttribute("allowCopyPaste", result.AllowCopyPaste.ToString());
        responseElement.SetAttribute("allowGrounding", result.AllowGrounding.ToString());
        root.AppendChild(responseElement);

        return FormatXmlOutput(doc);
    }

    /// <summary>
    /// Formats labels as XML
    /// </summary>
    private string FormatLabelsAsXml(IEnumerable<SensitivityLabel> labels, bool showDetails)
    {
        var doc = new XmlDocument();
        var root = doc.CreateElement("SensitivityLabels");
        doc.AppendChild(root);

        foreach (var label in labels)
        {
            var labelElement = doc.CreateElement("Label");
            labelElement.SetAttribute("id", label.Id);
            labelElement.SetAttribute("name", label.Name);
            labelElement.SetAttribute("priority", label.Priority.ToString());
            labelElement.SetAttribute("active", label.IsActive.ToString());

            if (showDetails)
            {
                var descElement = doc.CreateElement("Description");
                descElement.InnerText = label.Description;
                labelElement.AppendChild(descElement);

                var protectionElement = doc.CreateElement("Protection");
                protectionElement.SetAttribute("encryption", label.Protection.RequireEncryption.ToString());
                protectionElement.SetAttribute("preventExtraction", label.Protection.PreventExtraction.ToString());
                protectionElement.SetAttribute("preventCopyPaste", label.Protection.PreventCopyPaste.ToString());
                protectionElement.SetAttribute("preventGrounding", label.Protection.PreventGrounding.ToString());
                labelElement.AppendChild(protectionElement);
            }

            root.AppendChild(labelElement);
        }

        return FormatXmlOutput(doc);
    }

    /// <summary>
    /// Formats validation results as XML
    /// </summary>
    private string FormatValidationAsXml(IEnumerable<ValidationResult> results, bool detailed)
    {
        var doc = new XmlDocument();
        var root = doc.CreateElement("ValidationResults");
        doc.AppendChild(root);

        foreach (var result in results)
        {
            var resultElement = doc.CreateElement("ValidationResult");
            resultElement.SetAttribute("labelName", result.LabelName);
            resultElement.SetAttribute("isValid", result.IsValid.ToString());

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                var messageElement = doc.CreateElement("ErrorMessage");
                messageElement.InnerText = result.ErrorMessage;
                resultElement.AppendChild(messageElement);
            }

            if (detailed && result.ValidationDetails.Any())
            {
                var detailsElement = doc.CreateElement("Details");
                foreach (var detail in result.ValidationDetails)
                {
                    var detailElement = doc.CreateElement("Detail");
                    detailElement.InnerText = detail;
                    detailsElement.AppendChild(detailElement);
                }
                resultElement.AppendChild(detailsElement);
            }

            root.AppendChild(resultElement);
        }

        return FormatXmlOutput(doc);
    }

    /// <summary>
    /// Formats XML document for output
    /// </summary>
    private string FormatXmlOutput(XmlDocument doc)
    {
        var sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            OmitXmlDeclaration = false
        };

        using var writer = XmlWriter.Create(sb, settings);
        doc.WriteTo(writer);
        return sb.ToString();
    }

    /// <summary>
    /// Writes output to file or console
    /// </summary>
    private async Task WriteOutputAsync(string output, string? outputFile)
    {
        if (!string.IsNullOrEmpty(outputFile))
        {
            try
            {
                var directory = Path.GetDirectoryName(outputFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(outputFile, output);
                Console.WriteLine($"Output written to: {outputFile}");
                _logger.LogInformation($"Output written to file: {outputFile}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error writing to file: {outputFile}");
                Console.WriteLine($"Error writing to file: {ex.Message}");
                Console.WriteLine("Output:");
                Console.WriteLine(output);
            }
        }
        else
        {
            Console.WriteLine(output);
        }
    }
}