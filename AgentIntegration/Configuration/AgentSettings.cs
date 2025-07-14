using LLMSensitiveDataGoverance.Core.Models;
using System;
using System.Collections.Generic;

namespace LLMSensitiveDataGoverance.AgentIntegration.Configuration
{
    /// <summary>
    /// Configuration settings for AI agent integration
    /// </summary>
    public class AgentSettings
    {
        /// <summary>
        /// Maximum allowed sensitivity priority for grounding data
        /// </summary>
        public LabelPriority? MaxAllowedPriority { get; set; } = LabelPriority.HighlyConfidential;

        /// <summary>
        /// Whether to enable strict validation of grounding data
        /// </summary>
        public bool StrictValidation { get; set; } = false;

        /// <summary>
        /// Maximum content length for grounding data
        /// </summary>
        public int MaxContentLength { get; set; } = 100000;

        /// <summary>
        /// Maximum age of data that can be used for grounding
        /// </summary>
        public TimeSpan MaxDataAge { get; set; } = TimeSpan.FromDays(365);

        /// <summary>
        /// Allowed sources for grounding data
        /// </summary>
        public HashSet<string> AllowedSources { get; set; } = new() 
        { 
            "file", 
            "database", 
            "api", 
            "sharepoint", 
            "teams", 
            "unknown" 
        };

        /// <summary>
        /// Allowed data types for grounding
        /// </summary>
        public HashSet<string> AllowedDataTypes { get; set; } = new() 
        { 
            "text", 
            "document", 
            "spreadsheet", 
            "presentation", 
            "email", 
            "chat" 
        };

        /// <summary>
        /// Whether to automatically classify unlabeled data
        /// </summary>
        public bool AutoClassifyUnlabeledData { get; set; } = true;

        /// <summary>
        /// Default classification for unlabeled data
        /// </summary>
        public string DefaultClassification { get; set; } = "internal";

        /// <summary>
        /// Whether to enable visual indicators in responses
        /// </summary>
        public bool EnableVisualIndicators { get; set; } = true;

        /// <summary>
        /// Whether to enable copy-paste protection
        /// </summary>
        public bool EnableCopyPasteProtection { get; set; } = true;

        /// <summary>
        /// Whether to enable content encryption for sensitive data
        /// </summary>
        public bool EnableEncryption { get; set; } = false;

        /// <summary>
        /// Whether to log sensitivity label operations
        /// </summary>
        public bool EnableAuditLogging { get; set; } = true;

        /// <summary>
        /// Timeout for label classification operations
        /// </summary>
        public TimeSpan ClassificationTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Whether to cache classification results
        /// </summary>
        public bool EnableClassificationCache { get; set; } = true;

        /// <summary>
        /// Cache expiration time for classification results
        /// </summary>
        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Custom response formatting settings
        /// </summary>
        public ResponseFormattingSettings ResponseFormatting { get; set; } = new();

        /// <summary>
        /// Integration-specific settings
        /// </summary>
        public Dictionary<string, object> IntegrationSettings { get; set; } = new();

        /// <summary>
        /// Creates default agent settings
        /// </summary>
        /// <returns>Default configuration</returns>
        public static AgentSettings CreateDefault()
        {
            return new AgentSettings
            {
                MaxAllowedPriority = LabelPriority.HighlyConfidential,
                StrictValidation = false,
                AutoClassifyUnlabeledData = true,
                EnableVisualIndicators = true,
                EnableCopyPasteProtection = true,
                EnableAuditLogging = true,
                EnableClassificationCache = true
            };
        }

        /// <summary>
        /// Creates settings optimized for high-security environments
        /// </summary>
        /// <returns>High-security configuration</returns>
        public static AgentSettings CreateHighSecuritySettings()
        {
            return new AgentSettings
            {
                MaxAllowedPriority = LabelPriority.Confidential,
                StrictValidation = true,
                MaxContentLength = 50000,
                MaxDataAge = TimeSpan.FromDays(90),
                AutoClassifyUnlabeledData = true,
                DefaultClassification = "confidential",
                EnableVisualIndicators = true,
                EnableCopyPasteProtection = true,
                EnableEncryption = true,
                EnableAuditLogging = true,
                ClassificationTimeout = TimeSpan.FromSeconds(15),
                EnableClassificationCache = false
            };
        }
    }

    /// <summary>
    /// Settings for response formatting
    /// </summary>
    public class ResponseFormattingSettings
    {
        /// <summary>
        /// Whether to include sensitivity headers in responses
        /// </summary>
        public bool IncludeSensitivityHeaders { get; set; } = true;

        /// <summary>
        /// Whether to include protection warnings
        /// </summary>
        public bool IncludeProtectionWarnings { get; set; } = true;

        /// <summary>
        /// Whether to include classification metadata
        /// </summary>
        public bool IncludeClassificationMetadata { get; set; } = false;

        /// <summary>
        /// Custom formatting templates for different sensitivity levels
        /// </summary>
        public Dictionary<LabelPriority, string> CustomTemplates { get; set; } = new();

        /// <summary>
        /// Whether to use colored indicators (if supported)
        /// </summary>
        public bool UseColoredIndicators { get; set; } = false;

        /// <summary>
        /// Maximum response length before truncation
        /// </summary>
        public int MaxResponseLength { get; set; } = 10000;
    }
}
