using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for sensitivity label validation operations.
    /// </summary>
    public interface ILabelValidator
    {
        /// <summary>
        /// Validates a sensitivity label.
        /// </summary>
        /// <param name="label">The sensitivity label to validate.</param>
        /// <returns>A task representing whether the label is valid.</returns>
        Task<bool> ValidateAsync(SensitivityLabel label);

        /// <summary>
        /// Validates a sensitivity label and returns detailed validation results.
        /// </summary>
        /// <param name="label">The sensitivity label to validate.</param>
        /// <returns>A task representing the detailed validation result.</returns>
        Task<ValidationResult> ValidateDetailedAsync(SensitivityLabel label);

        /// <summary>
        /// Determines if a sensitivity label can be applied to the given grounding data.
        /// </summary>
        /// <param name="data">The grounding data to check.</param>
        /// <param name="label">The sensitivity label to apply.</param>
        /// <returns>A task representing whether the label can be applied.</returns>
        Task<bool> CanApplyLabelAsync(GroundingData data, SensitivityLabel label);

        /// <summary>
        /// Determines if two sensitivity labels are compatible.
        /// </summary>
        /// <param name="label1">The first sensitivity label.</param>
        /// <param name="label2">The second sensitivity label.</param>
        /// <returns>A task representing whether the labels are compatible.</returns>
        Task<bool> IsLabelCompatibleAsync(SensitivityLabel label1, SensitivityLabel label2);

        /// <summary>
        /// Validates the protection settings of a sensitivity label.
        /// </summary>
        /// <param name="protectionSettings">The protection settings to validate.</param>
        /// <returns>A task representing the validation result.</returns>
        Task<ValidationResult> ValidateProtectionSettingsAsync(ProtectionSettings protectionSettings);

        /// <summary>
        /// Validates if a user has permission to apply a sensitivity label.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="label">The sensitivity label to check.</param>
        /// <returns>A task representing whether the user has permission.</returns>
        Task<bool> HasPermissionAsync(string userId, SensitivityLabel label);

        /// <summary>
        /// Validates the priority hierarchy of sensitivity labels.
        /// </summary>
        /// <param name="labels">The collection of labels to validate.</param>
        /// <returns>A task representing the validation result.</returns>
        Task<ValidationResult> ValidatePriorityHierarchyAsync(IEnumerable<SensitivityLabel> labels);
    }
}
