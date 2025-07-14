using System.Collections.Generic;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for sensitivity label repository operations.
    /// </summary>
    public interface ILabelRepository
    {
        /// <summary>
        /// Gets a sensitivity label by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the label.</param>
        /// <returns>A task representing the label, or null if not found.</returns>
        Task<SensitivityLabel?> GetByIdAsync(string id);

        /// <summary>
        /// Gets a sensitivity label by its name.
        /// </summary>
        /// <param name="name">The name of the label.</param>
        /// <returns>A task representing the label, or null if not found.</returns>
        Task<SensitivityLabel?> GetByNameAsync(string name);

        /// <summary>
        /// Gets all sensitivity labels.
        /// </summary>
        /// <returns>A task representing the collection of all labels.</returns>
        Task<IEnumerable<SensitivityLabel>> GetAllAsync();

        /// <summary>
        /// Creates a new sensitivity label.
        /// </summary>
        /// <param name="label">The label to create.</param>
        /// <returns>A task representing the created label.</returns>
        Task<SensitivityLabel> CreateAsync(SensitivityLabel label);

        /// <summary>
        /// Updates an existing sensitivity label.
        /// </summary>
        /// <param name="label">The label to update.</param>
        /// <returns>A task representing the updated label.</returns>
        Task<SensitivityLabel> UpdateAsync(SensitivityLabel label);

        /// <summary>
        /// Deletes a sensitivity label by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the label to delete.</param>
        /// <returns>A task representing whether the deletion was successful.</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Gets all sensitivity labels with the specified priority.
        /// </summary>
        /// <param name="priority">The priority level to filter by.</param>
        /// <returns>A task representing the collection of labels with the specified priority.</returns>
        Task<IEnumerable<SensitivityLabel>> GetByPriorityAsync(LabelPriority priority);

        /// <summary>
        /// Gets all active sensitivity labels.
        /// </summary>
        /// <returns>A task representing the collection of active labels.</returns>
        Task<IEnumerable<SensitivityLabel>> GetActiveAsync();

        /// <summary>
        /// Checks if a sensitivity label exists by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier to check.</param>
        /// <returns>A task representing whether the label exists.</returns>
        Task<bool> ExistsAsync(string id);

        /// <summary>
        /// Gets the count of sensitivity labels.
        /// </summary>
        /// <returns>A task representing the count of labels.</returns>
        Task<int> GetCountAsync();
    }
}
