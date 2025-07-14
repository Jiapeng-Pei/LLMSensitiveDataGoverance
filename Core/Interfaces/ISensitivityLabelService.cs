using System.Collections.Generic;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for sensitivity label service operations.
    /// </summary>
    public interface ISensitivityLabelService
    {
        /// <summary>
        /// Classifies grounding data and applies appropriate sensitivity labels.
        /// </summary>
        /// <param name="data">The grounding data to classify.</param>
        /// <returns>A task representing the classification result.</returns>
        Task<SensitivityLabelResponse> ClassifyAsync(GroundingData data);

        /// <summary>
        /// Processes an LLM response with the given sensitivity label.
        /// </summary>
        /// <param name="response">The LLM response to process.</param>
        /// <param name="label">The sensitivity label to apply.</param>
        /// <returns>A task representing the processed response.</returns>
        Task<SensitivityLabelResponse> ProcessLLMResponseAsync(string response, SensitivityLabel label);

        /// <summary>
        /// Gets the highest priority label from a collection of labels.
        /// </summary>
        /// <param name="labels">The collection of labels to evaluate.</param>
        /// <returns>A task representing the highest priority label.</returns>
        Task<SensitivityLabel> GetHighestPriorityLabelAsync(IEnumerable<SensitivityLabel> labels);

        /// <summary>
        /// Validates a sensitivity label.
        /// </summary>
        /// <param name="label">The label to validate.</param>
        /// <returns>A task representing the validation result.</returns>
        Task<bool> ValidateLabelAsync(SensitivityLabel label);

        /// <summary>
        /// Formats a response with sensitivity label information.
        /// </summary>
        /// <param name="content">The content to format.</param>
        /// <param name="label">The sensitivity label to apply.</param>
        /// <returns>A task representing the formatted response.</returns>
        Task<string> FormatResponseWithLabelAsync(string content, SensitivityLabel label);

        /// <summary>
        /// Determines if content can be used for grounding based on its label.
        /// </summary>
        /// <param name="data">The grounding data to check.</param>
        /// <returns>A task representing whether grounding is allowed.</returns>
        Task<bool> CanUseForGroundingAsync(GroundingData data);

        /// <summary>
        /// Merges multiple sensitivity labels into a single label with combined restrictions.
        /// </summary>
        /// <param name="labels">The labels to merge.</param>
        /// <returns>A task representing the merged label.</returns>
        Task<SensitivityLabel> MergeLabelsAsync(IEnumerable<SensitivityLabel> labels);
    }
}
