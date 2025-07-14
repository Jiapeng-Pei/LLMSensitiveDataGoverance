using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LLMSensitiveDataGoverance.AgentIntegration
{
    /// <summary>
    /// Provides sensitivity label information and classification services for AI agents
    /// </summary>
    public class AgentLabelProvider
    {
        private readonly ISensitivityLabelService _labelService;
        private readonly ILabelRepository _labelRepository;
        private readonly ILogger<AgentLabelProvider> _logger;

        /// <summary>
        /// Initializes a new instance of the AgentLabelProvider
        /// </summary>
        /// <param name="labelService">The sensitivity label service</param>
        /// <param name="labelRepository">The label repository</param>
        /// <param name="logger">The logger instance</param>
        public AgentLabelProvider(
            ISensitivityLabelService labelService,
            ILabelRepository labelRepository,
            ILogger<AgentLabelProvider> logger)
        {
            _labelService = labelService ?? throw new ArgumentNullException(nameof(labelService));
            _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all available sensitivity labels for agent usage
        /// </summary>
        /// <returns>Collection of available sensitivity labels</returns>
        public async Task<IEnumerable<SensitivityLabel>> GetAvailableLabelsAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving available sensitivity labels");
                var labels = await _labelRepository.GetAllAsync();
                return labels.Where(l => l.IsActive).OrderBy(l => l.Priority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available labels");
                throw new SensitivityLabelException("system", "Failed to retrieve available labels");
            }
        }

        /// <summary>
        /// Classifies grounding data and returns appropriate sensitivity label
        /// </summary>
        /// <param name="groundingData">The grounding data to classify</param>
        /// <returns>Sensitivity label response with classification results</returns>
        public async Task<SensitivityLabelResponse> ClassifyGroundingDataAsync(GroundingData groundingData)
        {
            if (groundingData == null)
                throw new ArgumentNullException(nameof(groundingData));

            try
            {
                _logger.LogDebug("Classifying grounding data: {GroundingId}", groundingData.Id);
                return await _labelService.ClassifyAsync(groundingData);
            }
            catch (SensitivityLabelException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying grounding data: {GroundingId}", groundingData.Id);
                throw new SensitivityLabelException(groundingData.Id, "Failed to classify grounding data");
            }
        }

        /// <summary>
        /// Determines the highest priority label from a collection of labels
        /// </summary>
        /// <param name="labels">Collection of sensitivity labels</param>
        /// <returns>The highest priority label</returns>
        public async Task<SensitivityLabel> GetHighestPriorityLabelAsync(IEnumerable<SensitivityLabel> labels)
        {
            if (labels == null || !labels.Any())
                return null;

            try
            {
                _logger.LogDebug("Determining highest priority label from {Count} labels", labels.Count());
                return await _labelService.GetHighestPriorityLabelAsync(labels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining highest priority label");
                throw new SensitivityLabelException("system", "Failed to determine highest priority label");
            }
        }

        /// <summary>
        /// Validates if a label can be applied to specific grounding data
        /// </summary>
        /// <param name="groundingData">The grounding data</param>
        /// <param name="label">The sensitivity label to validate</param>
        /// <returns>True if the label can be applied, false otherwise</returns>
        public async Task<bool> CanApplyLabelAsync(GroundingData groundingData, SensitivityLabel label)
        {
            if (groundingData == null || label == null)
                return false;

            try
            {
                _logger.LogDebug("Validating label application: {LabelId} to {GroundingId}", 
                    label.Id, groundingData.Id);
                
                var isValid = await _labelService.ValidateLabelAsync(label);
                if (!isValid)
                    return false;

                // Additional validation logic for agent context
                return label.IsActive && 
                       !label.Protection.PreventGrounding;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating label application: {LabelId} to {GroundingId}", 
                    label.Id, groundingData.Id);
                return false;
            }
        }
    }
}