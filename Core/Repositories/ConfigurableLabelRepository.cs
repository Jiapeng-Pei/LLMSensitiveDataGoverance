using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Repositories
{
    /// <summary>
    /// Configurable label repository that can switch between different storage backends
    /// </summary>
    public class ConfigurableLabelRepository : ILabelRepository
    {
        private readonly ILabelRepository _underlyingRepository;
        private readonly bool _enableCaching;
        private readonly Dictionary<string, SensitivityLabel> _cache;
        private readonly object _cacheLock = new object();

        /// <summary>
        /// Initializes a new instance of the ConfigurableLabelRepository
        /// </summary>
        /// <param name="underlyingRepository">The underlying repository implementation</param>
        /// <param name="enableCaching">Whether to enable caching</param>
        public ConfigurableLabelRepository(ILabelRepository underlyingRepository, bool enableCaching = true)
        {
            _underlyingRepository = underlyingRepository ?? throw new ArgumentNullException(nameof(underlyingRepository));
            _enableCaching = enableCaching;
            _cache = enableCaching ? new Dictionary<string, SensitivityLabel>() : null;
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

            if (_enableCaching)
            {
                lock (_cacheLock)
                {
                    if (_cache.TryGetValue(id, out var cachedLabel))
                        return cachedLabel;
                }
            }

            var label = await _underlyingRepository.GetByIdAsync(id);
            
            if (label != null && _enableCaching)
            {
                lock (_cacheLock)
                {
                    _cache[id] = label;
                }
            }

            return label;
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

            if (_enableCaching)
            {
                lock (_cacheLock)
                {
                    foreach (var cachedLabel in _cache.Values)
                    {
                        if (cachedLabel.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                            return cachedLabel;
                    }
                }
            }

            var label = await _underlyingRepository.GetByNameAsync(name);
            
            if (label != null && _enableCaching)
            {
                lock (_cacheLock)
                {
                    _cache[label.Id] = label;
                }
            }

            return label;
        }

        /// <summary>
        /// Gets all labels
        /// </summary>
        /// <returns>Collection of all labels</returns>
        public async Task<IEnumerable<SensitivityLabel>> GetAllAsync()
        {
            var labels = await _underlyingRepository.GetAllAsync();
            
            if (_enableCaching)
            {
                lock (_cacheLock)
                {
                    foreach (var label in labels)
                    {
                        _cache[label.Id] = label;
                    }
                }
            }

            return labels;
        }

        /// <summary>
        /// Creates a new label
        /// </summary>
        /// <param name="label">Label to create</param>
        /// <returns>Created label</returns>
        public async Task<SensitivityLabel> CreateAsync(SensitivityLabel label)
        {
            var createdLabel = await _underlyingRepository.CreateAsync(label);
            
            if (_enableCaching)
            {
                lock (_cacheLock)
                {
                    _cache[createdLabel.Id] = createdLabel;
                }
            }

            return createdLabel;
        }

        /// <summary>
        /// Updates an existing label
        /// </summary>
        /// <param name="label">Label to update</param>
        /// <returns>Updated label</returns>
        public async Task<SensitivityLabel> UpdateAsync(SensitivityLabel label)
        {
            var updatedLabel = await _underlyingRepository.UpdateAsync(label);
            
            if (_enableCaching)
            {
                lock (_cacheLock)
                {
                    _cache[updatedLabel.Id] = updatedLabel;
                }
            }

            return updatedLabel;
        }

        /// <summary>
        /// Deletes a label
        /// </summary>
        /// <param name="id">Label ID to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        public async Task<bool> DeleteAsync(string id)
        {
            var deleted = await _underlyingRepository.DeleteAsync(id);
            
            if (deleted && _enableCaching)
            {
                lock (_cacheLock)
                {
                    _cache.Remove(id);
                }
            }

            return deleted;
        }

        /// <summary>
        /// Gets labels by priority
        /// </summary>
        /// <param name="priority">Label priority</param>
        /// <returns>Collection of labels with specified priority</returns>
        public async Task<IEnumerable<SensitivityLabel>> GetByPriorityAsync(LabelPriority priority)
        {
            return await _underlyingRepository.GetByPriorityAsync(priority);
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public void ClearCache()
        {
            if (_enableCaching)
            {
                lock (_cacheLock)
                {
                    _cache.Clear();
                }
            }
        }
    }
}
