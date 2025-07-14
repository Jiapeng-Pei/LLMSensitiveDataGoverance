using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Exceptions;
using LLMSensitiveDataGoverance.Core.Utilities;

namespace LLMSensitiveDataGoverance.Core.Services
{
    /// <summary>
    /// Core service for managing sensitivity labels and processing LLM responses
    /// </summary>
    public class SensitivityLabelService : ISensitivityLabelService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly ILabelValidator _labelValidator;
        private readonly LabelClassificationService _classificationService;
        private readonly LabelFormatter _labelFormatter;
        private readonly LabelPriorityComparer _priorityComparer;

        /// <summary>
        /// Initializes a new instance of the SensitivityLabelService
        /// </summary>
        /// <param name="labelRepository">Repository for label data access</param>
        /// <param name="encryptionService">Service for encryption operations</param>
        /// <param name="labelValidator">Service for label validation</param>
        /// <param name="classificationService">Service for content classification</param>
        /// <param name="labelFormatter">Utility for label formatting</param>
        /// <param name="priorityComparer">Utility for label priority comparison</param>
        public SensitivityLabelService(
            ILabelRepository labelRepository,
            IEncryptionService encryptionService,
            ILabelValidator labelValidator,
            LabelClassificationService classificationService,
            LabelFormatter labelFormatter,
            LabelPriorityComparer priorityComparer)
        {
            _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _labelValidator = labelValidator ?? throw new ArgumentNullException(nameof(labelValidator));
            _classificationService = classificationService ?? throw new ArgumentNullException(nameof(classificationService));
            _labelFormatter = labelFormatter ?? throw new ArgumentNullException(nameof(labelFormatter));
            _priorityComparer = priorityComparer ?? throw new ArgumentNullException(nameof(priorityComparer));
        }

        /// <summary>
        /// Classifies grounding data and returns appropriate sensitivity label response
        /// </summary>
        /// <param name="data">The grounding data to classify</param>
        /// <returns>A sensitivity label response with classification results</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="SensitivityLabelException">Thrown when classification fails</exception>
        public async Task<SensitivityLabelResponse> ClassifyAsync(GroundingData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try
            {
                // If data already has a label, validate and use it
                if (data.Label != null)
                {
                    var isValid = await _labelValidator.ValidateAsync(data.Label);
                    if (!isValid)
                        throw new InvalidLabelException(data.Label.Id, new ValidationResult { IsValid = false, ErrorMessage = "Invalid existing label" });

                    return await CreateLabelResponseAsync(data.Content, data.Label);
                }

                // Classify the content to determine appropriate label
                var classificationResult = await _classificationService.ClassifyContentAsync(data.Content, data.Metadata);
                var label = await _labelRepository.GetByIdAsync(classificationResult.SuggestedLabelId);

                if (label == null)
                {
                    // Fall back to default internal label if classification fails
                    label = await GetDefaultLabelAsync();
                }

                return await CreateLabelResponseAsync(data.Content, label);
            }
            catch (Exception ex) when (!(ex is SensitivityLabelException))
            {
                throw new SensitivityLabelException(data.Label?.Id ?? "unknown", $"Classification failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes LLM response with sensitivity label information
        /// </summary>
        /// <param name="response">The LLM response content</param>
        /// <param name="label">The sensitivity label to apply</param>
        /// <returns>A processed sensitivity label response</returns>
        public async Task<SensitivityLabelResponse> ProcessLLMResponseAsync(string response, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(response))
                throw new ArgumentException("Response cannot be null or empty", nameof(response));

            if (label == null)
                throw new ArgumentNullException(nameof(label));

            var isValid = await _labelValidator.ValidateAsync(label);
            if (!isValid)
                throw new InvalidLabelException(label.Id, new ValidationResult { IsValid = false, ErrorMessage = "Invalid label for response processing" });

            return await CreateLabelResponseAsync(response, label);
        }

        /// <summary>
        /// Gets the highest priority label from a collection of labels
        /// </summary>
        /// <param name="labels">Collection of sensitivity labels</param>
        /// <returns>The label with the highest priority</returns>
        public async Task<SensitivityLabel> GetHighestPriorityLabelAsync(IEnumerable<SensitivityLabel> labels)
        {
            if (labels == null || !labels.Any())
                return await GetDefaultLabelAsync();

            var validLabels = new List<SensitivityLabel>();
            foreach (var label in labels)
            {
                if (await _labelValidator.ValidateAsync(label))
                    validLabels.Add(label);
            }

            if (!validLabels.Any())
                return await GetDefaultLabelAsync();

            return validLabels.OrderByDescending(l => l.Priority, _priorityComparer).First();
        }

        /// <summary>
        /// Validates a sensitivity label
        /// </summary>
        /// <param name="label">The label to validate</param>
        /// <returns>True if the label is valid, false otherwise</returns>
        public async Task<bool> ValidateLabelAsync(SensitivityLabel label)
        {
            if (label == null)
                return false;

            return await _labelValidator.ValidateAsync(label);
        }

        /// <summary>
        /// Formats response content with sensitivity label information
        /// </summary>
        /// <param name="content">The content to format</param>
        /// <param name="label">The sensitivity label to apply</param>
        /// <returns>Formatted content with label information</returns>
        public async Task<string> FormatResponseWithLabelAsync(string content, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            if (label == null)
                return content;

            var isValid = await _labelValidator.ValidateAsync(label);
            if (!isValid)
                return content;

            return await _labelFormatter.FormatWithLabelAsync(content, label);
        }

        /// <summary>
        /// Creates a sensitivity label response from content and label
        /// </summary>
        private async Task<SensitivityLabelResponse> CreateLabelResponseAsync(string content, SensitivityLabel label)
        {
            var formattedResponse = await _labelFormatter.FormatWithLabelAsync(content, label);
            
            // Handle encryption if required
            if (label.Protection.RequireEncryption && await _encryptionService.ShouldEncryptAsync(label))
            {
                formattedResponse = await _encryptionService.EncryptAsync(formattedResponse, label);
            }

            return new SensitivityLabelResponse
            {
                Label = label,
                Content = content,
                FormattedResponse = formattedResponse,
                ShouldDisplay = !label.Protection.PreventExtraction,
                AllowCopyPaste = !label.Protection.PreventCopyPaste,
                AllowGrounding = !label.Protection.PreventGrounding
            };
        }

        /// <summary>
        /// Gets the default sensitivity label for unclassified content
        /// </summary>
        private async Task<SensitivityLabel> GetDefaultLabelAsync()
        {
            var defaultLabel = await _labelRepository.GetByNameAsync("Internal");
            if (defaultLabel != null)
                return defaultLabel;

            // Create a basic internal label if none exists
            return new SensitivityLabel
            {
                Id = "internal-default",
                Name = "Internal",
                Description = "Default internal sensitivity label",
                Priority = LabelPriority.Internal,
                Protection = new ProtectionSettings
                {
                    RequireEncryption = false,
                    PreventExtraction = false,
                    PreventCopyPaste = false,
                    PreventGrounding = false
                },
                CustomProperties = new Dictionary<string, string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }
    }
}