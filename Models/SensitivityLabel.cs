using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LLMSensitiveDataGoverance.Core.Models
{
    /// <summary>
    /// Represents a sensitivity label that can be applied to grounding data
    /// to control access, encryption, and usage in LLM responses.
    /// </summary>
    public class SensitivityLabel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the sensitivity label.
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable name of the sensitivity label.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the sensitivity label.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the priority level of the sensitivity label.
        /// Higher priority labels take precedence over lower priority ones.
        /// </summary>
        public LabelPriority Priority { get; set; } = LabelPriority.Public;

        /// <summary>
        /// Gets or sets the protection settings for this sensitivity label.
        /// </summary>
        [Required]
        public ProtectionSettings Protection { get; set; } = new ProtectionSettings();

        /// <summary>
        /// Gets or sets custom properties for extensibility.
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the creation timestamp of the sensitivity label.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp of the sensitivity label.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets a value indicating whether the sensitivity label is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the visual display color for the label (hex format).
        /// </summary>
        [StringLength(7)]
        public string Color { get; set; } = "#000000";

        /// <summary>
        /// Gets or sets the icon name for visual representation.
        /// </summary>
        [StringLength(50)]
        public string IconName { get; set; } = "default";

        /// <summary>
        /// Returns a string representation of the sensitivity label.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Priority})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current sensitivity label.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is SensitivityLabel other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for the sensitivity label.
        /// </summary>
        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}
