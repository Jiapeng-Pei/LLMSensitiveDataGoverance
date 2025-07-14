using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Services
{
    /// <summary>
    /// Service for validating sensitivity labels
    /// </summary>
    public class LabelValidationService : ILabelValidator
    {
        private readonly ILabelRepository _labelRepository;

        /// <summary>
        /// Initializes a new instance of the LabelValidationService
        /// </summary>
        /// <param name="labelRepository">Repository for label data access</param>
        public LabelValidationService(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
        }

        /// <summary>
        /// Validates a sensitivity label
        /// </summary>
        /// <param name="label">Label to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public async Task<bool> ValidateAsync(SensitivityLabel label)
        {
            var result = await ValidateDetailedAsync(label);
            return result.IsValid;
        }

        /// <summary>
        /// Validates a sensitivity label with detailed results
        /// </summary>
        /// <param name="label">Label to validate</param>
        /// <returns>Detailed validation result</returns>
        public async Task<ValidationResult> ValidateDetailedAsync(SensitivityLabel label)
        {
            var result = new ValidationResult { IsValid = true };

            if (label == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Label cannot be null";
                return result;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(label.Id))
            {
                result.IsValid = false;
                result.ErrorMessage = "Label ID is required";
                return result;
            }

            if (string.IsNullOrWhiteSpace(label.Name))
            {
                result.IsValid = false;
                result.ErrorMessage = "Label name is required";
                return result;
            }

            // Validate ID format
            if (!IsValidIdFormat(label.Id))
            {
                result.IsValid = false;
                result.ErrorMessage = "Label ID must be alphanumeric with hyphens only";
                return result;
            }

            // Validate name uniqueness
            var existingLabel = await _labelRepository.GetByNameAsync(label.Name);
            if (existingLabel != null && existingLabel.Id != label.Id)
            {
                result.IsValid = false;
                result.ErrorMessage = "Label name must be unique";
                return result;
            }

            // Validate priority
            if (!Enum.IsDefined(typeof(LabelPriority), label.Priority))
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid label priority";
                return result;
            }

            // Validate protection settings
            if (label.Protection == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Protection settings are required";
                return result;
            }

            // Validate custom properties
            if (label.CustomProperties != null)
            {
                foreach (var property in label.CustomProperties)
                {
                    if (string.IsNullOrWhiteSpace(property.Key))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "Custom property keys cannot be empty";
                        return result;
                    }
                }
            }

            // Validate dates
            if (label.CreatedAt == default || label.UpdatedAt == default)
            {
                result.IsValid = false;
                result.ErrorMessage = "Created and updated dates are required";
                return result;
            }

            if (label.UpdatedAt < label.CreatedAt)
            {
                result.IsValid = false;
                result.ErrorMessage = "Updated date cannot be before created date";
                return result;
            }

            result.IsValid = true;
            return result;
        }

        /// <summary>
        /// Determines if a label can be applied to grounding data
        /// </summary>
        /// <param name="data">Grounding data to check</param>
        /// <param name="label">Label to apply</param>
        /// <returns>True if label can be applied, false otherwise</returns>
        public async Task<bool> CanApplyLabelAsync(GroundingData data, SensitivityLabel label)
        {
            if (data == null || label == null)
                return false;

            // Check if label is active
            if (!label.IsActive)
                return false;

            // Validate the label itself
            if (!await ValidateAsync(label))
                return false;

            // Check if data type is compatible with label
            if (!IsDataTypeCompatible(data.DataType, label))
                return false;

            // Check if existing label can be overridden
            if (data.Label != null && !CanOverrideLabel(data.Label, label))
                return false;

            return true;
        }

        /// <summary>
        /// Determines if two labels are compatible
        /// </summary>
        /// <param name="label1">First label</param>
        /// <param name="label2">Second label</param>
        /// <returns>True if compatible, false otherwise</returns>
        public async Task<bool> IsLabelCompatibleAsync(SensitivityLabel label1, SensitivityLabel label2)
        {
            if (label1 == null || label2 == null)
                return false;

            // Labels are compatible if they have the same or compatible priorities
            // Higher priority labels can override lower priority ones
            if (label1.Priority == label2.Priority)
                return true;

            // Check if protection settings are compatible
            if (label1.Protection.RequireEncryption && !label2.Protection.RequireEncryption)
                return false;

            if (label1.Protection.PreventExtraction && !label2.Protection.PreventExtraction)
                return false;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validates ID format
        /// </summary>
        private bool IsValidIdFormat(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            // Allow alphanumeric characters and hyphens
            return id.All(c => char.IsLetterOrDigit(c) || c == '-');
        }

        /// <summary>
        /// Checks if data type is compatible with label
        /// </summary>
        private bool IsDataTypeCompatible(string dataType, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(dataType))
                return true; // No restriction

            // Check if label has restrictions on data types
            if (label.CustomProperties?.ContainsKey("SupportedDataTypes") == true)
            {
                var supportedTypes = label.CustomProperties["SupportedDataTypes"].Split(',');
                return supportedTypes.Contains(dataType, StringComparer.OrdinalIgnoreCase);
            }

            return true; // No restrictions
        }

        /// <summary>
        /// Determines if an existing label can be overridden
        /// </summary>
        private bool CanOverrideLabel(SensitivityLabel existingLabel, SensitivityLabel newLabel)
        {
            // Higher priority labels can override lower priority ones
            return newLabel.Priority >= existingLabel.Priority;
        }
    }

    /// <summary>
    /// Result of label validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }
}