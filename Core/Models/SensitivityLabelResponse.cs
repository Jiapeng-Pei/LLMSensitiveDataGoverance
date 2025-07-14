using System;
using System.ComponentModel.DataAnnotations;

namespace LLMSensitiveDataGoverance.Core.Models
{
    /// <summary>
    /// Represents the response from processing content with a sensitivity label,
    /// including the label information and formatted response.
    /// </summary>
    public class SensitivityLabelResponse
    {
        /// <summary>
        /// Gets or sets the sensitivity label applied to the content.
        /// </summary>
        [Required]
        public SensitivityLabel Label { get; set; } = new SensitivityLabel();

        /// <summary>
        /// Gets or sets the original content that was processed.
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formatted response with label information embedded.
        /// </summary>
        [Required]
        public string FormattedResponse { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the response should be displayed to the user.
        /// </summary>
        public bool ShouldDisplay { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether copy/paste operations are allowed.
        /// </summary>
        public bool AllowCopyPaste { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the content can be used for grounding.
        /// </summary>
        public bool AllowGrounding { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the content requires encryption.
        /// </summary>
        public bool RequiresEncryption { get; set; } = false;

        /// <summary>
        /// Gets or sets the timestamp when the response was generated.
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the processing metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the warning message if any restrictions apply.
        /// </summary>
        public string? WarningMessage { get; set; }

        /// <summary>
        /// Returns a string representation of the sensitivity label response.
        /// </summary>
        public override string ToString()
        {
            return $"Response for {Label.Name}: {(ShouldDisplay ? "Displayable" : "Restricted")}";
        }
    }
}
