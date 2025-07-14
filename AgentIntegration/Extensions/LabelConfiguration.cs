using LLMSensitiveDataGoverance.Core.Models;
using System;
using System.Collections.Generic;

namespace LLMSensitiveDataGoverance.AgentIntegration.Configuration
{
    /// <summary>
    /// Configuration settings for sensitivity label management
    /// </summary>
    public class LabelConfiguration
    {
        /// <summary>
        /// Default labels to be created during initialization
        /// </summary>
        public List<SensitivityLabel> DefaultLabels { get; set; } = new();

        /// <summary>
        /// Path to the label configuration file
        /// </summary>
        public string ConfigurationFilePath { get; set; } = "config/labels.json";

        /// <summary>
        /// Whether to auto-create default labels if they don't exist
        /// </summary>
        public bool AutoCreateDefaults { get; set; } = true;

        /// <summary>
        /// Default encryption settings for new labels
        /// </summary>
        public ProtectionSettings DefaultProtectionSettings { get; set; } = new();

        /// <summary>
        /// Custom properties that can be applied to labels
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; } = new();

        /// <summary>
        /// Label validation rules
        /// </summary>
        public LabelValidationRules ValidationRules { get; set; } = new();

        /// <summary>
        /// Whether to enable label inheritance from parent sources
        /// </summary>
        public bool EnableLabelInheritance { get; set; } = true;

        /// <summary>
        /// Maximum number of labels that can be applied to a single resource
        /// </summary>
        public int MaxLabelsPerResource { get; set; } = 5;

        /// <summary>
        /// Default label to apply when no specific label is determined
        /// </summary>
        public string DefaultLabelId { get; set; } = "public";

        /// <summary>
        /// Creates default configuration with standard Microsoft-compatible labels
        /// </summary>
        /// <returns>Configuration with default labels</returns>
        public static LabelConfiguration CreateDefault()
        {
            var config = new LabelConfiguration();
            
            config.DefaultLabels = new List<SensitivityLabel>
            {
                new SensitivityLabel
                {
                    Id = "public",
                    Name = "Public",
                    Description = "Information that can be shared publicly",
                    Priority = LabelPriority.Public,
                    Protection = new ProtectionSettings(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CustomProperties = new Dictionary<string, string>()
                },
                new SensitivityLabel
                {
                    Id = "internal",
                    Name = "Internal",
                    Description = "Information for internal use only",
                    Priority = LabelPriority.Internal,
                    Protection = new ProtectionSettings
                    {
                        AllowedUsers = new List<string>(),
                        AllowedGroups = new List<string> { "employees" }
                    },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CustomProperties = new Dictionary<string, string>()
                },
                new SensitivityLabel
                {
                    Id = "confidential",
                    Name = "Confidential",
                    Description = "Sensitive information requiring special handling",
                    Priority = LabelPriority.Confidential,
                    Protection = new ProtectionSettings
                    {
                        RequireEncryption = true,
                        PreventCopyPaste = true,
                        AllowedUsers = new List<string>(),
                        AllowedGroups = new List<string> { "confidential-access" }
                    },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CustomProperties = new Dictionary<string, string>()
                },
                new SensitivityLabel
                {
                    Id = "highly-confidential",
                    Name = "Highly Confidential",
                    Description = "Highly sensitive information with strict access controls",
                    Priority = LabelPriority.HighlyConfidential,
                    Protection = new ProtectionSettings
                    {
                        RequireEncryption = true,
                        PreventExtraction = true,
                        PreventCopyPaste = true,
                        PreventGrounding = false,
                        AllowedUsers = new List<string>(),
                        AllowedGroups = new List<string> { "highly-confidential-access" }
                    },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CustomProperties = new Dictionary<string, string>()
                },
                new SensitivityLabel
                {
                    Id = "restricted",
                    Name = "Restricted",
                    Description = "Restricted information that cannot be used for AI grounding",
                    Priority = LabelPriority.Restricted,
                    Protection = new ProtectionSettings
                    {
                        RequireEncryption = true,
                        PreventExtraction = true,
                        PreventCopyPaste = true,
                        PreventGrounding = true,
                        AllowedUsers = new List<string>(),
                        AllowedGroups = new List<string> { "restricted-access" }
                    },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CustomProperties = new Dictionary<string, string>()
                }
            };

            return config;
        }
    }

    /// <summary>
    /// Validation rules for sensitivity labels
    /// </summary>
    public class LabelValidationRules
    {
        /// <summary>
        /// Minimum length for label names
        /// </summary>
        public int MinNameLength { get; set; } = 3;

        /// <summary>
        /// Maximum length for label names
        /// </summary>
        public int MaxNameLength { get; set; } = 50;

        /// <summary>
        /// Maximum length for label descriptions
        /// </summary>
        public int MaxDescriptionLength { get; set; } = 500;

        /// <summary>
        /// Whether label IDs must be unique
        /// </summary>
        public bool RequireUniqueIds { get; set; } = true;

        /// <summary>
        /// Whether label names must be unique
        /// </summary>
        public bool RequireUniqueNames { get; set; } = true;

        /// <summary>
        /// Required custom properties for labels
        /// </summary>
        public List<string> RequiredCustomProperties { get; set; } = new();

        /// <summary>
        /// Pattern for valid label IDs
        /// </summary>
        public string IdPattern { get; set; } = @"^[a-z0-9-_]+$";

        /// <summary>
        /// Whether to validate protection settings consistency
        /// </summary>
        public bool ValidateProtectionConsistency { get; set; } = true;
    }
}