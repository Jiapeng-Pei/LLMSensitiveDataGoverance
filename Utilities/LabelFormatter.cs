using System;
using System.Text;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Utilities
{
    /// <summary>
    /// Utility for formatting content with sensitivity labels
    /// </summary>
    public class LabelFormatter
    {
        private readonly string _labelPrefix = "[SENSITIVITY: ";
        private readonly string _labelSuffix = "]";
        private readonly string _copyPasteWarning = "\n\n⚠️ This content contains sensitive information. Please handle appropriately.";

        /// <summary>
        /// Formats content with sensitivity label information
        /// </summary>
        /// <param name="content">Content to format</param>
        /// <param name="label">Sensitivity label to apply</param>
        /// <returns>Formatted content</returns>
        public async Task<string> FormatWithLabelAsync(string content, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            if (label == null)
                return content;

            var formattedContent = new StringBuilder();

            // Add label header
            formattedContent.AppendLine(CreateLabelHeader(label));
            formattedContent.AppendLine();

            // Add original content
            formattedContent.AppendLine(content);

            // Add copy/paste warning if needed
            if (label.Protection.PreventCopyPaste)
            {
                formattedContent.AppendLine(_copyPasteWarning);
            }

            // Add label footer
            formattedContent.AppendLine();
            formattedContent.AppendLine(CreateLabelFooter(label));

            return await Task.FromResult(formattedContent.ToString());
        }

        /// <summary>
        /// Creates a simple label tag for inline use
        /// </summary>
        /// <param name="label">Sensitivity label</param>
        /// <returns>Label tag string</returns>
        public string CreateLabelTag(SensitivityLabel label)
        {
            if (label == null)
                return string.Empty;

            return $"{_labelPrefix}{label.Name.ToUpper()}{_labelSuffix}";
        }

        /// <summary>
        /// Creates label header with visual formatting
        /// </summary>
        /// <param name="label">Sensitivity label</param>
        /// <returns>Formatted header</returns>
        private string CreateLabelHeader(SensitivityLabel label)
        {
            var header = new StringBuilder();
            var border = GetBorderForPriority(label.Priority);
            var icon = GetIconForPriority(label.Priority);

            header.AppendLine(border);
            header.AppendLine($"{icon} {label.Name.ToUpper()} - {label.Description}");
            header.AppendLine(border);

            return header.ToString();
        }

        /// <summary>
        /// Creates label footer with metadata
        /// </summary>
        /// <param name="label">Sensitivity label</param>
        /// <returns>Formatted footer</returns>
        private string CreateLabelFooter(SensitivityLabel label)
        {
            var footer = new StringBuilder();
            footer.AppendLine("═══════════════════════════════════════");
            footer.AppendLine($"Classification: {label.Name} | Priority: {label.Priority}");
            
            if (label.Protection.RequireEncryption)
                footer.AppendLine("🔒 Encryption Required");
            
            if (label.Protection.PreventExtraction)
                footer.AppendLine("🚫 Extraction Prohibited");
            
            if (label.Protection.PreventCopyPaste)
                footer.AppendLine("📋 Copy/Paste Restricted");

            footer.AppendLine("═══════════════════════════════════════");

            return footer.ToString();
        }

        /// <summary>
        /// Gets border style for priority level
        /// </summary>
        /// <param name="priority">Label priority</param>
        /// <returns>Border string</returns>
        private string GetBorderForPriority(LabelPriority priority)
        {
            return priority switch
            {
                LabelPriority.Public => "═══════════════════════════════════════",
                LabelPriority.Internal => "▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓",
                LabelPriority.Confidential => "████████████████████████████████████████",
                LabelPriority.HighlyConfidential => "■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■",
                LabelPriority.Restricted => "🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴",
                _ => "═══════════════════════════════════════"
            };
        }

        /// <summary>
        /// Gets icon for priority level
        /// </summary>
        /// <param name="priority">Label priority</param>
        /// <returns>Icon string</returns>
        private string GetIconForPriority(LabelPriority priority)
        {
            return priority switch
            {
                LabelPriority.Public => "🌍",
                LabelPriority.Internal => "🏢",
                LabelPriority.Confidential => "🔒",
                LabelPriority.HighlyConfidential => "🔐",
                LabelPriority.Restricted => "🚫",
                _ => "ℹ️"
            };
        }

        /// <summary>
        /// Strips label formatting from content
        /// </summary>
        /// <param name="formattedContent">Formatted content</param>
        /// <returns>Clean content without labels</returns>
        public async Task<string> StripLabelFormattingAsync(string formattedContent)
        {
            if (string.IsNullOrEmpty(formattedContent))
                return formattedContent;

            var lines = formattedContent.Split('\n', StringSplitOptions.None);
            var cleanContent = new StringBuilder();
            var inContent = false;

            foreach (var line in lines)
            {
                // Skip border lines and headers
                if (line.Contains("═══") || line.Contains("▓▓▓") || line.Contains("████") || 
                    line.Contains("■■■") || line.Contains("🔴"))
                {
                    if (inContent)
                        break; // End of content
                    continue;
                }

                // Skip icon lines
                if (line.Contains("🌍") || line.Contains("🏢") || line.Contains("🔒") || 
                    line.Contains("🔐") || line.Contains("🚫"))
                {
                    inContent = true;
                    continue;
                }

                // Skip warning lines
                if (line.Contains("⚠️") || line.Contains("Classification:") || 
                    line.Contains("🔒 Encryption") || line.Contains("🚫 Extraction") || 
                    line.Contains("📋 Copy/Paste"))
                {
                    continue;
                }

                // Add content lines
                if (inContent && !string.IsNullOrEmpty(line.Trim()))
                {
                    cleanContent.AppendLine(line);
                }
            }

            return await Task.FromResult(cleanContent.ToString().Trim());
        }
    }
}


namespace LLMSensitiveDataGoverance.Core.Models
{
    /// <summary>
    /// Result of label validation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates if the validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// List of validation warnings
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Additional validation metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}