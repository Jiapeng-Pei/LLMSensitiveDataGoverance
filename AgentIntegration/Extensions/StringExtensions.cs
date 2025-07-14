using LLMSensitiveDataGoverance.Core.Models;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace LLMSensitiveDataGoverance.AgentIntegration.Extensions
{
    /// <summary>
    /// Extension methods for string manipulation related to sensitivity labels
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Applies visual formatting to indicate sensitivity level
        /// </summary>
        /// <param name="content">The content to format</param>
        /// <param name="label">The sensitivity label</param>
        /// <returns>Formatted content with visual indicators</returns>
        public static string ApplyVisualSensitivityIndicators(this string content, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(content) || label == null)
                return content;

            var sb = new StringBuilder();
            
            // Add header with sensitivity indicator
            sb.AppendLine($"🔒 {label.Name} Information");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();
            
            // Add the content
            sb.AppendLine(content);
            sb.AppendLine();
            
            // Add footer with protection information
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"Sensitivity Level: {label.Priority}");
            
            if (label.Protection.PreventCopyPaste)
                sb.AppendLine("⚠️  Copy/Paste restrictions apply");
            
            if (label.Protection.RequireEncryption)
                sb.AppendLine("🔐 Encryption required");
            
            sb.AppendLine($"Classification: {label.Description}");
            
            return sb.ToString();
        }

        /// <summary>
        /// Sanitizes content by removing sensitive information patterns
        /// </summary>
        /// <param name="content">The content to sanitize</param>
        /// <returns>Sanitized content</returns>
        public static string SanitizeSensitiveContent(this string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            // Remove email addresses
            content = Regex.Replace(content, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", 
                "[EMAIL_REDACTED]", RegexOptions.IgnoreCase);

            // Remove phone numbers (basic patterns)
            content = Regex.Replace(content, @"(\+?1[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}", 
                "[PHONE_REDACTED]");

            // Remove SSN patterns
            content = Regex.Replace(content, @"\b\d{3}-\d{2}-\d{4}\b", "[SSN_REDACTED]");

            // Remove credit card numbers (basic pattern)
            content = Regex.Replace(content, @"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", 
                "[CARD_REDACTED]");

            return content;
        }

        /// <summary>
        /// Adds copy-paste protection markers to content
        /// </summary>
        /// <param name="content">The content to protect</param>
        /// <param name="label">The sensitivity label</param>
        /// <returns>Content with copy-paste protection markers</returns>
        public static string AddCopyPasteProtection(this string content, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(content) || label == null || !label.Protection.PreventCopyPaste)
                return content;

            var protectedContent = new StringBuilder();
            protectedContent.AppendLine("--- COPY PROTECTION START ---");
            protectedContent.AppendLine($"Label: {label.Name}");
            protectedContent.AppendLine($"Protection Level: {label.Priority}");
            protectedContent.AppendLine("--- PROTECTED CONTENT ---");
            protectedContent.AppendLine(content);
            protectedContent.AppendLine("--- COPY PROTECTION END ---");
            protectedContent.AppendLine("This content is protected and copying is restricted.");

            return protectedContent.ToString();
        }

        /// <summary>
        /// Truncates content if it exceeds maximum length for the given sensitivity level
        /// </summary>
        /// <param name="content">The content to truncate</param>
        /// <param name="label">The sensitivity label</param>
        /// <param name="maxLength">Maximum allowed length</param>
        /// <returns>Truncated content if necessary</returns>
        public static string TruncateForSensitivity(this string content, SensitivityLabel label, int maxLength = 1000)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
                return content;

            var truncated = content.Substring(0, maxLength - 50);
            return $"{truncated}... [Content truncated due to {label?.Name ?? "sensitivity"} restrictions]";
        }

        /// <summary>
        /// Extracts metadata tags from formatted content
        /// </summary>
        /// <param name="content">The content containing metadata</param>
        /// <returns>Extracted metadata or empty string if none found</returns>
        public static string ExtractSensitivityMetadata(this string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            var match = Regex.Match(content, @"Sensitivity Level: (.+?)$", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
    }
}