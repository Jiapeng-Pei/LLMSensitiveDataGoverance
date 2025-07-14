using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Repositories
{
    /// <summary>
    /// In-memory implementation of label repository for testing and development
    /// </summary>
    public class InMemoryLabelRepository : ILabelRepository
    {
        private readonly ConcurrentDictionary<string, SensitivityLabel> _labels;
        private readonly ConcurrentDictionary<string, string> _nameToIdMap;

        /// <summary>
        /// Initializes a new instance of the InMemoryLabelRepository
        /// </summary>
        public InMemoryLabelRepository()
        {
            _labels = new ConcurrentDictionary<string, SensitivityLabel>();
            _nameToIdMap = new ConcurrentDictionary<string, string>();
            InitializeDefaultLabels();
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

            _labels.TryGetValue(id, out var label);
            return await Task.FromResult(label);
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

            if (_nameToIdMap.TryGetValue(name, out var id))
            {
                return await GetByIdAsync(id);
            }

            return null;
        }

        /// <summary>
        /// Gets all labels
        /// </summary>
        /// <returns>Collection of all labels</returns>
        public async Task<IEnumerable<SensitivityLabel>> GetAllAsync()
        {
            return await Task.FromResult(_labels.Values.ToList());
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

            if (_labels.ContainsKey(label.Id))
                throw new ArgumentException($"Label with ID '{label.Id}' already exists");

            if (_nameToIdMap.ContainsKey(label.Name))
                throw new ArgumentException($"Label with name '{label.Name}' already exists");

            label.CreatedAt = DateTime.UtcNow;
            label.UpdatedAt = DateTime.UtcNow;

            _labels.TryAdd(label.Id, label);
            _nameToIdMap.TryAdd(label.Name, label.Id);

            return await Task.FromResult(label);
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

            if (!_labels.ContainsKey(label.Id))
                throw new ArgumentException($"Label with ID '{label.Id}' does not exist");

            var existingLabel = _labels[label.Id];
            
            // Remove old name mapping if name changed
            if (existingLabel.Name != label.Name)
            {
                _nameToIdMap.TryRemove(existingLabel.Name, out _);
                _nameToIdMap.TryAdd(label.Name, label.Id);
            }

            label.UpdatedAt = DateTime.UtcNow;
            _labels.TryUpdate(label.Id, label, existingLabel);

            return await Task.FromResult(label);
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

            if (_labels.TryRemove(id, out var label))
            {
                _nameToIdMap.TryRemove(label.Name, out _);
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets labels by priority
        /// </summary>
        /// <param name="priority">Label priority</param>
        /// <returns>Collection of labels with specified priority</returns>
        public async Task<IEnumerable<SensitivityLabel>> GetByPriorityAsync(LabelPriority priority)
        {
            var labels = _labels.Values.Where(l => l.Priority == priority).ToList();
            return await Task.FromResult(labels);
        }

        /// <summary>
        /// Initializes default labels for the repository
        /// </summary>
        private void InitializeDefaultLabels()
        {
            var defaultLabels = new[]
            {
                new SensitivityLabel
                {
                    Id = "public",
                    Name = "Public",
                    Description = "Information that can be freely shared",
                    Priority = LabelPriority.Public,
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
                },
                new SensitivityLabel
                {
                    Id = "internal",
                    Name = "Internal",
                    Description = "Information for internal use only",
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
                },
                new SensitivityLabel
                {
                    Id = "confidential",
                    Name = "Confidential",
                    Description = "Sensitive information requiring protection",
                    Priority = LabelPriority.Confidential,
                    Protection = new ProtectionSettings
                    {
                        RequireEncryption = true,
                        PreventExtraction = false,
                        PreventCopyPaste = true,
                        PreventGrounding = false
                    },
                    CustomProperties = new Dictionary<string, string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
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
                        PreventGrounding = false
                    },
                    CustomProperties = new Dictionary<string, string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new SensitivityLabel
                {
                    Id = "restricted",
                    Name = "Restricted",
                    Description = "Restricted information with maximum security",
                    Priority = LabelPriority.Restricted,
                    Protection = new ProtectionSettings
                    {
                        RequireEncryption = true,
                        PreventExtraction = true,
                        PreventCopyPaste = true,
                        PreventGrounding = true
                    },
                    CustomProperties = new Dictionary<string, string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            foreach (var label in defaultLabels)
            {
                _labels.TryAdd(label.Id, label);
                _nameToIdMap.TryAdd(label.Name, label.Id);
            }
        }
    }
}
