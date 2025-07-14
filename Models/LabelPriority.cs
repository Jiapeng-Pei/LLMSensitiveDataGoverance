using System;

namespace SensitivityLabelSystem.Core.Models
{
    /// <summary>
    /// Defines the priority levels for sensitivity labels.
    /// Higher priority labels take precedence over lower priority labels.
    /// </summary>
    public enum LabelPriority
    {
        /// <summary>
        /// Public information that can be shared freely.
        /// </summary>
        Public = 0,

        /// <summary>
        /// Internal information for organization use only.
        /// </summary>
        Internal = 1,

        /// <summary>
        /// Confidential information requiring special handling.
        /// </summary>
        Confidential = 2,

        /// <summary>
        /// Highly confidential information with strict access controls.
        /// </summary>
        HighlyConfidential = 3,

        /// <summary>
        /// Restricted information with the highest level of protection.
        /// </summary>
        Restricted = 4
    }
}
