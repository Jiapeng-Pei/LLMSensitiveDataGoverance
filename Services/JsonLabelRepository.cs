using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Repositories
{
    /// <summary>
    /// JSON file-based implementation of label repository
    /// </summary>
    public class JsonLabelRepository : ILabelRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly object _fileLock = new object();

        /// <summary>
        /// Initializes a new instance of the JsonLabelRepository
        /// </summary>
        /// <param name="filePath">Path to JSON file for storage</param>
        public JsonLabelRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            EnsureFileExists();
        }

        /// <summary>
        /// Gets a label by ID
        /// </summary>
        /// <param name="id">Label ID</param>
        /// <returns>Label if found, null otherwise</returns>
        public async Task<SensitivityLabel> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var labels = await LoadLabelsAsync();
            return labels.FirstOrDefault(l => l.Id == id);
        }

        /// <summary>
        /// Gets a label by name
        /// </summary>
        /// <param name="name">Label name</param>
        /// <returns>Label if found, null otherwise</returns>
        public async Task<SensitivityLabel> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var labels = await LoadLabelsAsync();
            return labels.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all labels
        /// </summary>
        /// <returns>Collection of all labels</returns>
        public async Task<IEnumerable<SensitivityLabel>> GetAllAsync()
        {
            return await LoadLabelsAsync();
        }

        /// <summary>
        /// Creates a new label
        /// </summary>
        /// <param name="label">Label to create</param>
        /// <returns>Created label</returns>
        /// <exception cref="ArgumentException">Thrown when label already exists</exception>
        public async Task<SensitivityLabel> CreateAsync(SensitivityLabel label)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));

            var labels = await LoadLabelsAsync();
            
            if (labels.Any(l => l.Id == label.Id))
                throw new ArgumentException($"Label with ID '{label.Id}' already exists");

            if (labels.Any(l => l.Name.Equals(label.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Label with name '{label.Name}' already exists");

            label.CreatedAt = DateTime.UtcNow;
            label.UpdatedAt = DateTime.UtcNow;

            var updatedLabels = labels.ToList();
            updatedLabels.Add(label);

            await SaveLabelsAsync(updatedLabels);
            return label;
        }

        /// <summary>
        /// Updates an existing label
        /// </summary>
        /// <param name="label">Label to update</param>
        /// <returns>Updated label</returns>
        /// <exception cref="ArgumentException">Thrown when label doesn't exist</exception>
        public async Task<SensitivityLabel> UpdateAsync(SensitivityLabel label)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));

            var labels = await LoadLabelsAsync();
            var existingLabel = labels.FirstOrDefault(l => l.Id == label.Id);
            
            if (existingLabel == null)
                throw new ArgumentException($"Label with ID '{label.Id}' does not exist");

            // Check for name conflicts with other labels
            if (labels.Any(l => l.Id != label.Id && l.Name.Equals(label.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Label with name '{label.Name}' already exists");

            label.UpdatedAt = DateTime.UtcNow;
            label.CreatedAt = existingLabel.CreatedAt; // Preserve original creation date

            var updatedLabels = labels.Where(l => l.Id != label.Id).ToList();
            updatedLabels.Add(label);

            await SaveLabelsAsync(updatedLabels);
            return label;
        }

        /// <summary>
        /// Deletes a label
        /// </summary>
        /// <param name="id">Label ID to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            var labels = await LoadLabelsAsync();
            var labelsToKeep = labels.Where(l => l.Id != id).ToList();
            
            if (labelsToKeep.Count == labels.Count())
                return false; // Label not found

            await SaveLabelsAsync(labelsToKeep);
            return true;
        }

        /// <summary>
        /// Gets labels by priority
        /// </summary>
        /// <param name="priority">Label priority</param>
        /// <returns>Collection of labels with specified priority</returns>
        public async Task<IEnumerable<SensitivityLabel>> GetByPriorityAsync(LabelPriority priority)
        {
            var labels = await LoadLabelsAsync();
            return labels.Where(l => l.Priority == priority).ToList();
        }

        /// <summary>
        /// Loads labels from JSON file
        /// </summary>
        private async Task<IEnumerable<SensitivityLabel>> LoadLabelsAsync()
        {
            try
            {
                string jsonContent;
                lock (_fileLock)
                {
                    if (!File.Exists(_filePath))
                        return new List<SensitivityLabel>();

                    jsonContent = File.ReadAllText(_filePath);
                }

                if (string.IsNullOrWhiteSpace(jsonContent))
                    return new List<SensitivityLabel>();

                var labels = JsonSerializer.Deserialize<List<SensitivityLabel>>(jsonContent, _jsonOptions);
                return await Task.FromResult(labels ?? new List<SensitivityLabel>());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load labels from file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves labels to JSON file
        /// </summary>
        private async Task SaveLabelsAsync(IEnumerable<SensitivityLabel> labels)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(labels, _jsonOptions);
                
                lock (_fileLock)
                {
                    File.WriteAllText(_filePath, jsonContent);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save labels to file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ensures the JSON file exists and has valid structure
        /// </summary>
        private void EnsureFileExists()
        {
            lock (_fileLock)
            {
                if (!File.Exists(_filePath))
                {
                    var directory = Path.GetDirectoryName(_filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Create empty JSON array
                    File.WriteAllText(_filePath, "[]");
                }
            }
        }
    }
}