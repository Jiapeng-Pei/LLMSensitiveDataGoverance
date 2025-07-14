using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Exceptions;
using LLMSensitiveDataGoverance.AgentIntegration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LLMSensitiveDataGoverance.AgentIntegration
{
    /// <summary>
    /// Processes and manages grounding data for AI agent integration
    /// </summary>
    public class GroundingDataProcessor
    {
        private readonly ISensitivityLabelService _labelService;
        private readonly ILabelRepository _labelRepository;
        private readonly AgentSettings _settings;
        private readonly ILogger<GroundingDataProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the GroundingDataProcessor
        /// </summary>
        /// <param name="labelService">The sensitivity label service</param>
        /// <param name="labelRepository">The label repository</param>
        /// <param name="settings">Agent configuration settings</param>
        /// <param name="logger">The logger instance</param>
        public GroundingDataProcessor(
            ISensitivityLabelService labelService,
            ILabelRepository labelRepository,
            IOptions<AgentSettings> settings,
            ILogger<GroundingDataProcessor> logger)
        {
            _labelService = labelService ?? throw new ArgumentNullException(nameof(labelService));
            _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes raw data into grounding data with sensitivity classification
        /// </summary>
        /// <param name="rawData">The raw data to process</param>
        /// <param name="source">The source of the data</param>
        /// <param name="dataType">The type of data</param>
        /// <returns>Processed grounding data with sensitivity labels</returns>
        public async Task<GroundingData> ProcessRawDataAsync(
            string rawData, 
            string source, 
            string dataType)
        {
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentNullException(nameof(rawData));

            try
            {
                _logger.LogDebug("Processing raw data from source: {Source}, type: {DataType}", 
                    source, dataType);

                var groundingData = new GroundingData
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = rawData,
                    Source = source ?? "unknown",
                    DataType = dataType ?? "text",
                    Metadata = new Dictionary<string, object>
                    {
                        { "ProcessedAt", DateTime.UtcNow },
                        { "ProcessorVersion", "1.0" }
                    },
                    LastModified = DateTime.UtcNow
                };

                // Classify the data
                var labelResponse = await _labelService.ClassifyAsync(groundingData);
                groundingData.Label = labelResponse.Label;

                _logger.LogDebug("Processed grounding data {Id} with label: {LabelName}", 
                    groundingData.Id, groundingData.Label?.Name ?? "None");

                return groundingData;
            }
            catch (SensitivityLabelException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing raw data from source: {Source}", source);
                throw new SensitivityLabelException("system", "Failed to process raw data");
            }
        }

        /// <summary>
        /// Filters grounding data based on agent configuration and protection settings
        /// </summary>
        /// <param name="groundingData">Collection of grounding data to filter</param>
        /// <returns>Filtered collection of grounding data</returns>
        public async Task<IEnumerable<GroundingData>> FilterGroundingDataAsync(
            IEnumerable<GroundingData> groundingData)
        {
            if (groundingData == null || !groundingData.Any())
                return Enumerable.Empty<GroundingData>();

            try
            {
                _logger.LogDebug("Filtering {Count} grounding data items", groundingData.Count());

                var filteredData = new List<GroundingData>();

                foreach (var data in groundingData)
                {
                    if (await ShouldIncludeGroundingDataAsync(data))
                    {
                        filteredData.Add(data);
                    }
                    else
                    {
                        _logger.LogDebug("Filtered out grounding data {Id} due to protection settings", 
                            data.Id);
                    }
                }

                _logger.LogDebug("Filtered to {Count} grounding data items", filteredData.Count);
                return filteredData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering grounding data");
                throw new SensitivityLabelException("system", "Failed to filter grounding data");
            }
        }

        /// <summary>
        /// Aggregates multiple grounding data sources and determines the overall sensitivity level
        /// </summary>
        /// <param name="groundingData">Collection of grounding data to aggregate</param>
        /// <returns>Aggregated sensitivity information</returns>
        public async Task<SensitivityLabel> AggregateDataSensitivityAsync(
            IEnumerable<GroundingData> groundingData)
        {
            if (groundingData == null || !groundingData.Any())
                return null;

            try
            {
                _logger.LogDebug("Aggregating sensitivity from {Count} grounding data sources", 
                    groundingData.Count());

                var labels = groundingData
                    .Select(g => g.Label)
                    .Where(l => l != null)
                    .ToList();

                if (!labels.Any())
                    return null;

                // Get the highest priority label
                var highestPriorityLabel = await _labelService.GetHighestPriorityLabelAsync(labels);

                _logger.LogDebug("Aggregated sensitivity level: {LabelName} (Priority: {Priority})", 
                    highestPriorityLabel?.Name, highestPriorityLabel?.Priority);

                return highestPriorityLabel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating data sensitivity");
                throw new SensitivityLabelException("system", "Failed to aggregate data sensitivity");
            }
        }

        /// <summary>
        /// Validates grounding data integrity and consistency
        /// </summary>
        /// <param name="groundingData">The grounding data to validate</param>
        /// <returns>True if the data is valid, false otherwise</returns>
        public async Task<bool> ValidateGroundingDataAsync(GroundingData groundingData)
        {
            if (groundingData == null)
                return false;

            try
            {
                _logger.LogDebug("Validating grounding data: {Id}", groundingData.Id);

                // Basic validation
                if (string.IsNullOrEmpty(groundingData.Content) || 
                    string.IsNullOrEmpty(groundingData.Id))
                {
                    return false;
                }

                // Label validation
                if (groundingData.Label != null)
                {
                    var isLabelValid = await _labelService.ValidateLabelAsync(groundingData.Label);
                    if (!isLabelValid)
                    {
                        _logger.LogWarning("Invalid label for grounding data: {Id}", groundingData.Id);
                        return false;
                    }
                }

                // Additional validation based on settings
                if (_settings.StrictValidation)
                {
                    return await PerformStrictValidationAsync(groundingData);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating grounding data: {Id}", groundingData.Id);
                return false;
            }
        }

        /// <summary>
        /// Determines if grounding data should be included based on protection settings
        /// </summary>
        /// <param name="data">The grounding data to check</param>
        /// <returns>True if data should be included, false otherwise</returns>
        private async Task<bool> ShouldIncludeGroundingDataAsync(GroundingData data)
        {
            if (data?.Label == null)
                return true; // No label means public

            // Check if grounding is prevented
            if (data.Label.Protection.PreventGrounding)
                return false;

            // Check if label is active
            if (!data.Label.IsActive)
                return false;

            // Check priority limits from settings
            if (_settings.MaxAllowedPriority.HasValue && 
                data.Label.Priority > _settings.MaxAllowedPriority.Value)
                return false;

            // Validate the label
            return await _labelService.ValidateLabelAsync(data.Label);
        }

        /// <summary>
        /// Performs strict validation on grounding data
        /// </summary>
        /// <param name="groundingData">The grounding data to validate</param>
        /// <returns>True if validation passes, false otherwise</returns>
        private async Task<bool> PerformStrictValidationAsync(GroundingData groundingData)
        {
            // Check content length limits
            if (groundingData.Content.Length > _settings.MaxContentLength)
                return false;

            // Check source validation
            if (!_settings.AllowedSources.Contains(groundingData.Source))
                return false;

            // Check data type validation
            if (!_settings.AllowedDataTypes.Contains(groundingData.DataType))
                return false;

            // Check modification time
            if (DateTime.UtcNow - groundingData.LastModified > _settings.MaxDataAge)
                return false;

            return true;
        }
    }
}