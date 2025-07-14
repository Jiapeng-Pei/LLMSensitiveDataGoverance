using System.ComponentModel;

namespace LLMSensitiveDataGoverance.Core.Models
{
    /// <summary>
    /// Defines the priority levels for sensitivity labels.
    /// Higher values indicate higher sensitivity and take precedence.
    /// </summary>
    public enum LabelPriority
    {
        /// <summary>
        /// Public information with no restrictions.
        /// </summary>
        [Description("Public")]
        Public = 0,

        /// <summary>
        /// Internal information for organization use.
        /// </summary>
        [Description("Internal")]
        Internal = 1,

        /// <summary>
        /// Confidential information with access restrictions.
        /// </summary>
        [Description("Confidential")]
        Confidential = 2,

        /// <summary>
        /// Highly confidential information with strict access controls.
        /// </summary>
        [Description("Highly Confidential")]
        HighlyConfidential = 3,

        /// <summary>
        /// Restricted information with the highest level of protection.
        /// </summary>
        [Description("Restricted")]
        Restricted = 4
    }
}
