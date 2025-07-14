using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Exceptions;
using LLMSensitiveDataGoverance.AgentIntegration.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LLMSensitiveDataGoverance.AgentIntegration
{
    /// <summary>
    /// Processes LLM responses to apply sensitivity labels and enforce protection policies
    /// </summary>
    public class LLMResponseProcessor
    {
        private readonly ISensitivityLabelService _labelService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<LLMResponseProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the LLMResponseProcessor
        /// </summary>
        /// <param name="labelService">The sensitivity label service</param>
        /// <param name="encryptionService">The encryption service</param>
        /// <param name="logger">The logger instance</param>
        public LLMResponseProcessor(
            ISensitivityLabelService labelService,
            IEncryptionService encryptionService,
            ILogger<LLMResponseProcessor> logger)
        {
            _labelService = labelService ?? throw new ArgumentNullException(nameof(labelService));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes an LLM response and applies sensitivity labels
        /// </summary>
        /// <param name="response">The raw LLM response</param>
        /// <param name="groundingData">The grounding data used to generate the response</param>
        /// <returns>Processed response with sensitivity labels applied</returns>
        public async Task<SensitivityLabelResponse> ProcessResponseAsync(
            string response, 
            IEnumerable<GroundingData> groundingData)
        {
            if (string.IsNullOrEmpty(response))
                throw new ArgumentNullException(nameof(response));

            if (groundingData == null || !groundingData.Any())
                return CreatePublicResponse(response);

            try
            {
                _logger.LogDebug("Processing LLM response with {Count} grounding data sources", 
                    groundingData.Count());

                // Determine the highest priority label from grounding data
                var labels = groundingData.Select(g => g.Label).Where(l => l != null);
                var highestPriorityLabel = await _labelService.GetHighestPriorityLabelAsync(labels);

                if (highestPriorityLabel == null)
                    return CreatePublicResponse(response);

                // Process response with the determined label
                return await _labelService.ProcessLLMResponseAsync(response, highestPriorityLabel);
            }
            catch (SensitivityLabelException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing LLM response");
                throw new SensitivityLabelException("system", "Failed to process LLM response");
            }
        }

        /// <summary>
        /// Validates if grounding data can be used for response generation
        /// </summary>
        /// <param name="groundingData">The grounding data to validate</param>
        /// <returns>Collection of valid grounding data that can be used</returns>
        public async Task<IEnumerable<GroundingData>> ValidateGroundingDataAsync(
            IEnumerable<GroundingData> groundingData)
        {
            if (groundingData == null || !groundingData.Any())
                return Enumerable.Empty<GroundingData>();

            try
            {
                _logger.LogDebug("Validating {Count} grounding data sources", groundingData.Count());

                var validatedData = new List<GroundingData>();

                foreach (var data in groundingData)
                {
                    if (await CanUseForGroundingAsync(data))
                    {
                        validatedData.Add(data);
                    }
                    else
                    {
                        _logger.LogWarning("Grounding data {Id} cannot be used due to protection settings", 
                            data.Id);
                    }
                }

                return validatedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating grounding data");
                throw new SensitivityLabelException("system", "Failed to validate grounding data");
            }
        }

        /// <summary>
        /// Formats response content with visual sensitivity indicators
        /// </summary>
        /// <param name="content">The content to format</param>
        /// <param name="label">The sensitivity label to apply</param>
        /// <returns>Formatted content with visual indicators</returns>
        public async Task<string> FormatWithVisualIndicatorsAsync(string content, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(content) || label == null)
                return content;

            try
            {
                _logger.LogDebug("Formatting content with visual indicators for label: {LabelId}", label.Id);
                return await _labelService.FormatResponseWithLabelAsync(content, label);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting content with visual indicators");
                return content; // Return original content if formatting fails
            }
        }

        /// <summary>
        /// Applies encryption to sensitive content if required
        /// </summary>
        /// <param name="content">The content to encrypt</param>
        /// <param name="label">The sensitivity label</param>
        /// <returns>Encrypted content or original content if encryption is not required</returns>
        public async Task<string> ApplyEncryptionAsync(string content, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(content) || label == null)
                return content;

            try
            {
                if (await _encryptionService.ShouldEncryptAsync(label))
                {
                    _logger.LogDebug("Applying encryption for label: {LabelId}", label.Id);
                    return await _encryptionService.EncryptAsync(content, label);
                }

                return content;
            }
            catch (EncryptionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying encryption for label: {LabelId}", label.Id);
                throw new EncryptionException(label.Id, "encrypt", ex.Message);
            }
        }

        /// <summary>
        /// Creates a public response for content without sensitivity labels
        /// </summary>
        /// <param name="content">The response content</param>
        /// <returns>Public sensitivity label response</returns>
        private SensitivityLabelResponse CreatePublicResponse(string content)
        {
            return new SensitivityLabelResponse
            {
                Label = new SensitivityLabel 
                { 
                    Id = "public", 
                    Name = "Public", 
                    Priority = LabelPriority.Public,
                    Protection = new ProtectionSettings(),
                    IsActive = true
                },
                Content = content,
                FormattedResponse = content,
                ShouldDisplay = true,
                AllowCopyPaste = true,
                AllowGrounding = true
            };
        }

        /// <summary>
        /// Determines if grounding data can be used for response generation
        /// </summary>
        /// <param name="data">The grounding data to check</param>
        /// <returns>True if the data can be used for grounding, false otherwise</returns>
        private async Task<bool> CanUseForGroundingAsync(GroundingData data)
        {
            if (data?.Label == null)
                return true; // No label means public

            return !data.Label.Protection.PreventGrounding && 
                   data.Label.IsActive &&
                   await _labelService.ValidateLabelAsync(data.Label);
        }
    }
}