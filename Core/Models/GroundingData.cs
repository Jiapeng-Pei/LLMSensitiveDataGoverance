using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LLMSensitiveDataGoverance.Core.Models
{
    /// <summary>
    /// Represents data that can be used for grounding LLM responses,
    /// along with its associated sensitivity label.
    /// </summary>
    public class GroundingData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the grounding data.
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content of the grounding data.
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source of the grounding data (e.g., file path, URL, database).
        /// </summary>
        [Required]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data type (e.g., "text", "json", "xml", "csv").
        /// </summary>
        [Required]
        public string DataType { get; set; } = "text";

        /// <summary>
        /// Gets or sets the sensitivity label associated with this grounding data.
        /// </summary>
        public SensitivityLabel? Label { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the grounding data.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the timestamp when the grounding data was last modified.
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the grounding data was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the size of the content in bytes.
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Gets or sets the checksum of the content for integrity verification.
        /// </summary>
        public string? Checksum { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grounding data is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the version of the grounding data.
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Returns a string representation of the grounding data.
        /// </summary>
        public override string ToString()
        {
            return $"GroundingData: {Id} ({DataType}) - {Label?.Name ?? "No Label"}";
        }
    }
}
